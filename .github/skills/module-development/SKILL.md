# Module Development Skill

Use this skill when creating built-in or external automation modules.

A module is precompiled C# code selected by a stable module ID. The workflow JSON contains only the ID and parameters.

## Rules

- Put reusable module contracts in NAutomate.Abstractions.
- Keep module implementations independent of UI projects.
- Provide stable metadata and parameter definitions.
- Execute asynchronously and accept CancellationToken.
- Validate parameters at the module boundary.
- Return structured results suitable for CLI reporting.
- Register the implementation through the runtime module registry.
- Never compile or execute C# source supplied by workflow JSON.

Future NuGet packages should be able to reference NAutomate.Abstractions without referencing MAUI or Blazor.
