# Workflow format

Workflow JSON is parsed and validated by NAutomate.Parser.WorkflowJson. The current schema version is 3. Schema versions 1 and 2 are migrated in memory to version 3.

The workflow is declarative data. It contains module IDs and typed control-flow data; it never contains executable C#, eval, dynamic compilation, or another general-purpose expression language.

## Workflow steps

A step is one of:

- module — invokes a precompiled module.
- if — evaluates a typed condition and executes then or else.
- for — iterates an integer range.
- foreach — iterates a collection variable.
- while — repeats while a condition is true.
- set — changes an execution variable.

Module steps written by older schema versions can omit type; they are interpreted as module.

## Variables

Variables are declared by the workflow and are mutable only inside an execution. Each execution creates an independent variable store.

```json
{
  "name": "Counter",
  "scope": "global",
  "type": "System.Int32",
  "defaultValue": 0
}
```

Supported references are $Name and @Name.

An exact reference preserves the runtime CLR value. For example, $Counter can be an Int32, not only text.

Inside a larger string, references are converted to invariant text.

```text
https://example.test/users/$UserId
```

The older environment placeholder syntax remains available:

```text
${baseUrl}/users/${login:user}
```

${...} resolves persisted execution-environment values. $... and @... resolve the current mutable workflow state.

## Types

Variable declarations carry a CLR type name. The runtime preserves values as objects and converts assignments to the declared type.

The supported primitive scenarios include strings, booleans, integral types, floating-point types, decimal, Guid, DateTime, TimeSpan, nullable forms supported by the existing parameter parser, and types already resolvable by WorkflowParameterParser.

## Conditions

Conditions have a fixed declarative shape:

```json
{
  "variable": "Counter",
  "operator": "GreaterThan",
  "value": 0
}
```

Operators are: Equals, NotEquals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Contains, StartsWith, EndsWith, IsNull, IsNotNull.

There is no arbitrary expression parser. The runtime evaluates these operators against typed values.

## IF / ELSE

```json
{
  "id": "check-counter",
  "type": "if",
  "condition": {
    "variable": "Counter",
    "operator": "GreaterThan",
    "value": 0
  },
  "then": [
    {
      "id": "positive",
      "type": "module",
      "module": "core.echo",
      "parameters": {
        "message": "greater"
      }
    }
  ],
  "else": [
    {
      "id": "zero",
      "type": "module",
      "module": "core.echo",
      "parameters": {
        "message": "smaller"
      }
    }
  ]
}
```

Nested control-flow steps are ordinary child arrays, so if, loops, and module steps can be nested arbitrarily subject to validation.

## FOR

```json
{
  "id": "count",
  "type": "for",
  "variable": "Counter",
  "from": 0,
  "to": 3,
  "step": 1,
  "inclusive": false,
  "steps": []
}
```

from is the initial value. to is exclusive by default and inclusive when inclusive is true. step cannot be zero and may be negative. The runtime checks cancellation between iterations and applies a safety limit to prevent accidental unbounded execution.

## FOREACH

```json
{
  "id": "users",
  "type": "foreach",
  "collection": "$Users",
  "itemVariable": "User",
  "steps": [
    {
      "id": "echo-user",
      "type": "module",
      "module": "core.echo",
      "parameters": {
        "message": "$User"
      }
    }
  ]
}
```

The collection is resolved from the current variable state. The item variable is execution-scoped and is restored or removed when the loop finishes, so it does not leak into subsequent workflow steps.

## WHILE

```json
{
  "id": "count",
  "type": "while",
  "condition": {
    "variable": "Counter",
    "operator": "LessThan",
    "value": 3
  },
  "maxIterations": 1000,
  "steps": [
    {
      "id": "echo",
      "type": "module",
      "module": "core.echo",
      "parameters": {
        "message": "$Counter"
      }
    },
    {
      "id": "increment",
      "type": "set",
      "variable": "Counter",
      "operation": "Increment",
      "value": 1
    }
  ]
}
```

If maxIterations is omitted, the runtime safety limit is 10,000 iterations. Cancellation is checked before every iteration.

## SET and core.set

set is a control-flow/runtime operation and core.set is also exposed as an official module descriptor for module discovery and future editor integration.

```json
{
  "id": "increment",
  "type": "set",
  "variable": "Counter",
  "operation": "Increment",
  "value": 1
}
```

Supported operations are Set, Increment, and Decrement.

## Runtime state

The mutable state is represented by IWorkflowVariableStore in NAutomate.Abstractions. ModuleExecutionContext exposes the store to modules without exposing WorkflowEngine.

There is no static/global execution state. Concurrent executions of the same workflow receive separate stores.

## Events

The runtime emits facts for control flow, including condition-evaluated, branch-selected, loop-started, loop-iteration-started, loop-iteration-completed, variable-changed, and the existing module running/output/success/failure events.

Hosts can use these events for consoles and debuggers without moving control-flow logic into UI code.
