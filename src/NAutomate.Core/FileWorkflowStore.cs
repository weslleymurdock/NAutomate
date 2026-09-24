using NAutomate.Abstractions;

namespace NAutomate.Core;

/// <summary>Persists workflows as JSON files.</summary>
public sealed class FileWorkflowStore
{
    public async Task SaveAsync(string path, AutomationWorkflow workflow, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await File.WriteAllTextAsync(path, WorkflowJson.Serialize(workflow), cancellationToken);
    }

    public async Task<AutomationWorkflow> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return WorkflowJson.Deserialize(await File.ReadAllTextAsync(path, cancellationToken));
    }
}
