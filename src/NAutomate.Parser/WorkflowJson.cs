using System.Text.Json;
using System.Text.Json.Serialization;
using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Reads, writes, and validates the versioned declarative workflow format.</summary>
public static class WorkflowJson
{
    public const int CurrentSchemaVersion = 3;

    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new WorkflowStepJsonConverter()
        }
    };

    public static AutomationWorkflow Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException("Workflow JSON is empty.");

        try
        {
            var workflow = JsonSerializer.Deserialize<AutomationWorkflow>(json, Options)
                ?? throw new InvalidDataException("Workflow JSON is empty.");

            if (workflow.SchemaVersion is 1 or 2)
            {
                workflow = workflow with
                {
                    SchemaVersion = CurrentSchemaVersion,
                    Variables = workflow.Variables ?? []
                };
            }

            Validate(workflow, requireSteps: false);
            return workflow;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Workflow JSON is invalid.", exception);
        }
    }

    public static string Serialize(AutomationWorkflow workflow)
    {
        Validate(workflow, requireSteps: false);
        return JsonSerializer.Serialize(workflow, Options);
    }

    public static void Validate(AutomationWorkflow workflow, bool requireSteps = true)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        if (workflow.SchemaVersion != CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported workflow schema version: {workflow.SchemaVersion}.");
        if (string.IsNullOrWhiteSpace(workflow.Name))
            throw new InvalidDataException("Workflow name is required.");
        if (requireSteps && (workflow.Steps is null || workflow.Steps.Count == 0))
            throw new InvalidDataException("Workflow must contain at least one step.");

        var variables = workflow.Variables ?? [];
        ValidateVariables(variables);

        var ids = new HashSet<string>(StringComparer.Ordinal);
        ValidateSteps(workflow.Steps, variables, "workflow", ids);

        foreach (var variable in variables.Where(x => x.Scope == AutomationVariableScope.Local))
        {
            if (!ids.Contains(variable.StepId!))
                throw new InvalidDataException($"Local workflow variable '{variable.Key}' references an unknown step '{variable.StepId}'.");
        }
    }

    private static void ValidateVariables(IReadOnlyList<AutomationVariableDefinition> variables)
    {
        if (variables.Any(variable => string.IsNullOrWhiteSpace(variable.Name)))
            throw new InvalidDataException("Every workflow variable requires a name.");

        if (variables.Any(variable => variable.Scope == AutomationVariableScope.Local && string.IsNullOrWhiteSpace(variable.StepId)))
            throw new InvalidDataException("Local workflow variables require a step id.");

        if (variables.Select(variable => variable.Key).Distinct(StringComparer.Ordinal).Count() != variables.Count)
            throw new InvalidDataException("Workflow variable keys must be unique.");

        foreach (var variable in variables)
        {
            if (string.IsNullOrWhiteSpace(variable.Type))
                throw new InvalidDataException($"Variable '{variable.Name}' requires a type.");

            try
            {
                _ = WorkflowParameterParser.ResolveType(variable.Type);
                if (variable.DefaultValue is not null)
                    _ = WorkflowParameterParser.Parse(variable.DefaultValue.ToString(), variable.Type);
            }
            catch (Exception exception) when (exception is InvalidDataException or FormatException or OverflowException or ArgumentException)
            {
                throw new InvalidDataException($"Variable '{variable.Name}' has an invalid type or default value.", exception);
            }
        }
    }

    private static void ValidateSteps(
        IReadOnlyList<WorkflowStep> steps,
        IReadOnlyList<AutomationVariableDefinition> variables,
        string path,
        HashSet<string> ids)
    {
        var knownVariables = variables.Select(variable => variable.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var step in steps)
        {
            if (step is null || string.IsNullOrWhiteSpace(step.Id))
                throw new InvalidDataException($"Every workflow step requires an id ({path}).");

            if (!ids.Add(step.Id))
                throw new InvalidDataException($"Workflow step id '{step.Id}' is duplicated.");

            switch (step)
            {
                case WorkflowStep moduleStep when moduleStep.Kind == WorkflowStepKind.Module:
                    if (string.IsNullOrWhiteSpace(moduleStep.Module))
                        throw new InvalidDataException($"Module step '{step.Id}' requires a module id.");
                    break;

                case IfStep ifStep:
                    ValidateCondition(ifStep.Condition, knownVariables, ifStep.Id);
                    ValidateSteps(ifStep.Then, variables, $"{path}/{ifStep.Id}/then", ids);
                    if (ifStep.Else is not null)
                        ValidateSteps(ifStep.Else, variables, $"{path}/{ifStep.Id}/else", ids);
                    break;

                case ForStep forStep:
                    RequireVariable(forStep.Variable, knownVariables, forStep.Id);
                    if (forStep.Step == 0)
                        throw new InvalidDataException($"For step '{forStep.Id}' cannot have a zero step.");
                    ValidateSteps(forStep.Body, variables, $"{path}/{forStep.Id}", ids);
                    break;

                case ForeachStep foreachStep:
                    RequireVariable(foreachStep.Collection.TrimStart('$', '@'), knownVariables, foreachStep.Id);
                    if (string.IsNullOrWhiteSpace(foreachStep.ItemVariable))
                        throw new InvalidDataException($"Foreach step '{foreachStep.Id}' requires an item variable.");
                    ValidateSteps(foreachStep.Body, variables, $"{path}/{foreachStep.Id}", ids);
                    break;

                case WhileStep whileStep:
                    ValidateCondition(whileStep.Condition, knownVariables, whileStep.Id);
                    if (whileStep.MaxIterations is <= 0)
                        throw new InvalidDataException($"While step '{whileStep.Id}' requires a positive maxIterations value.");
                    ValidateSteps(whileStep.Body, variables, $"{path}/{whileStep.Id}", ids);
                    break;

                case SetStep setStep:
                    RequireVariable(setStep.Variable, knownVariables, setStep.Id);
                    if (setStep.Operation != WorkflowSetOperation.Set && setStep.Value is null)
                        throw new InvalidDataException($"Set step '{setStep.Id}' requires a value for {setStep.Operation}.");
                    break;

                case ExitStep:
                    break;

                default:
                    throw new InvalidDataException($"Unsupported workflow step type '{step.Kind}'.");
            }
        }
    }

    private static void ValidateCondition(WorkflowCondition condition, HashSet<string> knownVariables, string stepId)
    {
        if (condition is null || string.IsNullOrWhiteSpace(condition.Variable))
            throw new InvalidDataException($"Conditional step '{stepId}' requires a variable.");

        RequireVariable(condition.Variable, knownVariables, stepId);
    }

    private static void RequireVariable(string name, HashSet<string> knownVariables, string stepId)
    {
        var normalized = name.TrimStart('$', '@');
        if (!knownVariables.Contains(normalized))
            throw new InvalidDataException($"Step '{stepId}' references unknown variable '{name}'.");
    }
}
