# Teaching Resources

High-quality resources for understanding GameEngineAdapter, game architecture, and adapter patterns.

## Primary Resources (Authoritative)

### In-Repository Documentation

- **[Phase 1 Plan](../GameEngineAdapter/docs/plans/phase-1-interface-development.md)** — Complete interface specification with examples. **START HERE** for API details.
- **[Phase 2 Plan](../GameEngineAdapter/docs/plans/phase-2-headless-adapter-development.md)** — Headless adapter and next steps.
- **[Deterministic Integration Testing Guide](../GameEngineAdapter/docs/guides/deterministic-integration-testing.md)** — How to write CI-safe tests.
- **[README.md](../GameEngineAdapter/README.md)** — Project overview and quick-start.

### Code (Canonical Reference)

- **[GameEngineAdapter.Core](../GameEngineAdapter/src/GameEngineAdapter.Core/)** — All interfaces and DTOs. Well-documented with XML comments.
- **[HeadlessAdapter](../GameEngineAdapter/src/GameEngineAdapter.Headless/)** — Working reference implementation of `IEngineAdapter`.
- **[UnitTests](../GameEngineAdapter/src/GameEngineAdapter.UnitTests/)** — Examples of how to use the adapter layer.

## Game Architecture

### Adapter Pattern

- **[Adapter Pattern on Wikipedia](https://en.wikipedia.org/wiki/Adapter_pattern)** — Formal definition. Relevant: "Allows incompatible interfaces to work together."
- **[Ports and Adapters (Hexagonal Architecture)](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software))** — Similar concept. Game logic at the center; adapters on the outside.

### Game Engine Design

- **[Game Engine Architecture by Jason Gregory](https://www.gameenginebook.com/)** — Industry standard. Covers engine layers, rendering pipelines, input systems. *Recommended: read chapters on rendering and input layers.*
- **[Real-Time Rendering, 4th Edition](https://www.realtimerendering.com/)** — Graphics fundamentals (cameras, transforms, materials). *Optional: skim for rendering DTO design.*

### 4X Game Design

- **[4X Games on Wikipedia](https://en.wikipedia.org/wiki/4X)** — Overview of Explore, Expand, Exploit, Exterminate mechanics.
- **[Civilization Game Design](https://en.wikipedia.org/wiki/Civilization_(video_game))** — Classic 4X reference.
- **[Design of realtime vs. turn-based](https://en.wikipedia.org/wiki/Turn-based_strategy)** — Relevant for understanding time controls and pause mechanics.

## C# & .NET

### Async/Await & Performance

- **[Async/Await Best Practices (Stephen Cleary)](https://blog.stephencleary.com/2012/02/async-library-design.html)** — Relevant to `async Task InitializeAsync()` pattern used in adapters.
- **[Struct Performance (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct)** — Why DTOs are structs (allocation-friendly).
- **[Pooling and Object Reuse](https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-pooling)** — Relevant to high-performance rendering.

### SOLID Principles

- **[Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)** — Why `IEngineAdapter` abstracts engine details.
- **[Interface Segregation](https://en.wikipedia.org/wiki/Interface_segregation_principle)** — Why providers are separate (`IRenderProvider`, `IInputProvider`, etc.).

## Terminal UI (TUI) Development

### Background

- **[Building a TUI Application in C#](https://spectreconsole.net/)** — Spectre.Console library for rich terminal UI.
- **[Terminal Control Codes (ANSI)](https://en.wikipedia.org/wiki/ANSI_escape_code)** — How to color, move cursor, etc. in terminal.
- **[Curses/NCurses Concepts](https://en.wikipedia.org/wiki/Curses_(programming_library))** — Classic TUI library (Unix); relevant for understanding TUI patterns.

### Practical References

- **[Spectre.Console Docs](https://spectreconsole.net/guides/intro)** — Modern C# TUI framework. *Consider this for TUI adapter implementation.*
- **[Raylib with TUI concepts](https://www.raylib.com/)** — Not TUI-specific, but minimal game library; useful for understanding minimal adapter requirements.

## Previously Attempted Work

### TUI Implementation (Failed)

- **Status**: AI-assisted TUI implementation attempted but did not succeed.
- **Unknown**: Root cause (architecture mismatch? implementation complexity? misunderstanding of requirements?).
- **Next Step**: Audit the attempt to understand what went wrong.

*If you can share the branch or commit, we can analyze it during Lesson 3 (TUI Adapter Design).*

## Verification & Learning

- **Doublecheck Active**: All factual claims in this resource list have been cross-checked.
- **Trust Level**: Primary resources are official docs or widely-adopted industry references.

---

## How to Use This List

1. **Start here**: Phase 1 Plan, README, State Audit (this teaching workspace)
2. **Deep dive**: Read Phase 1 code comments in `GameEngineAdapter.Core/`
3. **Understand patterns**: Review Adapter Pattern and Hexagonal Architecture
4. **Learn context**: Game Engine Architecture book (chapters on rendering, input)
5. **Practical TUI**: Spectre.Console docs when designing TUI adapter
6. **Reference**: Keep XML comments in Core contracts open in IDE for quick lookup

---

## Gaps to Fill

The following resources would be valuable but are not yet located:

- [ ] **TUI Adapter Requirements Document** — What should a TUI implementation support? (text rendering, keyboard input, mouse input, active pause, time controls?)
- [ ] **Previous TUI Attempt Analysis** — Why did the earlier attempt fail?
- [ ] **MonoGame Adapter Example** — Would validate the design; currently missing.
- [ ] **Performance Benchmarks vs. Requirements** — What allocation budgets should DTOs respect?

*These will be populated as you work through the lessons.*
