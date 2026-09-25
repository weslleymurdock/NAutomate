using NAutomate.Abstractions;

namespace NAutomate.Modules.Shell.Pwsh;

public static class ShellModule
{
    public static IAutomationModule GetModule()
    {
        var service = new PwshShellService();
        return ShellOperationInvoker.CreateModule(service);
    }
}
