namespace NAutomate.UI;

/// <summary>Represents the result of validating a host software dependency.</summary>
public sealed record SoftwareDependencyStatus(
    SoftwareDependencyDescriptor Dependency,
    bool IsAvailable,
    string? Details = null);
