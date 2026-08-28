using Avalonia;

namespace DriverGeek;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // --scan is what the scheduled task runs: scan, write the log, exit, no window.
        if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
            return HeadlessScan.Run();

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
