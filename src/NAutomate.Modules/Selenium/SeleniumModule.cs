using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

public static class SeleniumModule
{
    public static IReadOnlyList<IAutomationModule> GetModules()
    {
        var chrome = new ChromeSeleniumService();
        var firefox = new FirefoxSeleniumService();
        var edge = new EdgeSeleniumService();

        return SeleniumOperationInvoker.CreateModules(chrome, firefox, edge);
    }
}
