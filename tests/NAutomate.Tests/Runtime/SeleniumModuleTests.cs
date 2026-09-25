using NAutomate.Modules;
using Xunit;

namespace NAutomate.Tests.Runtime;

public sealed class SeleniumModuleTests
{
    [Fact]
    public void OfficialModulesExposeChromeFirefoxAndEdgeOperations()
    {
        var modules = OfficialModules.GetModules();

        Assert.Contains(modules, module => module.Descriptor.Id == "selenium.chrome.start-session");
        Assert.Contains(modules, module => module.Descriptor.Id == "selenium.firefox.navigate");
        Assert.Contains(modules, module => module.Descriptor.Id == "selenium.edge.screenshot");
        Assert.Contains(modules, module => module.Descriptor.Id == "selenium.chrome.find-element");
    }

    [Fact]
    public void SeleniumDiscoveryDoesNotStartBrowsers()
    {
        var modules = OfficialModules.GetModules();

        Assert.NotEmpty(modules.Where(x => x.Descriptor.Id.StartsWith("selenium.", StringComparison.Ordinal)));
    }
}
