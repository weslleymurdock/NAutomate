using System.Text.Json.Serialization;

namespace NAutomate.Abstractions;

public sealed record AutomationVariableDefinition(
    string Name,
    AutomationVariableScope Scope = AutomationVariableScope.Global,
    string? StepId = null,
    string? Description = null)
{
    [JsonIgnore]
    public string Key => Scope == AutomationVariableScope.Global
        ? Name
        : $"{StepId}:{Name}";
}
