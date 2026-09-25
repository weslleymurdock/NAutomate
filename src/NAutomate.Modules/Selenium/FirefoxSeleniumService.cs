using NAutomate.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace NAutomate.Modules.Selenium;

public sealed class FirefoxSeleniumService : SeleniumServiceBase, IFirefoxSeleniumService
{
    protected override IWebDriver CreateDriver(bool headless, string? binaryPath, IReadOnlyList<string> arguments)
    {
        var options = new FirefoxOptions();

        if (!string.IsNullOrWhiteSpace(binaryPath))
            options.BinaryLocation = binaryPath;

        if (headless)
            options.AddArgument("-headless");

        options.AddArguments(arguments);
        return new FirefoxDriver(options);
    }
}
