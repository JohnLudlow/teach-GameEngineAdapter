namespace UiContract;

public readonly record struct Size(int Width, int Height);

// Immutable consumer-owned UI description. Ids are stable across updates so a later
// incremental-update API can address nodes without replacing the whole tree.
public abstract record UiNode(string Id, bool Enabled);

public sealed record PanelNode(string Id, IReadOnlyList<UiNode> Children, bool Enabled = true)
    : UiNode(Id, Enabled);

public sealed record ButtonNode(string Id, string Label, bool Enabled = true)
    : UiNode(Id, Enabled);

public sealed record ListItem(string Id, string Text);

public sealed record ListNode(
    string Id,
    IReadOnlyList<ListItem> Items,
    string? InitialSelectedItemId,
    bool Enabled = true) : UiNode(Id, Enabled);

public sealed record UiTree(PanelNode Root, Size MinimumSize);

public enum UiKey
{
    Tab,
    ShiftTab,
    Up,
    Down,
    Left,
    Right,
    Space,
    Enter,
}

public abstract record UiInput;

public sealed record KeyInput(UiKey Key) : UiInput;

// The adapter owns hit-testing; the contract only sees which control was clicked.
public sealed record ClickInput(string ControlId) : UiInput;

public abstract record ConsumerEvent;

public sealed record ButtonActivated(string ButtonId) : ConsumerEvent;

public sealed record ListItemActivated(string ListId, string ItemId) : ConsumerEvent;

public enum SelectionChangeReason
{
    Navigation,
    Recovery,
}

public sealed record SelectionChanged(string ListId, string? ItemId, SelectionChangeReason Reason)
    : ConsumerEvent;

public enum LayoutOutcome
{
    Normal,
    MinimumSizeFallback,
}

// Everything an adapter needs to make the current state visible. Deliberately free of
// terminal concepts (no cells, cursors or escape sequences).
public sealed record RenderFrame(
    long Number,
    UiTree Tree,
    Size Viewport,
    LayoutOutcome Layout,
    string? FocusedId,
    IReadOnlyDictionary<string, string?> Selections);

public sealed class InvalidUiTreeException(string message) : Exception(message);
