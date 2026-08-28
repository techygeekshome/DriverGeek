namespace DriverGeek.Core.Services;

/// <summary>A scheduled scan, expressed in the terms schtasks.exe takes.</summary>
public sealed record ScanPlan(bool NeedsScheduledTask, string Describe, string Frequency, string StartTime, string Day)
{
    public static ScanPlan Manual() => new(false, "Only when you press Scan", "", "", "");
}

/// <summary>
/// Turns the Settings schedule choice into a schtasks.exe task definition. Three non-obvious
/// choices: no /RU or /RP, so the task registers for the current account, runs only when logged
/// on and never needs a stored password; /RL HIGHEST, or the task stalls behind a UAC prompt;
/// and a flat task name, because schtasks will not create a folder and registering into one that
/// does not exist silently does nothing.
/// </summary>
public static class ScanSchedule
{
    public const string TaskName = "DriverGeek Scheduled Scan";

    public static ScanPlan Parse(string? choice)
    {
        var text = (choice ?? "").Trim();

        return text switch
        {
            "Daily at 03:00" => new ScanPlan(true, "Every day at 03:00", "DAILY", "03:00", ""),
            "Daily at 12:00" => new ScanPlan(true, "Every day at 12:00", "DAILY", "12:00", ""),
            "Weekly on Sunday" => new ScanPlan(true, "Every Sunday at 03:00", "WEEKLY", "03:00", "SUN"),
            "Every time DriverGeek starts" => new ScanPlan(false, "Every time you open DriverGeek", "", "", ""),
            "Manually only" => ScanPlan.Manual(),
            _ => ScanPlan.Manual()
        };
    }

    /// <summary>
    /// The schtasks command line. Empty when no task is needed, which is also the signal to
    /// delete any task already registered rather than leaving an orphan behind.
    /// </summary>
    public static string CreateCommand(ScanPlan plan, string exePath)
    {
        if (!plan.NeedsScheduledTask) return "";

        var day = plan.Frequency == "WEEKLY" ? $" /d {plan.Day}" : "";
        return $"/create /f /tn \"{TaskName}\" /tr \"\\\"{exePath}\\\" --scan\" " +
               $"/sc {plan.Frequency}{day} /st {plan.StartTime} /rl HIGHEST";
    }

    public static string DeleteCommand() => $"/delete /f /tn \"{TaskName}\"";

    public static IReadOnlyList<string> Options =>
    [
        "Daily at 03:00",
        "Daily at 12:00",
        "Weekly on Sunday",
        "Every time DriverGeek starts",
        "Manually only"
    ];
}
