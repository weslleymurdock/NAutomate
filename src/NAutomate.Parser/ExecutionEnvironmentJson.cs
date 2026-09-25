using System.Text.Json;
using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Serializes and deserializes persisted execution environments.</summary>
public static class ExecutionEnvironmentJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static IReadOnlyList<AutomationEnvironment> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<AutomationEnvironment>>(json, Options) ?? [];
    }

    public static string Serialize(IReadOnlyList<AutomationEnvironment> environments) =>
        JsonSerializer.Serialize(environments, Options);
}
