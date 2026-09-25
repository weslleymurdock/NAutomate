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
