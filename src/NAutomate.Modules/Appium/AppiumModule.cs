using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using NAutomate.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;

namespace NAutomate.Modules.Appium;

[AutomationService(
    "appium.common",
    "Appium",
    "Common Appium operations shared by Android and iOS services.")]
public interface IAppiumService
{
    AppiumDriver Driver { get; }

    [AutomationOperation("start-session", "Start Session", "Starts a remote Appium session.")]
    Task<string> StartSessionAsync(
        [AutomationParameter("Server URL")] string serverUrl,
        [AutomationParameter("Device Name")] string deviceName,
        [AutomationParameter("Platform Version")] string? platformVersion = null,
        [AutomationParameter("Automation Name")] string? automationName = null,
        [AutomationParameter("Application")] string? app = null,
        [AutomationParameter("UDID")] string? udid = null,
        [AutomationParameter("Application Package")] string? appPackage = null,
        [AutomationParameter("Application Activity")] string? appActivity = null,
        [AutomationParameter("Bundle Identifier")] string? bundleId = null,
        [AutomationParameter("No Reset")] bool noReset = false,
        CancellationToken cancellationToken = default);

    [AutomationOperation("quit", "Quit", "Terminates the current Appium session.")]
    void Quit();

    [AutomationOperation("find-element", "Find Element", "Finds an element and returns a workflow resource identifier.")]
    string FindElement(string strategy, string value);

    [AutomationOperation("find-elements", "Find Elements", "Finds elements and returns workflow resource identifiers.")]
    IReadOnlyList<string> FindElements(string strategy, string value);

    [AutomationOperation("click", "Click", "Clicks a previously resolved element.")]
    void Click(string elementId);

    [AutomationOperation("clear", "Clear", "Clears a previously resolved element.")]
    void Clear(string elementId);

    [AutomationOperation("send-keys", "Send Keys", "Sends text to a previously resolved element.")]
    void SendKeys(string elementId, string text);

    [AutomationOperation("get-text", "Get Text", "Reads text from a previously resolved element.")]
    string GetText(string elementId);

    [AutomationOperation("get-attribute", "Get Attribute", "Reads an attribute from a previously resolved element.")]
    string? GetAttribute(string elementId, string attributeName);

    [AutomationOperation("is-displayed", "Is Displayed", "Checks whether an element is displayed.")]
    bool IsDisplayed(string elementId);

    [AutomationOperation("is-enabled", "Is Enabled", "Checks whether an element is enabled.")]
    bool IsEnabled(string elementId);

    [AutomationOperation("install-app", "Install App", "Installs an application.")]
    void InstallApp(string appPath);

    [AutomationOperation("remove-app", "Remove App", "Removes an application.")]
    void RemoveApp(string appId);

    [AutomationOperation("activate-app", "Activate App", "Activates an application.")]
    void ActivateApp(string appId);

    [AutomationOperation("terminate-app", "Terminate App", "Terminates an application.")]
    bool TerminateApp(string appId);

    [AutomationOperation("is-app-installed", "Is App Installed", "Checks whether an application is installed.")]
    bool IsAppInstalled(string appId);

    [AutomationOperation("pull-file", "Pull File", "Pulls a device file and returns base64 data.")]
    string PullFile(string pathOnDevice);

    [AutomationOperation("push-file", "Push File", "Pushes base64 data to a device file.")]
    void PushFile(string pathOnDevice, string base64Data);

    [AutomationOperation("background-app", "Background App", "Sends the application to the background.")]
    void BackgroundApp(int seconds = -1);

    [AutomationOperation("hide-keyboard", "Hide Keyboard", "Hides the on-screen keyboard.")]
    void HideKeyboard();

    [AutomationOperation("is-keyboard-shown", "Is Keyboard Shown", "Checks whether the keyboard is visible.")]
    bool IsKeyboardShown();

    [AutomationOperation("start-recording-screen", "Start Recording", "Starts screen recording and returns the recording identifier.")]
    string StartRecordingScreen();

