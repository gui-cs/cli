# Hero GIF Recording Guide

Produces `docs/images/hero.gif` — an animated GIF demonstrating the example app's CLI features.

## Prerequisites

- [tuirec](https://github.com/gui-cs/tuirec) v0.3.4+ on PATH (`go install github.com/gui-cs/tuirec/cmd/tuirec@latest`)
- .NET 10 SDK (for building the example app)
- `agg` is auto-downloaded by tuirec on first use

## Build the example app

```powershell
dotnet build examples/Terminal.Gui.Cli.ExampleApp -c Debug --nologo
```

## Record

```powershell
$binary = "./examples/Terminal.Gui.Cli.ExampleApp/bin/Debug/net10.0/Terminal.Gui.Cli.ExampleApp.exe"

# Keystroke script — demonstrates:
#   1. --help output
#   2. greet command with --initial and --json
#   3. info --cat for headless rendering
#   4. --opencli agent metadata
#   5. Interactive greet (launches TUI)
#
# Pacing: --keystroke-delay 60 for readable but snappy feel.
$ks = 'wait:500,`example-app --help`,Enter,wait:1500,`example-app greet --initial "World" --json`,Enter,wait:1500,`example-app info --cat`,Enter,wait:1500,`example-app --opencli`,Enter,wait:1500,`example-app greet --initial "World"`,Enter,wait:2000,Esc'

tuirec record `
    --binary $binary `
    --name "hero" `
    --show-command '$ example-app' `
    --keystrokes $ks `
    --startup-delay 1000 `
    --drain 1500 `
    --cols 100 `
    --rows 24 `
    --keystroke-delay 60 `
    --max-duration 30 `
    --cast-output ./artifacts/hero.cast `
    --verbosity high

# Copy to final location
Copy-Item ./artifacts/hero.gif ./docs/images/hero.gif -Force
```

## Demo sequence

| Time   | Feature                | Action                                               |
|--------|------------------------|------------------------------------------------------|
| 0-2s   | Help                   | `example-app --help` — shows commands and options    |
| 2-4s   | JSON envelope          | `example-app greet --initial "World" --json`         |
| 4-6s   | Headless viewer        | `example-app info --cat`                             |
| 6-8s   | Agent discovery        | `example-app --opencli` — machine-readable metadata  |
| 8-12s  | Interactive UI         | `example-app greet --initial "World"` — TUI launches |
| 12s    | Quit                   | Esc closes the app                                   |

## Tuning tips

- **Terminal size**: 100×24 is wide enough to show JSON output without wrapping but compact for a README GIF.
- **Keystroke delay**: 60ms feels natural for CLI demos. Reduce to 40 for snappier results.
- **Shell mode**: The example app is a standalone binary that processes args and exits (non-interactive mode for most invocations). The interactive `greet` launches Terminal.Gui briefly.
- **If the TUI doesn't close**: Add extra `Esc` or `Ctrl+Q` keystrokes. The app quits on Esc when no dialog is open.
- **GIF too large**: Reduce `--cols` or `--rows`, or shorten `wait:` values.
- **Wrong binary path**: After building, verify the exe exists at the expected path. On Linux/macOS omit the `.exe` extension.

## Troubleshooting

1. **Keys not registering**: Use `--verbosity high` and inspect stderr. Standard VT100 sequences work with Terminal.Gui.
2. **JSON output truncated**: Increase `--cols` to give the JSON envelope room to render without wrapping.
3. **Recording too long**: Reduce `wait:` values between commands. Current target is ~12s total.
4. **GIF not generated**: Ensure `agg` was downloaded. Check `./artifacts/` for the `.cast` file; if present, re-run `agg` manually.
