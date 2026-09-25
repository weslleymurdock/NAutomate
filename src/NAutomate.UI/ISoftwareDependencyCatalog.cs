namespace NAutomate.UI;

/// <summary>Provides software dependencies contributed by the host and installed modules.</summary>
public interface ISoftwareDependencyCatalog
{
    IReadOnlyList<SoftwareDependencyDescriptor> GetDependencies();
}
