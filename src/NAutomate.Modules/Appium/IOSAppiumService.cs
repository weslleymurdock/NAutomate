using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Enums;

namespace NAutomate.Modules.Appium;

public sealed class IOSAppiumService : AppiumServiceBase<IOSDriver>, IIOSAppiumService
{
    protected override IOSDriver CreateDriver(Uri serverUrl, AppiumOptions options) =>
        new(serverUrl, options);

    public void SetSetting(string setting, object value) =>
        ExecuteScript("mobile: setSetting", new Dictionary<string, object>
        {
            ["setting"] = setting,
            ["value"] = value
        });

    public void ShakeDevice() => TypedDriver.ShakeDevice();

    public void HideKeyboard(string key) => TypedDriver.HideKeyboard(key);

    public void PerformTouchID(bool match) => TypedDriver.PerformTouchID(match);

    public void InstallApp(string appPath, int? timeoutMs = null) =>
        TypedDriver.InstallApp(appPath, timeoutMs);

    public void LaunchAppWithArguments(
        string bundleId,
        IReadOnlyCollection<string>? processArguments = null,
        IDictionary<string, string>? environmentVariables = null) =>
        TypedDriver.LaunchAppWithArguments(bundleId, processArguments, environmentVariables);

    public bool IsLocked() =>
        Convert.ToBoolean(ExecuteScript("mobile: isLocked"));

    public void Lock(int? seconds = null) =>
        ExecuteScript(
            "mobile: lock",
            new Dictionary<string, object?>
            {
                ["seconds"] = seconds ?? 0
            });

    public void Unlock() => ExecuteScript("mobile: unlock");

    public void SetClipboardUrl(string url) => TypedDriver.SetClipboardUrl(url);

    public string GetClipboardUrl() => TypedDriver.GetClipboardUrl();

    public AppState GetAppState(string bundleId) => TypedDriver.GetAppState(bundleId);

    public Task StartSyslogBroadcast(string host = "127.0.0.1", int port = 4723) =>
        TypedDriver.StartSyslogBroadcast(host, port);

    public Task StopSyslogBroadcast() => TypedDriver.StopSyslogBroadcast();
}