    [AutomationOperation("stop-recording-screen", "Stop Recording", "Stops screen recording and returns base64 data.")]
    string StopRecordingScreen();

    [AutomationOperation("get-events", "Get Events", "Gets Appium event data.")]
    IReadOnlyDictionary<string, object> GetEvents(string? type = null);

    [AutomationOperation("log-event", "Log Event", "Writes an event to the Appium server.")]
    void LogEvent(string vendorName, string eventName);

    [AutomationOperation("execute-custom-driver-command", "Execute Custom Driver Command", "Executes a registered custom Appium command.")]
    object? ExecuteCustomDriverCommand(string commandName, IReadOnlyDictionary<string, object?>? parameters = null);

    [AutomationOperation("get-session-detail", "Get Session Detail", "Gets a value from the current session details.")]
    object? GetSessionDetail(string detail);

    [AutomationOperation("get-page-source", "Get Page Source", "Gets the current page source.")]
    string GetPageSource();

    [AutomationOperation("get-current-url", "Get Current URL", "Gets the current URL.")]
    string GetCurrentUrl();

    [AutomationOperation("get-title", "Get Title", "Gets the current page title.")]
    string GetTitle();

    [AutomationOperation("execute-script", "Execute Script", "Executes a JavaScript/Appium script.")]
    object? ExecuteScript(string script, IReadOnlyDictionary<string, object?>? arguments = null);

    [AutomationOperation("get-contexts", "Get Contexts", "Gets available automation contexts.")]
    IReadOnlyList<string> GetContexts();

    [AutomationOperation("set-context", "Set Context", "Changes the current automation context.")]
    void SetContext(string contextName);

    [AutomationOperation("get-screenshot", "Get Screenshot", "Captures the current screen and returns base64 PNG data.")]
    string GetScreenshot();
}

[AutomationService(
    "appium.android",
    "Android Appium",
    "Android-specific Appium automation service.")]
public interface IAndroidAppiumService : IAppiumService
{
    [AutomationOperation("start-activity", "Start Activity", "Starts an Android activity.")]
    void StartActivity(
        string intent,
        Dictionary<string, object>? arguments = null,
        string? user = null,
        bool? wait = null,
        bool? stop = null,
        int? windowingMode = null,
        int? activityType = null,
        string? action = null,
        string? uri = null,
        string? mimeType = null,
        string? identifier = null,
        string[]? categories = null,
        string? component = null,
        string? package = null,
        string[][]? extras = null,
        string? flags = null);

    [AutomationOperation("press-key-code", "Press Key Code", "Presses an Android key code.")]
    void PressKeyCode(int keyCode, int metastate = -1);

    [AutomationOperation("long-press-key-code", "Long Press Key Code", "Long-presses an Android key code.")]
    void LongPressKeyCode(int keyCode, int metastate = -1);

    [AutomationOperation("toggle-location-services", "Toggle Location Services", "Toggles Android location services.")]
    void ToggleLocationServices();

    [AutomationOperation("make-gsm-call", "Make GSM Call", "Starts a GSM call.")]
    void MakeGsmCall(string phoneNumber, GsmCallActions gsmCallAction);

    [AutomationOperation("send-sms", "Send SMS", "Sends an SMS.")]
    void SendSms(string phoneNumber, string message);

    [AutomationOperation("set-gsm-signal-strength", "Set GSM Signal Strength", "Sets GSM signal strength.")]
    void SetGsmSignalStrength(GsmSignalStrength gsmSignalStrength);

    [AutomationOperation("set-gsm-voice", "Set GSM Voice", "Sets GSM voice state.")]
    void SetGsmVoice(GsmVoiceState gsmVoiceState);

    [AutomationOperation("open-notifications", "Open Notifications", "Opens the Android notification shade.")]
    void OpenNotifications();

    [AutomationOperation("get-system-bars", "Get System Bars", "Gets Android system bar information.")]
    IDictionary<string, object> GetSystemBars();

    [AutomationOperation("get-display-density", "Get Display Density", "Gets display density.")]
    float GetDisplayDensity();

