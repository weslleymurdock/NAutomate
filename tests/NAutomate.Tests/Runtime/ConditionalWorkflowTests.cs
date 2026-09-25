using NAutomate.Abstractions;
using NAutomate.Core;
using NAutomate.Modules.Core;
using NAutomate.Parser;
using NAutomate.Tests.Sinks;

namespace NAutomate.Tests.Runtime;

public sealed class ConditionalWorkflowTests
{
    private static AutomationWorkflow Workflow(
        IReadOnlyList<WorkflowStep> steps,
        params AutomationVariableDefinition[] variables) =>
        new(WorkflowJson.CurrentSchemaVersion, "conditional", steps, variables);

    private static ModuleRegistry Registry() =>
        new([new EchoModule(), new SetModule()]);

    [Fact]
    public async Task WhileSetAndEchoUseTheCurrentTypedVariableValue()
    {
        var workflow = Workflow(
        [
            new SetStep("reset", "Counter", 0),
            new WhileStep(
                "loop",
                new WorkflowCondition("Counter", WorkflowConditionOperator.LessThan, 3),
                [
                    new WorkflowStep("echo", "core.echo", new Dictionary<string, object?> { ["message"] = "$Counter" }),
                    new SetStep("increment", "Counter", 1, WorkflowSetOperation.Increment)
                ])
        ],
        new AutomationVariableDefinition("Counter", Type: "System.Int32", DefaultValue: 0));

        var sink = new RecordingSink();
        var result = await new WorkflowEngine(Registry()).ExecuteAsync(workflow, sink);

        Assert.Equal(WorkflowExecutionStatus.Success, result.Status);
        Assert.Equal(["0", "1", "2"], sink.Events.Where(x => x.Kind == "output").Select(x => x.Message));
        Assert.Contains(sink.Events, x => x.Kind == "variable-changed");
    }

    [Fact]
    public async Task IfElseObservesAnUpdatedVariable()
    {
        var workflow = Workflow(
        [
            new SetStep("set", "Value", 10),
            new IfStep(
                "branch",
                new WorkflowCondition("Value", WorkflowConditionOperator.GreaterThan, 5),
                [new WorkflowStep("then", "core.echo", new Dictionary<string, object?> { ["message"] = "greater" })],
                [new WorkflowStep("else", "core.echo", new Dictionary<string, object?> { ["message"] = "smaller" })])
        ],
        new AutomationVariableDefinition("Value", Type: "System.Int32", DefaultValue: 0));

        var sink = new RecordingSink();
        var result = await new WorkflowEngine(Registry()).ExecuteAsync(workflow, sink);

        Assert.Equal(WorkflowExecutionStatus.Success, result.Status);
        Assert.Equal("greater", Assert.Single(sink.Events.Where(x => x.Kind == "output")).Message);
        Assert.Contains(sink.Events, x => x.Kind == "branch-selected" && x.Message == "then");
    }

    [Fact]
    public async Task ForSupportsDecrementAndForeachRestoresItemScope()
    {
        var workflow = Workflow(
        [
            new ForStep(
                "for",
                "Counter",
                3,
                0,
                -1,
                false,
                [new WorkflowStep("for-echo", "core.echo", new Dictionary<string, object?> { ["message"] = "$Counter" })]),
            new ForeachStep(
                "foreach",
                "$Items",
                "Item",
                [new WorkflowStep("foreach-echo", "core.echo", new Dictionary<string, object?> { ["message"] = "@Item" })])
        ],
        new AutomationVariableDefinition("Counter", Type: "System.Int32", DefaultValue: 0),
        new AutomationVariableDefinition("Items", Type: "System.Int32[]", DefaultValue: new[] { 7, 8 }));

        var sink = new RecordingSink();
        var result = await new WorkflowEngine(Registry()).ExecuteAsync(workflow, sink);

        Assert.Equal(WorkflowExecutionStatus.Success, result.Status);
        Assert.Equal(["3", "2", "1", "7", "8"], sink.Events.Where(x => x.Kind == "output").Select(x => x.Message));
    }

