# Teaching Notes

Preferences, observations, and working notes for this teaching session.

## User Preferences

- **Advanced C# developer** — Comfortable with async/await, dependency injection, SOLID principles. Don't explain language features.
- **Beginner game developer** — May need grounding on game architecture concepts (frame loop, render pipeline, cameras, etc.).
- **Task-oriented** — Wants to **continue implementing**, not just study. Prefer lessons that lead to concrete code.
- **Focused on TUI** — Previous AI-assisted TUI implementation failed. This is a near-term priority.
- **Time-aware** — A few months away from the codebase. Needs quick context restoration, not comprehensive tutorial.

## Teaching Approach

- **Audit first, teach second** — Validate current state against documentation before designing lessons.
- **Architecture before code** — Understand the adapter pattern and provider model before diving into specific interfaces.
- **Concrete over abstract** — Use HeadlessAdapter as working example; trace a frame through the system.
- **TUI as validation** — Building a TUI adapter will validate the interface design and fill documentation gaps.
- **Documentation as first-class output** — Improve subject repo docs so others can onboard without this teach repo.

## Session Plan (Tentative)

1. **Lesson 1: Architecture Overview** — Core vs. Adapter, Provider model, how Headless works
2. **Lesson 2: Contracts Deep Dive** — Walk each interface (IEngineAdapter, IRenderProvider, etc.)
3. **Lesson 3: TUI Design** — Clarify requirements, analyze previous attempt, design TextRenderProvider
4. **Lesson 4: TUI Implementation** — Build minimal TUI adapter, integrate with tests
5. **Lesson 5: Documentation & Completion** — Improve subject repo docs, prepare for next phase

## Key Insights (Updated as Learning Progresses)

- **Phase 1 is genuinely complete** — All contracts are implemented, well-documented, tested.
- **Headless adapter is reference-quality** — Can trace a full frame cycle through it.
- **TUI is a blocker** — Previous attempt failed; unclear why. Need to audit and understand before proceeding.
- **Documentation gaps exist** — especially around TUI-specific requirements and capability negotiation patterns.

## Questions to Investigate

1. **TUI Requirements** — What should the TUI implementation support?
   - Text-only rendering (no sprites, meshes)?
   - Keyboard input only, or mouse too?
   - Active pause support?
   - Time controls (speed up, slow down)?
   - Ref: Previous attempt, Phase 2 plan

2. **Capability Negotiation** — How should a TUI adapt respect `EngineCapabilities`?
   - What capabilities should TUI report (e.g., `Supports2D = false`)?
   - Should game logic check capabilities before calling unsupported features?
   - Any examples in Headless or docs?

3. **Rendering Strategy** — How to map `SpriteDrawDto`, `MeshDrawDto`, `TextDrawDto` to text output?
   - ASCII art for sprites?
   - Unicode block characters for simple graphics?
   - Just text, ignoring visual draw commands?

4. **Frame Pacing** — Should TUI adapter support time controls?
   - Variable tick rate (speed up, slow down)?
   - Frame skip for fast-forward?
   - Relevant to `DeterministicTickContext` and `EngineRunner`?

## Previous TUI Attempt (Branch 60, Discarded)

**Status**: Branch `60-prototype-tui-user-interface-elements-menu-list-panel-rendering` — changes discarded, not pushed.

**Failures Observed**:
- ⚠️ **Constant flickering** — render loop buffering issue; likely rendering every frame without proper double-buffering or debouncing
- ⚠️ **Non-functional controls** — checkboxes and other UI elements could not be interacted with; input event propagation broken
- ⚠️ **No mouse support** — mouse input not captured or handled
- ⚠️ **Unpredictable tab focus** — tab key navigation did not work reliably; focus management broken

**Root Causes (Hypothesized)**:
- **Render loop timing**: TUI tried to re-render every frame, but terminal rendering is expensive; flickering suggests no dirty-region tracking or frame buffering
- **Input model mismatch**: Terminal input (keyboard, mouse) requires event-based architecture; the TUI implementation may have tried polling or buffering incorrectly
- **Control architecture**: UI controls (checkboxes, menus) may have been implemented in-adapter rather than relying on `IUserInterfaceProvider`
- **Focus state**: Tab/focus management is not a terminal-native feature; had to be manually implemented and likely was done incorrectly

**Lesson**: Terminal UI is **fundamentally different** from graphical rendering. Cannot simply map `IRenderProvider` calls to ASCII art. Need separate abstraction.

## Working Hypotheses (To Be Tested in Lesson 3)

- **Hypothesis A**: TUI should **NOT** try to implement `IRenderProvider` for visual rendering. Instead, use `IUserInterfaceProvider` for all TUI output.
- **Hypothesis B**: TUI should report `Supports2D = false, Supports3D = false`, signaling it's **text-only**. Game logic should check capabilities and adapt.
- **Hypothesis C**: For a 4X game in TUI: render map via text (ASCII grid), render UI via `IUserInterfaceProvider` (text panels, buttons). Keep rendering separate from input.
- **Hypothesis D**: Input model must be **event-driven** with proper queue/dispatch, not polled. Mouse events need explicit handling.

*These will be validated during Lesson 3 (Requirements Design).*

## Blockers & Dependencies

- **Blocked on**: Previous TUI attempt analysis (branch, commit, or description needed)
- **Depends on**: Understanding current Phase 1 state (done; see `00-state-audit.md`)
- **Next gate**: TUI requirements clarification before Lesson 3

## Documentation Improvements Needed (In Subject Repo)

- [ ] Add examples for each provider (IRenderProvider, IInputProvider, IUserInterfaceProvider, IAudioPlayer, IAssetProvider)
- [ ] Explain capability negotiation with code examples
- [ ] Add TUI-specific guidance (requirements, design patterns, pitfalls)
- [ ] Create checklist for implementing a new adapter
- [ ] Improve Phase 2 plan (clarify what's actually left to do)
- [ ] Add architecture diagram or flowchart for frame cycle

## References

- **State Audit**: `docs/00-state-audit.md`
- **Mission**: `MISSION.md`
- **Resources**: `RESOURCES.md`
- **Subject Repo**: `GameEngineAdapter/`
- **Phase 1 Plan**: `GameEngineAdapter/docs/plans/phase-1-interface-development.md`
