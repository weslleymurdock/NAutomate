# Automation projects

The MAUI application stores projects under the application data directory:

`<AppDataDirectory>/NAutomate/Projects/`

Each project directory is named:

`{guid}-{lowercase-project-name}`

Each project also contains an `artifacts/` directory for files consumed or produced by the automation.

Each project contains four required JSON files:

- `automation.json` — the declarative workflow and its global variable definitions.
- `settings.json` — project-specific application settings, allowing different automations to use different host configuration such as an Appium server.
- `env.json` — typed global variable values grouped by execution environment.
- `project.json` — project metadata and SHA-256 integrity hashes for the three content files plus a canonical self-integrity hash.

A project is executable only when all four files exist, contain valid JSON for their schema, `automation.json` passes workflow validation, environment values match the declared global variable types, and all recorded hashes match the current files.

The `settings.json` object is exposed to module execution through `ModuleExecutionContext.Settings`, so a module can resolve project-specific application configuration without putting that configuration into the workflow step itself.

The project store writes the four files atomically per file. The integrity manifest is written last so an interrupted save is detected instead of silently being treated as a valid project.

The MAUI home page enumerates the project directory at startup and opens projects using their GUID.
