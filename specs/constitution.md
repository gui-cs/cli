# gui-cs/cli Constitution

**Version**: 1.0 | **Ratified**: 2026-05-23 | **Last Amended**: 2026-05-23

This constitution governs all contributions to `gui-cs/cli`.

## I. Purpose & Scope

`Terminal.Gui.Cli` is a `.NET` class library for CLI-focused APIs built on `Terminal.Gui`.

- Package ID: `Terminal.Gui.Cli`
- Namespace: `Terminal.Gui.Cli`
- TFM: `net10.0`

## II. Non-Goals

Until implementation begins, this repository is scaffold-only and should not accrue speculative feature code.

## III. Tenets

1. Keep scaffolding and tooling reliable across Linux, macOS, and Windows.
2. Keep changes surgical and high-signal.
3. Keep warnings at zero and CI green.
4. Prefer clear, maintainable project structure over premature complexity.

## IV. Architectural Rules

1. Production code lives under `src/Terminal.Gui.Cli`.
2. Test projects are executable xUnit v3 projects under `tests/`.
3. Examples live under `examples/` and must reference the source project.
4. Shared build settings belong in `Directory.Build.props` and `Directory.Build.targets`.

## V. Testing Tiers

- `Terminal.Gui.Cli.Tests` — unit tests.
- `Terminal.Gui.Cli.IntegrationTests` — integration tests.
- `Terminal.Gui.Cli.SmokeTests` — smoke-level validation.

All three test projects are run by CI on an OS matrix.

## VI. Governance

Constitution changes require a pull request that updates this file and explains the rationale.
