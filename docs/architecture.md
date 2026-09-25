# Architecture

## Components

| Project | Responsibility |
|---|---|
| NAutomate.Abstractions | Stable public contracts for modules, service metadata and execution integration |
| NAutomate.Core | Workflow model, serialization, storage, registry and execution |
| NAutomate.Modules | Official precompiled automation modules |
| NAutomate.CLI | Executable runtime host and console protocol |
| NAutomate | MAUI Blazor Hybrid desktop application |
| NAutomate.Web | Blazor web application |

## Dependency direction

```text
NAutomate.Abstractions
        ↑
        ├── NAutomate.Core
        └── NAutomate.Modules
                         ↑
                    NAutomate.CLI
```

Core does not reference official module implementations.

## Runtime flow

```text
Workflow JSON
   -> NAutomate.Core
   -> module registry
   -> precompiled module operation
   -> service
   -> external automation runtime
```

## Stateful module boundary

Automation drivers and sessions are runtime resources. They must never be serialized into workflow JSON.

For Appium:

```text
Workflow step parameters
        ↓
ModuleExecutionContext.Parameters
        ↓
Appium operation binder
        ↓
IAndroidAppiumService / IIOSAppiumService
        ↓
AndroidDriver / IOSDriver
        ↓
Appium server / device
```

The service owns the driver and any runtime-only element resources.

## Service metadata

Module services and operations use annotations so a future Web/MAUI designer can discover:

- service identity and description;
- operation identity and description;
- method parameter names/types/descriptions.

The metadata layer is UI-agnostic and does not introduce MudBlazor, Blazor or MAUI dependencies into Core.

## Boundary rule

The workflow describes what to run. It never contains the implementation or runtime objects being run.
