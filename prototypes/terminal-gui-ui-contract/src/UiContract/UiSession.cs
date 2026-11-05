namespace UiContract;

/// <summary>
/// Terminal-agnostic reference semantics for the Phase 1 baseline. Tests and the Terminal.Gui adapter
/// both drive this one object, so the contract behaviour is verified once.
/// </summary>
public sealed class UiSession
{
    private readonly Dictionary<string, string?> _selections = [];
    private readonly List<ConsumerEvent> _events = [];
    private UiTree? _tree;
    private Size _viewport;
    private List<UiNode> _focusOrder = [];
    private string? _focusedId;
    private bool _dirty;
    private long _frameNumber;

    public string? FocusedId => _focusedId;

    public LayoutOutcome Layout =>
        _tree is not null && (_viewport.Width < _tree.MinimumSize.Width || _viewport.Height < _tree.MinimumSize.Height)
            ? LayoutOutcome.MinimumSizeFallback
            : LayoutOutcome.Normal;

    public IReadOnlyDictionary<string, string?> Selections => new Dictionary<string, string?>(_selections);

    public bool HasPendingRedraw => _dirty;

    public IReadOnlyList<ConsumerEvent> DrainEvents()
    {
        var drained = _events.ToArray();
        _events.Clear();
        return drained;
    }

    public void Resize(Size viewport)
    {
        if (viewport == _viewport)
        {
            return;
        }

        _viewport = viewport;
        _dirty = true;
    }

    // Whole-tree update. Focus and selection are adapter-held between updates and reconciled here.
    public void Update(UiTree tree)
    {
        var nodes = Flatten(tree.Root).ToList();
        Validate(nodes, existingLists: _selections);

        var oldOrder = _focusOrder.Select(n => n.Id).ToList();
        var oldLists = _tree is null
            ? []
            : Flatten(_tree.Root).OfType<ListNode>().ToDictionary(l => l.Id);
        _tree = tree;
        ReconcileSelections(nodes, oldLists);
        _focusOrder = nodes.Where(IsFocusable).ToList();
        ReconcileFocus(oldOrder);
        _dirty = true;
    }

    public void Handle(UiInput input)
    {
        if (_tree is null || Layout == LayoutOutcome.MinimumSizeFallback)
        {
            return;
        }

        switch (input)
        {
            case KeyInput { Key: UiKey.Tab }:
                MoveFocus(+1);
                break;
            case KeyInput { Key: UiKey.ShiftTab }:
                MoveFocus(-1);
                break;
            case KeyInput key:
                HandleKey(key.Key);
                break;
            case ClickInput click:
                HandleClick(click.ControlId);
                break;
        }
    }

    // Returns a frame only when something visible changed since the last one, so an idle UI never redraws.
    public bool TryTakeFrame(out RenderFrame frame)
    {
        if (!_dirty || _tree is null)
        {
            frame = null!;
            return false;
        }

        _dirty = false;
        frame = new RenderFrame(++_frameNumber, _tree, _viewport, Layout, _focusedId, Selections);
        return true;
    }

    private static IEnumerable<UiNode> Flatten(UiNode node, bool parentEnabled = true)
    {
        // A disabled container disables its whole subtree.
        var effective = node.Enabled && parentEnabled ? node : node with { Enabled = false };
        yield return effective;
        if (node is PanelNode panel)
        {
            foreach (var child in panel.Children)
            {
                foreach (var descendant in Flatten(child, effective.Enabled))
                {
                    yield return descendant;
                }
            }
        }
    }

    private static bool IsFocusable(UiNode node) => node is ButtonNode or ListNode { Items.Count: > 0 } && node.Enabled;

    private static void Validate(List<UiNode> nodes, Dictionary<string, string?> existingLists)
    {
        var seen = new HashSet<string>();
        foreach (var node in nodes)
        {
            if (!seen.Add(node.Id))
            {
                throw new InvalidUiTreeException($"Duplicate control id '{node.Id}'.");
            }

            // Existing non-empty lists keep adapter-held selection; new or refilled lists need a valid initial one.
            if (node is ListNode { Items.Count: > 0 } list && existingLists.GetValueOrDefault(list.Id) is null
                && (list.InitialSelectedItemId is null || list.Items.All(i => i.Id != list.InitialSelectedItemId)))
            {
                throw new InvalidUiTreeException($"List '{list.Id}' needs a valid initial selection.");
            }
        }
    }

