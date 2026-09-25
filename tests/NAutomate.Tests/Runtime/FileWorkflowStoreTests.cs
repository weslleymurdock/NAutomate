using NAutomate.Core;

namespace NAutomate.Tests.Runtime;

public sealed class FileWorkflowStoreTests
{
    [Fact]
    public async Task SaveAndLoadRoundTripInTemporaryDirectory()
    {
        var directory = Directory.CreateTempSubdirectory("nautomate-tests-");
        try
        {
            var path = Path.Combine(directory.FullName, "workflow.json");
            var workflow = WorkflowJsonTests.Workflow("stored", ("one", "core.echo", WorkflowJsonTests.Parameters("value")));

            var store = new FileWorkflowStore();
            await store.SaveAsync(path, workflow, TestContext.Current.CancellationToken);
            var loaded = await store.LoadAsync(path, TestContext.Current.CancellationToken);

            Assert.Equal(workflow.SchemaVersion, loaded.SchemaVersion);
            Assert.Equal(workflow.Name, loaded.Name);
            Assert.Equal(workflow.Steps.Count, loaded.Steps.Count);
            Assert.Equal(workflow.Steps[0].Id, loaded.Steps[0].Id);
            Assert.Equal("value", loaded.Steps[0].Parameters["message"]?.ToString());
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task LoadMissingFileAndCancelledSaveFail()
    {
        var store = new FileWorkflowStore();
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        await Assert.ThrowsAsync<FileNotFoundException>(() => store.LoadAsync(missing, TestContext.Current.CancellationToken));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.SaveAsync(path,
            WorkflowJsonTests.Workflow("cancelled", ("one", "core.echo", WorkflowJsonTests.Parameters("value"))), cancellation.Token));
    }
}
