using NAutomate.Core;

namespace NAutomate.Tests.Runtime;

public sealed class EchoModuleTests
{
    [Fact]
    public async Task EchoReturnsMessageAndRejectsMissingOrEmptyMessage()
    {
        var module = new ModuleRegistry().Resolve("core.echo");
        var workflow = WorkflowJsonTests.Workflow("echo", ("one", "core.echo", WorkflowJsonTests.Parameters("hello")));

        var result = await module.ExecuteAsync(new(workflow, workflow.Steps[0], CancellationToken.None));
        Assert.Equal("hello", result.Output);

        foreach (var parameters in new[] { new Dictionary<string, object?>(), new() { ["message"] = null }, new() { ["message"] = " " } })
        {
            var invalidWorkflow = WorkflowJsonTests.Workflow("echo", ("one", "core.echo", parameters));
            await Assert.ThrowsAsync<ArgumentException>(() => module.ExecuteAsync(new(invalidWorkflow, invalidWorkflow.Steps[0], CancellationToken.None)));
        }

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => module.ExecuteAsync(new(workflow, workflow.Steps[0], cancellation.Token)));
    }
}
