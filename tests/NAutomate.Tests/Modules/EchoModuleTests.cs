using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAutomate.Abstractions;
using NAutomate.Modules.Core;
using Xunit;

namespace NAutomate.Tests.Modules;

public sealed class EchoModuleTests
{
    private static IAutomationModule CreateModule() => new EchoModule();

    [Fact]
    public async Task ExecuteAsync_ReturnsMessage_WhenParameterIsValid()
    {
        // Arrange
        var module = CreateModule();
        var parameters = new Dictionary<string, object?> { ["message"] = "Hello world" };
        var step = new WorkflowStep("step-1", "core.echo", parameters);
        var workflow = new AutomationWorkflow(1, "Test", new[] { step });
        var context = new ModuleExecutionContext(workflow, step, parameters, CancellationToken.None);

        // Act
        var result = await module.ExecuteAsync(context);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Hello world", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenMessageMissing()
    {
        // Arrange
        var module = CreateModule();
        var parameters = new Dictionary<string, object?>(); // no "message" key
        var step = new WorkflowStep("step-1", "core.echo", parameters);
        var workflow = new AutomationWorkflow(1, "Test", new[] { step });
        var context = new ModuleExecutionContext(workflow, step, parameters, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await module.ExecuteAsync(context));
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenMessageEmpty()
    {
        // Arrange
        var module = CreateModule();
        var parameters = new Dictionary<string, object?> { ["message"] = "   " };
        var step = new WorkflowStep("step-1", "core.echo", parameters);
        var workflow = new AutomationWorkflow(1, "Test", new[] { step });
        var context = new ModuleExecutionContext(workflow, step, parameters, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await module.ExecuteAsync(context));
    }
}
