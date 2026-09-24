# Architecture Skill

Use this skill when adding or changing projects, dependencies, public contracts, runtime boundaries, or domain structure.

## Rules

- Preserve the dependency direction documented in `docs/architecture.md`.
- Keep NAutomate.Abstractions dependency-light and UI-agnostic.
- Keep Core independent of MAUI, Blazor, MudBlazor, and process execution.
- Treat workflow JSON as declarative data.
- Prefer the smallest abstraction that establishes a stable boundary.
- Do not introduce databases, brokers, remote execution, or arbitrary-code execution for the first milestone.
- Update architecture documentation when a boundary changes.