    [AutomationOperation("get-performance-data", "Get Performance Data", "Gets Android performance data.")]
    IReadOnlyList<object> GetPerformanceData(string packageName, string performanceDataType, int dataReadAttempts = 1);

    [AutomationOperation("get-performance-data-types", "Get Performance Data Types", "Gets supported performance data types.")]
    IReadOnlyList<string> GetPerformanceDataTypes();

    [AutomationOperation("lock", "Lock Device", "Locks the Android device.")]
    void Lock(int? seconds = null);

    [AutomationOperation("is-locked", "Is Locked", "Checks whether the Android device is locked.")]
    bool IsLocked();

    [AutomationOperation("unlock", "Unlock Device", "Unlocks the Android device using the Appium unlock command.")]
    void Unlock(string key, string type, string? strategy = null, int? timeoutMs = null);

    [AutomationOperation("set-setting", "Set Setting", "Sets an Android driver setting.")]
    void SetSetting(string setting, object value);

    [AutomationOperation("ignore-unimportant-views", "Ignore Unimportant Views", "Configures UiAutomator to ignore unimportant views.")]
    void IgnoreUnimportantViews(bool compress);

    [AutomationOperation("configurator-set-wait-for-idle-timeout", "Set Wait For Idle Timeout", "Sets UiAutomator idle timeout.")]
    void ConfiguratorSetWaitForIdleTimeout(int timeout);

    [AutomationOperation("configurator-set-wait-for-selector-timeout", "Set Wait For Selector Timeout", "Sets UiAutomator selector timeout.")]
    void ConfiguratorSetWaitForSelectorTimeout(int timeout);

    [AutomationOperation("configurator-set-scroll-acknowledgment-timeout", "Set Scroll Acknowledgment Timeout", "Sets UiAutomator scroll acknowledgment timeout.")]
    void ConfiguratorSetScrollAcknowledgmentTimeout(int timeout);

    [AutomationOperation("configurator-set-key-injection-delay", "Set Key Injection Delay", "Sets UiAutomator key injection delay.")]
    void ConfiguratorSetKeyInjectionDelay(int delay);

    [AutomationOperation("configurator-set-action-acknowledgment-timeout", "Set Action Acknowledgment Timeout", "Sets UiAutomator action acknowledgment timeout.")]
    void ConfiguratorSetActionAcknowledgmentTimeout(int timeout);

    [AutomationOperation("get-current-activity", "Get Current Activity", "Gets the current Android activity.")]
    string CurrentActivity { get; }

    [AutomationOperation("get-current-package", "Get Current Package", "Gets the current Android package.")]
    string CurrentPackage { get; }
}

[AutomationService(
    "appium.ios",
    "iOS Appium",
    "iOS-specific Appium automation service.")]
public interface IIOSAppiumService : IAppiumService
{
    [AutomationOperation("set-setting", "Set Setting", "Sets an iOS driver setting.")]
    void SetSetting(string setting, object value);

    [AutomationOperation("shake-device", "Shake Device", "Shakes the iOS simulator/device.")]
    void ShakeDevice();

    [AutomationOperation("hide-keyboard", "Hide Keyboard", "Hides the iOS keyboard using a key.")]
    void HideKeyboard(string key);

    [AutomationOperation("perform-touch-id", "Perform Touch ID", "Performs Touch ID authentication.")]
    void PerformTouchID(bool match);

    [AutomationOperation("install-app", "Install App", "Installs an iOS application.")]
    void InstallApp(string appPath, int? timeoutMs = null);

    [AutomationOperation("launch-app-with-arguments", "Launch App With Arguments", "Launches an iOS application with process arguments.")]
    void LaunchAppWithArguments(
        string bundleId,
        IReadOnlyCollection<string>? processArguments = null,
        IDictionary<string, string>? environmentVariables = null);

    [AutomationOperation("is-locked", "Is Locked", "Checks whether the iOS device is locked.")]
    bool IsLocked();

