using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

[AutomationService("selenium.firefox", "Firefox Selenium", "Firefox browser automation through Selenium.")]
public interface IFirefoxSeleniumService : ISeleniumService
{
}
