# UI Skill

Use this skill for UI work in `NAutomate` or `NAutomate.Web`.

## Required framework

Use MudBlazor components for application UI and layout.

Do not use Bootstrap components, another component library, or raw HTML/CSS as a replacement for an available MudBlazor component.

## Component design

- Keep Razor components focused on presentation and interaction.
- Inject application services instead of putting persistence or process orchestration in components.
- Use async event handlers.
- Represent loading/running/success/failure/cancellation explicitly.
- Keep user-facing execution output incremental.
- Reuse shared workflow/module models rather than creating UI-specific duplicate models.

## First milestone

Implement only the simple project editor and execution console needed by the first vertical slice. Do not build drag-and-drop editing yet.