    [AutomationOperation("lock", "Lock Device", "Locks the iOS device.")]
    void Lock(int? seconds = null);

    [AutomationOperation("unlock", "Unlock Device", "Unlocks the iOS device.")]
    void Unlock();

    [AutomationOperation("set-clipboard-url", "Set Clipboard URL", "Sets a URL in the iOS clipboard.")]
    void SetClipboardUrl(string url);

    [AutomationOperation("get-clipboard-url", "Get Clipboard URL", "Gets a URL from the iOS clipboard.")]
    string GetClipboardUrl();

    [AutomationOperation("get-app-state", "Get App State", "Gets iOS application state.")]
    AppState GetAppState(string bundleId);

    [AutomationOperation("start-syslog-broadcast", "Start Syslog Broadcast", "Starts iOS syslog broadcasting.")]
    Task StartSyslogBroadcast(string host = "127.0.0.1", int port = 4723);

    [AutomationOperation("stop-syslog-broadcast", "Stop Syslog Broadcast", "Stops iOS syslog broadcasting.")]
    Task StopSyslogBroadcast();
}

public abstract class AppiumServiceBase<TDriver> where TDriver : AppiumDriver
{
    private readonly ConcurrentDictionary<string, AppiumElement> _elements = new(StringComparer.Ordinal);
    private int _nextElementId;
    private TDriver? _driver;

    public TDriver Driver =>
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
        StoreElement(Driver.FindElement(CreateSelector(strategy, value)));

    public IReadOnlyList<string> FindElements(string strategy, string value) =>
        Driver.FindElements(CreateSelector(strategy, value))
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

    public void InstallApp(string appPath) => Driver.InstallApp(appPath);

    public void RemoveApp(string appId) => Driver.RemoveApp(appId);

    public void ActivateApp(string appId) => Driver.ActivateApp(appId);

    public bool TerminateApp(string appId) => Driver.TerminateApp(appId);

    public bool IsAppInstalled(string appId) => Driver.IsAppInstalled(appId);

    public string PullFile(string pathOnDevice) => Convert.ToBase64String(Driver.PullFile(pathOnDevice));

    public void PushFile(string pathOnDevice, string base64Data) =>
        Driver.PushFile(pathOnDevice, Convert.FromBase64String(base64Data));

    public void BackgroundApp(int seconds = -1) =>
        Driver.BackgroundApp(TimeSpan.FromSeconds(seconds));

    public void HideKeyboard() => Driver.HideKeyboard();

    public bool IsKeyboardShown() => Driver.IsKeyboardShown();

    public string StartRecordingScreen() => Driver.StartRecordingScreen();

    public string StopRecordingScreen() => Driver.StopRecordingScreen();

    public IReadOnlyDictionary<string, object> GetEvents(string? type = null) =>
        Driver.GetEvents(type);

    public void LogEvent(string vendorName, string eventName) =>
        Driver.LogEvent(vendorName, eventName);

    public object? ExecuteCustomDriverCommand(
        string commandName,
        IReadOnlyDictionary<string, object?>? parameters = null) =>
        Driver.ExecuteCustomDriverCommand(
            commandName,
            parameters is null ? null : new Dictionary<string, object>(parameters
                .Where(x => x.Value is not null)
                .ToDictionary(x => x.Key, x => x.Value!)));

    public object? GetSessionDetail(string detail) => Driver.GetSessionDetail(detail);

    public string GetPageSource() => Driver.PageSource;

    public string GetCurrentUrl() => Driver.Url;

    public string GetTitle() => Driver.Title;

    public object? ExecuteScript(
        string script,
        IReadOnlyDictionary<string, object?>? arguments = null) =>
        arguments is null
            ? Driver.ExecuteScript(script)
            : Driver.ExecuteScript(
                script,
                arguments.ToDictionary(x => x.Key, x => x.Value));

    public IReadOnlyList<string> GetContexts() => Driver.Contexts.ToArray();

    public void SetContext(string contextName) => Driver.Context = contextName;

    public string GetScreenshot() => Convert.ToBase64String(Driver.GetScreenshot().AsByteArray);


}

