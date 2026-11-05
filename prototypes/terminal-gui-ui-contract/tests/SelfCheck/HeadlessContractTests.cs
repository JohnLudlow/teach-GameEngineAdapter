using UiContract;

namespace SelfCheck;

// Headless path: drives the contract only, no terminal library involved.
public class HeadlessContractTests
{
    private static ListNode Items(params string[] ids) =>
        new("items", [.. ids.Select(i => new ListItem(i, i.ToUpperInvariant()))], ids.FirstOrDefault());

    private static UiTree Tree(ListNode list, bool removeEnabled = true) => new(
        new PanelNode("root", [list, new ButtonNode("btn", "Go", removeEnabled), new ButtonNode("other", "Other")]),
        new Size(40, 10));

    private static UiSession Start(UiTree tree, Size? viewport = null)
    {
        var session = new UiSession();
        session.Resize(viewport ?? new Size(80, 24));
        session.Update(tree);
        session.TryTakeFrame(out _);
        return session;
    }

    [Fact]
    public void TabCyclesDeterministicallyAndWraps()
    {
        var s = Start(Tree(Items("a", "b")));
        Assert.Equal("items", s.FocusedId);
        s.Handle(new KeyInput(UiKey.Tab));
        Assert.Equal("btn", s.FocusedId);
        s.Handle(new KeyInput(UiKey.Tab));
        s.Handle(new KeyInput(UiKey.Tab));
        Assert.Equal("items", s.FocusedId);
        s.Handle(new KeyInput(UiKey.ShiftTab));
        Assert.Equal("other", s.FocusedId);
    }

    [Theory]
    [InlineData(UiKey.Space)]
    [InlineData(UiKey.Enter)]
    public void ButtonActivatesOncePerKeyPress(UiKey key)
    {
        var s = Start(Tree(Items("a")));
        s.Handle(new KeyInput(UiKey.Tab));
        s.Handle(new KeyInput(key));
        Assert.Equal([new ButtonActivated("btn")], s.DrainEvents());
    }

    [Fact]
    public void ClickFocusesThenActivatesOnce()
    {
        var s = Start(Tree(Items("a")));
        s.Handle(new ClickInput("other"));
        Assert.Equal("other", s.FocusedId);
        Assert.Equal([new ButtonActivated("other")], s.DrainEvents());
    }

    [Fact]
    public void DisabledFocusedButtonKeepsFocusButDoesNotActivate()
    {
        var s = Start(Tree(Items("a")));
        s.Handle(new KeyInput(UiKey.Tab));
        s.Update(Tree(Items("a"), removeEnabled: false));
        s.Handle(new KeyInput(UiKey.Enter));
        Assert.Equal("btn", s.FocusedId);
        Assert.Empty(s.DrainEvents());
        s.Update(Tree(Items("a")));
        s.Handle(new KeyInput(UiKey.Enter));
        Assert.Equal([new ButtonActivated("btn")], s.DrainEvents());
    }

    [Fact]
    public void RemovingFocusedControlMovesFocusToNextWrapping()
    {
        var s = Start(Tree(Items("a")));
        s.Handle(new KeyInput(UiKey.ShiftTab));
        Assert.Equal("other", s.FocusedId);
        s.Update(new UiTree(new PanelNode("root", [Items("a"), new ButtonNode("btn", "Go")]), new Size(40, 10)));
        Assert.Equal("items", s.FocusedId);
    }

    [Fact]
    public void ListNavigationClampsAtBounds()
    {
        var s = Start(Tree(Items("a", "b")));
        s.Handle(new KeyInput(UiKey.Up));
        Assert.Equal("a", s.Selections["items"]);
            Assert.Empty(s.DrainEvents());
            s.Handle(new KeyInput(UiKey.Down));
        s.Handle(new KeyInput(UiKey.Down));
        Assert.Equal("b", s.Selections["items"]);
        Assert.Single(s.DrainEvents());
    }

    [Theory]
    [InlineData("b", "c")]
    [InlineData("c", "b")]
    public void RemovedSelectionRecoversToNextElsePreviousAndIsReported(string selected, string expected)
    {
        var s = Start(Tree(Items("a", "b", "c")));
        while (s.Selections["items"] != selected)
        {
            s.Handle(new KeyInput(UiKey.Down));
        }

        s.DrainEvents();
        var remaining = new[] { "a", "b", "c" }.Where(i => i != selected).ToArray();
        s.Update(Tree(Items(remaining)));

        Assert.Equal(expected, s.Selections["items"]);
        Assert.Equal([new SelectionChanged("items", expected, SelectionChangeReason.Recovery)], s.DrainEvents());
    }

    [Fact]
    public void ListRefilledAfterEmptyGetsItsInitialSelection()
    {
        var s = Start(Tree(Items("a")));
        s.Update(Tree(new ListNode("items", [], null)));
        Assert.Null(s.Selections["items"]);
        s.Update(Tree(Items("x", "y")));
        Assert.Equal("x", s.Selections["items"]);
    }

    [Fact]
    public void TabFromDisabledFocusedControlMovesToItsNeighbour()
    {
        var s = Start(Tree(Items("a")));
        s.Handle(new KeyInput(UiKey.Tab));
        s.Update(Tree(Items("a"), removeEnabled: false));
        Assert.Equal("btn", s.FocusedId);
        s.Handle(new KeyInput(UiKey.Tab));
        Assert.Equal("other", s.FocusedId);
    }

    [Fact]
    public void MissingOrUnknownInitialSelectionIsRejected()
    {
        var s = new UiSession();
        Assert.Throws<InvalidUiTreeException>(() => s.Update(Tree(Items("a") with { InitialSelectedItemId = null })));
        Assert.Throws<InvalidUiTreeException>(() => s.Update(Tree(Items("a") with { InitialSelectedItemId = "zzz" })));
    }

    [Fact]
    public void ResizeBelowMinimumShowsFallbackWithoutChangingState()
    {
        var s = Start(Tree(Items("a", "b")));
        s.Handle(new KeyInput(UiKey.Down));
        s.Handle(new KeyInput(UiKey.Tab));
        s.Resize(new Size(20, 5));
        Assert.True(s.TryTakeFrame(out var frame));
        Assert.Equal(LayoutOutcome.MinimumSizeFallback, frame.Layout);
        Assert.Equal("btn", frame.FocusedId);
        Assert.Equal("b", frame.Selections["items"]);

        s.Resize(new Size(80, 24));
        Assert.True(s.TryTakeFrame(out frame));
        Assert.Equal(LayoutOutcome.Normal, frame.Layout);
        Assert.Equal("btn", frame.FocusedId);
    }

    [Fact]
    public void VisibleChangesRenderByNextFrameAndIdleDoesNot()
    {
        var s = Start(Tree(Items("a", "b")));
        Assert.False(s.TryTakeFrame(out _));
        s.Handle(new KeyInput(UiKey.Down));
        Assert.True(s.TryTakeFrame(out _));
        Assert.False(s.TryTakeFrame(out _));
        s.Handle(new KeyInput(UiKey.Down));
        Assert.False(s.TryTakeFrame(out _));
    }
}
