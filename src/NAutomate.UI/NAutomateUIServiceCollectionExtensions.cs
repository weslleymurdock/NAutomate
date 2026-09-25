using Microsoft.Extensions.DependencyInjection;

namespace NAutomate.UI;

/// <summary>Registers the shared NAutomate UI services.</summary>
public static class NAutomateUIServiceCollectionExtensions
{
    public static IServiceCollection AddNAutomateUI(
        this IServiceCollection services,
        Action<SoftwareDependencyCatalog>? configureDependencies = null)
    {
        var catalog = new SoftwareDependencyCatalog();
        configureDependencies?.Invoke(catalog);

        services.AddSingleton<ISoftwareDependencyCatalog>(catalog);
        services.AddSingleton<SoftwareDependencyCatalog>(catalog);
        services.AddSingleton<ISoftwareDependencyChecker, SoftwareDependencyChecker>();
        services.AddSingleton<DependencyStatusStore>();
        services.AddHostedService<DependencyValidationHostedService>();

        return services;
    }
}
