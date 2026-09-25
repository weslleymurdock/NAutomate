using System.Reflection;
using System.Text.Json;
using NAutomate.Abstractions;

namespace NAutomate.Modules.Selenium;

internal static class SeleniumOperationInvoker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static IReadOnlyList<IAutomationModule> CreateModules(
        IChromeSeleniumService chrome,
        IFirefoxSeleniumService firefox,
        IEdgeSeleniumService edge,
        ISafariSeleniumService safari)
    {
        var modules = new List<IAutomationModule>();
        Add(modules, chrome, typeof(IChromeSeleniumService));
        Add(modules, firefox, typeof(IFirefoxSeleniumService));
        Add(modules, edge, typeof(IEdgeSeleniumService));
        Add(modules, safari, typeof(ISafariSeleniumService));
        return modules;
    }

    private static void Add(ICollection<IAutomationModule> modules, object service, Type serviceType)
    {
        var serviceMetadata = serviceType.GetCustomAttribute<AutomationServiceAttribute>()
            ?? throw new InvalidOperationException($"Service '{serviceType.Name}' is missing AutomationServiceAttribute.");

        foreach (var method in typeof(ISeleniumService).GetMethods())
        {
            var operation = method.GetCustomAttribute<AutomationOperationAttribute>();
            if (operation is null)
                continue;

            var operationId = $"{serviceMetadata.Id}.{operation.Id}";
            modules.Add(new ReflectionSeleniumModule(
                operationId,
                operation,
                serviceMetadata,
                serviceType,
                service,
                method));
        }
    }

    private sealed class ReflectionSeleniumModule(
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
                .Select(parameter => new ModuleParameterDefinition(
                    parameter.Name!,
                    parameter.ParameterType.FullName ?? parameter.ParameterType.Name,
                    !parameter.IsOptional && !parameter.HasDefaultValue,
                    parameter.GetCustomAttribute<AutomationParameterAttribute>()?.Description))
                .ToArray();

            var operationDescriptor = new AutomationOperationDescriptor(
                operation.Id,
                operation.DisplayName,
                operation.Description,
                method.ReturnType.FullName ?? method.ReturnType.Name,
                parameters.Select(x => new AutomationParameterDescriptor(
                    x.Name, x.Type, x.Required, x.Description)).ToArray());

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
            IReadOnlyDictionary<string, object?> parameters,
            CancellationToken cancellationToken)
        {
            return [.. method.GetParameters().Select(parameter =>
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

                return JsonSerializer.Deserialize(json, parameter.ParameterType, JsonOptions);
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
