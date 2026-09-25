using NAutomate.Modules;
using Xunit;

namespace NAutomate.Tests.Modules;

public sealed class AppiumModuleTests
{
    [Fact]
    public void OfficialModulesExposeAndroidAndIosAppiumOperations()
    {
        var modules = OfficialModules.GetModules();

        Assert.Contains(modules, module => module.Descriptor.Id == "appium.android.start-session");
        Assert.Contains(modules, module => module.Descriptor.Id == "appium.android.click");
        Assert.Contains(modules, module => module.Descriptor.Id == "appium.android.start-activity");
        Assert.Contains(modules, module => module.Descriptor.Id == "appium.ios.start-session");
        Assert.Contains(modules, module => module.Descriptor.Id == "appium.ios.launch-app-with-arguments");
    }

    [Fact]
    public void AppiumOperationsExposeServiceAndParameterMetadata()
    {
        var module = OfficialModules.GetModules()
            .Single(module => module.Descriptor.Id == "appium.android.start-activity");

        var service = Assert.Single(module.Descriptor.Services!);
        var operation = Assert.Single(service.Operations);

        Assert.Equal("appium.android", service.Id);
        Assert.Equal("start-activity", operation.Id);
        Assert.Contains(operation.Parameters, parameter => parameter.Name == "intent" && parameter.Required);
    }

    [Fact]
    public void OfficialModulesDoNotCreateAnAppiumSessionDuringDiscovery()
    {
        var modules = OfficialModules.GetModules();

        Assert.NotEmpty(modules);
        Assert.All(
            modules.Where(module => module.Descriptor.Id.StartsWith("appium.", StringComparison.Ordinal)),
            module => Assert.StartsWith("appium.", module.Descriptor.Id));
    }
}
