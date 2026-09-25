using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NAutomate.Abstractions;
using NAutomate.Abstractions.Projects;
using NAutomate.Parser;

namespace NAutomate.Core.Projects;

public sealed class FileAutomationProjectStore(string projectsDirectory) : IAutomationProjectStore
{
    private const string AutomationFileName = "automation.json";
    private const string SettingsFileName = "settings.json";
    private const string EnvironmentFileName = "env.json";
    private const string ProjectFileName = "project.json";
    private const string ArtifactsDirectoryName = "artifacts";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string ProjectsDirectory { get; } =
        Path.GetFullPath(projectsDirectory ?? throw new ArgumentNullException(nameof(projectsDirectory)));

    public async Task<IReadOnlyList<AutomationProjectInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(ProjectsDirectory);

        var projects = new List<AutomationProjectInfo>();
        foreach (var directory in Directory.EnumerateDirectories(ProjectsDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var manifestPath = Path.Combine(directory, ProjectFileName);

            try
            {
                if (!File.Exists(manifestPath))
                {
                    projects.Add(new AutomationProjectInfo(
                        Guid.Empty,
                        Path.GetFileName(directory),
                        Path.GetFileName(directory),
                        directory,
                        DateTimeOffset.MinValue,
                        DateTimeOffset.MinValue,
                        false,
                        [$"Missing required file '{ProjectFileName}'."]));
                    continue;
                }

                var manifest = await ReadManifestAsync(manifestPath, cancellationToken);
                var validation = await ValidateDirectoryAsync(directory, manifest, cancellationToken);
                projects.Add(ToInfo(manifest, directory, validation));
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                projects.Add(new AutomationProjectInfo(
                    Guid.Empty,
                    Path.GetFileName(directory),
                    Path.GetFileName(directory),
                    directory,
                    DateTimeOffset.MinValue,
                    DateTimeOffset.MinValue,
                    false,
                    [$"Unable to read project metadata: {ex.Message}"]));
            }
        }

        return projects.OrderByDescending(x => x.UpdatedUtc).ToArray();
    }

    public async Task<AutomationProjectInfo> CreateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        Directory.CreateDirectory(ProjectsDirectory);

        var id = Guid.NewGuid();
        var directoryName = $"{id:D}-{Slugify(name)}";
        var directory = Path.Combine(ProjectsDirectory, directoryName);
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(Path.Combine(directory, ArtifactsDirectoryName));

        var now = DateTimeOffset.UtcNow;
        var state = new AutomationProjectState
        {
            Info = new AutomationProjectInfo(id, name.Trim(), directoryName, directory, now, now, false, []),
            Workflow = new AutomationWorkflow(WorkflowJson.CurrentSchemaVersion, name.Trim(), [], []),
            Settings = new AutomationProjectSettings(),
            Environment = new AutomationEnvironmentFile()
        };

        await SaveAsync(state, cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public async Task<AutomationProjectInfo> GetAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var directory = FindProjectDirectory(projectId);
        var manifest = await ReadManifestAsync(Path.Combine(directory, ProjectFileName), cancellationToken);
        var validation = await ValidateDirectoryAsync(directory, manifest, cancellationToken);
        return ToInfo(manifest, directory, validation);
    }

    public async Task<AutomationProjectState> LoadAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var directory = FindProjectDirectory(projectId);
        var manifest = await ReadManifestAsync(Path.Combine(directory, ProjectFileName), cancellationToken);
        var validation = await ValidateDirectoryAsync(directory, manifest, cancellationToken);
        if (!validation.IsValid)
            throw new InvalidDataException(
                $"Project '{manifest.Name}' is not valid: {string.Join(" ", validation.Errors)}");

        var workflow = WorkflowJson.Deserialize(
            await File.ReadAllTextAsync(Path.Combine(directory, AutomationFileName), cancellationToken));

        var settings = JsonSerializer.Deserialize<AutomationProjectSettings>(
            await File.ReadAllTextAsync(Path.Combine(directory, SettingsFileName), cancellationToken),
            JsonOptions) ?? new();

