using NAutomate.Abstractions;

namespace NAutomate.Tests.Sinks;

internal sealed class RecordingSink : IExecutionEventSink
{
    public List<ExecutionEvent> Events { get; } = [];

    public ValueTask OnEventAsync(ExecutionEvent executionEvent, CancellationToken cancellationToken = default)
    {
        Events.Add(executionEvent);
        return ValueTask.CompletedTask;
    }
}
