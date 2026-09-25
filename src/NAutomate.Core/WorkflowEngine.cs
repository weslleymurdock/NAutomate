using System.Collections;
using System.Globalization;
using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Core;

public sealed class WorkflowEngine(IModuleRegistry registry)
{
    private const int DefaultWhileIterationLimit = 10_000;
    private readonly WorkflowConditionEvaluator _conditionEvaluator = new();

    internal IAutomationModule Resolve(string moduleId) => registry.Resolve(moduleId);

    internal WorkflowVariableStore CreateVariableStore(
        AutomationWorkflow workflow,
        AutomationEnvironment? environment) =>
        new(workflow.Variables, AutomationWorkflowVariables.CreateValues(
            workflow,
            environment?.Values.ToDictionary(x => x.Key, x => (object?)x.Value, StringComparer.Ordinal)));

    public Task<WorkflowExecutionResult> ExecuteAsync(
        AutomationWorkflow workflow,
        IExecutionEventSink sink,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(workflow, sink, null, cancellationToken);

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        AutomationWorkflow workflow,
        IExecutionEventSink sink,
        AutomationEnvironment? environment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);
        WorkflowJson.Validate(workflow);
        var variables = CreateVariableStore(workflow, environment);

        await sink.OnEventAsync(new("workflow", Message: $"Workflow: {workflow.Name}"), cancellationToken);

