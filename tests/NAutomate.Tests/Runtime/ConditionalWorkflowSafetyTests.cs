using System.Collections.Generic;
using NAutomate.Abstractions;
using NAutomate.Core;
using NAutomate.Modules;
using NAutomate.Modules.Core;
using NAutomate.Parser;
using NAutomate.Tests.Sinks;

namespace NAutomate.Tests.Runtime;

public sealed class ConditionalWorkflowSafetyTests
{
    [Fact]
    public async Task WhileStopsWhenCancellationIsRequested()
    {
        using var cancellation = new CancellationTokenSource();
        var workflow = new AutomationWorkflow(
            WorkflowJson.CurrentSchemaVersion,
            "cancel",
            [
                new WhileStep(
                    "loop",
                    new WorkflowCondition("Flag", WorkflowConditionOperator.Equals, true),
                    [new WorkflowStep("echo", "core.echo", new Dictionary<string, object?> { ["message"] = "running" })])
            ],
            [new AutomationVariableDefinition("Flag", Type: "System.Boolean", DefaultValue: true)]);

        var sink = new RecordingSink();
        cancellation.Cancel();

        var result = await new WorkflowEngine(new ModuleRegistry([new EchoModule()]))
            .ExecuteAsync(workflow, sink, cancellation.Token);

        Assert.Equal(WorkflowExecutionStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task WhileRejectsAnExceededIterationLimit()
    {
        var workflow = new AutomationWorkflow(
            WorkflowJson.CurrentSchemaVersion,
            "limit",
            [
                new WhileStep(
                    "loop",
                    new WorkflowCondition("Flag", WorkflowConditionOperator.Equals, true),
                    [new SetStep("set", "Flag", true)],
                    MaxIterations: 2)
            ],
            [new AutomationVariableDefinition("Flag", Type: "System.Boolean", DefaultValue: true)]);

        var result = await new WorkflowEngine(new ModuleRegistry([new SetModule()]))
            .ExecuteAsync(workflow, new RecordingSink());

        Assert.Equal(WorkflowExecutionStatus.Exception, result.Status);
        Assert.Contains("maximum iteration", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OfficialModulesExposeEchoAndSet()
    {
        var ids = new ModuleRegistry(OfficialModules.GetModules())
            .List()
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("core.echo", ids);
        Assert.Contains("core.set", ids);
    }
}
