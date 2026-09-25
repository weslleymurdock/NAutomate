using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using NAutomate.Abstractions;
using NAutomate.Core;
using NAutomate.Modules;
using NAutomate.Modules.Appium;
using NAutomate.Components;

namespace NAutomate;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});
		builder.Services.AddMudServices();
		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<IModuleRegistry>(_ => new ModuleRegistry(OfficialModules.GetModules()));
		builder.Services.AddSingleton<WorkflowEngine>();
		builder.Services.AddSingleton<FileWorkflowStore>();
		builder.Services.AddSingleton<IExecutionEnvironmentStore, FileExecutionEnvironmentStore>();
		builder.Services.AddSingleton<IExecutionEnvironmentService, ExecutionEnvironmentService>();
		builder.Services.AddSingleton<IModuleSettingsTabProvider, AppiumSettingsTabProvider>();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
