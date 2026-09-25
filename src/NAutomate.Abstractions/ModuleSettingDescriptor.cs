namespace NAutomate.Abstractions;

public sealed record ModuleSettingDescriptor(
    string Key,
    string DisplayName,
    string Description,
    string Type,
    string? DefaultValue = null);
