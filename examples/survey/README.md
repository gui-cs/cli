# survey

A sample CLI app built with `Terminal.Gui.Cli` that demonstrates the
**Terminal.Gui + Spectre.Console** collaboration described in
[spectre.console#2128](https://github.com/spectreconsole/spectre.console/issues/2128).

It is a port of Spectre.Console's `Prompt` example: where Spectre uses blocking
console prompts, this app uses Terminal.Gui for interaction, then renders the
collected profile with Spectre.Console.

| Concern | Owner |
|---------|-------|
| Interaction (form, navigation, validation) | Terminal.Gui |
| Rich rendering (Panel, Table, BarChart) | Spectre.Console |
| Scriptable surfaces (`--json`, `--cat`, `--opencli`, agent guide) | `Terminal.Gui.Cli` |

## Usage

```bash
# Interactive Terminal.Gui form (press F2 to submit)
survey

# Headless: provide answers as options
survey --name Ada --age 36 --sport Fencing --fruits "Apple,Cherry" --color Teal

# Structured JSON envelope for scripts and agents
survey --name Ada --age 36 --fruits "Apple,Cherry" --json

# Render the profile as a Spectre.Console card to stdout
card --name Ada --age 36 --sport Fencing --color Teal --cat

# Browse help in the TUI markdown viewer
survey help
```

## Commands

| Command  | Description |
|----------|-------------|
| `survey` | Collect a profile and return it as structured data. |
| `card`   | Render a profile as a rich Spectre.Console card. |
| `help`   | Show command help in a TUI markdown viewer. |

## Spectre.Console in the TUI

With `--cat`, the `card` command renders its Spectre.Console content directly to
stdout. Rendering that same content *inside* the Terminal.Gui window uses the
[`Terminal.Gui.Interop.Spectre`](https://github.com/gui-cs/Terminal.Gui/pull/5393)
bridge (`SpectreView`); the TUI path is wired to that bridge once the package is
published.

## Running

```bash
dotnet run --project examples/survey -- survey --name Ada --json
```
