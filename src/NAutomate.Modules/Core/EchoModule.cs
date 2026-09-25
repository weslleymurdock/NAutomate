using NAutomate.Abstractions;

namespace NAutomate.Modules.Core;

/// <summary>Outputs a workflow message after runtime variable resolution.</summary>
public sealed class EchoModule : IAutomationModule
{
    public ModuleDescriptor Descriptor { get; } = new(
        Id: "core.echo",
        DisplayName: "Echo",
        Description: "Returns the provided message as output.",
        Version: "1.0.0",
        Parameters:
        [
            new ModuleParameterDefinition("message", "string", Required: true, Description: "The message to echo.")
        ]);

    public Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!context.Parameters.TryGetValue("message", out var messageObj))
            throw new ArgumentException("A non-empty 'message' parameter is required.");

        var message = messageObj?.ToString();
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A non-empty 'message' parameter is required.");

        return Task.FromResult(new ModuleExecutionResult(message));
    }
}
