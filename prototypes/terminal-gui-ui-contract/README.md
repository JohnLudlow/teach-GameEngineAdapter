---
title: "Can a minimal shared UI contract plus a Terminal.Gui adapter satisfy the Phase 1 TUI baseline?"
technique: "Immutable whole-tree updates with stable IDs, a terminal-agnostic UiSession, and a thin Terminal.Gui adapter verified by the same semantic observations"
stack: "C# / .NET 10, Terminal.Gui 2.5.0, xUnit"
status: "keep"
---

# Terminal.Gui UI contract vertical slice

Prototype for [issue #1](https://github.com/JohnLudlow/teach-GameEngineAdapter/issues/1).

## Objective

Validate whether a minimal shared UI contract and a Terminal.Gui adapter can satisfy the agreed Phase 1 TUI behaviour,
and record a go/no-go for production implementation.

## Technique demonstrated

- `UiContract` holds an immutable tree (panel, list, button) with stable IDs, semantic inputs (key, click-on-ID),
  consumer events and a `RenderFrame`. `UiSession` implements focus, selection, resize and redraw rules with no
  terminal types.
- `TerminalGuiDemo` is a thin adapter: native keys and clicks become contract inputs; frames are painted only when the
  session reports a visible change.
- The same contract semantics (`UiSession`) are driven by a headless path (tests call the contract directly) and by a
  real Terminal.Gui path (native keys, clicks and resize through `TerminalGuiAdapter` plus `DemoConsumer`). Tests assert
  only focus ID, selection ID, consumer events, layout outcome and frame presence.

Scope covered: list selection and selected-item removal recovery, button activation (keyboard and click) with focus,
resize with minimum-size fallback, redraw-by-next-frame, headless observations. Not covered: other controls, tab
controls, panels as focus targets in the UI, incremental update API, production hardening.

## Tech stack

.NET 10 target (builds with the installed SDK), Terminal.Gui 2.5.0 (ANSI driver for the automated path), xUnit.

## How to run

1. `cd prototypes/terminal-gui-ui-contract`
2. Self-check: `dotnet test tests/SelfCheck`
3. Manual terminal demo: `dotnet run --project src/TerminalGuiDemo` (Tab/Shift+Tab, Up/Down, Space/Enter, mouse
   click; "Remove selected" shows selection recovery; "Quit" exits; resize the window below 40x14 for the fallback).

## Self-check

`tests/SelfCheck` has headless contract tests (14 cases) and 4 tests that drive real Terminal.Gui key, mouse and
screen-size input. They assert semantic outcomes only, never escape sequences or cursor positions.

**Result**: pass (18/18).

## Findings

- **Go (conditionally).** Terminal.Gui 2.x can host the contract: its Application keyboard/mouse hooks let the adapter
  own all behaviour, and its ANSI driver supports headless injection and `SetScreenSize`.
- **Contract gap — selection ownership.** The baseline says a missing or unknown selection invalidates a control, yet
  removal recovery needs the adapter to choose a new selection. The prototype validates `InitialSelectedItemId` only
  for new lists; for existing lists the adapter-held selection wins and recovery is reported as an event. This needs an
  explicit requirement.
- **Contract gap — events lag one tick.** Updates made while handling an event emit follow-up events (recovery) that
  the consumer sees on the next drain. The contract should state this.
- **Gap — fallback input.** Input is ignored while the minimum-size fallback is shown; the baseline does not say.
- **Terminal.Gui caveat.** Synthesised press/release did not produce a click in headless injection; the adapter listens
  for `LeftButtonClicked`, which the test injects directly. Real-terminal click behaviour needs manual confirmation.
- **Not verified here.** Visible flicker on a real terminal (the idle test only checks the adapter's own frame counter,
  and the test pump forces a full redraw) and mouse in a real terminal. Known adapter limits: disabled panels are not
  propagated to child views, native Shift+Tab is untested, and views are rebuilt on every tree update.

## Keep / discard status

- **Status**: keep — expected to evolve into real work.
- **Reasoning**: all scoped behaviours pass on both paths; remaining items are contract clarifications, not blockers.

## Branch

`1-prototype-validate-terminalgui-ui-contract-vertical-slice` (the user's existing branch, chosen instead of
`prototype/terminal-gui-ui-contract`) — kept local unless explicitly pushed or opened as a PR at the user's request.
