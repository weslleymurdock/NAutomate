using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

public static class SeleniumModule
{
    public static IReadOnlyList<IAutomationModule> GetModules()
    {
        var chrome = new ChromeSeleniumService();
        var firefox = new FirefoxSeleniumService();
        var edge = new EdgeSeleniumService();
        var safari = new SafariSeleniumService();

        return SeleniumOperationInvoker.CreateModules(chrome, firefox, edge, safari);
    }
}
