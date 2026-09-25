# Modules

A module is precompiled C# code selected by a stable module ID. Workflow JSON never contains executable code.

## Official modules

core.echo outputs a resolved workflow parameter.

core.set is the official variable-assignment module descriptor. The runtime also has a native set workflow step because variable assignment is part of workflow control flow rather than an arbitrary external module operation.

Official modules remain registered through OfficialModules.GetModules() and ModuleRegistry.

## Runtime context

Modules receive the workflow, current step, resolved parameters, cancellation, optional persisted execution environment, and the execution-local IWorkflowVariableStore.

Modules therefore do not need a dependency on NAutomate.Core to consume or update workflow variables.

## Discovery

The registry exposes every registered module through IModuleRegistry.List(). UI hosts must not assume every module has service metadata. Standalone modules such as core.echo and core.set are valid registry entries even when Services is null.
