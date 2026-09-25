using NAutomate.Abstractions.Projects;
using NAutomate.Core.Projects;

namespace NAutomate.Tests.Projects;

public sealed class FileAutomationProjectStoreTests
{
    [Fact]
    public async Task CreateAsync_creates_all_required_project_files()
    {
        var root = Path.Combine(Path.GetTempPath(), $"nautomate-tests-{Guid.NewGuid():N}");
        try
        {
            var store = new FileAutomationProjectStore(root);
            var project = await store.CreateAsync("My Mobile Automation", TestContext.Current.CancellationToken);

            Assert.False(project.IsValid);
            Assert.Contains(project.ValidationErrors, error => error.Contains("at least one step", StringComparison.OrdinalIgnoreCase));
            Assert.Equal($"{project.Id:D}-my-mobile-automation", project.DirectoryName);

            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "automation.json")));
            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "settings.json")));
            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "env.json")));
            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "project.json")));

            var validation = await store.ValidateAsync(project.Id, TestContext.Current.CancellationToken);
            Assert.False(validation.IsValid);
            Assert.Contains(validation.Errors, error => error.Contains("at least one step", StringComparison.OrdinalIgnoreCase));

            var state = await store.LoadAsync(project.Id, TestContext.Current.CancellationToken);
            Assert.Empty(state.Workflow.Steps);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ValidateAsync_detects_modified_automation_file()
    {
        var root = Path.Combine(Path.GetTempPath(), $"nautomate-tests-{Guid.NewGuid():N}");
        try
        {
            var store = new FileAutomationProjectStore(root);
            var project = await store.CreateAsync("Integrity", TestContext.Current.CancellationToken);

            await File.AppendAllTextAsync(
                Path.Combine(project.DirectoryPath, "automation.json"),
                Environment.NewLine, TestContext.Current.CancellationToken);

            var validation = await store.ValidateAsync(project.Id, TestContext.Current.CancellationToken);

            Assert.False(validation.IsValid);
            Assert.Contains(validation.Errors, error => error.Contains("automation.json hash", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAsync_removes_project_directory_and_artifacts()
    {
        var root = Path.Combine(Path.GetTempPath(), $"nautomate-tests-{Guid.NewGuid():N}");
        try
        {
            var store = new FileAutomationProjectStore(root);
            var project = await store.CreateAsync("Delete me", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(project.DirectoryPath, "artifacts", "sample.txt"), "artifact", TestContext.Current.CancellationToken);

            await store.DeleteAsync(project, TestContext.Current.CancellationToken);

            Assert.False(Directory.Exists(project.DirectoryPath));
            Assert.Empty(await store.ListAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RepairAsync_recreates_missing_project_manifest()
    {
        var root = Path.Combine(Path.GetTempPath(), $"nautomate-tests-{Guid.NewGuid():N}");
        try
        {
            var store = new FileAutomationProjectStore(root);
            var project = await store.CreateAsync("Repair me", TestContext.Current.CancellationToken);
            File.Delete(Path.Combine(project.DirectoryPath, "project.json"));

            var broken = (await store.ListAsync(TestContext.Current.CancellationToken)).Single();
            Assert.False(broken.IsValid);
            Assert.Equal(Guid.Empty, broken.Id);

            var repaired = await store.RepairAsync(broken, TestContext.Current.CancellationToken);

            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "project.json")));
            Assert.Equal(project.Id, repaired.Id);
            Assert.False(repaired.IsValid);
            Assert.Contains(repaired.ValidationErrors, error => error.Contains("at least one step", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