    private void ReconcileSelections(List<UiNode> nodes, Dictionary<string, ListNode> oldLists)
    {
        var lists = nodes.OfType<ListNode>().ToDictionary(l => l.Id);

        foreach (var id in _selections.Keys.Where(id => !lists.ContainsKey(id)).ToList())
        {
            _selections.Remove(id);
        }

        foreach (var list in lists.Values)
        {
            if (!_selections.TryGetValue(list.Id, out var current))
            {
                _selections[list.Id] = list.Items.Count == 0 ? null : list.InitialSelectedItemId;
                continue;
            }

            if (current is null && list.Items.Count > 0)
            {
                // Refilled after being empty: behaves like a new list (already validated).
                _selections[list.Id] = list.InitialSelectedItemId;
                continue;
            }

            if (current is null || list.Items.Any(i => i.Id == current))
            {
                continue;
            }

            // Removed selection: the adapter picks next, else previous, and tells the consumer.
            var recovered = Recover(list, oldLists.GetValueOrDefault(list.Id), current);
            _selections[list.Id] = recovered;
            _events.Add(new SelectionChanged(list.Id, recovered, SelectionChangeReason.Recovery));
        }
    }

    // "Next" is defined by the previous item order, so recovery needs the old list.
    private static string? Recover(ListNode list, ListNode? old, string removedId)
    {
        if (list.Items.Count == 0)
        {
            return null;
        }

        var previous = old?.Items.Select(i => i.Id).ToList() ?? [];
        var oldIndex = previous.IndexOf(removedId);
        bool Survives(string id) => list.Items.Any(i => i.Id == id);

        var chosen = oldIndex < 0 ? null : previous.Skip(oldIndex + 1).FirstOrDefault(Survives);
        chosen ??= oldIndex < 0 ? null : previous.Take(oldIndex).LastOrDefault(Survives);
        return chosen ?? list.Items[0].Id;
    }

    private void ReconcileFocus(List<string> oldOrder)
    {
        var newIds = _focusOrder.Select(n => n.Id).ToList();

        if (_focusedId is null)
        {
            _focusedId = newIds.FirstOrDefault();
            return;
        }

        var stillExists = _tree is not null && Flatten(_tree.Root).Any(n => n.Id == _focusedId);
        if (stillExists)
        {
            // Disabled-but-present controls keep focus; they just cannot activate.
            return;
        }

        var oldIndex = oldOrder.IndexOf(_focusedId);
        _focusedId = null;
        for (var step = 1; step <= oldOrder.Count && oldIndex >= 0; step++)
        {
            var candidate = oldOrder[(oldIndex + step) % oldOrder.Count];
            if (newIds.Contains(candidate))
            {
                _focusedId = candidate;
                return;
            }
        }

        _focusedId = newIds.FirstOrDefault();
    }

    private void MoveFocus(int direction)
    {
        if (_focusOrder.Count == 0 || _tree is null)
        {
            return;
        }

        // Scan from the focused control's tree position so a retained-but-disabled focus still has a neighbour.
        var all = Flatten(_tree.Root).Select(n => n.Id).ToList();
        var start = all.IndexOf(_focusedId ?? string.Empty);
        for (var step = 1; step <= all.Count; step++)
        {
            var candidate = start < 0
                ? all[direction > 0 ? step - 1 : all.Count - step]
                : all[((start + direction * step) % all.Count + all.Count) % all.Count];
            if (_focusOrder.Any(n => n.Id == candidate))
            {
                SetFocus(candidate);
                return;
            }
        }
    }

    private void SetFocus(string id)
    {
        if (_focusedId != id)
        {
            _focusedId = id;
            _dirty = true;
        }
    }

    private UiNode? Find(string id) => _tree is null ? null : Flatten(_tree.Root).FirstOrDefault(n => n.Id == id);

    private void HandleClick(string controlId)
    {
        var node = Find(controlId);
        if (node is null || !node.Enabled || !IsFocusable(node))
        {
            return;
        }

        SetFocus(controlId);
        Activate(node);
    }

    private void HandleKey(UiKey key)
    {
        var node = _focusedId is null ? null : Find(_focusedId);
        if (node is null || !node.Enabled)
        {
            return;
        }

        switch (node)
        {
            case ButtonNode when key is UiKey.Space or UiKey.Enter:
                Activate(node);
                break;
            case ListNode list when key is UiKey.Up or UiKey.Down:
                MoveListSelection(list, key == UiKey.Down ? +1 : -1);
                break;
            case ListNode when key is UiKey.Space or UiKey.Enter:
                Activate(node);
                break;
        }
    }

    private void Activate(UiNode node)
    {
        switch (node)
        {
            case ButtonNode button:
                _events.Add(new ButtonActivated(button.Id));
                break;
            case ListNode list when _selections.GetValueOrDefault(list.Id) is { } itemId:
                _events.Add(new ListItemActivated(list.Id, itemId));
                break;
        }
    }

    private void MoveListSelection(ListNode list, int direction)
    {
        var index = list.Items.ToList().FindIndex(i => i.Id == _selections[list.Id]);
        var target = Math.Clamp(index + direction, 0, list.Items.Count - 1);
        if (target == index)
        {
            return;
        }

        var id = list.Items[target].Id;
        _selections[list.Id] = id;
        _events.Add(new SelectionChanged(list.Id, id, SelectionChangeReason.Navigation));
        _dirty = true;
    }
}
