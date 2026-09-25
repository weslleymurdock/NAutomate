using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

[AutomationService("selenium.safari", "Safari Selenium", "Safari browser automation through Selenium. Supported on macOS.")]
public interface ISafariSeleniumService : ISeleniumService
{
}
