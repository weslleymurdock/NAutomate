using MudBlazor.Services;
using NAutomate.Abstractions;
using NAutomate.Core;
using NAutomate.Modules;
using NAutomate.Modules.Appium;
using NAutomate.UI;
using NAutomate.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
});

builder.Services.AddNAutomateUI(dependencies =>
{
    dependencies.Add(new SoftwareDependencyDescriptor(
        "powershell",
        "PowerShell 7",
        "pwsh",
        ["--version"],
        true,
        "Install PowerShell 7 or configure the shell.pwsh module to use an available host runtime."));
});

builder.Services.AddSingleton<IModuleRegistry>(_ => new ModuleRegistry(OfficialModules.GetModules()));
builder.Services.AddSingleton<WorkflowEngine>();
builder.Services.AddSingleton<FileWorkflowStore>();
builder.Services.AddSingleton<IExecutionEnvironmentStore, FileExecutionEnvironmentStore>();
builder.Services.AddSingleton<IExecutionEnvironmentService, ExecutionEnvironmentService>();
builder.Services.AddSingleton<IModuleSettingsTabProvider, AppiumSettingsTabProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(NAutomate.UI.Components.Pages.Editor).Assembly);

app.Run();
