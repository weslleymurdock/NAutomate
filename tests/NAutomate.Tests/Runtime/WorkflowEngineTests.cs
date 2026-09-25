using NAutomate.Core;
using NAutomate.Tests.Modules;
using NAutomate.Tests.Sinks;
namespace NAutomate.Tests.Runtime;


public sealed class WorkflowEngineTests
{
    [Fact]
    public async Task ExecutesStepsInOrderAndEmitsStepAwareEvents()
    {
        var executed = new List<string>();
        var registry = new ModuleRegistry([new TestModule("first", executed), new TestModule("second", executed)]);
        var sink = new RecordingSink();
        var workflow = WorkflowJsonTests.Workflow("ordered",
            ("one", "first", new Dictionary<string, object?>()),
            ("two", "second", new Dictionary<string, object?>()));

        await new WorkflowEngine(registry).ExecuteAsync(workflow, sink, TestContext.Current.CancellationToken);

        Assert.Equal(["first", "second"], executed);
        Assert.Equal(["workflow", "running", "output", "success", "running", "output", "success", "completed"], sink.Events.Select(item => item.Kind));
        Assert.Equal("one", sink.Events[1].StepId);
        Assert.Equal("two", sink.Events[4].StepId);
    }

    [Fact]
    public async Task UnknownModuleAndModuleFailurePropagateAndCancellationIsPreserved()
    {
        var workflow = WorkflowJsonTests.Workflow("unknown", ("one", "missing", new Dictionary<string, object?>()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => new WorkflowEngine(new ModuleRegistry([])).ExecuteAsync(workflow, new RecordingSink(), TestContext.Current.CancellationToken));

        var failing = WorkflowJsonTests.Workflow("failure", ("one", "failure", new Dictionary<string, object?>()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new WorkflowEngine(new ModuleRegistry([new TestModule("failure", failure: true)]))
            .ExecuteAsync(failing, new RecordingSink(), TestContext.Current.CancellationToken));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
            new WorkflowEngine(new ModuleRegistry([])).ExecuteAsync(
                WorkflowJsonTests.Workflow(
                    "cancelled", 
                    ("one", "missing", new Dictionary<string, object?>())), 
                    new RecordingSink(),
                    cancellation.Token));
    }
}