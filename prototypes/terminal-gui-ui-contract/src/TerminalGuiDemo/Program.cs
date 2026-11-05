using Terminal.Gui.App;
using TerminalGuiDemo;
using UiContract;

using var app = Application.Create();
app.Init();

var session = new UiSession();
var consumer = new DemoConsumer(session);
consumer.Start();

var adapter = new TerminalGuiAdapter(app, session);
app.AddTimeout(TimeSpan.FromMilliseconds(30), () =>
{
    adapter.Pump();
    consumer.Process(adapter.DrainEvents());
    if (consumer.QuitRequested)
    {
        app.RequestStop();
    }

    return true;
});

app.Run(adapter.Window);
