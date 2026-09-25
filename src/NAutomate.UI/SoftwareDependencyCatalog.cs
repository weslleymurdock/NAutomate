namespace NAutomate.UI;

/// <summary>Mutable dependency catalog used during host composition.</summary>
public sealed class SoftwareDependencyCatalog : ISoftwareDependencyCatalog
{
    private readonly List<SoftwareDependencyDescriptor> _dependencies = [];

    public IReadOnlyList<SoftwareDependencyDescriptor> GetDependencies() => _dependencies;

    public SoftwareDependencyCatalog Add(SoftwareDependencyDescriptor dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        _dependencies.RemoveAll(x => x.Id.Equals(dependency.Id, StringComparison.OrdinalIgnoreCase));
        _dependencies.Add(dependency);
        return this;
    }
}
