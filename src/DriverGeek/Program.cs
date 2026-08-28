using Avalonia;

namespace DriverGeek;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // --scan is what the scheduled task runs. It scans, writes the log, and exits without
        // ever showing a window - and it cannot install anything, because DriverGeek has no
        // install path that a schedule can reach. See Core/Services/InstallGate.
        if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
            return HeadlessScan.Run();

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
