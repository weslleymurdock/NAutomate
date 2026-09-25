using NAutomate.Abstractions;

namespace NAutomate.Core;

public sealed class ModuleRegistry : IModuleRegistry
{
    private readonly Dictionary<string, IAutomationModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    public ModuleRegistry(IEnumerable<IAutomationModule>? modules = null)
    {
        if (modules is not null)
        {
            foreach (var module in modules)
                Register(module);
        }
    }

    public void Register(IAutomationModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(module.Descriptor);

        if (string.IsNullOrWhiteSpace(module.Descriptor.Id))
            throw new ArgumentException("A module must have a non-empty id.", nameof(module));

        if (!_modules.TryAdd(module.Descriptor.Id, module))
            throw new InvalidOperationException(
                $"A module with id '{module.Descriptor.Id}' is already registered.");
    }

    public IReadOnlyCollection<ModuleDescriptor> List() =>
        _modules.Values.Select(module => module.Descriptor).ToArray();

    public IAutomationModule Resolve(string moduleId) =>
        _modules.TryGetValue(moduleId, out var module)
            ? module
            : throw new KeyNotFoundException($"Module '{moduleId}' is not registered.");
}