public sealed class AndroidAppiumService : AppiumServiceBase<AndroidDriver>, IAndroidAppiumService
{
    protected override AndroidDriver CreateDriver(Uri serverUrl, AppiumOptions options) =>
        new(serverUrl, options);

    public void StartActivity(
        string intent,
        Dictionary<string, object>? arguments = null,
        string? user = null,
        bool? wait = null,
        bool? stop = null,
        int? windowingMode = null,
        int? activityType = null,
        string? action = null,
        string? uri = null,
        string? mimeType = null,
        string? identifier = null,
        string[]? categories = null,
        string? component = null,
        string? package = null,
        string[][]? extras = null,
        string? flags = null) =>
        Driver.StartActivity(intent, arguments, user, wait, stop, windowingMode, activityType, action, uri,
            mimeType, identifier, categories, component, package, extras, flags);

    public void PressKeyCode(int keyCode, int metastate = -1) => Driver.PressKeyCode(keyCode, metastate);

    public void LongPressKeyCode(int keyCode, int metastate = -1) =>
        Driver.LongPressKeyCode(keyCode, metastate);

    public void ToggleLocationServices() => Driver.ToggleLocationServices();

    public void MakeGsmCall(string phoneNumber, GsmCallActions gsmCallAction) =>
        Driver.MakeGsmCall(phoneNumber, gsmCallAction);

    public void SendSms(string phoneNumber, string message) => Driver.SendSms(phoneNumber, message);

    public void SetGsmSignalStrength(GsmSignalStrength gsmSignalStrength) =>
        Driver.SetGsmSignalStrength(gsmSignalStrength);

    public void SetGsmVoice(GsmVoiceState gsmVoiceState) => Driver.SetGsmVoice(gsmVoiceState);

    public void OpenNotifications() => Driver.OpenNotifications();

    public IDictionary<string, object> GetSystemBars() => Driver.GetSystemBars();

    public float GetDisplayDensity() => Driver.GetDisplayDensity();

    public IReadOnlyList<object> GetPerformanceData(
        string packageName,
        string performanceDataType,
        int dataReadAttempts = 1) =>
        Driver.GetPerformanceData(packageName, performanceDataType, dataReadAttempts);

    public IReadOnlyList<string> GetPerformanceDataTypes() => Driver.GetPerformanceDataTypes();

    public void Lock(int? seconds = null) => Driver.Lock(seconds);

    public bool IsLocked() => Driver.IsLocked();

    public void Unlock(string key, string type, string? strategy = null, int? timeoutMs = null) =>
        Driver.Unlock(key, type, strategy, timeoutMs);

    public void SetSetting(string setting, object value) => Driver.SetSetting(setting, value);

    public void IgnoreUnimportantViews(bool compress) => Driver.IgnoreUnimportantViews(compress);

    public void ConfiguratorSetWaitForIdleTimeout(int timeout) =>
        Driver.ConfiguratorSetWaitForIdleTimeout(timeout);

    public void ConfiguratorSetWaitForSelectorTimeout(int timeout) =>
        Driver.ConfiguratorSetWaitForSelectorTimeout(timeout);

    public void ConfiguratorSetScrollAcknowledgmentTimeout(int timeout) =>
        Driver.ConfiguratorSetScrollAcknowledgmentTimeout(timeout);

    public void ConfiguratorSetKeyInjectionDelay(int delay) =>
        Driver.ConfiguratorSetKeyInjectionDelay(delay);

    public void ConfiguratorSetActionAcknowledgmentTimeout(int timeout) =>
        Driver.ConfiguratorSetActionAcknowledgmentTimeout(timeout);

    public string CurrentActivity => Driver.CurrentActivity;

    public string CurrentPackage => Driver.CurrentPackage;
}

public sealed class IOSAppiumService : AppiumServiceBase<IOSDriver>, IIOSAppiumService
{
    protected override IOSDriver CreateDriver(Uri serverUrl, AppiumOptions options) =>
        new(serverUrl, options);

