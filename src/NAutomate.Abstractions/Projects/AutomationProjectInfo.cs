namespace NAutomate.Abstractions.Projects;

public sealed record AutomationProjectInfo(
    Guid Id,
    string Name,
    string DirectoryName,
    string DirectoryPath,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc,
    bool IsValid,
    IReadOnlyList<string> ValidationErrors);
