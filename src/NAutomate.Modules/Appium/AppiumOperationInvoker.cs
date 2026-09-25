using System.Reflection;
using System.Text.Json;
using NAutomate.Abstractions;

namespace NAutomate.Modules.Appium;

internal static class AppiumOperationInvoker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static IReadOnlyList<IAutomationModule> CreateModules(
        IAndroidAppiumService android,
        IIOSAppiumService ios)
    {
        var modules = new List<IAutomationModule>();
        Add(modules, android, typeof(IAndroidAppiumService));
        Add(modules, ios, typeof(IIOSAppiumService));
        return modules;
    }

    private static void Add(
        ICollection<IAutomationModule> modules,
        object service,
        Type serviceType)
    {
        var serviceMetadata = serviceType.GetCustomAttribute<AutomationServiceAttribute>()
            ?? throw new InvalidOperationException($"Service '{serviceType.Name}' is missing AutomationServiceAttribute.");

        foreach (var method in serviceType.GetMethods())
        {
            var operation = method.GetCustomAttribute<AutomationOperationAttribute>();
            if (operation is null)
                continue;

            var operationId = $"appium.{serviceMetadata.Id.Replace("appium.", string.Empty)}.{operation.Id}";
            modules.Add(new ReflectionAppiumModule(
                operationId,
                operation,
                serviceMetadata,
                serviceType,
                service,
                method));
        }
    }

    private sealed class ReflectionAppiumModule(
        string id,
        AutomationOperationAttribute operation,
        AutomationServiceAttribute serviceMetadata,
        Type serviceType,
        object service,
        MethodInfo method) : IAutomationModule
    {
        public ModuleDescriptor Descriptor { get; } = CreateDescriptor(
            id, operation, serviceMetadata, serviceType, method);

        public async Task<ModuleExecutionResult> ExecuteAsync(ModuleExecutionContext context)
        {
            if (!operation.WorkflowInvocable)
                throw new InvalidOperationException(
                    $"Operation '{Descriptor.Id}' is metadata-only and cannot be invoked from a workflow.");

            var arguments = BindArguments(method, context.Parameters, context.CancellationToken);
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
                parameters.Select(x => new AutomationParameterDescriptor(
                    x.Name, x.Type, x.Required, x.Description)).ToArray());

            var properties = serviceType.GetProperties()
                .Select(property => (property, metadata: property.GetCustomAttribute<AutomationOperationAttribute>()))
                .Where(item => item.metadata is not null)
                .Select(item => new AutomationPropertyDescriptor(
                    item.property.Name,
                    item.metadata!.DisplayName,
                    item.metadata.Description,
                    item.property.PropertyType.FullName ?? item.property.PropertyType.Name,
                    item.property.CanRead,
                    item.property.CanWrite))
                .ToArray();

            var serviceDescriptor = new AutomationServiceDescriptor(
                serviceMetadata.Id,
                serviceMetadata.DisplayName,
                serviceMetadata.Description,
                serviceMetadata.Version,
                [operationDescriptor],
                properties);

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
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken cancellationToken)
        {
            return [.. method.GetParameters()
                .Select(parameter =>
                {
                    if (parameter.ParameterType == typeof(CancellationToken))
                        return cancellationToken;

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

                    return JsonSerializer.Deserialize(
                        json,
                        parameter.ParameterType,
                        JsonOptions);
                })];
        }

        private static string SerializeResult(object? result)
        {
            if (result is null)
                return string.Empty;

            if (result is string text)
                return text;

            return JsonSerializer.Serialize(result, JsonOptions);
        }
    }
}
