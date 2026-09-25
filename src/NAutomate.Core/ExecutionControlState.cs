namespace NAutomate.Core;

/// <summary>Represents the current debugger execution mode.</summary>
public enum ExecutionControlState
{
    Paused,
    Running,
    StepInto,
    StepOver,
    Completed,
    Stopped
}
