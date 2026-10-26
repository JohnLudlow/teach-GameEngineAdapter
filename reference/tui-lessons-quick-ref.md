# TUI Implementation — Quick Reference

## The Problem with Previous Attempt

| Issue | Symptom | Root Cause |
|-------|---------|-----------|
| **Flickering** | Screen constantly redrawing | Rendering every frame without buffering |
| **Dead controls** | Checkboxes, buttons unresponsive | Input model broken; event dispatch failed |
| **No mouse** | Can't click anything | Mouse input not captured/handled |
| **Bad focus** | Tab key unreliable | Manual focus state management broken |

**Lesson**: Terminal rendering is **fundamentally different** from graphics. Can't just convert sprite calls to ASCII.

---

## Key Architectural Principles

### 1. **Separate Concerns via Providers**

```plain text
NOT this:
  TUI adapter → map IRenderProvider calls → ASCII art (FRAGILE)

Instead:
  TUI adapter → only implement IUserInterfaceProvider
  Game logic → check Capabilities, use IUserInterfaceProvider for TUI
```

### 2. **Capability Negotiation**

```csharp
// TUI reports what it can actually do
public HeadlessEngineCapabilities Capabilities => new()
{
    Supports2D = false,      // No sprite rendering
    Supports3D = false,      // No 3D meshes
    SupportsAudio = false,   // No audio in terminal
    SupportsText = true      // Text panels, buttons OK
};

// Game logic adapts
if (adapter.Capabilities.SupportsText)
    adapter.UserInterfaceProvider.SubmitPanel(...);
```

### 3. **Event-Driven Input (Not Polling)**

```csharp
// Terminal input (keyboard, mouse) is inherently event-based
// Queue events properly; process them each frame
public class HeadlessInputProvider : IInputProvider
{
    private Queue<InputEvent> _events = new();

    public IInputEvent[] GetInputEvents()
        => _events.Dequeue();  // Deliver and clear per frame
}
```

### 4. **Frame Cycle is Universal**

```plain text
BeginFrame()
  ↓
GetInput()
  ↓
UpdateSimulation()
  ↓
SubmitRenderCommands()  // For TUI: IUserInterfaceProvider only
  ↓
EndFrame()
```

Same cycle for MonoGame, TUI, Headless. TUI just implements it differently.

---

## What TUI Should Support (Design Questions)

### Must-Have

- [ ] Text-based UI rendering (panels, buttons, labels)
- [ ] Keyboard input (arrow keys, enter, escape)
- [ ] Event-driven input model
- [ ] Proper focus/tab management

### Should-Have

- [ ] Mouse input (click, move, scroll)
- [ ] Mouse focus (click to focus)
- [ ] Proper frame pacing (no flicker)

### Nice-to-Have

- [ ] Time controls (speed up, slow down)
- [ ] Active pause support
- [ ] Color/styling in terminal

---

## HeadlessAdapter as Reference

Study this to understand the pattern TUI should follow:

```plain text
GameEngineAdapter.Headless/
├── HeadlessAdapter.cs              # Main IEngineAdapter impl
├── HeadlessRenderProvider.cs        # Records draw calls
├── HeadlessInputProvider.cs         # Queue/deliver events
├── HeadlessAudioPlayer.cs           # Stub (no actual audio)
├── HeadlessAssetProvider.cs         # Asset caching
├── DeterministicTickContext.cs      # Immutable frame state
└── DeterministicEngineRunner.cs     # Replay engine for tests
```

Each provider:

- Implements its interface cleanly
- Records/delivers data per frame
- **Does NOT try to do multiple things**

---

## Testing Pattern

```csharp
[Fact]
public async Task TuiAdapter_RendersTextPanel()
{
    // Setup
    var adapter = new TuiAdapter();
    await adapter.InitializeAsync(new EngineConfig("tui"), ct);

    // Act
    using (var frame = adapter.BeginFrame())
    {
        // Submit UI panel
        adapter.UserInterfaceProvider.SubmitElement(new Panel { Text = "Test" });
    }

    // Assert
    var commands = adapter.RecordedUICommands;
    Assert.Contains(commands, c => c.Type == CommandType.RenderPanel);
}
```

Test strategy: Record commands, assert on frame's recorded output.

---

## Files to Read

1. **Phase 1 Plan** → `GameEngineAdapter/docs/plans/phase-1-interface-development.md`
2. **HeadlessAdapter** → `GameEngineAdapter/src/GameEngineAdapter.Headless/HeadlessAdapter.cs`
3. **IUserInterfaceProvider** → `GameEngineAdapter/src/GameEngineAdapter.Core/IUserInterfaceProvider.cs`
4. **Unit Tests** → `GameEngineAdapter/src/GameEngineAdapter.UnitTests/`

---

## Lessons Sequence

1. ✅ **Lesson 1: Architecture** → understand patterns
2. ⏳ **Lesson 2: Contracts** → read interfaces in detail
3. ⏳ **Lesson 3: TUI Design** → clarify requirements, design contracts
4. ⏳ **Lesson 4: TUI Implementation** → build minimal adapter
5. ⏳ **Lesson 5: Documentation** → improve subject repo docs

---

## Red Flags to Avoid

- ❌ Trying to map sprite rendering to terminal
- ❌ Polling for input instead of event-driven model
- ❌ Re-rendering entire screen every frame (cause of flicker)
- ❌ Manual focus management (let framework handle it)
- ❌ Monolithic TUI code (split into providers; keep it simple)

**Golden rule**: If it's complex, you're doing it wrong. The adapter should be **minimal and focused**.
