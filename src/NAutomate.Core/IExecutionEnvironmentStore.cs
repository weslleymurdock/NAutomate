using NAutomate.Abstractions;

namespace NAutomate.Core;

public interface IExecutionEnvironmentStore
{
    Task<IReadOnlyList<AutomationEnvironment>> ListAsync(string workflowName, CancellationToken cancellationToken = default);
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
