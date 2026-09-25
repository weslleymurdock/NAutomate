using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Core;

/// <summary>Execution-local mutable storage for workflow variables.</summary>
public sealed class WorkflowVariableStore : IWorkflowVariableStore
{
    private readonly Dictionary<string, object?> _values;
    private readonly Dictionary<string, string> _types;

    public WorkflowVariableStore(IReadOnlyDictionary<string, object?> initialValues)
        : this(null, initialValues)
    {
    }

    public WorkflowVariableStore(
        IReadOnlyList<AutomationVariableDefinition>? definitions = null,
        IReadOnlyDictionary<string, object?>? initialValues = null)
    {
        _values = new Dictionary<string, object?>(StringComparer.Ordinal);
        _types = (definitions ?? []).ToDictionary(x => x.Key, x => x.Type, StringComparer.Ordinal);

        foreach (var definition in definitions ?? [])
        {
            var value = initialValues is not null && initialValues.TryGetValue(definition.Key, out var initial)
                ? initial
                : definition.DefaultValue;
            _values[definition.Key] = ConvertValue(value, definition.Type);
        }

        if (definitions is null && initialValues is not null)
        {
            foreach (var pair in initialValues)
                _values[pair.Key] = pair.Value;
        }
    }

    public IReadOnlyCollection<string> Names => _values.Keys.ToArray();

    public bool Contains(string name) => _values.ContainsKey(Normalize(name));

    public object? Get(string name) =>
        _values.TryGetValue(Normalize(name), out var value)
            ? value
            : throw new KeyNotFoundException($"Workflow variable '{name}' is not defined.");

    public bool TryGet(string name, out object? value) =>
        _values.TryGetValue(Normalize(name), out value);

    public void Set(string name, object? value)
    {
        var key = Normalize(name);
        if (_types.TryGetValue(key, out var typeName))
            value = ConvertValue(value, typeName);

        _values[key] = value;
    }

    public bool Remove(string name) => _values.Remove(Normalize(name));

    private static object? ConvertValue(object? value, string typeName)
    {
        if (value is null)
            return null;

        if (value is System.Text.Json.JsonElement element)
        {
            value = element.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => element.GetString(),
                System.Text.Json.JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
                System.Text.Json.JsonValueKind.Number => element.GetDecimal(),
                System.Text.Json.JsonValueKind.True or System.Text.Json.JsonValueKind.False => element.GetBoolean(),
                System.Text.Json.JsonValueKind.Null => null,
                _ => value
            };
        }

        var target = Type.GetType(typeName, false)
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName, false))
                .FirstOrDefault(candidate => candidate is not null);

        if (target is null)
            throw new InvalidDataException($"Unsupported workflow variable type '{typeName}'.");

        if (target.IsInstanceOfType(value))
            return value;

        return WorkflowParameterParser.Parse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), typeName);
    }

    private static string Normalize(string name) => name.TrimStart('$', '@');
}
