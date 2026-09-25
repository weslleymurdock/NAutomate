using System.Diagnostics;

namespace NAutomate.UI;

/// <summary>Checks executable availability by invoking each dependency command.</summary>
public sealed class SoftwareDependencyChecker(ISoftwareDependencyCatalog catalog) : ISoftwareDependencyChecker
{
    public async Task<IReadOnlyList<SoftwareDependencyStatus>> CheckAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<SoftwareDependencyStatus>();

        foreach (var dependency in catalog.GetDependencies())
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = dependency.Executable,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                foreach (var argument in dependency.Arguments ?? [])
                    startInfo.ArgumentList.Add(argument);

                using var process = new Process { StartInfo = startInfo };

                if (!process.Start())
                {
                    results.Add(new(dependency, false, "The process could not be started."));
                    continue;
                }

                var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                var output = await outputTask.ConfigureAwait(false);
                var error = await errorTask.ConfigureAwait(false);
                var details = string.IsNullOrWhiteSpace(output) ? error.Trim() : output.Trim();

                results.Add(new(dependency, process.ExitCode == 0, details));
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                results.Add(new(dependency, false, ex.Message));
            }
        }

        return results;
    }
}
