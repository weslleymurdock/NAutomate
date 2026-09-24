namespace NAutomate.Abstractions;

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

/// <summary>Describes a parameter accepted by a module.</summary>
public sealed record ModuleParameterDefinition(string Name, string Type, bool Required, string? Description = null);

/// <summary>Discoverable metadata for a module.</summary>
public sealed record ModuleDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string Version,
    IReadOnlyList<ModuleParameterDefinition> Parameters);

/// <summary>Input supplied to a module during execution.</summary>
public sealed record ModuleExecutionContext(
    AutomationWorkflow Workflow,
    WorkflowStep Step,
    CancellationToken CancellationToken);

/// <summary>Structured output from a module.</summary>
public sealed record ModuleExecutionResult(string Output, bool Succeeded = true);

/// <summary>Precompiled automation module selected by a stable module ID.</summary>
public interface IAutomationModule
{
    ModuleDescriptor Descriptor { get; }

    Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context);
}

/// <summary>Resolves precompiled modules by stable identifier.</summary>
public interface IModuleRegistry
{
    IReadOnlyCollection<ModuleDescriptor> List();
    IAutomationModule Resolve(string moduleId);
}

/// <summary>Receives line-oriented execution events.</summary>
public interface IExecutionEventSink
{
    ValueTask OnEventAsync(ExecutionEvent executionEvent, CancellationToken cancellationToken = default);
}

/// <summary>An execution status or module output event.</summary>
public sealed record ExecutionEvent(string Kind, string? ModuleId = null, string? Message = null);
