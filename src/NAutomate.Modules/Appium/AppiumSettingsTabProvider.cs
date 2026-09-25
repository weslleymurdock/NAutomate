using NAutomate.Abstractions;

namespace NAutomate.Modules.Appium;

public sealed class AppiumSettingsTabProvider : IModuleSettingsTabProvider
{
    public IReadOnlyList<ModuleSettingsTabDescriptor> GetTabs() =>
    [
        new ModuleSettingsTabDescriptor(
            "appium", "appium", "Appium",
            "Configuration supplied by the Appium module NuGet.",
            [
                new("serverUrl", "Server URL", "Default Appium server endpoint.", "url", "http://127.0.0.1:4723"),
                new("androidDevice", "Android device", "Default Android device/emulator name.", "text", ""),
                new("iosDevice", "iOS device", "Default iOS simulator/device name.", "text", "")
            ])
    ];
}