    [Fact]
    public async Task VariableStateIsIsolatedBetweenExecutions()
    {
        var workflow = Workflow(
        [
            new SetStep("increment", "Counter", 1, WorkflowSetOperation.Increment),
            new WorkflowStep("echo", "core.echo", new Dictionary<string, object?> { ["message"] = "$Counter" })
        ],
        new AutomationVariableDefinition("Counter", Type: "System.Int32", DefaultValue: 0));

        var engine = new WorkflowEngine(Registry());
        var first = new RecordingSink();
        var second = new RecordingSink();

        await engine.ExecuteAsync(workflow, first);
        await engine.ExecuteAsync(workflow, second);

        Assert.Equal("1", Assert.Single(first.Events.Where(x => x.Kind == "output")).Message);
        Assert.Equal("1", Assert.Single(second.Events.Where(x => x.Kind == "output")).Message);
    }


    [Fact]
    public async Task ExitZeroCompletesAndNonZeroFailsWithoutExecutingFollowingSteps()
    {
        var workflow = Workflow(
        [
            new WorkflowStep("before", "core.echo", new Dictionary<string, object?> { ["message"] = "before" }),
            new ExitStep("exit", 7),
            new WorkflowStep("after", "core.echo", new Dictionary<string, object?> { ["message"] = "after" })
        ]);

        var sink = new RecordingSink();
        var result = await new WorkflowEngine(Registry()).ExecuteAsync(workflow, sink);

        Assert.Equal(WorkflowExecutionStatus.Failure, result.Status);
        Assert.Equal(["before"], sink.Events.Where(x => x.Kind == "output").Select(x => x.Message));
        Assert.Contains(sink.Events, x => x.Kind == "exit" && x.Message == "7");
    }

    [Fact]
    public void VariableReferencesSupportDollarAndAtSyntax()
    {
        var store = new WorkflowVariableStore(new Dictionary<string, object?> { ["Name"] = "Alice", ["Count"] = 42 });

        Assert.Equal("Alice", WorkflowValueResolver.Resolve("$Name", store));
        Assert.Equal(42, WorkflowValueResolver.Resolve("@Count", store));
        Assert.Equal("Hello Alice #42", WorkflowValueResolver.Resolve("Hello $Name #@Count", store));
    }

    [Fact]
    public void ConditionalJsonRoundTripsAsNestedDeclarativeSteps()
    {
        var workflow = Workflow(
        [
            new IfStep(
                "branch",
                new WorkflowCondition("Flag", WorkflowConditionOperator.Equals, true),
                [new WorkflowStep("yes", "core.echo", new Dictionary<string, object?> { ["message"] = "yes" })])
        ],
        new AutomationVariableDefinition("Flag", Type: "System.Boolean", DefaultValue: true));

        var restored = WorkflowJson.Deserialize(WorkflowJson.Serialize(workflow));

        var branch = Assert.IsType<IfStep>(Assert.Single(restored.Steps));
        Assert.Equal("Flag", branch.Condition.Variable);
        Assert.Equal("core.echo", Assert.Single(branch.Then).Module);
    }

    [Fact]
    public void ValidationRejectsUnknownVariablesAndDuplicateNestedIds()
    {
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(
            Workflow([new SetStep("set", "Missing", 1)])));

        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(
            Workflow(
            [
                new IfStep(
                    "branch",
                    new WorkflowCondition("Value", WorkflowConditionOperator.Equals, 1),
                    [new WorkflowStep("duplicate", "core.echo", new Dictionary<string, object?>())],
                    [new WorkflowStep("duplicate", "core.echo", new Dictionary<string, object?>())])
            ],
            new AutomationVariableDefinition("Value", Type: "System.Int32", DefaultValue: 0))));
    }
}
