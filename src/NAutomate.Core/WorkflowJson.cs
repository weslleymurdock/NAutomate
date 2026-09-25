using System.Text.Json;
using System.Text.Json.Serialization;
using NAutomate.Abstractions;

namespace NAutomate.Core;

/// <summary>Reads and writes the versioned declarative workflow format.</summary>
public static class WorkflowJson
{
    public const int CurrentSchemaVersion = 2;

    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static AutomationWorkflow Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException("Workflow JSON is empty.");

        try
        {
            var workflow = JsonSerializer.Deserialize<AutomationWorkflow>(json, Options)
                ?? throw new InvalidDataException("Workflow JSON is empty.");
            if (workflow.SchemaVersion == 1)
            {
                workflow = workflow with
                {
                    SchemaVersion = CurrentSchemaVersion,
                    Variables = workflow.Variables ?? []
                };
            }

            Validate(workflow);
            return workflow;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Workflow JSON is invalid.", exception);
        }
    }

    public static string Serialize(AutomationWorkflow workflow)
    {
        Validate(workflow);
        return JsonSerializer.Serialize(workflow, Options);
    }

    public static void Validate(AutomationWorkflow workflow)
    {
        if (workflow.SchemaVersion != CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported workflow schema version: {workflow.SchemaVersion}.");
        if (string.IsNullOrWhiteSpace(workflow.Name))
            throw new InvalidDataException("Workflow name is required.");
        if (workflow.Steps is null || workflow.Steps.Count == 0)
            throw new InvalidDataException("Workflow must contain at least one step.");
        if (workflow.Steps.Any(step => step is null))
            throw new InvalidDataException("Workflow steps cannot be null.");
        if (workflow.Steps.Any(step => string.IsNullOrWhiteSpace(step.Id) || string.IsNullOrWhiteSpace(step.Module)))
            throw new InvalidDataException("Every workflow step requires an id and module.");
        if (workflow.Steps.Any(step => step.Parameters is null))
            throw new InvalidDataException("Every workflow step requires a parameters object.");

        var variables = workflow.Variables ?? [];
        if (variables.Any(variable => string.IsNullOrWhiteSpace(variable.Name)))
            throw new InvalidDataException("Every workflow variable requires a name.");

        if (variables.Any(variable =>
            variable.Scope == AutomationVariableScope.Local &&
            string.IsNullOrWhiteSpace(variable.StepId)))
            throw new InvalidDataException("Local workflow variables require a step id.");

        var stepIds = workflow.Steps.Select(step => step.Id).ToHashSet(StringComparer.Ordinal);
        if (variables.Any(variable =>
            variable.Scope == AutomationVariableScope.Local &&
            !stepIds.Contains(variable.StepId!)))
            throw new InvalidDataException("Local workflow variables must reference an existing workflow step.");

        if (variables.Select(variable => variable.Key).Distinct(StringComparer.Ordinal).Count() != variables.Count)
            throw new InvalidDataException("Workflow variable keys must be unique.");
        if (workflow.Steps.Select(step => step.Id).Distinct(StringComparer.Ordinal).Count() != workflow.Steps.Count)
            throw new InvalidDataException("Workflow step ids must be unique.");
    }
}
