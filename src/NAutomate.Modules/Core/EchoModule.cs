using NAutomate.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace NAutomate.Modules.Core;

public sealed class EchoModule : IAutomationModule
{
    public ModuleDescriptor Descriptor { get; } = new(
        Id: "core.echo",
        DisplayName: "Echo",
        Description: "Returns the provided message as output.",
        Version: "1.0.0",
        Parameters: new[]
        {
            new ModuleParameterDefinition("message", "string", Required: true, Description: "The message to echo.")
        });

    public Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!context.Step.Parameters.TryGetValue("message", out var messageObj) || messageObj is not string message || string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("A non-empty 'message' parameter is required.");
        }

        return Task.FromResult(new ModuleExecutionResult(message, Succeeded: true));
    }
}
