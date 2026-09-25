using System.Diagnostics;

namespace NAutomate.Modules.Shell.Pwsh;

public sealed class PwshShellService : IPwshShellService
{
    public async Task<ShellCommandResult> Run(
        string run,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(run);

        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add(run);

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        var standardOutput = await standardOutputTask.ConfigureAwait(false);
        var standardError = await standardErrorTask.ConfigureAwait(false);

        return new ShellCommandResult(
            process.ExitCode,
            standardOutput,
            standardError);
    }

    private static void TryKill(Process process)
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
    }
}
