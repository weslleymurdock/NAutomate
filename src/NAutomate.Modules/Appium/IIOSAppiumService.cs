using NAutomate.Abstractions;

namespace NAutomate.Modules.Appium;

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

    [AutomationOperation("hide-keyboard-with-key", "Hide Keyboard With Key", "Hides the iOS keyboard using a key.")]
    void HideKeyboard(string key);

    [AutomationOperation("perform-touch-id", "Perform Touch ID", "Performs Touch ID authentication.")]
    void PerformTouchID(bool match);

    [AutomationOperation("install-app-with-timeout", "Install App With Timeout", "Installs an iOS application with an optional timeout.")]
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
