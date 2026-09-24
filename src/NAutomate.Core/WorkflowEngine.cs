using NAutomate.Abstractions;

namespace NAutomate.Core;

/// <summary>Executes a validated workflow using registered precompiled modules.</summary>
public sealed class WorkflowEngine(IModuleRegistry registry)
{
    public async Task ExecuteAsync(AutomationWorkflow workflow, IExecutionEventSink sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);
        WorkflowJson.Validate(workflow);
        await sink.OnEventAsync(new("workflow", Message: $"Workflow: {workflow.Name}"), cancellationToken);
        foreach (var step in workflow.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var module = registry.Resolve(step.Module);
            await sink.OnEventAsync(new("running", module.Descriptor.Id), cancellationToken);
            var result = await module.ExecuteAsync(new(workflow, step, cancellationToken));
            await sink.OnEventAsync(new("output", module.Descriptor.Id, result.Output), cancellationToken);
            await sink.OnEventAsync(new("success", module.Descriptor.Id), cancellationToken);
        }
        await sink.OnEventAsync(new("completed", Message: "Execution completed successfully."), cancellationToken);
    }
}
