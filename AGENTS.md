# NAutomate Agent Instructions

## Project purpose

NAutomate is a .NET 10 automation platform. Users compose workflows from precompiled C# modules. Workflows are persisted as declarative JSON and executed by NAutomate.CLI.

The workflow file is data, not source code. Never compile or execute arbitrary C# contained in a workflow.

## Solution

- `NAutomate.Abstractions`: stable contracts for runtime integrations and future NuGet modules.
- `NAutomate.Core`: workflow model, serialization, storage, module registry, and execution engine.
- `NAutomate.CLI`: console runtime host and process protocol.
- `NAutomate`: MAUI Blazor Hybrid desktop UI.
- `NAutomate.Web`: Blazor web UI.

Target .NET 10 and preserve the existing .slnx solution.

## Dependency direction

Keep dependencies directional:

```text
External module packages -> NAutomate.Abstractions
NAutomate.Core -> NAutomate.Abstractions
NAutomate.CLI -> NAutomate.Core
NAutomate -> NAutomate.Core
NAutomate.Web -> NAutomate.Core
```

Abstractions must not depend on UI frameworks. Core must not depend on MAUI, Blazor, MudBlazor, or process-launching concerns.

## Workflow model

Workflow JSON must contain stable module IDs and serializable parameters. Use an explicit schema version.

Example:

```json
{
  "schemaVersion": 1,
  "name": "Hello World",
  "steps": [
    {
      "id": "step-001",
      "module": "core.echo",
      "parameters": {
        "message": "Hello"
      }
    }
  ]
}
```

## Modules

Modules are precompiled implementations resolved by module ID. The first reference implementation is `core.echo`.

Design the registry so future NuGet packages can register modules without coupling them to the UI.

## UI

For both `NAutomate` and `NAutomate.Web`, use MudBlazor components for application UI. Do not introduce another UI component library or raw Bootstrap components for application controls/layout.

Keep process management and business logic out of Razor components. Use injected services and asynchronous event handlers.

## CLI

The CLI exposes the runtime boundary. The first command is:

```text
nautomate run <automation.json>
```

stdout is for execution/status output; stderr is for errors. Exit code 0 means successful execution. Non-zero means invalid input or failed execution.

## Engineering

- Use nullable reference types.
- Prefer async APIs and CancellationToken.
- Add XML documentation to public contracts and important runtime types.
- Keep abstractions dependency-light.
- Avoid premature infrastructure, databases, brokers, remote execution, or arbitrary-code execution.
- Preserve schema compatibility intentionally.
- Keep platform-specific process orchestration outside Core.
- Do not add tests, commits, or pushes unless the issue explicitly requests them.

## Documentation

Before changing architecture, read the relevant files under `docs/` and `.github/skills/`. When implementation changes a documented contract, update the documentation in the same change.

## Primary feature

The first end-to-end feature is tracked by GitHub issue #1: create a workflow in the UI, save JSON, execute through CLI/Core/module, and stream the result back to the UI.
