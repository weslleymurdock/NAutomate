# Architecture

NAutomate is split into stable contracts, reusable parsing/application services, runtime execution, modules, and presentation hosts.

## Projects

- NAutomate.Abstractions — public runtime/module contracts and declarative workflow models.
- NAutomate.Parser — workflow JSON, validation, environment placeholders, typed parameter conversion, and typed workflow-variable reference resolution.
- NAutomate.Core — runtime execution, mutable execution state, condition evaluation, module registry, debugger session, and persistence/application services.
- NAutomate.Modules — official precompiled automation modules.
- NAutomate.Modules.Shell — shell module implementations.
- NAutomate.CLI — command-line composition and execution host.
- NAutomate.Web — Blazor/MudBlazor presentation host.
- NAutomate — MAUI presentation host.

## Dependency direction

```text
External modules ────────────────┐
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

NAutomate.Abstractions contains WorkflowStep, the declarative control-flow contracts, WorkflowCondition, IWorkflowVariableStore, and ModuleExecutionContext.

NAutomate.Parser owns the JSON converter for the polymorphic step model. It does not depend on UI or runtime execution.

NAutomate.Core interprets the step tree recursively. WorkflowEngine does not compile or evaluate arbitrary source code. WorkflowConditionEvaluator implements the finite condition operator set and WorkflowVariableStore owns mutable state for one execution.

## Runtime state

```text
WorkflowEngine
 ├── AutomationWorkflow
 ├── WorkflowVariableStore
 │    ├── typed current values
 │    └── execution-local isolation
 ├── WorkflowConditionEvaluator
 └── recursive step executor
       ├── ModuleStep
       ├── IfStep
       ├── ForStep
       ├── ForeachStep
       ├── WhileStep
       └── SetStep
```

Modules receive IWorkflowVariableStore through ModuleExecutionContext. They do not receive the engine itself.

## Execution environments

Persisted AutomationEnvironment values remain separate from mutable workflow state. ${name} and ${stepId:name} continue to resolve persisted environment values. $Name and @Name resolve current execution variables.

## Debugger

WorkflowExecutionSession remains the UI-facing execution-control boundary. The runtime itself does not depend on the debugger. Control-flow execution happens recursively in Core, and runtime events identify the relevant step IDs so presentation hosts can observe nested execution.

The current editor does not construct control-flow nodes visually; JSON/runtime support is intentionally ahead of the visual editor.


## Shared UI

The reusable application UI is implemented in src/NAutomate.UI as a Razor Class Library. The Web host references the RCL, and a MAUI Blazor Hybrid host can reference the same project instead of maintaining a second copy of the workflow editor.

The RCL owns:

- workflow editor and debugger pages;
- workflow tree components;
- drag-and-drop workflow composition;
- environment and global-variable dialog;
- shared navigation/layout components;
- startup software-dependency validation and notifications.

Host-specific capabilities are provided through dependency injection. The shared UI must not reference MAUI APIs or Web-host APIs directly.

Routable components from the RCL are exposed to each host through the host router's additional assemblies.

## Startup software dependencies

Hosts register the software dependencies required by their enabled modules through SoftwareDependencyCatalog. NAutomate.UI validates registered executables when the host starts and publishes the results through DependencyStatusStore.

Only dependencies registered by the host/modules are validated. This prevents unrelated tools such as Node.js, npm, Docker, or PowerShell from generating warnings unless they are actually configured as dependencies for the running host.

Required dependencies that are unavailable are presented as persistent warning notifications in the top-right notification area.


## Automation projects

The MAUI host uses `FileAutomationProjectStore` rooted at `FileSystem.AppDataDirectory/NAutomate/Projects`. The shared UI consumes `IAutomationProjectStore`, so project discovery and the four-file integrity model are host-independent.

Execution from the editor saves and validates the project before starting the workflow.