# Architecture

NAutomate is split into stable contracts, reusable parsing/application services, runtime execution, modules, and presentation hosts.

## Projects

- **NAutomate.Abstractions** — public contracts and declarative workflow/environment models. It must remain free of UI, runtime, browser, shell, and persistence dependencies.
- **NAutomate.Parser** — reusable parsing and normalization layer. It owns workflow JSON serialization/validation, environment-variable placeholder resolution, typed operation-parameter parsing, environment JSON serialization, workflow-variable value synchronization, and module-descriptor string parsing. It is independent of the UI and runtime engine so Web and future MAUI hosts can consume the same behavior.
- **NAutomate.Core** — runtime execution and persistence/application services. It executes workflows, manages debugger sessions, and persists workflows/environments. It consumes NAutomate.Parser; it does not own workflow/environment parsing.
- **NAutomate.Modules** — official precompiled automation modules.
- **NAutomate.Modules.Shell** — shell module implementations.
- **NAutomate.CLI** — command-line composition and execution host.
- **NAutomate.Web** — Blazor/MudBlazor presentation host. It owns presentation state and rendering only.
- **NAutomate** — future MAUI presentation host. It can consume the same Parser/Core contracts without duplicating parsing rules.

## Dependency direction

```text
NAutomate.Modules ───────────────┐
NAutomate.Modules.Shell ─────────┤
                                 ▼
                         NAutomate.Abstractions
                                 ▲
                                 │
                         NAutomate.Parser
                                 ▲
                                 │
                         NAutomate.Core
                           ▲           ▲
                           │           │
                     NAutomate.CLI   NAutomate.Web
                           │
                     NAutomate (MAUI)
```

The important boundary is that presentation projects do not parse workflow formats, operation values, variable placeholders, or persisted environments. They collect user input and call reusable application/parser services.

## Environment lifecycle

IExecutionEnvironmentService is the application-facing contract for listing, creating, saving, deleting, and synchronizing environments. Its implementation lives in Core.

IExecutionEnvironmentStore remains the persistence abstraction. FileExecutionEnvironmentStore persists data but delegates JSON serialization/deserialization to NAutomate.Parser.

This keeps environment persistence independent from Blazor and allows MAUI to use the same lifecycle service later.
