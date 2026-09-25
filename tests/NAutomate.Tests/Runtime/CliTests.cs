using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using NAutomate.Core;
using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Tests.Runtime;

public sealed class CliTests
{
    [Fact]
    public async Task SuccessfulRunUsesStdoutAndNoStderr()
    {
        var directory = Directory.CreateTempSubdirectory("nautomate-cli-");
        try
        {
            var path = Path.Combine(directory.FullName, "workflow.json");
            await File.WriteAllTextAsync(path, WorkflowJson.Serialize(WorkflowJsonTests.Workflow("Hello", ("one", "core.echo", WorkflowJsonTests.Parameters("Hello from test")))), TestContext.Current.CancellationToken);
            using var stdout = new StringWriter();
            using var stderr = new StringWriter();

            var exitCode = await CliRunner.RunAsync(["run", path], stdout, stderr, TestContext.Current.CancellationToken);

            Assert.Equal(0, exitCode);
            Assert.Contains("[RUNNING] core.echo", stdout.ToString());
            Assert.Contains("[OUTPUT] Hello from test", stdout.ToString());
            Assert.Contains("Execution completed successfully.", stdout.ToString());
            Assert.Empty(stderr.ToString());
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task InvalidArgumentsAndUnknownModuleUseStderrAndNonZeroExit()
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        Assert.NotEqual(0, await CliRunner.RunAsync([], stdout, stderr, TestContext.Current.CancellationToken));
        Assert.Contains("Usage:", stderr.ToString());

        var directory = Directory.CreateTempSubdirectory("nautomate-cli-");
        try
        {
            var path = Path.Combine(directory.FullName, "workflow.json");
            await File.WriteAllTextAsync(path, WorkflowJson.Serialize(WorkflowJsonTests.Workflow("Unknown", ("one", "missing", new Dictionary<string, object?>()))), TestContext.Current.CancellationToken);
            stdout.GetStringBuilder().Clear();
            stderr.GetStringBuilder().Clear();

            Assert.NotEqual(0, await CliRunner.RunAsync(["run", path], stdout, stderr, TestContext.Current.CancellationToken));
            Assert.Contains("Error:", stderr.ToString());
            Assert.DoesNotContain("[SUCCESS]", stdout.ToString());
        }
        finally
        {
            directory.Delete(true);
        }
    }
}