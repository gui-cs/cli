# card

Render a profile as a rich Spectre.Console card.

[Back to main help](help:help)

## Usage

```
card --name Ada --age 36 --sport Fencing --cat
card --name Ada --fruits "Apple,Cherry" --color Teal --cat
card                                                   Show a sample card in the TUI
```

## Options

The `card` command accepts the same options as [survey](help:survey): `--name`,
`--fruits`, `--sport`, `--age`, and `--color`. When no name is given it renders a
sample profile.

## Behavior

With `--cat`, Spectre.Console renders the card (a Panel, Table, and BarChart) to
stdout. Without `--cat`, the profile is shown in a Terminal.Gui window.
