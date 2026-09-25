using System.Text.RegularExpressions;
using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Resolves declarative environment variable placeholders in workflow parameters.</summary>
public static partial class EnvironmentVariableResolver
{
    public static IReadOnlyDictionary<string, object?> ResolveParameters(
        WorkflowStep step,
        AutomationEnvironment? environment)
    {
        ArgumentNullException.ThrowIfNull(step);

        if (environment is null || environment.Values.Count == 0)
            return step.Parameters;

        return step.Parameters.ToDictionary(
            pair => pair.Key,
            pair => ResolveValue(pair.Value, environment.Values),
            StringComparer.Ordinal);
    }

    public static string ResolveString(
        string value,
        IReadOnlyDictionary<string, string?> values)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(values);

        return PlaceholderRegex().Replace(value, match =>
        {
            var key = match.Groups["key"].Value;
            return values.TryGetValue(key, out var resolved)
                ? resolved ?? string.Empty
                : match.Value;
        });
    }

    private static object? ResolveValue(
        object? value,
        IReadOnlyDictionary<string, string?> values)
    {
        return value switch
        {
            string text => ResolveString(text, values),
            IReadOnlyDictionary<string, object?> dictionary =>
                dictionary.ToDictionary(
                    pair => pair.Key,
                    pair => ResolveValue(pair.Value, values),
                    StringComparer.Ordinal),
            IEnumerable<object?> collection =>
                collection.Select(item => ResolveValue(item, values)).ToArray(),
            _ => value
        };
    }

    [GeneratedRegex(@"\$\{(?<key>[A-Za-z_][A-Za-z0-9_:\.-]*)\}")]
    private static partial Regex PlaceholderRegex();
}
