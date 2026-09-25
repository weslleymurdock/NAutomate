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
            var project = await store.CreateAsync("My Mobile Automation");

            Assert.True(project.IsValid);
            Assert.Equal($"{project.Id:D}-my-mobile-automation", project.DirectoryName);

            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "automation.json")));
            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "settings.json")));
            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "env.json")));
            Assert.True(File.Exists(Path.Combine(project.DirectoryPath, "project.json")));

            var validation = await store.ValidateAsync(project.Id);
            Assert.True(validation.IsValid);
            Assert.Empty(validation.Errors);
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
            var project = await store.CreateAsync("Integrity");

            await File.AppendAllTextAsync(
                Path.Combine(project.DirectoryPath, "automation.json"),
                Environment.NewLine);

            var validation = await store.ValidateAsync(project.Id);

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
            var project = await store.CreateAsync("Delete me");
            await File.WriteAllTextAsync(Path.Combine(project.DirectoryPath, "artifacts", "sample.txt"), "artifact");

            await store.DeleteAsync(project);

            Assert.False(Directory.Exists(project.DirectoryPath));
            Assert.Empty(await store.ListAsync());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

}
