namespace NAutomate.Abstractions;

public sealed record ModuleSettingsTabDescriptor(
    string ModuleId,
    string Id,
    string DisplayName,
    string Description,
    IReadOnlyList<ModuleSettingDescriptor> Settings);
