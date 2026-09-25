namespace NAutomate.Abstractions;

/// <summary>Coordinates execution environment persistence for a workflow.</summary>
public interface IExecutionEnvironmentService
{
    Task<IReadOnlyList<AutomationEnvironment>> ListAsync(
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default);

    Task<AutomationEnvironment> CreateAsync(
        string name,
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default);

    Task<AutomationEnvironment> SaveAsync(
        AutomationEnvironment environment,
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutomationEnvironment>> SynchronizeAsync(
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default);
}
