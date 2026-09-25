using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Enums;
namespace NAutomate.Modules.Appium;

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
        TypedDriver.StartActivity(intent, arguments, user, wait, stop, windowingMode, activityType, action, uri,
            mimeType, identifier, categories, component, package, extras, flags);

    public void PressKeyCode(int keyCode, int metastate = -1) => TypedDriver.PressKeyCode(keyCode, metastate);

    public void LongPressKeyCode(int keyCode, int metastate = -1) =>
        TypedDriver.LongPressKeyCode(keyCode, metastate);

    public void ToggleLocationServices() => TypedDriver.ToggleLocationServices();

    public void MakeGsmCall(string phoneNumber, GsmCallActions gsmCallAction) =>
        TypedDriver.MakeGsmCall(phoneNumber, gsmCallAction);

    public void SendSms(string phoneNumber, string message) => TypedDriver.SendSms(phoneNumber, message);

    public void SetGsmSignalStrength(GsmSignalStrength gsmSignalStrength) =>
        TypedDriver.SetGsmSignalStrength(gsmSignalStrength);

    public void SetGsmVoice(GsmVoiceState gsmVoiceState) => TypedDriver.SetGsmVoice(gsmVoiceState);

    public void OpenNotifications() => TypedDriver.OpenNotifications();

    public IDictionary<string, object> GetSystemBars() => TypedDriver.GetSystemBars();

    public float GetDisplayDensity() => TypedDriver.GetDisplayDensity();

    public IReadOnlyList<object> GetPerformanceData(
        string packageName,
        string performanceDataType,
        int dataReadAttempts = 1) =>
        TypedDriver.GetPerformanceData(packageName, performanceDataType, dataReadAttempts);

    public IReadOnlyList<string> GetPerformanceDataTypes() => TypedDriver.GetPerformanceDataTypes();

    public void Lock(int? seconds = null) => TypedDriver.Lock(seconds);

    public bool IsLocked() => TypedDriver.IsLocked();

    public void Unlock(string key, string type, string? strategy = null, int? timeoutMs = null) =>
        TypedDriver.Unlock(key, type, strategy, timeoutMs);

    public void SetSetting(string setting, object value) => TypedDriver.SetSetting(setting, value);

    public void IgnoreUnimportantViews(bool compress) => TypedDriver.IgnoreUnimportantViews(compress);

    public void ConfiguratorSetWaitForIdleTimeout(int timeout) =>
        TypedDriver.ConfiguratorSetWaitForIdleTimeout(timeout);

    public void ConfiguratorSetWaitForSelectorTimeout(int timeout) =>
        TypedDriver.ConfiguratorSetWaitForSelectorTimeout(timeout);

    public void ConfiguratorSetScrollAcknowledgmentTimeout(int timeout) =>
        TypedDriver.ConfiguratorSetScrollAcknowledgmentTimeout(timeout);

    public void ConfiguratorSetKeyInjectionDelay(int delay) =>
        TypedDriver.ConfiguratorSetKeyInjectionDelay(delay);

    public void ConfiguratorSetActionAcknowledgmentTimeout(int timeout) =>
        TypedDriver.ConfiguratorSetActionAcknowledgmentTimeout(timeout);

    public string CurrentActivity => TypedDriver.CurrentActivity;

    public string CurrentPackage => TypedDriver.CurrentPackage;

    AppiumDriver Driver { get; };

}

