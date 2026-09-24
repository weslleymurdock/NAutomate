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

Future packages may contribute additional modules. Do not implement dynamic C# compilation or arbitrary source execution.

## Module metadata

The runtime should eventually expose descriptors containing ID, display name, description, version, and parameter metadata so UI designers can discover modules and generate configuration forms.
