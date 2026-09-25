using NAutomate.Abstractions;
using NAutomate.Core;

namespace NAutomate.Tests.Runtime;


public sealed class WorkflowJsonTests
{
    [Fact]
    public void SerializeAndDeserializePreservesWorkflow()
    {
        var workflow = Workflow("Hello", ("one", "core.echo", new Dictionary<string, object?> { ["message"] = "world", ["count"] = 2 }));

        var json = WorkflowJson.Serialize(workflow);
        var restored = WorkflowJson.Deserialize(json);

        Assert.Equal(workflow.SchemaVersion, restored.SchemaVersion);
        Assert.Equal(workflow.Name, restored.Name);
        Assert.Equal(workflow.Steps[0].Id, restored.Steps[0].Id);
        Assert.Equal("world", restored.Steps[0].Parameters["message"]?.ToString());
        Assert.Equal("2", restored.Steps[0].Parameters["count"]?.ToString());
    }

    [Fact]
    public void ValidationRejectsInvalidWorkflowShapes()
    {
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(Workflow("", ("one", "core.echo", Parameters("ok")))));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(new(1, "name", [])));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(Workflow("name", ("", "core.echo", Parameters("ok")))));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(Workflow("name", ("one", "", Parameters("ok")))));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Validate(new(1, "name", [
            new WorkflowStep("one", "core.echo", Parameters("a")),
            new WorkflowStep("one", "core.echo", Parameters("b"))])));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Deserialize("{"));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Deserialize(""));
        Assert.Throws<InvalidDataException>(() => WorkflowJson.Deserialize("{\"schemaVersion\":99,\"name\":\"x\",\"steps\":[]}"));
    }

    internal static AutomationWorkflow Workflow(string name, params (string Id, string Module, IReadOnlyDictionary<string, object?> Parameters)[] steps) =>
        new(WorkflowJson.CurrentSchemaVersion, name, steps.Select(step => new WorkflowStep(step.Id, step.Module, step.Parameters)).ToArray());

    internal static Dictionary<string, object?> Parameters(string message) => new() { ["message"] = message };
}
