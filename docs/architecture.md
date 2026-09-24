# Architecture

## Components

| Project | Responsibility |
|---|---|
| NAutomate.Abstractions | Stable public contracts for modules and execution integration |
| NAutomate.Core | Workflow domain/model, serialization, storage, registry, execution |
| NAutomate.CLI | Executable runtime host and console protocol |
| NAutomate | MAUI Blazor Hybrid desktop application |
| NAutomate.Web | Blazor web application |

## Runtime flow

```text
MAUI UI
  -> workflow model
  -> automation.json
  -> NAutomate.CLI
  -> NAutomate.Core
  -> module registry
  -> precompiled module
  -> CLI stdout/stderr
  -> MAUI execution console
```

## Boundary rule

The JSON workflow describes what to run. It never contains the implementation of what is run.

Core is UI-agnostic. UI projects orchestrate presentation and, for the desktop application, the CLI process.

Future custom modules should reference NAutomate.Abstractions rather than the UI.
