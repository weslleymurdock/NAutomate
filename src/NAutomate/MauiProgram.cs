using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using NAutomate.Abstractions;
using NAutomate.Core;
using NAutomate.Modules;
using NAutomate.Modules.Appium;
using NAutomate.UI;
using NAutomate.Core.Projects;
using Microsoft.Maui.Storage;

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
		builder.Services.AddMudServices(configuration =>
		{
			configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
		});
		builder.Services.AddMauiBlazorWebView();

		var projectsDirectory = Path.Combine(
			FileSystem.AppDataDirectory,
			"NAutomate",
			"Projects");

		builder.Services.AddSingleton<IAutomationProjectStore>(
			_ => new FileAutomationProjectStore(projectsDirectory));

		builder.Services.AddNAutomateUI();
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
