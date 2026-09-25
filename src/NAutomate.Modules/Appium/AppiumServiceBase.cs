using System.Collections.Concurrent;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace NAutomate.Modules.Appium;

public abstract class AppiumServiceBase<TDriver> where TDriver : AppiumDriver
{
    private readonly ConcurrentDictionary<string, AppiumElement> _elements = new(StringComparer.Ordinal);
    private int _nextElementId;
    private TDriver? _driver;

    public AppiumDriver Driver =>
        _driver ?? throw new InvalidOperationException("No Appium session is active.");

    protected TDriver TypedDriver =>
        _driver ?? throw new InvalidOperationException("No Appium session is active.");

    protected abstract TDriver CreateDriver(Uri serverUrl, AppiumOptions options);

    public async Task<string> StartSessionAsync(
        string serverUrl,
        string deviceName,
        string? platformVersion = null,
        string? automationName = null,
        string? app = null,
        string? udid = null,
        string? appPackage = null,
        string? appActivity = null,
        string? bundleId = null,
        bool noReset = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_driver is not null)
            throw new InvalidOperationException("An Appium session is already active.");

        var options = new AppiumOptions
        {
            DeviceName = deviceName,
            AutomationName = automationName ?? string.Empty,
            App = app ?? string.Empty
        };

        Add(options, "platformVersion", platformVersion);
        Add(options, "udid", udid);
        Add(options, "appPackage", appPackage);
        Add(options, "appActivity", appActivity);
        Add(options, "bundleId", bundleId);
        Add(options, "noReset", noReset);

        _driver = CreateDriver(new Uri(serverUrl), options);

        return await Task.FromResult(_driver.SessionId);
    }

    private static void Add(AppiumOptions options, string name, object? value)
    {
        if (value is not null)
            options.AddAdditionalAppiumOption(name, value);
    }

    protected string StoreElement(AppiumElement element)
    {
        var id = $"element-{Interlocked.Increment(ref _nextElementId)}";
        _elements[id] = element;
        return id;
    }

    protected AppiumElement ResolveElement(string elementId) =>
        _elements.TryGetValue(elementId, out var element)
            ? element
            : throw new KeyNotFoundException($"Appium element resource '{elementId}' was not found.");

    protected static By CreateSelector(string strategy, string value) =>
        strategy.Trim().ToLowerInvariant() switch
        {
            "id" => By.Id(value),
            "xpath" => By.XPath(value),
            "classname" or "class" => By.ClassName(value),
            "accessibilityid" => MobileBy.AccessibilityId(value),
            "androiduiautomator" => MobileBy.AndroidUIAutomator(value),
            "iosnspredicate" => MobileBy.IosNSPredicate(value),
            _ => throw new ArgumentException(
                $"Unsupported Appium selector strategy '{strategy}'.",
                nameof(strategy))
        };

    public void Quit() => Driver.Quit();

    public string FindElement(string strategy, string value) =>
        StoreElement(TypedDriver.FindElement(CreateSelector(strategy, value)));

    public IReadOnlyList<string> FindElements(string strategy, string value) =>
        TypedTypedDriver.FindElements(CreateSelector(strategy, value))
            .Select(StoreElement)
            .ToArray();

    public void Click(string elementId) => ResolveElement(elementId).Click();

    public void Clear(string elementId) => ResolveElement(elementId).Clear();

    public void SendKeys(string elementId, string text) => ResolveElement(elementId).SendKeys(text);

    public string GetText(string elementId) => ResolveElement(elementId).Text;

    public string? GetAttribute(string elementId, string attributeName) =>
        ResolveElement(elementId).GetAttribute(attributeName);

    public bool IsDisplayed(string elementId) => ResolveElement(elementId).Displayed;

    public bool IsEnabled(string elementId) => ResolveElement(elementId).Enabled;

    public void InstallApp(string appPath) => TypedDriver.InstallApp(appPath);

    public void RemoveApp(string appId) => TypedDriver.RemoveApp(appId);

    public void ActivateApp(string appId) => TypedDriver.ActivateApp(appId);

    public bool TerminateApp(string appId) => TypedDriver.TerminateApp(appId);

    public bool IsAppInstalled(string appId) => TypedDriver.IsAppInstalled(appId);

    public string PullFile(string pathOnDevice) => Convert.ToBase64String(TypedDriver.PullFile(pathOnDevice));

    public void PushFile(string pathOnDevice, string base64Data) =>
        TypedDriver.PushFile(pathOnDevice, Convert.FromBase64String(base64Data));

    public void BackgroundApp(int seconds = -1) =>
        TypedDriver.BackgroundApp(TimeSpan.FromSeconds(seconds));

    public void HideKeyboard() => TypedDriver.HideKeyboard();

    public bool IsKeyboardShown() => TypedDriver.IsKeyboardShown();

    public string StartRecordingScreen() => TypedDriver.StartRecordingScreen();

    public string StopRecordingScreen() => TypedDriver.StopRecordingScreen();

    public IReadOnlyDictionary<string, object> GetEvents(string? type = null) =>
        TypedDriver.GetEvents(type);

    public void LogEvent(string vendorName, string eventName) =>
        TypedDriver.LogEvent(vendorName, eventName);

    public object? ExecuteCustomDriverCommand(
        string commandName,
        IReadOnlyDictionary<string, object?>? parameters = null) =>
        TypedDriver.ExecuteCustomDriverCommand(
            commandName,
            parameters is null ? null : new Dictionary<string, object>(parameters
                .Where(x => x.Value is not null)
                .ToDictionary(x => x.Key, x => x.Value!)));

    public object? GetSessionDetail(string detail) => TypedDriver.GetSessionDetail(detail);

    public string GetPageSource() => TypedDriver.PageSource;

    public string GetCurrentUrl() => TypedDriver.Url;

    public string GetTitle() => TypedDriver.Title;

    public object? ExecuteScript(
        string script,
        IReadOnlyDictionary<string, object?>? arguments = null) =>
        arguments is null
            ? TypedDriver.ExecuteScript(script)
            : TypedDriver.ExecuteScript(
                script,
                arguments.ToDictionary(x => x.Key, x => x.Value));

    public IReadOnlyList<string> GetContexts() => TypedTypedDriver.Contexts.ToArray();

    public void SetContext(string contextName) => TypedDriver.Context = contextName;

    public string GetScreenshot() => Convert.ToBase64String(TypedDriver.GetScreenshot().AsByteArray);


}

