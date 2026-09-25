using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Creates the initial typed variable values for a workflow execution.</summary>
public static class AutomationWorkflowVariables
{
    public static IReadOnlyDictionary<string, object?> CreateValues(
        AutomationWorkflow workflow,
        IReadOnlyDictionary<string, string?>? existing = null)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var variable in workflow.Variables ?? [])
        {
            if (existing is not null && existing.TryGetValue(variable.Key, out var existingValue) && existingValue is not null)
            {
                values[variable.Key] = WorkflowParameterParser.Parse(existingValue, variable.Type);
                continue;
            }

            values[variable.Key] = variable.DefaultValue;
        }

        return values;
    }
}
