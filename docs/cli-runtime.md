# CLI Runtime

The CLI is the process boundary used by the desktop UI.

Initial command:

```text
nautomate run <automation.json>
```

## Output

Use line-oriented output so the desktop application can consume it incrementally.

Example:

```text
NAutomate
Workflow: Hello World

[RUNNING] core.echo
[OUTPUT] Hello from NAutomate
[SUCCESS] core.echo

Execution completed successfully.
```

stdout carries status/output. stderr carries errors.

The runtime emits step IDs in Core execution events for hosts that need to
correlate output with a workflow step. The text protocol remains line-oriented
and keeps the existing status lines stable.

The current built-in module is `core.echo`; it reads the serializable `message`
parameter and emits its value as an `[OUTPUT]` line.

Exit codes:

- 0: successful workflow execution.
- non-zero: invalid arguments, invalid workflow, unresolved module, cancellation/failure, or fatal runtime error.

Operational failures, including module exceptions, are reported as `Error:`
lines on stderr and never produce a successful exit code.

The CLI must not depend on UI code.
