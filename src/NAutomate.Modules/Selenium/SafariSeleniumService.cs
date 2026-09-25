using NAutomate.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Safari;

namespace NAutomate.Modules.Selenium;

public sealed class SafariSeleniumService : SeleniumServiceBase, ISafariSeleniumService
{
    protected override IWebDriver CreateDriver(bool headless, string? binaryPath, IReadOnlyList<string> arguments)
    {
        if (!OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Safari Selenium automation is supported only on macOS.");

        var options = new SafariOptions();

        if (headless)
            throw new PlatformNotSupportedException("Safari does not support the generic headless option.");

        if (arguments.Count != 0)
            throw new NotSupportedException("Safari does not support arbitrary browser command-line arguments through this service.");

        if (!string.IsNullOrWhiteSpace(binaryPath))
            throw new NotSupportedException("Safari browser binary paths are managed by SafariDriver and cannot be overridden by SeleniumService.");

        return new SafariDriver(options);
    }
}
