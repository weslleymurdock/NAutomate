using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Tests.Parser;

public sealed class WorkflowParameterParserTests
{
    [Fact]
    public void ParsesTypedOperationParameters()
    {
        var values = new Dictionary<string, string?>
        {
            ["headless"] = "true",
            ["arguments"] = "[\"--incognito\", \"--disable-gpu\"]"
        };

        var parameters = new[]
        {
            new AutomationParameterDescriptor("headless", "System.Boolean", true),
            new AutomationParameterDescriptor("arguments", "System.String[]", false)
        };

        var parsed = WorkflowParameterParser.Parse(values, parameters);

        Assert.Equal(true, parsed["headless"]);
        Assert.Equal(["--incognito", "--disable-gpu"], parsed["arguments"]);
    }

    [Fact]
    public void ResolvesGlobalAndLocalEnvironmentPlaceholders()
    {
        var step = new WorkflowStep(
            "login",
            "core.echo",
            new Dictionary<string, object?> { ["message"] = "$" + "{baseUrl}/login/$" + "{login:user}" });

        var environment = new AutomationEnvironment(
            "env",
            "Environment test",
            "Development",
            new Dictionary<string, string?>
            {
                ["baseUrl"] = "https://example.test",
                ["login:user"] = "alice"
            });

        var result = EnvironmentVariableResolver.ResolveParameters(step, environment);

        Assert.Equal("https://example.test/login/alice", result["message"]);
    }
}
