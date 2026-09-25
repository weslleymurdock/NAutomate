using NAutomate.Abstractions;
using NAutomate.Core;

return await CliRunner.RunAsync(args, Console.Out, Console.Error);

public static class CliRunner
{
    public static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, CancellationToken cancellationToken = default)
    {
        if (args.Length != 2 || !string.Equals(args[0], "run", StringComparison.OrdinalIgnoreCase))
        {
            await error.WriteLineAsync("Usage: nautomate run <automation.json>");
            return 2;
        }
        try
        {
            var workflow = await new FileWorkflowStore().LoadAsync(args[1], cancellationToken);
            await output.WriteLineAsync("NAutomate");
            await new WorkflowEngine(new ModuleRegistry()).ExecuteAsync(workflow, new ConsoleEventSink(output), cancellationToken);
            return 0;
        }
        catch (OperationCanceledException) { await error.WriteLineAsync("Execution cancelled."); return 1; }
        catch (Exception exception)
        { await error.WriteLineAsync($"Error: {exception.Message}"); return 1; }
    }

    private sealed class ConsoleEventSink(TextWriter output) : IExecutionEventSink
    {
        public async ValueTask OnEventAsync(ExecutionEvent item, CancellationToken cancellationToken = default)
        {
            var line = item.Kind switch
            {
                "workflow" => item.Message,
                "running" => $"[RUNNING] {item.ModuleId}",
                "output" => $"[OUTPUT] {item.Message}",
                "success" => $"[SUCCESS] {item.ModuleId}",
                "completed" => item.Message,
                _ => item.Message
            };
            if (!string.IsNullOrWhiteSpace(line)) await output.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }
}
