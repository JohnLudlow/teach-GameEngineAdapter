# Teaching Mission: GameEngineAdapter Onboarding & TUI Implementation

## Your Goal

You are an **advanced C# developer** returning to the **GameEngineAdapter** project after a few months away. Your
primary mission is to **continue implementing the project**, with a secondary focus on
**evaluating and understanding the architecture**. re**.

## The Project

**GameEngineAdapter** is a C# library providing stable, minimal adapter contracts for decoupling game logic from
engine-specific code. It enables a single game to run on multiple engines (MonoGame, Stride, Raylib, TUI, Headless,
etc.) by defining a clear boundary between simulation and engine bindings.

## Your Burning Questions

You want to understand:

1. **Does the implementation match the diagram, documented plans, and project tasks?**
   - Is what's coded in `src/` aligned with what's documented in `Phase 1`?
   - What's been done? What's marked as done but missing?

2. **Current state snapshot**
   - What is implemented, what is missing?
   - Any mismatches between code and documentation?

3. **How do we proceed?**
   - What's the next logical step after Phase 1?
   - How does Phase 2 (Headless adapter) fit in?

4. **TUI Implementation Focus**
   - A previous AI-assisted TUI implementation failed
   - You want to understand how to build an effective TUI implementation
   - This is a concrete, near-term focus for the work

## Learning Path

This teaching will help you:

- **Restore context** on the project structure, implemented interfaces, and test suite
- **Audit the current state** — verify that code matches the documented plan
- **Identify gaps** — what's documented but not implemented, or vice versa
- **Build TUI-ready foundation** — understand how a TUI adapter would integrate with the current interfaces
- **Plan next steps** — decide how to proceed safely without breaking existing work

## Success Criteria

- [ ] You can explain the project structure and current state to someone else
- [ ] You've identified any mismatches between code, documentation, and planned tasks
- [ ] You understand how a TUI adapter fits into the architecture
- [ ] You have a clear plan for the next phase of work
- [ ] Documentation is improved so onboarding can happen without this teach repo

## Notes

- You have strong C# fundamentals; teaching will focus on game architecture and project-specific patterns, not language
  features.
- This is a beginner-level game developer; game concepts (4x mechanics, realtime vs. turn-based, etc.) may need
  grounding.
- Previous TUI attempt failed — we'll examine why and build a better model.
