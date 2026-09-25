namespace NAutomate.Core;

/// <summary>Signals an explicit workflow termination requested by an Exit step.</summary>
internal sealed class WorkflowExitException(int exitCode) : Exception
{
    public int ExitCode { get; } = exitCode;
}
