# CLI Runtime Skill

Use this skill when changing NAutomate.CLI or the desktop-to-CLI execution boundary.

## Rules

- Keep the CLI independent of UI projects.
- Support `run <automation.json>`.
- Use stdout for normal status/output and stderr for errors.
- Use line-oriented output for incremental consumption.
- Return zero only when workflow execution succeeds.
- Return non-zero for invalid input, unresolved modules, execution failures, cancellation, or fatal runtime errors.
- Avoid blocking APIs where asynchronous equivalents exist.
- Keep platform-specific process launching in the MAUI application, not Core.