    public void SetSetting(string setting, object value) => Driver.SetSetting(setting, value);

    public void ShakeDevice() => Driver.ShakeDevice();

    public void HideKeyboard(string key) => Driver.HideKeyboard(key);

    public void PerformTouchID(bool match) => Driver.PerformTouchID(match);

    public void InstallApp(string appPath, int? timeoutMs = null) =>
        Driver.InstallApp(appPath, timeoutMs);

    public void LaunchAppWithArguments(
        string bundleId,
        IReadOnlyCollection<string>? processArguments = null,
        IDictionary<string, string>? environmentVariables = null) =>
        Driver.LaunchAppWithArguments(bundleId, processArguments, environmentVariables);

    public bool IsLocked() => Driver.IsLocked();

    public void Lock(int? seconds = null) => Driver.Lock(seconds);

    public void Unlock() => Driver.Unlock();

    public void SetClipboardUrl(string url) => Driver.SetClipboardUrl(url);

    public string GetClipboardUrl() => Driver.GetClipboardUrl();

    public AppState GetAppState(string bundleId) => Driver.GetAppState(bundleId);

    public Task StartSyslogBroadcast(string host = "127.0.0.1", int port = 4723) =>
        Driver.StartSyslogBroadcast(host, port);

    public Task StopSyslogBroadcast() => Driver.StopSyslogBroadcast();
}

public static class AndroidAppiumSessionFactory
{
    public static AndroidAppiumService Create(
        string serverUrl,
        string deviceName,
        string? platformVersion = null,
        string? automationName = "UiAutomator2",
        string? app = null,
        string? udid = null,
        string? appPackage = null,
        string? appActivity = null,
        bool noReset = false)
    {
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            DeviceName = deviceName,
            AutomationName = automationName ?? "UiAutomator2",
            App = app ?? string.Empty
        };

        Add(options, "platformVersion", platformVersion);
        Add(options, "udid", udid);
        Add(options, "appPackage", appPackage);
        Add(options, "appActivity", appActivity);
        Add(options, "noReset", noReset);

        var service = new AndroidAppiumService();
        return service;
    }

    private static void Add(AppiumOptions options, string name, object? value)
    {
        if (value is not null)
            options.AddAdditionalAppiumOption(name, value);
    }
}

public static class IOSAppiumSessionFactory
{
    public static IOSAppiumService Create(
        string serverUrl,
        string deviceName,
        string? platformVersion = null,
        string? automationName = "XCUITest",
        string? app = null,
        string? udid = null,
        string? bundleId = null,
        bool noReset = false)
    {
        var options = new AppiumOptions
        {
            PlatformName = "iOS",
            DeviceName = deviceName,
            AutomationName = automationName ?? "XCUITest",
            App = app ?? string.Empty
        };

        Add(options, "platformVersion", platformVersion);
        Add(options, "udid", udid);
        Add(options, "bundleId", bundleId);
        Add(options, "noReset", noReset);

        return new IOSAppiumService();
    }

    private static void Add(AppiumOptions options, string name, object? value)
    {
        if (value is not null)
            options.AddAdditionalAppiumOption(name, value);
    }
}

