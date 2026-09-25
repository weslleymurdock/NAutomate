using NAutomate.Core;
using NAutomate.Tests.Modules;
namespace NAutomate.Tests.Runtime;

public sealed class ModuleRegistryTests
{
    [Fact]
    public void RegistersListsAndResolvesCaseInsensitively()
    {
        var module = new TestModule("test.module");
        var registry = new ModuleRegistry([module]);

        Assert.Same(module, registry.Resolve("TEST.MODULE"));
        Assert.Single(registry.List());
    }

    [Fact]
    public void RejectsDuplicateAndInvalidIds()
    {
        var registry = new ModuleRegistry([new TestModule("test.module")]);
        Assert.Throws<InvalidOperationException>(() => registry.Register(new TestModule("TEST.MODULE")));
        Assert.Throws<ArgumentException>(() => registry.Register(new TestModule(" ")));
        Assert.Throws<KeyNotFoundException>(() => registry.Resolve("unknown"));
    }
}
