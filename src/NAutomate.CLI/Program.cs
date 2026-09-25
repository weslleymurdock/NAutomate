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
            var result = await new WorkflowEngine(new ModuleRegistry()).ExecuteAsync(workflow, new ConsoleEventSink(output), cancellationToken);
            
            if (result.Status != WorkflowExecutionStatus.Success && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                await error.WriteLineAsync($"Error: {result.ErrorMessage}");
            }
            
            return result.Status switch
            {
                WorkflowExecutionStatus.Success => 0,
                WorkflowExecutionStatus.Cancelled => 1,
                _ => 1
            };
        }
        catch (Exception exception)
        { 
            await error.WriteLineAsync($"Error: {exception.Message}"); 
            return 1; 
        }
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
                "failure" => $"[FAILURE] {item.ModuleId}: {item.Message}",
                "exception" => $"[EXCEPTION] {item.Message}",
                "cancelled" => $"[CANCELLED] {item.Message}",
                "completed" => item.Message,
                _ => item.Message
            };
            if (!string.IsNullOrWhiteSpace(line)) await output.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }
}
