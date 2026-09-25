using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Core;

/// <summary>Provides debugger-style control over top-level workflow nodes.</summary>
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
        get { lock (_gate) return _state; }
    }

    public void Play() => Resume(ExecutionControlState.Running);

    public void Pause()
    {
        lock (_gate)
        {
            if (_state is ExecutionControlState.Completed or ExecutionControlState.Stopped) return;
            _state = ExecutionControlState.Paused;
        }
    }

    public void StepInto() => Resume(ExecutionControlState.StepInto);

    public void StepOver() => Resume(ExecutionControlState.StepOver);

    public void Stop()
    {
        lock (_gate)
        {
            if (_state is ExecutionControlState.Completed or ExecutionControlState.Stopped) return;
            _state = ExecutionControlState.Stopped;
            _resumeSignal.TrySetResult(true);
        }
        _stopSource.Cancel();
    }

    /// <summary>Runs top-level workflow nodes while preserving mutable state across debugger steps.</summary>
    public async Task<WorkflowExecutionResult> RunAsync(CancellationToken cancellationToken = default)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopSource.Token);
        var token = linkedCancellation.Token;
        WorkflowJson.Validate(_workflow);
        var variables = _engine.CreateVariableStore(_workflow, _environment);

        try
        {
            await _sink.OnEventAsync(new("workflow", Message: $"Workflow: {_workflow.Name}"), token);

            foreach (var step in _workflow.Steps)
            {
                await WaitForExecutionPermissionAsync(token);
                if (State == ExecutionControlState.Stopped)
                    return new WorkflowExecutionResult(WorkflowExecutionStatus.Cancelled);

                await _engine.ExecuteStepAsync(_workflow, step, variables, _environment, _sink, token);

                if (State is ExecutionControlState.StepInto or ExecutionControlState.StepOver)
                    PauseAfterSingleStep();
            }

            SetTerminalState(ExecutionControlState.Completed);
            await _sink.OnEventAsync(new("completed", Message: "Execution completed successfully."), CancellationToken.None);
            return new WorkflowExecutionResult(WorkflowExecutionStatus.Success);
        }
        catch (OperationCanceledException)
        {
            SetTerminalState(ExecutionControlState.Stopped);
            await _sink.OnEventAsync(new("cancelled", Message: "Execution cancelled."), CancellationToken.None);
            return new WorkflowExecutionResult(WorkflowExecutionStatus.Cancelled);
        }
        catch (WorkflowExitException ex)
        {
            SetTerminalState(ExecutionControlState.Completed);
            await _sink.OnEventAsync(new("completed", Message: $"Execution exited with code {ex.ExitCode}."), CancellationToken.None);
            return new WorkflowExecutionResult(
                ex.ExitCode == 0 ? WorkflowExecutionStatus.Success : WorkflowExecutionStatus.Failure,
                ex.ExitCode == 0 ? null : $"Workflow exited with code {ex.ExitCode}.");
        }
        catch (Exception ex)
        {
            SetTerminalState(ExecutionControlState.Stopped);
            await _sink.OnEventAsync(new("exception", Message: ex.Message), CancellationToken.None);
            return new WorkflowExecutionResult(WorkflowExecutionStatus.Exception, ex.Message, ex);
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
            if (_state is ExecutionControlState.Completed or ExecutionControlState.Stopped) return;
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
                if (_state is not ExecutionControlState.Paused) return;
                signal = _resumeSignal.Task;
            }

            await signal.WaitAsync(cancellationToken);
            lock (_gate)
            {
                if (_state is not ExecutionControlState.Paused) return;
                _resumeSignal = CreateSignal();
            }
        }
    }

    private void PauseAfterSingleStep()
    {
        lock (_gate)
        {
            _state = ExecutionControlState.Paused;
            _resumeSignal = CreateSignal();
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
