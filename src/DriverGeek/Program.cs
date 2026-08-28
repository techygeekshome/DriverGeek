using Avalonia;
using DriverGeek.Services;

namespace DriverGeek;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Anything that gets this far would otherwise close the window with no explanation.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log.Write("Unhandled: " + e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Write("Unobserved: " + e.Exception);
            e.SetObserved();
        };

        // --scan is what the scheduled task runs: scan, write the log, exit, no window.
        if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
            return HeadlessScan.Run();

        try
        {
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Write("DriverGeek stopped: " + ex);
            return 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
