using System.Reflection;
using System.Text.Json;
using NAutomate.Abstractions;

namespace NAutomate.Modules.Shell.Pwsh;

internal static class PwshShellOperationInvoker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IAutomationModule CreateModule(IPwshShellService service)
    {
        var serviceType = typeof(IPwshShellService);
        var serviceMetadata = serviceType.GetCustomAttribute<AutomationServiceAttribute>()
            ?? throw new InvalidOperationException($"Service '{serviceType.Name}' is missing AutomationServiceAttribute.");

        var method = serviceType.GetMethod(nameof(IPwshShellService.Run))
            ?? throw new InvalidOperationException($"Operation '{nameof(IPwshShellService.Run)}' was not found.");

        var operation = method.GetCustomAttribute<AutomationOperationAttribute>()
            ?? throw new InvalidOperationException($"Operation '{method.Name}' is missing AutomationOperationAttribute.");

        return new ReflectionPwshShellModule(
            $"shell.pwsh.{operation.Id}",
            operation,
            serviceMetadata,
            serviceType,
            service,
            method);
    }

    private sealed class ReflectionPwshShellModule(
        string id,
        AutomationOperationAttribute operation,
        AutomationServiceAttribute serviceMetadata,
        Type serviceType,
        object service,
        MethodInfo method) : IAutomationModule
    {
        public ModuleDescriptor Descriptor { get; } = CreateDescriptor(id, operation, serviceMetadata, serviceType, method);

        public async Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
        {
            if (!operation.WorkflowInvocable)
                throw new InvalidOperationException(
                    $"Operation '{Descriptor.Id}' is metadata-only and cannot be invoked from a workflow.");

            var arguments = BindArguments(method, context.Parameters);
            var result = method.Invoke(service, arguments);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                result = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            return new ModuleExecutionResult(SerializeResult(result));
        }

        private static ModuleDescriptor CreateDescriptor(
            string id,
            AutomationOperationAttribute operation,
            AutomationServiceAttribute serviceMetadata,
            Type serviceType,
            MethodInfo method)
        {
            var parameters = method.GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken))
                .Select(parameter =>
                {
                    var metadata = parameter.GetCustomAttribute<AutomationParameterAttribute>();
                    var optional = parameter.IsOptional || parameter.HasDefaultValue;

                    return new ModuleParameterDefinition(
                        parameter.Name!,
                        parameter.ParameterType.FullName ?? parameter.ParameterType.Name,
                        !optional,
                        metadata?.Description);
                })
                .ToArray();

            var operationDescriptor = new AutomationOperationDescriptor(
                operation.Id,
                operation.DisplayName,
                operation.Description,
                method.ReturnType.FullName ?? method.ReturnType.Name,
                parameters.Select(parameter => new AutomationParameterDescriptor(
                    parameter.Name,
                    parameter.Type,
                    parameter.Required,
                    parameter.Description)).ToArray());

            var serviceDescriptor = new AutomationServiceDescriptor(
                serviceMetadata.Id,
                serviceMetadata.DisplayName,
                serviceMetadata.Description,
                serviceMetadata.Version,
                [operationDescriptor]);

            return new ModuleDescriptor(
                id,
                $"{serviceMetadata.DisplayName}: {operation.DisplayName}",
                operation.Description,
                serviceMetadata.Version,
                parameters,
                [serviceDescriptor]);
        }

        private static object?[] BindArguments(
            MethodInfo method,
            IReadOnlyDictionary<string, object?> parameters)
        {
            return [.. method.GetParameters().Select(parameter =>
            {
                if (parameter.ParameterType == typeof(CancellationToken))
                    return default(CancellationToken);

                if (!parameters.TryGetValue(parameter.Name!, out var raw))
                {
                    if (parameter.IsOptional || parameter.HasDefaultValue)
                        return parameter.DefaultValue;

                    throw new ArgumentException(
                        $"Required parameter '{parameter.Name}' is missing for operation '{method.Name}'.");
                }

                if (raw is null)
                    return null;

                if (parameter.ParameterType.IsInstanceOfType(raw))
                    return raw;

                var json = raw is JsonElement element
                    ? element.GetRawText()
                    : JsonSerializer.Serialize(raw);

                return JsonSerializer.Deserialize(json, parameter.ParameterType, JsonOptions);
            })];
        }

        private static string SerializeResult(object? result) =>
            result is null
                ? string.Empty
                : result is string text
                    ? text
                    : JsonSerializer.Serialize(result, JsonOptions);
    }
}
