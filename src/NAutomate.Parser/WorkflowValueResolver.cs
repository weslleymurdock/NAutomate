using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Resolves typed workflow-variable references against the current execution state.</summary>
public static partial class WorkflowValueResolver
{
    public static object? Resolve(object? value, IWorkflowVariableStore variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        if (value is JsonElement element)
            value = ReadJsonElement(element);

        return value switch
        {
            string text => ResolveString(text, variables),
            IReadOnlyDictionary<string, object?> dictionary => dictionary.ToDictionary(
                pair => pair.Key,
                pair => Resolve(pair.Value, variables),
                StringComparer.Ordinal),
            IEnumerable<object?> collection => collection.Select(item => Resolve(item, variables)).ToArray(),
            _ => value
        };
    }

    public static object? ResolveString(string value, IWorkflowVariableStore variables)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(variables);

        var exact = ReferenceRegex().Match(value);
        if (exact.Success && exact.Length == value.Length)
            return variables.Get(exact.Groups["name"].Value);

        return ReferenceRegex().Replace(value, match =>
        {
            var name = match.Groups["name"].Value;
            return variables.TryGet(name, out var resolved)
                ? Convert.ToString(resolved, CultureInfo.InvariantCulture) ?? string.Empty
                : match.Value;
        });
    }

    private static object? ReadJsonElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ReadJsonElement).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(x => x.Name, x => ReadJsonElement(x.Value)),
            _ => null
        };

    [GeneratedRegex(@"[$@](?<name>[A-Za-z_][A-Za-z0-9_.:-]*)")]
    private static partial Regex ReferenceRegex();
}
