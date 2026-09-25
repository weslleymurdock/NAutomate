using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Converts editor text values into the typed values expected by module operations.</summary>
public static class WorkflowParameterParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static IReadOnlyDictionary<string, object?> Parse(
        IReadOnlyDictionary<string, string?> values,
        IReadOnlyList<AutomationParameterDescriptor> parameters)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(parameters);

        var descriptors = parameters.ToDictionary(x => x.Name, StringComparer.Ordinal);
        return values.ToDictionary(
            pair => pair.Key,
            pair => descriptors.TryGetValue(pair.Key, out var descriptor)
                ? Parse(pair.Value, descriptor.Type)
                : pair.Value,
            StringComparer.Ordinal);
    }

    public static object? Parse(string? value, string typeName)
    {
        if (typeName is null)
            throw new ArgumentNullException(nameof(typeName));

        var type = ResolveType(typeName);

        if (value is null)
            return null;

        if (type == typeof(string))
            return value;

        if (type == typeof(CancellationToken))
            throw new InvalidOperationException("CancellationToken is supplied by the runtime and cannot be configured.");

        if (Nullable.GetUnderlyingType(type) is Type nullableType)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            type = nullableType;
        }

        if (type.IsEnum)
            return Enum.Parse(type, value, ignoreCase: true);

        if (type == typeof(Guid))
            return Guid.Parse(value);

        if (type == typeof(TimeSpan))
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);

        if (type == typeof(Uri))
            return new Uri(value, UriKind.RelativeOrAbsolute);

        try
        {
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException or JsonException)
        {
            try
            {
                return JsonSerializer.Deserialize(value, type, JsonOptions)
                    ?? throw new InvalidDataException($"Could not parse value for parameter type '{typeName}'.");
            }
            catch (JsonException jsonException)
            {
                throw new InvalidDataException($"Could not parse value for parameter type '{typeName}'.", jsonException);
            }
        }
    }

    public static Type ResolveType(string typeName)
    {
        var normalized = typeName.Trim();

        var type = Type.GetType(normalized, throwOnError: false)
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(normalized, throwOnError: false))
                .FirstOrDefault(static candidate => candidate is not null);
        if (type is not null)
            return type;

        if (normalized.EndsWith("[]", StringComparison.Ordinal))
        {
            var elementType = ResolveType(normalized[..^2]);
            return elementType.MakeArrayType();
        }

        throw new InvalidDataException($"Unsupported automation parameter type '{typeName}'.");
    }
}
