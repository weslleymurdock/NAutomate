using NAutomate.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace NAutomate.Modules.Selenium;

public sealed class EdgeSeleniumService : SeleniumServiceBase, IEdgeSeleniumService
{
    protected override IWebDriver CreateDriver(bool headless, string? binaryPath, IReadOnlyList<string> arguments)
    {
        var options = new EdgeOptions();

        if (!string.IsNullOrWhiteSpace(binaryPath))
            options.BinaryLocation = binaryPath;

        if (headless)
            options.AddArgument("--headless=new");

        options.AddArguments(arguments);
        return new EdgeDriver(options);
    }
}
