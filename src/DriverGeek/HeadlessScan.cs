using DriverGeek.Services;

namespace DriverGeek;

/// <summary>The scheduled scan: no window and no interaction. Reads, writes a log line and exits.</summary>
internal static class HeadlessScan
{
    public static int Run()
    {
        try
        {
            var settings = new SettingsService();
            settings.Load();

            var result = new ScanService().Run(settings.Current);

            Log.Write(result.Error is null
                ? $"Scheduled scan: {result.DeviceCount} devices, {result.UpdateCount} updates " +
                  $"({result.OptionalCount} of them hidden as optional). Nothing was installed."
                : $"Scheduled scan finished with a search problem: {result.Error}");

            return 0;
        }
        catch (Exception ex)
        {
            // Task Scheduler records only that the task failed, so record why.
            Log.Write("Scheduled scan failed: " + ex);
            return 1;
        }
    }
}
