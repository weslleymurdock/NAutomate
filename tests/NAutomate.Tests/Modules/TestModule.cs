using NAutomate.Abstractions;

namespace NAutomate.Tests.Modules;

internal sealed class TestModule(string id, List<string>? executed = null, bool failure = false) : IAutomationModule
{
    public ModuleDescriptor Descriptor { get; } = new(id, id, "Test module", "1.0", []);

    public Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        executed?.Add(Descriptor.Id);
        if (failure)
            return Task.FromResult(new ModuleExecutionResult(Descriptor.Id, Succeeded: false));
        if (context.Step.Parameters.TryGetValue("throw", out var throwObj) && throwObj is bool shouldThrow && shouldThrow)
            throw new InvalidOperationException("test module failed");
        return Task.FromResult(new ModuleExecutionResult(Descriptor.Id));
    }
}
