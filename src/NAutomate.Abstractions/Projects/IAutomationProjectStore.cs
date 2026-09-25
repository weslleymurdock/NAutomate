namespace NAutomate.Abstractions.Projects;

public interface IAutomationProjectStore
{
    string ProjectsDirectory { get; }

    Task<IReadOnlyList<AutomationProjectInfo>> ListAsync(CancellationToken cancellationToken = default);

    Task<AutomationProjectInfo> CreateAsync(string name, CancellationToken cancellationToken = default);

    Task<AutomationProjectInfo> GetAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<AutomationProjectState> LoadAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task SaveAsync(AutomationProjectState project, CancellationToken cancellationToken = default);

    Task<AutomationProjectValidationResult> ValidateAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task DeleteAsync(AutomationProjectInfo project, CancellationToken cancellationToken = default);

    Task<AutomationProjectInfo> RepairAsync(AutomationProjectInfo project, CancellationToken cancellationToken = default);
}
