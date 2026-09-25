using NAutomate.Abstractions;

namespace NAutomate.Core;

/// <summary>Application service for workflow execution environment lifecycle.</summary>
public sealed class ExecutionEnvironmentService(IExecutionEnvironmentStore store) : IExecutionEnvironmentService
{
    public Task<IReadOnlyList<AutomationEnvironment>> ListAsync(
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        return store.ListAsync(workflow.Name, cancellationToken);
    }

    public Task<AutomationEnvironment> CreateAsync(
        string name,
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        return store.CreateAsync(name, workflow, cancellationToken);
    }

    public Task<AutomationEnvironment> SaveAsync(
        AutomationEnvironment environment,
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(workflow);
        return store.SaveAsync(environment, workflow, cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        store.DeleteAsync(id, cancellationToken);

    public Task<IReadOnlyList<AutomationEnvironment>> SynchronizeAsync(
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        return store.SynchronizeAsync(workflow, cancellationToken);
    }
}
