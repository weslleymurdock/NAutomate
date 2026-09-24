# UI Development

Both `NAutomate` and `NAutomate.Web` are Blazor-based UI projects.

## MudBlazor

Use MudBlazor components exclusively for application UI controls and layout.

Do not introduce Bootstrap components, another component library, or hand-built replacements when an appropriate MudBlazor component exists.

Keep Razor components focused on presentation and user interaction. Put persistence, CLI process management, and orchestration in injected services.

## First vertical slice

The first desktop UI only needs:

- project name;
- one `core.echo` step;
- message input;
- Save;
- Run;
- execution console.

The execution console should update incrementally as CLI output arrives.

Do not implement drag-and-drop workflow editing in the first vertical slice.
