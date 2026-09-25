using NAutomate.Abstractions;

namespace NAutomate.Core;

/// <summary>Provides debugger-style control over sequential workflow execution.</summary>
public sealed class WorkflowExecutionSession : IAsyncDisposable
{
    private readonly WorkflowEngine _engine;
    private readonly AutomationWorkflow _workflow;
    private readonly IExecutionEventSink _sink;
    private readonly AutomationEnvironment? _environment;
    private readonly CancellationTokenSource _stopSource = new();
    private readonly object _gate = new();
    private TaskCompletionSource<bool> _resumeSignal = CreateSignal();
    private ExecutionControlState _state = ExecutionControlState.Paused;

    public WorkflowExecutionSession(
        WorkflowEngine engine,
        AutomationWorkflow workflow,
        IExecutionEventSink sink,
        AutomationEnvironment? environment = null)
    {
        _engine = engine;
        _workflow = workflow;
        _sink = sink;
        _environment = environment;
    }

    public ExecutionControlState State
    {
        get
        {
            lock (_gate)
                return _state;
        }
    }

    /// <summary>Starts or resumes continuous execution.</summary>
    public void Play() => Resume(ExecutionControlState.Running);

    /// <summary>Pauses before the next workflow step. A step already running is allowed to finish.</summary>
    public void Pause()
    {
        lock (_gate)
        {
            if (_state is ExecutionControlState.Completed or ExecutionControlState.Stopped)
                return;

            _state = ExecutionControlState.Paused;
        }
    }

    /// <summary>Executes exactly one workflow step and then pauses.</summary>
    public void StepInto() => Resume(ExecutionControlState.StepInto);

    /// <summary>Executes exactly one workflow step and then pauses. Step-over currently has the same granularity because workflows have no nested calls.</summary>
    public void StepOver() => Resume(ExecutionControlState.StepOver);

    /// <summary>Stops execution and terminates the currently running module when it observes cancellation.</summary>
    public void Stop()
    {
        lock (_gate)
        {
            if (_state is ExecutionControlState.Completed or ExecutionControlState.Stopped)
                return;

            _state = ExecutionControlState.Stopped;
            _resumeSignal.TrySetResult(true);
        }

        _stopSource.Cancel();
    }

    /// <summary>Runs the workflow until completion, cancellation, or an execution failure.</summary>
    public async Task<WorkflowExecutionResult> RunAsync(CancellationToken cancellationToken = default)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _stopSource.Token);

        var token = linkedCancellation.Token;
        WorkflowJson.Validate(_workflow);

        await _sink.OnEventAsync(
            new("workflow", Message: $"Workflow: {_workflow.Name}"),
            token);

        try
        {
            foreach (var step in _workflow.Steps)
            {
                await WaitForExecutionPermissionAsync(token);

                if (State == ExecutionControlState.Stopped)
                    return new WorkflowExecutionResult(WorkflowExecutionStatus.Cancelled);

                var module = _engine.Resolve(step.Module);
                await _sink.OnEventAsync(
                    new("running", module.Descriptor.Id, StepId: step.Id),
                    token);

                var result = await module.ExecuteAsync(
                    new(_workflow, step, EnvironmentVariableResolver.ResolveParameters(step, _environment), token));

                await _sink.OnEventAsync(
                    new("output", module.Descriptor.Id, result.Output, step.Id),
                    token);

                if (!result.Succeeded)
                {
                    await _sink.OnEventAsync(
                        new("failure", module.Descriptor.Id, Message: "Step failed", StepId: step.Id),
                        token);

                    SetTerminalState(ExecutionControlState.Stopped);
                    return new WorkflowExecutionResult(
                        WorkflowExecutionStatus.Failure,
                        ErrorMessage: $"Step {step.Id} failed");
                }

                await _sink.OnEventAsync(
                    new("success", module.Descriptor.Id, StepId: step.Id),
                    token);

                PauseAfterSingleStep();
            }

            SetTerminalState(ExecutionControlState.Completed);
            await _sink.OnEventAsync(
                new("completed", Message: "Execution completed successfully."),
                CancellationToken.None);

            return new WorkflowExecutionResult(WorkflowExecutionStatus.Success);
        }
        catch (OperationCanceledException)
        {
            SetTerminalState(ExecutionControlState.Stopped);
            await _sink.OnEventAsync(
                new("cancelled", Message: "Execution cancelled."),
                CancellationToken.None);

            return new WorkflowExecutionResult(WorkflowExecutionStatus.Cancelled);
        }
        catch (Exception ex)
        {
            SetTerminalState(ExecutionControlState.Stopped);
            await _sink.OnEventAsync(
                new("exception", Message: ex.Message),
                CancellationToken.None);

            return new WorkflowExecutionResult(
                WorkflowExecutionStatus.Exception,
                ErrorMessage: ex.Message,
                Exception: ex);
        }
    }

    public ValueTask DisposeAsync()
    {
        Stop();
        _stopSource.Dispose();
        return ValueTask.CompletedTask;
    }

    private void Resume(ExecutionControlState requestedState)
    {
        lock (_gate)
        {
            if (_state is ExecutionControlState.Completed or ExecutionControlState.Stopped)
                return;

            _state = requestedState;
            _resumeSignal.TrySetResult(true);
        }
    }

    private async Task WaitForExecutionPermissionAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            Task signal;

            lock (_gate)
            {
                if (_state is not ExecutionControlState.Paused)
                    return;

                signal = _resumeSignal.Task;
            }

            await signal.WaitAsync(cancellationToken);

            lock (_gate)
            {
                if (_state is not ExecutionControlState.Paused)
                    return;

                _resumeSignal = CreateSignal();
            }
        }
    }

    private void PauseAfterSingleStep()
    {
        lock (_gate)
        {
            if (_state is ExecutionControlState.StepInto or ExecutionControlState.StepOver)
            {
                _state = ExecutionControlState.Paused;
                _resumeSignal = CreateSignal();
            }
        }
    }

    private void SetTerminalState(ExecutionControlState state)
    {
        lock (_gate)
        {
            _state = state;
            _resumeSignal.TrySetResult(true);
        }
    }

    private static TaskCompletionSource<bool> CreateSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

