using System.Text.Json;
using System.Text.Json.Serialization;
using NAutomate.Abstractions;

namespace NAutomate.Parser;

internal sealed class WorkflowStepJsonConverter : JsonConverter<WorkflowStep>
{
    public override WorkflowStep Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var id = root.TryGetProperty("id", out var idProperty)
            ? idProperty.GetString()
            : null;
        if (string.IsNullOrWhiteSpace(id))
            throw new JsonException("Workflow step requires an id.");

        var kind = root.TryGetProperty("type", out var typeProperty)
            ? ParseKind(typeProperty.GetString())
            : WorkflowStepKind.Module;

        return kind switch
        {
            WorkflowStepKind.Module => new WorkflowStep(
                id,
                root.GetProperty("module").GetString()
                    ?? throw new JsonException("Module step requires a module id."),
                ReadParameters(root, options)),
            WorkflowStepKind.If => new IfStep(
                id,
                JsonSerializer.Deserialize<WorkflowCondition>(root.GetProperty("condition"), options)
                    ?? throw new JsonException("If step requires a condition."),
                JsonSerializer.Deserialize<IReadOnlyList<WorkflowStep>>(root.GetProperty("then"), options) ?? [],
                root.TryGetProperty("else", out var elseElement)
                    ? JsonSerializer.Deserialize<IReadOnlyList<WorkflowStep>>(elseElement, options)
                    : null),
            WorkflowStepKind.For => new ForStep(
                id,
                root.GetProperty("variable").GetString() ?? throw new JsonException("For step requires a variable."),
                root.GetProperty("from").GetInt64(),
                root.GetProperty("to").GetInt64(),
                root.TryGetProperty("step", out var stepElement) ? stepElement.GetInt64() : 1,
                root.TryGetProperty("inclusive", out var inclusiveElement) && inclusiveElement.GetBoolean(),
                root.TryGetProperty("steps", out var forBody) ? JsonSerializer.Deserialize<IReadOnlyList<WorkflowStep>>(forBody, options) : []),
            WorkflowStepKind.Foreach => new ForeachStep(
                id,
                root.GetProperty("collection").GetString() ?? throw new JsonException("Foreach step requires a collection."),
                root.GetProperty("itemVariable").GetString() ?? throw new JsonException("Foreach step requires an itemVariable."),
                root.TryGetProperty("steps", out var foreachBody) ? JsonSerializer.Deserialize<IReadOnlyList<WorkflowStep>>(foreachBody, options) : []),
            WorkflowStepKind.While => new WhileStep(
                id,
                JsonSerializer.Deserialize<WorkflowCondition>(root.GetProperty("condition"), options)
                    ?? throw new JsonException("While step requires a condition."),
                root.TryGetProperty("steps", out var whileBody) ? JsonSerializer.Deserialize<IReadOnlyList<WorkflowStep>>(whileBody, options) : [],
                root.TryGetProperty("maxIterations", out var maxIterations) ? maxIterations.GetInt32() : null),
            WorkflowStepKind.Set => new SetStep(
                id,
                root.GetProperty("variable").GetString() ?? throw new JsonException("Set step requires a variable."),
                root.TryGetProperty("value", out var value) ? value.Clone() : null,
                root.TryGetProperty("operation", out var operation)
                    ? Enum.Parse<WorkflowSetOperation>(operation.GetString() ?? string.Empty, true)
                    : WorkflowSetOperation.Set),
            WorkflowStepKind.Exit => new ExitStep(
                id,
                root.TryGetProperty("exitCode", out var exitCode) ? exitCode.GetInt32() : 0),
            _ => throw new JsonException($"Unsupported workflow step type '{kind}'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, WorkflowStep value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);

        switch (value)
        {
            case IfStep ifStep:
                writer.WriteString("type", "if");
                writer.WritePropertyName("condition");
                JsonSerializer.Serialize(writer, ifStep.Condition, options);
                writer.WritePropertyName("then");
                JsonSerializer.Serialize(writer, ifStep.Then, options);
                if (ifStep.Else is not null)
                {
                    writer.WritePropertyName("else");
                    JsonSerializer.Serialize(writer, ifStep.Else, options);
                }
                break;
            case ForStep forStep:
                writer.WriteString("type", "for");
                writer.WriteString("variable", forStep.Variable);
                writer.WriteNumber("from", forStep.From);
                writer.WriteNumber("to", forStep.To);
                writer.WriteNumber("step", forStep.Step);
                writer.WriteBoolean("inclusive", forStep.Inclusive);
                writer.WritePropertyName("steps");
                JsonSerializer.Serialize(writer, forStep.Body, options);
                break;
            case ForeachStep foreachStep:
                writer.WriteString("type", "foreach");
                writer.WriteString("collection", foreachStep.Collection);
                writer.WriteString("itemVariable", foreachStep.ItemVariable);
                writer.WritePropertyName("steps");
                JsonSerializer.Serialize(writer, foreachStep.Body, options);
                break;
            case WhileStep whileStep:
                writer.WriteString("type", "while");
                writer.WritePropertyName("condition");
                JsonSerializer.Serialize(writer, whileStep.Condition, options);
                writer.WritePropertyName("steps");
                JsonSerializer.Serialize(writer, whileStep.Body, options);
                if (whileStep.MaxIterations is not null)
                    writer.WriteNumber("maxIterations", whileStep.MaxIterations.Value);
                break;
            case SetStep setStep:
                writer.WriteString("type", "set");
                writer.WriteString("variable", setStep.Variable);
                writer.WriteString("operation", setStep.Operation.ToString());
                if (setStep.Value is not null)
                {
                    writer.WritePropertyName("value");
                    JsonSerializer.Serialize(writer, setStep.Value, options);
                }
                break;
            case ExitStep exitStep:
                writer.WriteString("type", "exit");
                writer.WriteNumber("exitCode", exitStep.ExitCode);
                break;
            default:
                writer.WriteString("type", "module");
                writer.WriteString("module", value.Module);
                writer.WritePropertyName("parameters");
                JsonSerializer.Serialize(writer, value.Parameters, options);
                break;
        }

        writer.WriteEndObject();
    }

    private static IReadOnlyDictionary<string, object?> ReadParameters(JsonElement root, JsonSerializerOptions options) =>
        root.TryGetProperty("parameters", out var parameters)
            ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, object?>>(parameters, options) ?? new Dictionary<string, object?>()
            : new Dictionary<string, object?>();

    private static WorkflowStepKind ParseKind(string? value) =>
        value?.ToLowerInvariant() switch
        {
            null or "" or "module" => WorkflowStepKind.Module,
            "if" => WorkflowStepKind.If,
            "for" => WorkflowStepKind.For,
            "foreach" => WorkflowStepKind.Foreach,
            "while" => WorkflowStepKind.While,
            "set" => WorkflowStepKind.Set,
            "exit" => WorkflowStepKind.Exit,
            _ => throw new JsonException($"Unsupported workflow step type '{value}'.")
        };
}
