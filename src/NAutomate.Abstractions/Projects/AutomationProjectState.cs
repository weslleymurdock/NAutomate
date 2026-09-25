using System.Text.Json;

namespace NAutomate.Abstractions.Projects;

public sealed class AutomationProjectState
{
    public required AutomationProjectInfo Info { get; init; }
    public required AutomationWorkflow Workflow { get; init; }
    public AutomationProjectSettings Settings { get; init; } = new();
    public AutomationEnvironmentFile Environment { get; init; } = new();
}

public sealed class AutomationProjectSettings
{
    public int SchemaVersion { get; set; } = 1;
    public Dictionary<string, JsonElement> Values { get; set; } = new(StringComparer.Ordinal);
}

public sealed class AutomationEnvironmentFile
{
    public int SchemaVersion { get; set; } = 1;
    public Dictionary<string, Dictionary<string, JsonElement>> Environments { get; set; } =
        new(StringComparer.Ordinal);
}

public sealed class AutomationProjectManifest
{
    public int SchemaVersion { get; set; } = 1;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DirectoryName { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
    public AutomationProjectHashes Hashes { get; set; } = new();
}

public sealed class AutomationProjectHashes
{
    public string Automation { get; set; } = string.Empty;
    public string Settings { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
}

public sealed record AutomationProjectValidationResult(bool IsValid, IReadOnlyList<string> Errors);
