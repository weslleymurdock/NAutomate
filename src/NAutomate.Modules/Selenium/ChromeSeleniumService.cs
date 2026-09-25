using NAutomate.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace NAutomate.Modules.Selenium;

public sealed class ChromeSeleniumService : SeleniumServiceBase, IChromeSeleniumService
{
    protected override IWebDriver CreateDriver(bool headless, string? binaryPath, IReadOnlyList<string> arguments)
    {
        var options = new ChromeOptions();

        if (!string.IsNullOrWhiteSpace(binaryPath))
            options.BinaryLocation = binaryPath;

        if (headless)
            options.AddArgument("--headless=new");

        options.AddArguments(arguments);
        return new ChromeDriver(options);
    }
}
