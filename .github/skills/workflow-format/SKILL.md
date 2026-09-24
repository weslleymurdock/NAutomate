# Workflow Format Skill

Use this skill when changing workflow persistence or serialization.

## Rules

- Workflow JSON must be declarative.
- Include an explicit schema version.
- Use stable module IDs.
- Parameters must be serializable data.
- Step IDs must be stable within a workflow.
- Do not serialize executable delegates, C# source, generated code, or assembly information.
- Treat compatibility as a product concern: document schema changes and avoid accidental breaking changes.
