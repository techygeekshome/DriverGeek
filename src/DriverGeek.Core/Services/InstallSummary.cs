using DriverGeek.Core.Models;

namespace DriverGeek.Core.Services;

/// <summary>The one line that goes in the status bar after a run of installs.</summary>
public static class InstallSummary
{
    public static string For(IReadOnlyList<InstallReport> reports)
    {
        if (reports.Count == 0) return "Nothing was installed.";

        var done = reports.Count(r => r.Succeeded);
        var failed = reports.Count - done;
        var reboot = reports.Any(r => r.RebootRequired);

        var line = done switch
        {
            0 => failed == 1 ? "The driver was not installed." : $"None of the {failed} drivers were installed.",
            1 when failed == 0 => "1 driver installed.",
            _ when failed == 0 => $"{done} drivers installed.",
            _ => $"{done} installed, {failed} not."
        };

        return reboot ? line + " Windows wants a restart to finish." : line;
    }
}