        var environment = JsonSerializer.Deserialize<AutomationEnvironmentFile>(
            await File.ReadAllTextAsync(Path.Combine(directory, EnvironmentFileName), cancellationToken),
            JsonOptions) ?? new();

        return new AutomationProjectState
        {
            Info = ToInfo(manifest, directory, validation),
            Workflow = workflow,
            Settings = settings,
            Environment = environment
        };
    }

    public async Task SaveAsync(
        AutomationProjectState project,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        var directory = project.Info.DirectoryPath;
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(Path.Combine(directory, ArtifactsDirectoryName));

        var automationJson = WorkflowJson.Serialize(project.Workflow);
        var settingsJson = JsonSerializer.Serialize(project.Settings, JsonOptions);
        var environmentJson = JsonSerializer.Serialize(project.Environment, JsonOptions);

        var manifest = new AutomationProjectManifest
        {
            SchemaVersion = 1,
            Id = project.Info.Id,
            Name = project.Info.Name,
            DirectoryName = project.Info.DirectoryName,
            CreatedUtc = project.Info.CreatedUtc,
            UpdatedUtc = DateTimeOffset.UtcNow,
            Hashes = new AutomationProjectHashes
            {
                Automation = ComputeHash(automationJson),
                Settings = ComputeHash(settingsJson),
                Environment = ComputeHash(environmentJson)
            }
        };

        manifest.Hashes.Project = ComputeManifestHash(manifest);
        var projectJson = JsonSerializer.Serialize(manifest, JsonOptions);

        await WriteAtomicAsync(Path.Combine(directory, AutomationFileName), automationJson, cancellationToken);
        await WriteAtomicAsync(Path.Combine(directory, SettingsFileName), settingsJson, cancellationToken);
        await WriteAtomicAsync(Path.Combine(directory, EnvironmentFileName), environmentJson, cancellationToken);
        await WriteAtomicAsync(Path.Combine(directory, ProjectFileName), projectJson, cancellationToken);
    }

    public async Task<AutomationProjectValidationResult> ValidateAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var directory = FindProjectDirectory(projectId);
        var manifest = await ReadManifestAsync(Path.Combine(directory, ProjectFileName), cancellationToken);
        return await ValidateDirectoryAsync(directory, manifest, cancellationToken);
    }

    private async Task<AutomationProjectValidationResult> ValidateDirectoryAsync(
        string directory,
        AutomationProjectManifest manifest,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (manifest.SchemaVersion != 1)
            errors.Add("Unsupported project.json schema version.");

        if (manifest.Id == Guid.Empty)
            errors.Add("project.json contains an empty project id.");

        if (string.IsNullOrWhiteSpace(manifest.Name))
            errors.Add("project.json contains an empty project name.");

        foreach (var fileName in new[] { AutomationFileName, SettingsFileName, EnvironmentFileName, ProjectFileName })
        {
            if (!File.Exists(Path.Combine(directory, fileName)))
                errors.Add($"Missing required file '{fileName}'.");
        }

        if (errors.Count > 0)
            return new(false, errors);

        try
        {
            var automationJson = await File.ReadAllTextAsync(Path.Combine(directory, AutomationFileName), cancellationToken);
            var workflow = WorkflowJson.Deserialize(automationJson);
            WorkflowJson.Validate(workflow);

            var settingsJson = await File.ReadAllTextAsync(Path.Combine(directory, SettingsFileName), cancellationToken);
            var settings = JsonSerializer.Deserialize<AutomationProjectSettings>(settingsJson, JsonOptions);
            if (settings is null || settings.SchemaVersion != 1)
                errors.Add("settings.json is invalid.");

            var environmentJson = await File.ReadAllTextAsync(Path.Combine(directory, EnvironmentFileName), cancellationToken);
            var environment = JsonSerializer.Deserialize<AutomationEnvironmentFile>(environmentJson, JsonOptions);
            if (environment is null || environment.SchemaVersion != 1)
            {
                errors.Add("env.json is invalid.");
            }
            else
            {
                var variables = (workflow.Variables ?? [])
                    .Where(x => x.Scope == AutomationVariableScope.Global)
                    .ToDictionary(x => x.Name, StringComparer.Ordinal);

                foreach (var environmentEntry in environment.Environments)
                {
                    if (string.IsNullOrWhiteSpace(environmentEntry.Value.Name))
                        errors.Add($"Environment '{environmentEntry.Key}' has no name.");

                    foreach (var value in environmentEntry.Value.Values)
                    {
                        if (!variables.TryGetValue(value.Key, out var variable))
                        {
                            errors.Add($"Environment '{environmentEntry.Key}' contains unknown variable '{value.Key}'.");
                            continue;
                        }

                        if (value.Value.ValueKind == JsonValueKind.Null)
                            continue;

                        try
                        {
                            WorkflowParameterParser.Parse(value.Value.ToString(), variable.Type);
                        }
                        catch (Exception ex) when (ex is FormatException or InvalidDataException or ArgumentException)
                        {
                            errors.Add(
                                $"Environment '{environmentEntry.Key}' variable '{value.Key}' does not match type '{variable.Type}': {ex.Message}");
                        }
                    }
                }
            }

            if (!string.Equals(manifest.Hashes.Automation, ComputeHash(automationJson), StringComparison.OrdinalIgnoreCase))
                errors.Add("automation.json hash does not match project.json.");

            if (!string.Equals(manifest.Hashes.Settings, ComputeHash(settingsJson), StringComparison.OrdinalIgnoreCase))
                errors.Add("settings.json hash does not match project.json.");

            if (!string.Equals(manifest.Hashes.Environment, ComputeHash(environmentJson), StringComparison.OrdinalIgnoreCase))
                errors.Add("env.json hash does not match project.json.");

            if (!string.Equals(manifest.Hashes.Project, ComputeManifestHash(manifest), StringComparison.OrdinalIgnoreCase))
                errors.Add("project.json integrity hash does not match its canonical content.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or IOException)
        {
            errors.Add($"Project content is invalid: {ex.Message}");
        }

        return new(errors.Count == 0, errors);
    }

    private string FindProjectDirectory(Guid projectId)
    {
        Directory.CreateDirectory(ProjectsDirectory);
        var directory = Directory
            .EnumerateDirectories(ProjectsDirectory)
            .FirstOrDefault(x => Path.GetFileName(x)
                .StartsWith($"{projectId:D}-", StringComparison.OrdinalIgnoreCase));

        return directory
            ?? throw new DirectoryNotFoundException($"Project '{projectId:D}' was not found.");
    }

    private static AutomationProjectInfo ToInfo(
        AutomationProjectManifest manifest,
        string directory,
        AutomationProjectValidationResult validation) =>
        new(
            manifest.Id,
            manifest.Name,
            manifest.DirectoryName,
            directory,
            manifest.CreatedUtc,
            manifest.UpdatedUtc,
            validation.IsValid,
            validation.Errors);

    private static async Task<AutomationProjectManifest> ReadManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<AutomationProjectManifest>(json, JsonOptions)
            ?? throw new InvalidDataException("project.json is empty.");
    }

    private static string ComputeManifestHash(AutomationProjectManifest manifest)
    {
        var original = manifest.Hashes.Project;
        manifest.Hashes.Project = string.Empty;
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        manifest.Hashes.Project = original;
        return ComputeHash(json);
    }

    private static string ComputeHash(string content) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();

    private static async Task WriteAtomicAsync(
        string path,
        string content,
        CancellationToken cancellationToken)
    {
        var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(temporary, content, Encoding.UTF8, cancellationToken);
        File.Move(temporary, path, true);
    }

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(character))
                builder.Append(character);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.Length == 0 ? "project" : builder.ToString().Trim('-');
    }
}
