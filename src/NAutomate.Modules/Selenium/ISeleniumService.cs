using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

public interface ISeleniumService
{
    [AutomationOperation("start-session", "Start Session", "Starts a local browser session using Selenium Manager.")]
    void StartSession(bool headless = false, string? binaryPath = null, string[]? arguments = null);

    [AutomationOperation("navigate", "Navigate", "Navigates the current browser session to a URL.")]
    void Navigate(string url);

    [AutomationOperation("back", "Back", "Navigates back in the browser history.")]
    void Back();

    [AutomationOperation("forward", "Forward", "Navigates forward in the browser history.")]
    void Forward();

    [AutomationOperation("refresh", "Refresh", "Refreshes the current page.")]
    void Refresh();

    [AutomationOperation("get-title", "Get Title", "Gets the current page title.")]
    string GetTitle();

    [AutomationOperation("get-url", "Get URL", "Gets the current page URL.")]
    string GetUrl();

    [AutomationOperation("find-element", "Find Element", "Finds an element and stores it under an opaque runtime identifier.")]
    string FindElement(string selector, string strategy = "css");

    [AutomationOperation("click", "Click", "Clicks a previously located element.")]
    void Click(string elementId);

    [AutomationOperation("send-keys", "Send Keys", "Sends text to a previously located element.")]
    void SendKeys(string elementId, string text);

    [AutomationOperation("get-text", "Get Text", "Gets visible text from a previously located element.")]
    string GetText(string elementId);

    [AutomationOperation("get-attribute", "Get Attribute", "Gets an attribute from a previously located element.")]
    string? GetAttribute(string elementId, string attribute);

    [AutomationOperation("screenshot", "Screenshot", "Captures the current browser viewport as a base64 PNG.")]
    string Screenshot();

    [AutomationOperation("stop-session", "Stop Session", "Closes the current browser session.")]
    void StopSession();
}
