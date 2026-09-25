using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

[AutomationService("selenium.chrome", "Chrome Selenium", "Chrome browser automation through Selenium.")]
public interface IChromeSeleniumService : ISeleniumService
{
}
