# Shared UI

NAutomate.UI is a Razor Class Library that contains the reusable Blazor UI for NAutomate.

## Consumers

The repository contains the .NET MAUI Blazor Hybrid host at `src/NAutomate` and the Web host at `src/NAutomate.Web`. Both reference the shared UI library.

Both hosts should reference:

    src/NAutomate.UI/NAutomate.UI.csproj

The host remains responsible for platform-specific services and DI registrations. The UI library must not reference MAUI APIs.

## Routable pages

The shared Routes component is:

    NAutomate.UI.Components.Routes

A host router should discover the RCL assembly. For the Web host this is done through the Razor component endpoint mapping. A MAUI Blazor Hybrid host should use the shared Routes component as its BlazorWebView root component.

## Software dependencies

Register dependencies required by the host or its enabled modules through SoftwareDependencyCatalog.

Example:

    builder.Services.AddNAutomateUI(dependencies =>
    {
        dependencies.Add(new SoftwareDependencyDescriptor(
            "powershell",
            "PowerShell 7",
            "pwsh",
            ["--version"],
            true,
            "Install PowerShell 7 or configure the shell module."));
    });

The startup validator checks only registered dependencies. Therefore Node.js, npm, Docker, PowerShell, Appium, or other tools should be registered only when the corresponding feature/module actually requires them.

Missing required dependencies are exposed through DependencyStatusStore and shown by StartupDependencyNotifications using the host's MudSnackbarProvider. Hosts should configure the snackbar position to the top-right.

## Workflow editor

The workflow editor is composed of:

- Editor.razor
- EditorFlowStep.cs
- EditorFlowStepNode.razor

Palette items are draggable. Every executable branch exposes an explicit drop target, including the workflow root, IF THEN, IF ELSE, FOR BODY, FOREACH BODY and WHILE BODY. The destination is therefore selected by the physical drop location instead of an implicit "selected container" state. Drop targets are also available between sibling steps, so a dragged step can be inserted at an exact position without changing the surrounding editor layout.

The workflow canvas is the scrolling region. Environment selection and global variables are kept in a dialog so they do not consume the editor's permanent vertical space.
