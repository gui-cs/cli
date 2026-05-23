# CLAUDE.md

This file provides guidance to AI coding agents working in this repository.

## Project status

This repository currently contains scaffolding only for `Terminal.Gui.Cli`.
No library implementation is present yet.

## Project structure

- `src/Terminal.Gui.Cli` — class library package (`Terminal.Gui.Cli`)
- `tests/Terminal.Gui.Cli.Tests` — unit tests
- `tests/Terminal.Gui.Cli.IntegrationTests` — integration tests
- `tests/Terminal.Gui.Cli.SmokeTests` — smoke tests
- `examples/Terminal.Gui.Cli.ExampleApp` — sample console app

## Build and test

- `dotnet restore Terminal.Gui.Cli.slnx`
- `dotnet build Terminal.Gui.Cli.slnx --no-restore`
- `dotnet format Terminal.Gui.Cli.slnx --no-restore --verify-no-changes`
- `dotnet run --project tests/Terminal.Gui.Cli.Tests --no-build`
- `dotnet run --project tests/Terminal.Gui.Cli.IntegrationTests --no-build`
- `dotnet run --project tests/Terminal.Gui.Cli.SmokeTests --no-build`

## Coding standards

- Target framework is `net10.0`.
- Keep warnings at zero (`TreatWarningsAsErrors=true`).
- Follow repository `.editorconfig` conventions.
- Do not add unrelated implementation while scaffolding.