internal static class AppiumOperationInvoker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static IReadOnlyList<IAutomationModule> CreateModules(
        IAndroidAppiumService android,
        IIOSAppiumService ios)
    {
        var modules = new List<IAutomationModule>();
        Add(modules, android, typeof(IAndroidAppiumService));
        Add(modules, ios, typeof(IIOSAppiumService));
        return modules;
    }

    private static void Add(
        ICollection<IAutomationModule> modules,
        object service,
        Type serviceType)
    {
        var serviceMetadata = serviceType.GetCustomAttribute<AutomationServiceAttribute>()
            ?? throw new InvalidOperationException($"Service '{serviceType.Name}' is missing AutomationServiceAttribute.");

        foreach (var method in serviceType.GetMethods())
        {
            var operation = method.GetCustomAttribute<AutomationOperationAttribute>();
            if (operation is null)
                continue;

            var operationId = $"appium.{serviceMetadata.Id.Replace("appium.", string.Empty)}.{operation.Id}";
            modules.Add(new ReflectionAppiumModule(
                operationId,
                operation,
                serviceMetadata,
                serviceType,
                service,
                method));
        }
    }

    private sealed class ReflectionAppiumModule(
        string id,
        AutomationOperationAttribute operation,
        AutomationServiceAttribute serviceMetadata,
        Type serviceType,
        object service,
        MethodInfo method) : IAutomationModule
    {
        public ModuleDescriptor Descriptor { get; } = CreateDescriptor(
            id, operation, serviceMetadata, serviceType, method);

        public async Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
        {
            if (!operation.WorkflowInvocable)
                throw new InvalidOperationException(
                    $"Operation '{Descriptor.Id}' is metadata-only and cannot be invoked from a workflow.");

            var arguments = BindArguments(method, context.Parameters, context.CancellationToken);
            var result = method.Invoke(service, arguments);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                result = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            return new ModuleExecutionResult(SerializeResult(result));
        }

        private static ModuleDescriptor CreateDescriptor(
            string id,
            AutomationOperationAttribute operation,
            AutomationServiceAttribute serviceMetadata,
            Type serviceType,
            MethodInfo method)
        {
            var parameters = method.GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken))
                .Select(parameter =>
                {
                    var metadata = parameter.GetCustomAttribute<AutomationParameterAttribute>();
                    var optional = parameter.IsOptional || parameter.HasDefaultValue;
                    return new ModuleParameterDefinition(
                        parameter.Name!,
                        parameter.ParameterType.FullName ?? parameter.ParameterType.Name,
                        !optional,
                        metadata?.Description);
                })
                .ToArray();

            var operationDescriptor = new AutomationOperationDescriptor(
                operation.Id,
                operation.DisplayName,
                operation.Description,
                method.ReturnType.FullName ?? method.ReturnType.Name,
                parameters.Select(x => new AutomationParameterDescriptor(
                    x.Name, x.Type, x.Required, x.Description)).ToArray());

            var serviceDescriptor = new AutomationServiceDescriptor(
                serviceMetadata.Id,
                serviceMetadata.DisplayName,
                serviceMetadata.Description,
                serviceMetadata.Version,
                [operationDescriptor]);

            return new ModuleDescriptor(
                id,
                $"{serviceMetadata.DisplayName}: {operation.DisplayName}",
                operation.Description,
                serviceMetadata.Version,
                parameters,
                [serviceDescriptor]);
        }

        private static object?[] BindArguments(
            MethodInfo method,
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken cancellationToken)
        {
            return method.GetParameters()
                .Select(parameter =>
                {
                    if (parameter.ParameterType == typeof(CancellationToken))
                        return cancellationToken;

                    if (!parameters.TryGetValue(parameter.Name!, out var raw))
                    {
                        if (parameter.IsOptional || parameter.HasDefaultValue)
                            return parameter.DefaultValue;

                        throw new ArgumentException(
                            $"Required parameter '{parameter.Name}' is missing for operation '{method.Name}'.");
                    }

                    if (raw is null)
                        return null;

                    if (parameter.ParameterType.IsInstanceOfType(raw))
                        return raw;

                    var json = raw is JsonElement element
                        ? element.GetRawText()
                        : JsonSerializer.Serialize(raw);

                    return JsonSerializer.Deserialize(
                        json,
                        parameter.ParameterType,
                        JsonOptions);
                })
                .ToArray();
        }

        private static string SerializeResult(object? result)
        {
            if (result is null)
                return string.Empty;

            if (result is string text)
                return text;

            return JsonSerializer.Serialize(result, JsonOptions);
        }
    }
}

public static class AppiumModule
{
    public static IReadOnlyList<IAutomationModule> GetModules()
    {
        var android = new AndroidAppiumService();
        var ios = new IOSAppiumService();

        return AppiumOperationInvoker.CreateModules(android, ios);
    }
}
