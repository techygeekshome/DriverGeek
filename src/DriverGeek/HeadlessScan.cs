using DriverGeek.Services;

namespace DriverGeek;

/// <summary>
/// The scheduled scan. No window, no interaction, and - the important part - no way to change
/// anything. It reads, it writes a line to the log, and it exits.
/// </summary>
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
            // A scheduled task that throws leaves a red entry in Task Scheduler and no
            // explanation. Write the reason down where a person can find it.
            Log.Write("Scheduled scan failed: " + ex);
            return 1;
        }
    }
}
