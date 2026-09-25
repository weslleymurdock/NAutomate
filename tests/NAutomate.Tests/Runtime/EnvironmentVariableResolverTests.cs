using NAutomate.Abstractions;
using NAutomate.Core;
using Xunit;

namespace NAutomate.Tests.Runtime;

public sealed class EnvironmentVariableResolverTests
{
    [Fact]
    public void ResolvesGlobalAndLocalPlaceholders()
    {
        var step = new WorkflowStep(
            "login",
            "core.echo",
            new Dictionary<string, object?>
            {
                ["message"] = "${baseUrl}/login/${login:user}"
            });

        var environment = new AutomationEnvironment(
            "env",
            "Environment test",
            "Development",
            new Dictionary<string, string?>
            {
                ["baseUrl"] = "https://example.test",
                ["login:user"] = "alice"
            });

        var parameters = EnvironmentVariableResolver.ResolveParameters(step, environment);

        Assert.Equal("https://example.test/login/alice", parameters["message"]);
    }
}
