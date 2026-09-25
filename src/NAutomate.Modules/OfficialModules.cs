using NAutomate.Abstractions;
using NAutomate.Modules.Appium;
using NAutomate.Modules.Core;
using NAutomate.Modules.Shell;
using NAutomate.Modules.Selenium;

namespace NAutomate.Modules;

public static class OfficialModules
{
    public static IEnumerable<IAutomationModule> GetModules()
    {
        yield return new EchoModule();
        yield return new SetModule();

        foreach (var module in AppiumModule.GetModules())
            yield return module;

        foreach (var module in ShellModule.GetModules())
            yield return module;

        foreach (var module in SeleniumModule.GetModules())
            yield return module;
    }
}
