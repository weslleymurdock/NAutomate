using NAutomate.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAutomate.Tests.Modules;
using NAutomate.Core;
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
    public async Task UnknownModuleAndModuleExceptionPropagateAsException()
    {
        var workflow = WorkflowJsonTests.Workflow("unknown", ("one", "missing", new Dictionary<string, object?>()));
        var sink = new RecordingSink();
        var result = await new WorkflowEngine(new ModuleRegistry([])).ExecuteAsync(workflow, sink, TestContext.Current.CancellationToken);
        
        Assert.Equal(WorkflowExecutionStatus.Exception, result.Status);
        Assert.IsType<KeyNotFoundException>(result.Exception);
        Assert.Contains(sink.Events, e => e.Kind == "exception");

        var failing = WorkflowJsonTests.Workflow("exception", ("one", "exception_module", new Dictionary<string, object?> { ["throw"] = true }));
        var sink2 = new RecordingSink();
        var result2 = await new WorkflowEngine(new ModuleRegistry([new TestModule("exception_module")]))
            .ExecuteAsync(failing, sink2, TestContext.Current.CancellationToken);
            
        Assert.Equal(WorkflowExecutionStatus.Exception, result2.Status);
        Assert.IsType<InvalidOperationException>(result2.Exception);
        Assert.Contains(sink2.Events, e => e.Kind == "exception");
    }

    [Fact]
    public async Task ModuleFailureStopsExecutionAndReturnsFailure()
    {
        var executed = new List<string>();
        var registry = new ModuleRegistry([new TestModule("first", executed), new TestModule("second", executed, failure: true), new TestModule("third", executed)]);
        var sink = new RecordingSink();
        var workflow = WorkflowJsonTests.Workflow("failure_test",
            ("one", "first", new Dictionary<string, object?>()),
            ("two", "second", new Dictionary<string, object?>()),
            ("three", "third", new Dictionary<string, object?>()));

        var result = await new WorkflowEngine(registry).ExecuteAsync(workflow, sink, TestContext.Current.CancellationToken);

        Assert.Equal(WorkflowExecutionStatus.Failure, result.Status);
        Assert.Equal(["first", "second"], executed);
        Assert.Contains(sink.Events, e => e.Kind == "failure" && e.StepId == "two");
        Assert.DoesNotContain(sink.Events, e => e.Kind == "success" && e.StepId == "two");
    }

    [Fact]
    public async Task CancellationIsPreservedAndReturnsCancelled()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        
        var sink = new RecordingSink();
        var result = await new WorkflowEngine(new ModuleRegistry([])).ExecuteAsync(
            WorkflowJsonTests.Workflow(
                "cancelled", 
                ("one", "missing", new Dictionary<string, object?>())), 
                sink,
                cancellation.Token);
                
        Assert.Equal(WorkflowExecutionStatus.Cancelled, result.Status);
        Assert.Contains(sink.Events, e => e.Kind == "cancelled");
    }
}