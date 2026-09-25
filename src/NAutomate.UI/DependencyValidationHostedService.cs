using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace NAutomate.UI;

/// <summary>Runs software dependency validation when the application host starts.</summary>
public sealed class DependencyValidationHostedService(
    ISoftwareDependencyChecker checker,
    DependencyStatusStore store,
    ILogger<DependencyValidationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            store.SetStatuses(await checker.CheckAsync(cancellationToken).ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Software dependency validation failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