        try
        {
            await ExecuteStepsAsync(workflow, workflow.Steps, variables, environment, sink, cancellationToken);
            await sink.OnEventAsync(new("completed", Message: "Execution completed successfully."), cancellationToken);
            return new WorkflowExecutionResult(WorkflowExecutionStatus.Success);
        }
        catch (OperationCanceledException)
        {
            await sink.OnEventAsync(new("cancelled", Message: "Execution cancelled."), CancellationToken.None);
            return new WorkflowExecutionResult(WorkflowExecutionStatus.Cancelled);
        }
        catch (Exception ex)
        {
            await sink.OnEventAsync(new("exception", Message: ex.Message), CancellationToken.None);
            return new WorkflowExecutionResult(WorkflowExecutionStatus.Exception, ex.Message, ex);
        }
    }

    internal async Task ExecuteStepsAsync(
        AutomationWorkflow workflow,
        IReadOnlyList<WorkflowStep> steps,
        IWorkflowVariableStore variables,
        AutomationEnvironment? environment,
        IExecutionEventSink sink,
        CancellationToken cancellationToken)
    {
        foreach (var step in steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteStepAsync(workflow, step, variables, environment, sink, cancellationToken);
        }
    }

    internal async Task ExecuteStepAsync(
        AutomationWorkflow workflow,
        WorkflowStep step,
        IWorkflowVariableStore variables,
        AutomationEnvironment? environment,
        IExecutionEventSink sink,
        CancellationToken cancellationToken)
    {
        switch (step)
        {
            case IfStep ifStep:
            {
                var result = _conditionEvaluator.Evaluate(ifStep.Condition, variables);
                await sink.OnEventAsync(new("condition-evaluated", Message: result.ToString(), StepId: step.Id), cancellationToken);
                await sink.OnEventAsync(new("branch-selected", Message: result ? "then" : "else", StepId: step.Id), cancellationToken);
                var branch = result ? ifStep.Then : ifStep.Else;
                if (branch is not null)
                    await ExecuteStepsAsync(workflow, branch, variables, environment, sink, cancellationToken);
                break;
            }
            case ForStep forStep:
            {
                if (forStep.Step == 0)
                    throw new InvalidDataException($"For step '{step.Id}' cannot have a zero step.");

                await sink.OnEventAsync(new("loop-started", Message: "for", StepId: step.Id), cancellationToken);
                long value = forStep.From;
                var iterations = 0;
                while (IsInRange(value, forStep.To, forStep.Step, forStep.Inclusive))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    variables.Set(forStep.Variable, value);
                    await sink.OnEventAsync(new("loop-iteration-started", Message: value.ToString(CultureInfo.InvariantCulture), StepId: step.Id), cancellationToken);
                    await ExecuteStepsAsync(workflow, forStep.Body, variables, environment, sink, cancellationToken);
                    await sink.OnEventAsync(new("loop-iteration-completed", Message: value.ToString(CultureInfo.InvariantCulture), StepId: step.Id), cancellationToken);
                    if (++iterations > DefaultWhileIterationLimit)
                        throw new InvalidOperationException($"For step '{step.Id}' exceeded the execution iteration limit.");
                    checked { value += forStep.Step; }
                }
                break;
            }
            case ForeachStep foreachStep:
            {
                var collectionValue = WorkflowValueResolver.ResolveString(foreachStep.Collection, variables);
                if (collectionValue is not IEnumerable collection || collectionValue is string)
                    throw new InvalidDataException($"Foreach step '{step.Id}' requires a collection variable.");

                await sink.OnEventAsync(new("loop-started", Message: "foreach", StepId: step.Id), cancellationToken);
                var hadPrevious = variables.TryGet(foreachStep.ItemVariable, out var previous);
                try
                {
                    foreach (var item in collection)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        variables.Set(foreachStep.ItemVariable, item);
                        await sink.OnEventAsync(new("loop-iteration-started", StepId: step.Id), cancellationToken);
                        await ExecuteStepsAsync(workflow, foreachStep.Body, variables, environment, sink, cancellationToken);
                        await sink.OnEventAsync(new("loop-iteration-completed", StepId: step.Id), cancellationToken);
                    }
                }
                finally
                {
                    if (hadPrevious)
                        variables.Set(foreachStep.ItemVariable, previous);
                    else
                        variables.Remove(foreachStep.ItemVariable);
                }
                break;
            }
            case WhileStep whileStep:
            {
                var maxIterations = whileStep.MaxIterations ?? DefaultWhileIterationLimit;
                await sink.OnEventAsync(new("loop-started", Message: "while", StepId: step.Id), cancellationToken);
                var iteration = 0;
                while (_conditionEvaluator.Evaluate(whileStep.Condition, variables))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (iteration >= maxIterations)
                        throw new InvalidOperationException($"While step '{step.Id}' exceeded the maximum iteration limit of {maxIterations}.");

                    await sink.OnEventAsync(new("loop-iteration-started", Message: iteration.ToString(CultureInfo.InvariantCulture), StepId: step.Id), cancellationToken);
                    await ExecuteStepsAsync(workflow, whileStep.Body, variables, environment, sink, cancellationToken);
                    await sink.OnEventAsync(new("loop-iteration-completed", Message: iteration.ToString(CultureInfo.InvariantCulture), StepId: step.Id), cancellationToken);
                    iteration++;
                }
                break;
            }
            case SetStep setStep:
            {
                var value = WorkflowValueResolver.Resolve(setStep.Value, variables);
                var current = variables.Get(setStep.Variable);
                var updated = setStep.Operation switch
                {
                    WorkflowSetOperation.Set => value,
                    WorkflowSetOperation.Increment => NumericOperation(current, value, 1),
                    WorkflowSetOperation.Decrement => NumericOperation(current, value, -1),
                    _ => throw new InvalidDataException($"Unsupported set operation '{setStep.Operation}'.")
                };
                variables.Set(setStep.Variable, updated);
                await sink.OnEventAsync(new("variable-changed", Message: $"{setStep.Variable}={Convert.ToString(updated, CultureInfo.InvariantCulture)}", StepId: setStep.Id), cancellationToken);
                break;
            }
            default:
                await ExecuteModuleAsync(workflow, step, variables, environment, sink, cancellationToken);
                break;
        }
    }

    private async Task ExecuteModuleAsync(
        AutomationWorkflow workflow,
        WorkflowStep step,
        IWorkflowVariableStore variables,
        AutomationEnvironment? environment,
        IExecutionEventSink sink,
        CancellationToken cancellationToken)
    {
        var module = registry.Resolve(step.Module ?? throw new InvalidDataException($"Step '{step.Id}' has no module id."));
        var environmentParameters = EnvironmentVariableResolver.ResolveParameters(step, environment);
        var parameters = environmentParameters.ToDictionary(
            pair => pair.Key,
            pair => WorkflowValueResolver.Resolve(pair.Value, variables),
            StringComparer.Ordinal);

        await sink.OnEventAsync(new("running", module.Descriptor.Id, StepId: step.Id), cancellationToken);
        var result = await module.ExecuteAsync(new(workflow, step, parameters, cancellationToken, environment, variables));

        if (!result.Succeeded)
        {
            await sink.OnEventAsync(new("failure", module.Descriptor.Id, "Step failed", step.Id), cancellationToken);
            throw new InvalidOperationException($"Step {step.Id} failed.");
        }

        await sink.OnEventAsync(new("output", module.Descriptor.Id, result.Output, step.Id), cancellationToken);
        await sink.OnEventAsync(new("success", module.Descriptor.Id, StepId: step.Id), cancellationToken);
    }

    private static bool IsInRange(long value, long end, long step, bool inclusive) =>
        step > 0 ? (inclusive ? value <= end : value < end) : (inclusive ? value >= end : value > end);

    private static object NumericOperation(object? current, object? operand, int direction)
    {
        if (current is null)
            throw new InvalidDataException("Numeric variable cannot be null.");

        var amount = operand is null ? 1m : Convert.ToDecimal(operand, CultureInfo.InvariantCulture);
        var result = Convert.ToDecimal(current, CultureInfo.InvariantCulture) + amount * direction;

        return current switch
        {
            byte => checked((byte)result),
            short => checked((short)result),
            int => checked((int)result),
            long => checked((long)result),
            float => (float)result,
            double => (double)result,
            decimal => result,
            _ => throw new InvalidDataException($"Type '{current.GetType()}' does not support numeric updates.")
        };
    }
}
