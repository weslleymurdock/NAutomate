using NAutomate.Abstractions;

namespace NAutomate.Modules.Core;

/// <summary>Official module descriptor for declarative variable assignment.</summary>
public sealed class SetModule : IAutomationModule
{
    public ModuleDescriptor Descriptor { get; } = new(
        Id: "core.set",
        DisplayName: "Set Variable",
        Description: "Assigns or updates a workflow variable.",
        Version: "1.0.0",
        Parameters:
        [
            new ModuleParameterDefinition("variable", "string", true, "Variable name."),
            new ModuleParameterDefinition("value", "object", false, "Value or variable reference."),
            new ModuleParameterDefinition("operation", nameof(WorkflowSetOperation), false, "Set, Increment, or Decrement.")
        ]);

    public Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (context.Variables is null)
            throw new InvalidOperationException("The core.set module requires a workflow execution variable store.");

        if (!context.Parameters.TryGetValue("variable", out var variable) || variable is not string variableName)
            throw new ArgumentException("A 'variable' parameter is required.");

        var value = context.Parameters.TryGetValue("value", out var valueObject) ? valueObject : null;
        var operation = context.Parameters.TryGetValue("operation", out var operationObject)
            ? Enum.Parse<WorkflowSetOperation>(operationObject?.ToString() ?? nameof(WorkflowSetOperation.Set), true)
            : WorkflowSetOperation.Set;

        var current = context.Variables.Get(variableName);
        var updated = operation switch
        {
            WorkflowSetOperation.Set => value,
            WorkflowSetOperation.Increment => NumericOperation(current, value, 1),
            WorkflowSetOperation.Decrement => NumericOperation(current, value, -1),
            _ => throw new InvalidDataException($"Unsupported set operation '{operation}'.")
        };

        context.Variables.Set(variableName, updated);
        return Task.FromResult(new ModuleExecutionResult(Convert.ToString(updated, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty));
    }

    private static object NumericOperation(object? current, object? value, int direction)
    {
        if (current is null)
            throw new InvalidDataException("Numeric variable cannot be null.");

        var amount = value is null ? 1m : Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
        var result = Convert.ToDecimal(current, System.Globalization.CultureInfo.InvariantCulture) + amount * direction;

        return current switch
        {
            int => checked((int)result),
            long => checked((long)result),
            decimal => result,
            double => (double)result,
            float => (float)result,
            _ => throw new InvalidDataException($"Type '{current.GetType()}' does not support numeric updates.")
        };
    }
}
