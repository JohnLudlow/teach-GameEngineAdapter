using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using UiContract;
using Size = UiContract.Size;

namespace TerminalGuiDemo;

/// <summary>
/// Thin Terminal.Gui adapter: translates native keys/clicks into contract inputs and paints
/// <see cref="RenderFrame"/>s. All behavioural decisions stay in <see cref="UiSession"/>.
/// </summary>
public sealed class TerminalGuiAdapter
{
    private readonly IApplication _app;
    private readonly UiSession _session;
    private readonly Dictionary<string, View> _views = [];
    private readonly Label _fallback = new() { Visible = false, X = 0, Y = 0 };
    private UiTree? _paintedTree;

    public TerminalGuiAdapter(IApplication app, UiSession session)
    {
        _app = app;
        _session = session;
        Window = new Window { Title = "GameEngineAdapter prototype" };
        Window.Add(_fallback);
        _app.Keyboard.KeyDown += OnKeyDown;
    }

    public Window Window { get; }

    public int FramesPresented { get; private set; }

    // Called from the application loop: sync viewport, then paint only if the session says something changed.
    public void Pump()
    {
        var screen = _app.Screen.Size;
        _session.Resize(new Size(screen.Width, screen.Height));

        if (!_session.TryTakeFrame(out var frame))
        {
            return;
        }

        Present(frame);
        FramesPresented++;
    }

    public IReadOnlyList<ConsumerEvent> DrainEvents() => _session.DrainEvents();

    private void OnKeyDown(object? sender, Key key)
    {
        UiKey? mapped = null;
        if (key == Key.Tab.WithShift) { mapped = UiKey.ShiftTab; }
        else if (key == Key.Tab) { mapped = UiKey.Tab; }
        else if (key == Key.CursorUp) { mapped = UiKey.Up; }
        else if (key == Key.CursorDown) { mapped = UiKey.Down; }
        else if (key == Key.CursorLeft) { mapped = UiKey.Left; }
        else if (key == Key.CursorRight) { mapped = UiKey.Right; }
        else if (key == Key.Space) { mapped = UiKey.Space; }
        else if (key == Key.Enter) { mapped = UiKey.Enter; }

        if (mapped is { } uiKey)
        {
            _session.Handle(new KeyInput(uiKey));
            key.Handled = true;
        }
    }

    private void Present(RenderFrame frame)
    {
        if (!ReferenceEquals(_paintedTree, frame.Tree))
        {
            Rebuild(frame.Tree);
            _paintedTree = frame.Tree;
        }

        var fallback = frame.Layout == LayoutOutcome.MinimumSizeFallback;
        _fallback.Visible = fallback;
        _fallback.Text = $"Terminal too small. Need {frame.Tree.MinimumSize.Width}x{frame.Tree.MinimumSize.Height}.";

        foreach (var view in _views.Values)
        {
            view.Visible = !fallback;
        }

        if (fallback)
        {
            return;
        }

        foreach (var (id, itemId) in frame.Selections)
        {
            if (_views[id] is ListView list && frame.Tree.Root is { } root
                && FindList(root, id) is { } node)
            {
                var index = node.Items.ToList().FindIndex(i => i.Id == itemId);
                list.SelectedItem = index < 0 ? null : index;
            }
        }

        if (frame.FocusedId is { } focused && _views.TryGetValue(focused, out var target) && !target.HasFocus)
        {
            target.SetFocus();
        }

        Window.SetNeedsDraw();
    }

    private void Rebuild(UiTree tree)
    {
        foreach (var view in _views.Values)
        {
            Window.Remove(view);
            view.Dispose();
        }

        _views.Clear();
        var row = 2;
        foreach (var node in Flatten(tree.Root))
        {
            View? view = node switch
            {
                ButtonNode button => new Button { Text = button.Label, Y = row, Enabled = button.Enabled },
                ListNode list => new ListView
                {
                    Y = row,
                    Width = 30,
                    Height = Math.Max(list.Items.Count, 1),
                    Source = new ListWrapper<string>([.. list.Items.Select(i => i.Text)]),
                    Enabled = list.Enabled,
                },
                _ => null,
            };

            if (view is null)
            {
                continue;
            }

            var id = node.Id;
            view.MouseEvent += (_, mouse) =>
            {
                if (mouse.Flags.HasFlag(MouseFlags.LeftButtonClicked))
                {
                    _session.Handle(new ClickInput(id));
                    mouse.Handled = true;
                }
            };
            _views[id] = view;
            Window.Add(view);
            row += (node is ListNode listNode ? Math.Max(listNode.Items.Count, 1) : 1) + 1;
        }
    }

    private static IEnumerable<UiNode> Flatten(UiNode node)
    {
        yield return node;
        if (node is PanelNode panel)
        {
            foreach (var descendant in panel.Children.SelectMany(Flatten))
            {
                yield return descendant;
            }
        }
    }

    private static ListNode? FindList(UiNode root, string id) =>
        Flatten(root).OfType<ListNode>().FirstOrDefault(l => l.Id == id);
}
