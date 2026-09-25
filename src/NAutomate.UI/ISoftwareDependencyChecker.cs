namespace NAutomate.UI;

/// <summary>Validates software dependencies on the current application host.</summary>
public interface ISoftwareDependencyChecker
{
    Task<IReadOnlyList<SoftwareDependencyStatus>> CheckAsync(CancellationToken cancellationToken = default);
}
