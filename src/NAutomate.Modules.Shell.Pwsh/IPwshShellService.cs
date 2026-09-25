using NAutomate.Abstractions;

namespace NAutomate.Modules.Shell.Pwsh;

[AutomationService(
    "shell.pwsh",
    "PowerShell",
    "Executes PowerShell Core commands through the pwsh executable.")]
public interface IPwshShellService
{
    [AutomationOperation("run", "Run", "Executes a PowerShell script and captures its standard output, standard error, and exit code.")]
    Task<ShellCommandResult> Run(
        string run,
        CancellationToken cancellationToken = default);
}
