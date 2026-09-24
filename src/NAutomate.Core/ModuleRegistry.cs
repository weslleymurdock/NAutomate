using NAutomate.Abstractions;

namespace NAutomate.Core;

/// <summary>In-process registry for precompiled automation modules.</summary>
public sealed class ModuleRegistry : IModuleRegistry
{
    private readonly Dictionary<string, IAutomationModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    public ModuleRegistry(IEnumerable<IAutomationModule>? modules = null)
    {
        foreach (var module in modules ?? [new EchoModule()])
            Register(module);
    }

    public void Register(IAutomationModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        _modules[module.Descriptor.Id] = module;
    }

    public IReadOnlyCollection<ModuleDescriptor> List() => _modules.Values.Select(module => module.Descriptor).ToArray();

    public IAutomationModule Resolve(string moduleId) =>
        _modules.TryGetValue(moduleId, out var module)
            ? module
            : throw new KeyNotFoundException($"Module '{moduleId}' is not registered.");
}

internal sealed class EchoModule : IAutomationModule
{
    public ModuleDescriptor Descriptor { get; } = new(
        "core.echo", "Echo", "Writes a message to the execution output.", "1.0",
        [new ModuleParameterDefinition("message", "string", true, "Message to write.")]);

    public Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        if (!context.Step.Parameters.TryGetValue("message", out var value) || value is null || string.IsNullOrWhiteSpace(value.ToString()))
            throw new ArgumentException("The 'message' parameter is required.");
        return Task.FromResult(new ModuleExecutionResult(value.ToString()!));
    }
}
