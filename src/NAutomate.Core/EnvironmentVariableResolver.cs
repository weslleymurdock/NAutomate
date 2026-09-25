using System.Text.RegularExpressions;
using NAutomate.Abstractions;

namespace NAutomate.Core;

public static partial class EnvironmentVariableResolver
{
    public static IReadOnlyDictionary<string, object?> ResolveParameters(
        WorkflowStep step,
        AutomationEnvironment? environment)
    {
        if (environment is null || environment.Values.Count == 0)
            return step.Parameters;

        return step.Parameters.ToDictionary(
            pair => pair.Key,
            pair => ResolveValue(pair.Value, step.Id, environment.Values),
            StringComparer.Ordinal);
    }

    public static string ResolveString(
        string value,
        string stepId,
        IReadOnlyDictionary<string, string?> values)
    {
        return PlaceholderRegex().Replace(value, match =>
        {
            var key = match.Groups["key"].Value;
            var scopedKey = key.Contains(':', StringComparison.Ordinal) ? key : key;
            return values.TryGetValue(scopedKey, out var resolved)
                ? resolved ?? string.Empty
                : match.Value;
        });
    }

    private static object? ResolveValue(
        object? value,
        string stepId,
        IReadOnlyDictionary<string, string?> values)
    {
        return value switch
        {
            string text => ResolveString(text, stepId, values),
            IReadOnlyDictionary<string, object?> dictionary =>
                dictionary.ToDictionary(
                    pair => pair.Key,
                    pair => ResolveValue(pair.Value, stepId, values),
                    StringComparer.Ordinal),
            IEnumerable<object?> collection =>
                collection.Select(item => ResolveValue(item, stepId, values)).ToArray(),
            _ => value
        };
    }

    [GeneratedRegex(@"${(?<key>[A-Za-z_][A-Za-z0-9_:.-]*)}")]
    private static partial Regex PlaceholderRegex();
}
