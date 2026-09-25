using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Tests.Parser;

public sealed class ModuleDescriptorParserTests
{
    [Fact]
    public void ExtractsModuleFamilyAndId()
    {
        Assert.Equal("selenium", ModuleDescriptorParser.GetFamilyId("selenium.chrome.start-session"));
        Assert.Equal("selenium.chrome.start-session", ModuleDescriptorParser.GetModuleId("selenium.chrome.start-session · Start Session"));
    }
}
