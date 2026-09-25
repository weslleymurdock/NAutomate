namespace NAutomate.UI;

/// <summary>Describes a software dependency that must be available to the host.</summary>
public sealed record SoftwareDependencyDescriptor(
    string Id,
    string DisplayName,
    string Executable,
    IReadOnlyList<string>? Arguments = null,
    bool Required = true,
    string? ConfigurationHint = null);
