using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Enums;
using NAutomate.Abstractions;

namespace NAutomate.Modules.Appium;

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
