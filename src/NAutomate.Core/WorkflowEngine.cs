using NAutomate.Abstractions;

namespace NAutomate.Core;

public sealed class WorkflowEngine(IModuleRegistry registry)
{
    public async Task<WorkflowExecutionResult> ExecuteAsync(
        AutomationWorkflow workflow,
        IExecutionEventSink sink,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);
        WorkflowJson.Validate(workflow);
        await sink.OnEventAsync(new("workflow", Message: $"Workflow: {workflow.Name}"), cancellationToken);

        try
        {
            foreach (var step in workflow.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var module = registry.Resolve(step.Module);
                await sink.OnEventAsync(
                    new("running", module.Descriptor.Id, StepId: step.Id),
                    cancellationToken);

                var result = await module.ExecuteAsync(
                    new(workflow, step, step.Parameters, cancellationToken));

                await sink.OnEventAsync(
                    new("output", module.Descriptor.Id, result.Output, step.Id),
                    cancellationToken);

                if (!result.Succeeded)
                {
                    await sink.OnEventAsync(
                        new("failure", module.Descriptor.Id, Message: "Step failed", StepId: step.Id),
                        cancellationToken);

                    return new WorkflowExecutionResult(
                        WorkflowExecutionStatus.Failure,
                        ErrorMessage: $"Step {step.Id} failed");
                }

                await sink.OnEventAsync(
                    new("success", module.Descriptor.Id, StepId: step.Id),
                    cancellationToken);
            }

            await sink.OnEventAsync(
                new("completed", Message: "Execution completed successfully."),
                cancellationToken);

            return new WorkflowExecutionResult(WorkflowExecutionStatus.Success);
        }
        catch (OperationCanceledException)
        {
            await sink.OnEventAsync(
                new("cancelled", Message: "Execution cancelled."),
                CancellationToken.None);

            return new WorkflowExecutionResult(WorkflowExecutionStatus.Cancelled);
        }
        catch (Exception ex)
        {
            await sink.OnEventAsync(
                new("exception", Message: ex.Message),
                CancellationToken.None);

            return new WorkflowExecutionResult(
                WorkflowExecutionStatus.Exception,
                ErrorMessage: ex.Message,
                Exception: ex);
        }
    }
}
