using NAutomate.Abstractions;
using NAutomate.Modules.Appium;
using NAutomate.Modules.Core;

namespace NAutomate.Modules;

public static class OfficialModules
{
    public static IEnumerable<IAutomationModule> GetModules()
    {
        yield return new EchoModule();

        foreach (var module in AppiumModule.GetModules())
            yield return module;
    }
}
