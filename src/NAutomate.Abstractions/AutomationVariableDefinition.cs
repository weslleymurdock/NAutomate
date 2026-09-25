using System.Text.Json.Serialization;

namespace NAutomate.Abstractions;

/// <summary>Declares the type and scope of a workflow variable.</summary>
public sealed record AutomationVariableDefinition(
    string Name,
    AutomationVariableScope Scope = AutomationVariableScope.Global,
    string? StepId = null,
    string? Description = null,
    string Type = "System.String",
    object? DefaultValue = null)
{
    [JsonIgnore]
    public string Key => Scope == AutomationVariableScope.Global
        ? Name
        : $"{StepId}:{Name}";
}
