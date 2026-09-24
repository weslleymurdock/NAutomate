# Modules

A module is a precompiled implementation that the engine resolves by a stable module ID.

## Contract boundary

Module contracts belong in `NAutomate.Abstractions` so third-party NuGet packages can reference them without depending on MAUI or Blazor.

A module needs enough contract surface to provide:

- identity and metadata;
- parameter information;
- asynchronous execution;
- execution context;
- result/status information.

## Registry

Core owns the runtime registry. It maps a module ID to a precompiled implementation.

Conceptually:

```text
"core.echo" -> EchoModule
```

The first implementation is registered by `ModuleRegistry` automatically. New
precompiled modules can be passed to its constructor or registered explicitly;
workflow files continue to contain only module IDs and parameter data.

Future packages may contribute additional modules. Do not implement dynamic C# compilation or arbitrary source execution.

## Module metadata

The runtime should eventually expose descriptors containing ID, display name, description, version, and parameter metadata so UI designers can discover modules and generate configuration forms.
