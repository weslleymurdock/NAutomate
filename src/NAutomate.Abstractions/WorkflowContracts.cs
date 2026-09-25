namespace NAutomate.Abstractions;

[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class AutomationServiceAttribute(
    string id,
    string displayName,
    string description,
    string version = "1.0.0") : Attribute
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
    public string Version { get; } = version;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class AutomationOperationAttribute(
    string id,
    string displayName,
    string description,
bool workflowInvocable = true) : Attribute
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
    public bool WorkflowInvocable { get; } = workflowInvocable;
}

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class AutomationParameterAttribute(
    string displayName,
    string? description = null) : Attribute
{
    public string DisplayName { get; } = displayName;
    public string? Description { get; } = description;
}

public sealed record AutomationParameterDescriptor(
    string Name,
    string Type,
    bool Required,
    string? Description = null);

public sealed record AutomationOperationDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string ReturnType,
    IReadOnlyList<AutomationParameterDescriptor> Parameters);

public sealed record AutomationServiceDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string Version,
    IReadOnlyList<AutomationOperationDescriptor> Operations);

/// <summary>A declarative workflow persisted by NAutomate.</summary>
public sealed record AutomationWorkflow(
    int SchemaVersion,
    string Name,
    IReadOnlyList<WorkflowStep> Steps);

/// <summary>A single precompiled module invocation.</summary>
public sealed record WorkflowStep(
    string Id,
    string Module,
    IReadOnlyDictionary<string, object?> Parameters);

public sealed record ModuleParameterDefinition(string Name, string Type, bool Required, string? Description = null);

public sealed record ModuleDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string Version,
    IReadOnlyList<ModuleParameterDefinition> Parameters,
    IReadOnlyList<AutomationServiceDescriptor>? Services = null);

public sealed record ModuleExecutionContext(
    AutomationWorkflow Workflow,
    WorkflowStep Step,
    IReadOnlyDictionary<string, object?> Parameters,
    CancellationToken CancellationToken);

public sealed record ModuleExecutionResult(string Output, bool Succeeded = true);

public interface IAutomationModule
{
    ModuleDescriptor Descriptor { get; }
    Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context);
}

public interface IModuleRegistry
{
    IReadOnlyCollection<ModuleDescriptor> List();
    IAutomationModule Resolve(string moduleId);
}

public interface IExecutionEventSink
{
    ValueTask OnEventAsync(ExecutionEvent executionEvent, CancellationToken cancellationToken = default);
}

public sealed record ExecutionEvent(
    string Kind,
    string? ModuleId = null,
    string? Message = null,
    string? StepId = null);

public sealed record WorkflowExecutionResult(
    WorkflowExecutionStatus Status,
    string? ErrorMessage = null,
    Exception? Exception = null);

public enum WorkflowExecutionStatus
{
    Success,
    Failure,
    Cancelled,
    Exception
}
