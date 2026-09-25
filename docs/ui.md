# UI Development

The shared application UI lives in `NAutomate.UI` and is consumed by both the .NET MAUI Blazor Hybrid host (`src/NAutomate`) and the Web host (`src/NAutomate.Web`).

## MudBlazor

Use MudBlazor components exclusively for application UI controls and layout.

Do not introduce Bootstrap components, another component library, or hand-built replacements when an appropriate MudBlazor component exists.

Keep Razor components focused on presentation and user interaction. Put persistence, runtime orchestration, and host/platform concerns in injected services.

## Projects

The MAUI home page loads persisted projects through `IAutomationProjectStore`. Project data is stored under the MAUI application data directory and each project must contain valid `automation.json`, `settings.json`, `env.json`, and `project.json` files before execution is allowed.

## Workflow editor

The existing editor layout is preserved. Adding steps uses drag-and-drop from the palette. The workflow root and every executable branch expose insertion targets, including positions between sibling steps. The palette supports HTML drag-and-drop and a pointer-event fallback for touch/MAUI WebView input, so the destination is selected by the physical drop position rather than an implicit selected container.

The workflow canvas owns the scrolling as the automation grows. Environment selection and variables remain in the existing dialog rather than permanently occupying editor space.
