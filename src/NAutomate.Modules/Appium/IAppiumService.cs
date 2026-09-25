using NAutomate.Abstractions;

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