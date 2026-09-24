# Development Guide

## Recommended implementation order

1. Define stable contracts in NAutomate.Abstractions.
2. Implement versioned workflow types and JSON serialization in Core.
3. Implement module registry and `core.echo`.
4. Implement the execution engine and structured results.
5. Implement CLI `run`, output, errors, and exit codes.
6. Implement filesystem workflow storage.
7. Implement the MAUI CLI process service.
8. Implement the minimal MudBlazor workflow editor.
9. Implement streaming execution console.
10. Verify the complete UI -> JSON -> CLI -> Core -> module -> UI path.

## Constraints

Target .NET 10. Prefer async APIs and CancellationToken. Keep public contracts documented with XML comments. Avoid databases, remote services, brokers, arbitrary code execution, and premature plugin infrastructure in the first milestone.

When documentation and implementation diverge, update documentation as part of the same change.
