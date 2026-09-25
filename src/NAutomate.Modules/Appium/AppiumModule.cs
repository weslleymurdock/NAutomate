using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using NAutomate.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;

namespace NAutomate.Modules.Appium;

public static class AppiumModule
{
    public static IReadOnlyList<IAutomationModule> GetModules()
    {
        var android = new AndroidAppiumService();
        var ios = new IOSAppiumService();

        return AppiumOperationInvoker.CreateModules(android, ios);
    }
}
