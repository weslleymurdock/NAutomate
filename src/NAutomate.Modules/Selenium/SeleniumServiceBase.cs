using System.Collections.Concurrent;
using NAutomate.Abstractions;
using OpenQA.Selenium;

namespace NAutomate.Modules.Selenium;

public abstract class SeleniumServiceBase : ISeleniumService, IDisposable
{
    private readonly ConcurrentDictionary<string, IWebElement> _elements = new(StringComparer.Ordinal);
    private IWebDriver? _driver;

    protected IWebDriver Driver =>
        _driver ?? throw new InvalidOperationException("A Selenium session has not been started.");

    protected abstract IWebDriver CreateDriver(bool headless, string? binaryPath, IReadOnlyList<string> arguments);

    public void StartSession(bool headless = false, string? binaryPath = null, string[]? arguments = null)
    {
        StopSession();

        var effectiveArguments = arguments?.Where(static x => !string.IsNullOrWhiteSpace(x)).ToArray()
            ?? [];

        _driver = CreateDriver(headless, binaryPath, effectiveArguments);
        _elements.Clear();
    }

    public void Navigate(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        Driver.Navigate().GoToUrl(url);
    }

    public void Back() => Driver.Navigate().Back();

    public void Forward() => Driver.Navigate().Forward();

    public void Refresh() => Driver.Navigate().Refresh();

    public string GetTitle() => Driver.Title;

    public string GetUrl() => Driver.Url;

    public string FindElement(string selector, string strategy = "css")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);

        var element = Driver.FindElement(CreateBy(strategy, selector));
        var id = Guid.NewGuid().ToString("N");
        _elements[id] = element;
        return id;
    }

    public void Click(string elementId) => GetElement(elementId).Click();

    public void SendKeys(string elementId, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);
        GetElement(elementId).SendKeys(text ?? string.Empty);
    }

    public string GetText(string elementId) => GetElement(elementId).Text;

    public string? GetAttribute(string elementId, string attribute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attribute);
        return GetElement(elementId).GetAttribute(attribute);
    }

    public string Screenshot() => ((ITakesScreenshot)Driver).GetScreenshot().AsBase64EncodedString;

    public void StopSession()
    {
        var driver = Interlocked.Exchange(ref _driver, null);
        _elements.Clear();

        if (driver is null)
            return;

        try
        {
            driver.Quit();
        }
        finally
        {
            driver.Dispose();
        }
    }

    public void Dispose() => StopSession();

    private IWebElement GetElement(string elementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);

        return _elements.TryGetValue(elementId, out var element)
            ? element
            : throw new KeyNotFoundException($"Selenium element '{elementId}' was not found.");
    }

    private static By CreateBy(string strategy, string selector)
    {
        return strategy.Trim().ToLowerInvariant() switch
        {
            "id" => By.Id(selector),
            "name" => By.Name(selector),
            "xpath" => By.XPath(selector),
            "css" or "css-selector" => By.CssSelector(selector),
            "class" or "class-name" => By.ClassName(selector),
            "tag" or "tag-name" => By.TagName(selector),
            "link-text" => By.LinkText(selector),
            "partial-link-text" => By.PartialLinkText(selector),
            _ => throw new ArgumentException(
                $"Unsupported Selenium locator strategy '{strategy}'. Supported strategies: css, id, name, xpath, class, tag, link-text, partial-link-text.")
        };
    }
}
