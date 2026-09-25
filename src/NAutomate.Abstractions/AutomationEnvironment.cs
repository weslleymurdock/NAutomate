namespace NAutomate.Abstractions;

public sealed record AutomationEnvironment(
    string Id,
    string WorkflowName,
    string Name,
    IReadOnlyDictionary<string, string?> Values);
