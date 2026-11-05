using UiContract;

namespace TerminalGuiDemo;

/// <summary>Consumer-side state: the source of truth that rebuilds the immutable tree on change.</summary>
public sealed class DemoConsumer(UiSession session)
{
    private readonly List<ListItem> _items =
    [
        new("a", "Alpha"), new("b", "Bravo"), new("c", "Charlie"), new("d", "Delta"),
    ];

    private string? _selected = "a";

    public bool QuitRequested { get; private set; }

    public IReadOnlyList<ConsumerEvent> Log => _log;

    private readonly List<ConsumerEvent> _log = [];

    public void Start() => session.Update(BuildTree());

    public void Process(IEnumerable<ConsumerEvent> events)
    {
        foreach (var e in events)
        {
            _log.Add(e);
            switch (e)
            {
                case SelectionChanged changed:
                    _selected = changed.ItemId;
                    break;
                case ButtonActivated { ButtonId: "remove" } when _selected is not null:
                    _items.RemoveAll(i => i.Id == _selected);
                    session.Update(BuildTree());
                    break;
                case ButtonActivated { ButtonId: "quit" }:
                    QuitRequested = true;
                    break;
            }
        }
    }

    private UiTree BuildTree() => new(
        new PanelNode("root",
        [
            new ListNode("items", [.. _items], _selected),
            new ButtonNode("remove", "Remove selected"),
            new ButtonNode("quit", "Quit"),
        ]),
        new Size(40, 14));
}
