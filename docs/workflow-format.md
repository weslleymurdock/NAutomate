# Workflow Format

Workflow files are versioned declarative documents.

Minimum shape:

```json
{
  "schemaVersion": 1,
  "name": "Hello World",
  "steps": [
    {
      "id": "step-001",
      "module": "core.echo",
      "parameters": {
        "message": "Hello from NAutomate"
      }
    }
  ]
}
```

## Rules

- `schemaVersion` is mandatory.
- Module identifiers are stable strings.
- Parameters are serializable data.
- Step IDs identify execution records.
- Do not store C# source, generated code, assemblies, or executable delegates in workflow JSON.
- Schema changes must be deliberate and documented.

The first implementation only requires `core.echo`.
