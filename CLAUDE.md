# CLAUDE.md

This file provides guidance to AI coding agents working in this repository.

## Project status

This repository currently contains scaffolding only for `Terminal.Gui.Cli`.
No library implementation is present yet.

## Source of truth

- Engineering authority: `specs/constitution.md`
- Build/style defaults: `.editorconfig`, `Directory.Build.props`

If guidance conflicts, follow `specs/constitution.md`.

## Project structure

- `src/Terminal.Gui.Cli` — class library package (`Terminal.Gui.Cli`)
- `tests/Terminal.Gui.Cli.Tests` — unit tests
- `tests/Terminal.Gui.Cli.IntegrationTests` — integration tests
- `tests/Terminal.Gui.Cli.SmokeTests` — smoke tests
- `examples/Terminal.Gui.Cli.ExampleApp` — sample console app

## Build and test

- `dotnet restore Terminal.Gui.Cli.slnx`
- `dotnet build Terminal.Gui.Cli.slnx --no-restore -c Debug`
- `dotnet format Terminal.Gui.Cli.slnx --no-restore --verify-no-changes`
- `dotnet run --project tests/Terminal.Gui.Cli.Tests --no-build -c Debug`
- `dotnet run --project tests/Terminal.Gui.Cli.IntegrationTests --no-build -c Debug`
- `dotnet run --project tests/Terminal.Gui.Cli.SmokeTests --no-build -c Debug`

## Coding standards

- Target framework is `net10.0`.
- Keep warnings at zero (`TreatWarningsAsErrors=true`).
- Use file-scoped namespaces for new C# files.
- Use Allman braces and always include braces for conditionals/loops.
- Prefer guard clauses / early returns to deep nesting.
- One type per file for non-trivial public/internal types.
- Do not add unrelated implementation while scaffolding.
