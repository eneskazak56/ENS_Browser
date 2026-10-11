using CefSharp;
using CefSharp.Wpf;
using System.IO;

namespace ENSBrowser;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        var settings = new CefSettings
        {
            CachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ENSBrowser",
                "Chromium")
        };
        settings.CefCommandLineArgs["disable-features"] = "msEdgeShoppingAssistant";

        if (!Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null))
            throw new InvalidOperationException("Chromium could not be initialized.");

        base.OnStartup(e);
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        Cef.Shutdown();
        base.OnExit(e);
    }
}
