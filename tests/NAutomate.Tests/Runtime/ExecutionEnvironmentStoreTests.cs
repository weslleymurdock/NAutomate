using NAutomate.Abstractions;
using NAutomate.Core;
using Xunit;

namespace NAutomate.Tests.Runtime;

public sealed class ExecutionEnvironmentStoreTests
{
    [Fact]
    public async Task NewEnvironmentContainsEveryWorkflowVariableWithEmptyValues()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "environments.json");
        var store = new FileExecutionEnvironmentStore(path);
        var workflow = CreateWorkflow(
            new AutomationVariableDefinition("baseUrl"),
            new AutomationVariableDefinition("username"));

        var environment = await store.CreateAsync("Development", workflow, TestContext.Current.CancellationToken);

        Assert.Null(environment.Values["baseUrl"]);
        Assert.Null(environment.Values["username"]);
    }

    [Fact]
    public async Task SynchronizationAddsNewVariablesToEverySavedEnvironment()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "environments.json");
        var store = new FileExecutionEnvironmentStore(path);
        var initial = CreateWorkflow(new AutomationVariableDefinition("baseUrl"));
        var development = await store.CreateAsync("Development", initial, TestContext.Current.CancellationToken);
        var staging = await store.CreateAsync("Staging", initial, TestContext.Current.CancellationToken);

        var updated = initial with
        {
            Variables =
            [
                new AutomationVariableDefinition("baseUrl"),
                new AutomationVariableDefinition("token")
            ]
        };

        var environments = await store.SynchronizeAsync(updated, TestContext.Current.CancellationToken);

        Assert.Equal(2, environments.Count);
        Assert.All(environments, environment =>
        {
            Assert.True(environment.Values.ContainsKey("baseUrl"));
            Assert.True(environment.Values.ContainsKey("token"));
            Assert.Null(environment.Values["token"]);
        });
        Assert.Equal(development.Id, environments.Single(x => x.Name == "Development").Id);
        Assert.Equal(staging.Id, environments.Single(x => x.Name == "Staging").Id);
    }

    private static AutomationWorkflow CreateWorkflow(params AutomationVariableDefinition[] variables) =>
        new(
            WorkflowJson.CurrentSchemaVersion,
            "Environment test",
            [new WorkflowStep("step", "core.echo", new Dictionary<string, object?>())],
            variables);
}
