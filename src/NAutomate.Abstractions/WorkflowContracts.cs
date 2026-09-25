namespace NAutomate.Abstractions;

public enum WorkflowStepKind
{
    Module,
    If,
    For,
    Foreach,
    While,
    Set,
    Exit
}

public enum WorkflowConditionOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull
}

public enum WorkflowSetOperation
{
    Set,
    Increment,
    Decrement
}

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

public sealed record AutomationPropertyDescriptor(
    string Name,
    string DisplayName,
    string Description,
    string Type,
    bool Readable,
    bool Writable);

public sealed record AutomationServiceDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string Version,
    IReadOnlyList<AutomationOperationDescriptor> Operations,
    IReadOnlyList<AutomationPropertyDescriptor>? Properties = null);

/// <summary>A declarative workflow persisted by NAutomate.</summary>
public sealed record AutomationWorkflow(
    int SchemaVersion,
    string Name,
    IReadOnlyList<WorkflowStep> Steps,
    IReadOnlyList<AutomationVariableDefinition>? Variables = null)
{
    
}


/// <summary>Base type for executable declarative workflow nodes.</summary>
public record WorkflowStep
{
    protected WorkflowStep(
        string id,
        WorkflowStepKind kind,
        string? module,
        IReadOnlyDictionary<string, object?>? parameters)
    {
        Id = id;
        Kind = kind;
        Module = module;
        Parameters = parameters ?? new Dictionary<string, object?>();
    }

    /// <summary>Creates the backwards-compatible module-step representation.</summary>
    public WorkflowStep(
        string id,
        string module,
        IReadOnlyDictionary<string, object?> parameters)
        : this(id, WorkflowStepKind.Module, module, parameters)
    {
    }

    public string Id { get; init; }
    public WorkflowStepKind Kind { get; init; }
    public string? Module { get; init; }
    public IReadOnlyDictionary<string, object?> Parameters { get; init; }
}

/// <summary>A precompiled module invocation.</summary>
public sealed record ModuleStep(
    string Id,
    string ModuleId,
    IReadOnlyDictionary<string, object?> ModuleParameters)
    : WorkflowStep(Id, WorkflowStepKind.Module, ModuleId, ModuleParameters);

/// <summary>A conditional branch with optional else steps.</summary>
public sealed record IfStep(
    string Id,
    WorkflowCondition Condition,
    IReadOnlyList<WorkflowStep> Then,
    IReadOnlyList<WorkflowStep>? Else = null)
    : WorkflowStep(Id, WorkflowStepKind.If, null, null);

/// <summary>A numeric loop over an integer range.</summary>
public sealed record ForStep(
    string Id,
    string Variable,
    long From,
    long To,
    long Step = 1,
    bool Inclusive = false,
    IReadOnlyList<WorkflowStep>? Steps = null)
    : WorkflowStep(Id, WorkflowStepKind.For, null, null)
{
    public IReadOnlyList<WorkflowStep> Body { get; init; } = Steps ?? [];
}

/// <summary>A loop over the value of a collection variable.</summary>
public sealed record ForeachStep(
    string Id,
    string Collection,
    string ItemVariable,
    IReadOnlyList<WorkflowStep>? Steps = null)
    : WorkflowStep(Id, WorkflowStepKind.Foreach, null, null)
{
    public IReadOnlyList<WorkflowStep> Body { get; init; } = Steps ?? [];
}

/// <summary>A condition-controlled loop.</summary>
public sealed record WhileStep(
    string Id,
    WorkflowCondition Condition,
    IReadOnlyList<WorkflowStep>? Steps = null,
    int? MaxIterations = null)
    : WorkflowStep(Id, WorkflowStepKind.While, null, null)
{
    public IReadOnlyList<WorkflowStep> Body { get; init; } = Steps ?? [];
}

/// <summary>A declarative variable assignment or numeric update.</summary>
public sealed record SetStep(
    string Id,
    string Variable,
    object? Value = null,
    WorkflowSetOperation Operation = WorkflowSetOperation.Set)
    : WorkflowStep(Id, WorkflowStepKind.Set, null, null);

/// <summary>Terminates the current workflow execution with an explicit exit code.</summary>
public sealed record ExitStep(
    string Id,
    int ExitCode = 0)
    : WorkflowStep(Id, WorkflowStepKind.Exit, null, null);

/// <summary>A typed comparison against a workflow variable.</summary>
public sealed record WorkflowCondition(
    string Variable,
    WorkflowConditionOperator Operator,
    object? Value = null);

public sealed record ModuleParameterDefinition(string Name, string Type, bool Required, string? Description = null);

public sealed record ModuleDescriptor(
    string Id,
    string DisplayName,
    string Description,
    string Version,
    IReadOnlyList<ModuleParameterDefinition> Parameters,
    IReadOnlyList<AutomationServiceDescriptor>? Services = null);

/// <summary>Mutable variable state shared by all steps in one workflow execution.</summary>
public interface IWorkflowVariableStore
{
    IReadOnlyCollection<string> Names { get; }
    bool Contains(string name);
    object? Get(string name);
    bool TryGet(string name, out object? value);
    void Set(string name, object? value);
    bool Remove(string name);
}

/// <summary>Context supplied to a module during one workflow execution.</summary>
public sealed record ModuleExecutionContext(
    AutomationWorkflow Workflow,
    WorkflowStep Step,
    IReadOnlyDictionary<string, object?> Parameters,
    CancellationToken CancellationToken,
    AutomationEnvironment? Environment = null,
    IWorkflowVariableStore? Variables = null,
    Projects.AutomationProjectSettings? Settings = null);

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
