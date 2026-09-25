# Workflow format

Workflow JSON is parsed and validated by NAutomate.Parser.WorkflowJson.

The workflow remains declarative: JSON contains module IDs and serializable parameter values; it never contains executable C#.

## Variables

Variables are declared in the workflow and values are stored separately in execution environments.

- Global variable: ${name}
- Local variable: ${stepId:name}

Placeholder resolution is implemented by NAutomate.Parser.EnvironmentVariableResolver, not by a UI.

## Operation parameters

Presentation hosts may collect parameter values as text, but conversion to the operation's declared CLR type is performed by NAutomate.Parser.WorkflowParameterParser.

This is important for values such as booleans, numbers, enums, GUIDs, arrays, and other typed module parameters. A future MAUI editor must use the same parser instead of implementing a second conversion system.

## Persistence

Execution-environment JSON serialization is centralized in NAutomate.Parser.ExecutionEnvironmentJson. Environment lifecycle and persistence orchestration remain in Core.
