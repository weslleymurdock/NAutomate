using NAutomate.Abstractions;
using NAutomate.Modules.Shell.Pwsh;

namespace NAutomate.Modules.Shell;

public static class ShellModule
{
    public static IEnumerable<IAutomationModule> GetModules()
    {
        var pwsh = new PwshShellService();

        yield return PwshShellOperationInvoker.CreateModule(pwsh);
    }
}
