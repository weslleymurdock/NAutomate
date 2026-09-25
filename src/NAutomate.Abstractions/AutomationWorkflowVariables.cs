namespace NAutomate.Abstractions;

public static class AutomationWorkflowVariables
{
    public static IReadOnlyDictionary<string, string?> CreateValues(
        AutomationWorkflow workflow,
        IReadOnlyDictionary<string, string?>? existing = null)
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var variable in workflow.Variables ?? [])
            values[variable.Key] = existing is not null && existing.TryGetValue(variable.Key, out var value)
                ? value
                : null;

        return values;
    }
}
