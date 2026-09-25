using NAutomate.Abstractions;

namespace NAutomate.Parser;

/// <summary>Provides stable grouping information for module descriptors without UI-specific parsing.</summary>
public static class ModuleDescriptorParser
{
    public static string GetFamilyId(ModuleDescriptor module)
    {
        ArgumentNullException.ThrowIfNull(module);
        return GetFamilyId(module.Id);
    }

    public static string GetFamilyId(string moduleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);

        var separator = moduleId.IndexOf('.');
        return separator > 0 ? moduleId[..separator] : moduleId;
    }

    public static string GetServiceGroupId(ModuleDescriptor module)
    {
        ArgumentNullException.ThrowIfNull(module);
        return module.Services?.FirstOrDefault()?.Id ?? module.Id;
    }
}
