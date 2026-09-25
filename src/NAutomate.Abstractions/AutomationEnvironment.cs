namespace NAutomate.Abstractions;

public sealed record AutomationEnvironment(
    string Id,
    string Name,
    IReadOnlyDictionary<string, string?> Values);
