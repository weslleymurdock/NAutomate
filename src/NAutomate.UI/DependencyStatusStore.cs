namespace NAutomate.UI;

/// <summary>Stores the latest startup dependency validation result for UI consumption.</summary>
public sealed class DependencyStatusStore
{
    private IReadOnlyList<SoftwareDependencyStatus> _statuses = [];

    public IReadOnlyList<SoftwareDependencyStatus> Statuses => _statuses;

    public event Action? Changed;

    public void SetStatuses(IReadOnlyList<SoftwareDependencyStatus> statuses)
    {
        _statuses = statuses;
        Changed?.Invoke();
    }
}
