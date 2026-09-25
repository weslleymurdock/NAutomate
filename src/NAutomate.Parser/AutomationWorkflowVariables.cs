using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Creates the initial typed variable values for a workflow execution.</summary>
public static class AutomationWorkflowVariables
{
    public static IReadOnlyDictionary<string, object?> CreateValues(
        AutomationWorkflow workflow,
        IReadOnlyDictionary<string, object?>? existing = null)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var variable in workflow.Variables ?? [])
        {
            if (existing is not null && existing.TryGetValue(variable.Key, out var existingValue) && existingValue is not null)
            {
                values[variable.Key] = existingValue is string text ? WorkflowParameterParser.Parse(text, variable.Type) : existingValue;
                continue;
            }

            values[variable.Key] = variable.DefaultValue;
        }

        return values;
    }

    public static IReadOnlyDictionary<string, string?> ToPersistedValues(
        IReadOnlyDictionary<string, object?> values) =>
        values.ToDictionary(
            pair => pair.Key,
            pair => pair.Value is null
                ? null
                : Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture),
            StringComparer.Ordinal);
}
