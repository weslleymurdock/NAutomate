using NAutomate.Abstractions;
using NAutomate.Parser;

namespace NAutomate.Core;

public sealed class FileExecutionEnvironmentStore : IExecutionEnvironmentStore
{
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileExecutionEnvironmentStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NAutomate",
            "environments.json");
    }

    public async Task<IReadOnlyList<AutomationEnvironment>> ListAsync(
        string workflowName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowName);
        await _gate.WaitAsync(cancellationToken);

        try
        {
            return (await ReadAsync(cancellationToken))
                .Where(x => string.Equals(x.WorkflowName, workflowName, StringComparison.Ordinal))
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<AutomationEnvironment> CreateAsync(
        string name,
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var environments = await ReadAsync(cancellationToken);
            if (environments.Any(x => string.Equals(x.WorkflowName, workflow.Name, StringComparison.Ordinal) && string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"An execution environment named '{name}' already exists.");

            var environment = new AutomationEnvironment(
                Guid.NewGuid().ToString("N"),
                workflow.Name,
                name.Trim(),
                AutomationWorkflowVariables.ToPersistedValues(AutomationWorkflowVariables.CreateValues(workflow)));

            environments = [.. environments, environment];
            await WriteAsync(environments, cancellationToken);
            return environment;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<AutomationEnvironment> SaveAsync(
        AutomationEnvironment environment,
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(environment);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var environments = await ReadAsync(cancellationToken);
            var index = environments.FindIndex(x =>
                x.Id == environment.Id &&
                string.Equals(x.WorkflowName, workflow.Name, StringComparison.Ordinal));
            if (index < 0)
                throw new KeyNotFoundException($"Execution environment '{environment.Id}' was not found.");

            var values = AutomationWorkflowVariables.CreateValues(
                workflow,
                environment.Values.ToDictionary(x => x.Key, x => (object?)x.Value, StringComparer.Ordinal));
            var saved = environment with { Values = AutomationWorkflowVariables.ToPersistedValues(values) };
            environments[index] = saved;
            await WriteAsync(environments, cancellationToken);
            return saved;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var environments = await ReadAsync(cancellationToken);
            environments.RemoveAll(x => x.Id == id);
            await WriteAsync(environments, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<AutomationEnvironment>> SynchronizeAsync(
        AutomationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var environments = await ReadAsync(cancellationToken);
            var synchronized = environments
                .Select(environment => string.Equals(environment.WorkflowName, workflow.Name, StringComparison.Ordinal)
                    ? environment with
                    {
                        Values = AutomationWorkflowVariables.ToPersistedValues(AutomationWorkflowVariables.CreateValues(workflow, environment.Values.ToDictionary(x => x.Key, x => (object?)x.Value)))
                    }
                    : environment)
                .ToList();

            if (synchronized.Any())
                await WriteAsync(synchronized, cancellationToken);

            return synchronized
                .Where(x => string.Equals(x.WorkflowName, workflow.Name, StringComparison.Ordinal))
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<AutomationEnvironment>> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
            return [];

        var json = await File.ReadAllTextAsync(_path, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return [.. ExecutionEnvironmentJson.Deserialize(json)];
    }

    private async Task WriteAsync(
        IReadOnlyList<AutomationEnvironment> environments,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = ExecutionEnvironmentJson.Serialize(environments);
        await File.WriteAllTextAsync(_path, json, cancellationToken);
    }

}
