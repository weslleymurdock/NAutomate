namespace NAutomate.Abstractions;

public interface IModuleSettingsTabProvider
{
    IReadOnlyList<ModuleSettingsTabDescriptor> GetTabs();
}
