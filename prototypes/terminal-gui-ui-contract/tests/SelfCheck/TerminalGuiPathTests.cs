using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using TerminalGuiDemo;
using UiContract;

namespace SelfCheck;

// Terminal path: real Terminal.Gui views and input pipeline, observed only through the contract.
public class TerminalGuiPathTests
{
    private static (IApplication App, UiSession Session, DemoConsumer Consumer, TerminalGuiAdapter Adapter) Start()
    {
        var app = Application.Create();
        app.Init(DriverRegistry.Names.ANSI);
        app.Driver!.SetScreenSize(80, 24);

        var session = new UiSession();
        var consumer = new DemoConsumer(session);
        consumer.Start();
        var adapter = new TerminalGuiAdapter(app, session);
        app.Begin(adapter.Window);
        Pump(adapter, consumer);
        return (app, session, consumer, adapter);
    }

    private static void Pump(TerminalGuiAdapter adapter, DemoConsumer consumer)
    {
        adapter.Pump();
        consumer.Process(adapter.DrainEvents());
        adapter.Pump();
        consumer.Process(adapter.DrainEvents());
        adapter.Window.App?.LayoutAndDraw(true);
    }

    [Fact]
    public void NativeKeysDriveContractFocusSelectionAndButtons()
    {
        var (app, session, consumer, adapter) = Start();
        using var _ = app;

        app.Keyboard.RaiseKeyDownEvent(Key.CursorDown);
        Pump(adapter, consumer);
        Assert.Equal("b", session.Selections["items"]);

        app.Keyboard.RaiseKeyDownEvent(Key.Tab);
        app.Keyboard.RaiseKeyDownEvent(Key.Enter);
        Pump(adapter, consumer);

        // "remove" removed b, so selection recovered to c and focus stayed on the button.
        Assert.Equal("c", session.Selections["items"]);
        Assert.Equal("remove", session.FocusedId);
        Assert.Contains(new ButtonActivated("remove"), consumer.Log);
        Assert.Contains(new SelectionChanged("items", "c", SelectionChangeReason.Recovery), consumer.Log);
    }

    [Fact]
    public void NativeMouseClickFocusesAndActivatesButtonOnce()
    {
        var (app, session, consumer, adapter) = Start();
        using var _ = app;

        // Row 7 is the "remove" button inside the window's 1-cell border (y = 8 on screen).
        app.Mouse.RaiseMouseEvent(new Mouse { Flags = MouseFlags.LeftButtonClicked, ScreenPosition = new(4, 8) });
        Pump(adapter, consumer);

        Assert.Equal("remove", session.FocusedId);
        Assert.Single(consumer.Log, e => e is ButtonActivated { ButtonId: "remove" });
    }

    [Fact]
    public void ResizeBelowMinimumRendersFallbackAndRecovers()
    {
        var (app, session, consumer, adapter) = Start();
        using var _ = app;

        app.Driver!.SetScreenSize(20, 5);
        Pump(adapter, consumer);
        Assert.Equal(LayoutOutcome.MinimumSizeFallback, session.Layout);

        app.Driver.SetScreenSize(80, 24);
        Pump(adapter, consumer);
        Assert.Equal(LayoutOutcome.Normal, session.Layout);
        Assert.Equal("items", session.FocusedId);
    }

    [Fact]
    public void IdleDoesNotRepaint()
    {
        var (app, _, consumer, adapter) = Start();
        using var _ = app;

        var before = adapter.FramesPresented;
        for (var i = 0; i < 5; i++)
        {
            Pump(adapter, consumer);
        }

        Assert.Equal(before, adapter.FramesPresented);
    }
}
