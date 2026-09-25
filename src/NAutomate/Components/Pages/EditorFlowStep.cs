using NAutomate.Abstractions;

namespace NAutomate.Components.Pages;

public sealed class EditorFlowStep
{
    public EditorFlowStep(
        string id,
        string title,
        WorkflowStepKind kind,
        string icon,
        string description,
        string? moduleId = null,
        IReadOnlyList<AutomationParameterDescriptor>? parameters = null)
    {
        Id = id;
        Title = title;
        Kind = kind;
        Icon = icon;
        Description = description;
        ModuleId = moduleId;
        Parameters = parameters?.ToList() ?? [];
    }

    public string Id { get; }
    public string Title { get; set; }
    public WorkflowStepKind Kind { get; }
    public string Icon { get; }
    public string Description { get; }
    public string? ModuleId { get; }
    public List<AutomationParameterDescriptor> Parameters { get; }
    public Dictionary<string, string?> Values { get; } = new(StringComparer.Ordinal);
    public List<EditorFlowStep> Children { get; } = [];
    public List<EditorFlowStep> ElseChildren { get; } = [];
    public int Sequence { get; set; }

    public bool IsContainer => Kind is WorkflowStepKind.If or WorkflowStepKind.For or WorkflowStepKind.Foreach or WorkflowStepKind.While;
    public bool HasElse => Kind == WorkflowStepKind.If;
    public string BranchLabel => Kind switch
    {
        WorkflowStepKind.If => "IF",
        WorkflowStepKind.For => "FOR",
        WorkflowStepKind.Foreach => "FOREACH",
        WorkflowStepKind.While => "WHILE",
        _ => Kind.ToString().ToUpperInvariant()
    };
}
