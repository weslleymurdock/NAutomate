namespace NAutomate.Modules.Shell.Pwsh;

/// <summary>Contains the result of a shell process execution.</summary>
public sealed record ShellCommandResult(
    int ExitCode,
    string StandardOutput,
    string StandardError)
{
    /// <summary>Gets whether the process completed successfully.</summary>
    public bool Succeeded => ExitCode == 0;
}
