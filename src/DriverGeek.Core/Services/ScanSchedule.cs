namespace DriverGeek.Core.Services;

/// <summary>A scheduled scan, turned into something schtasks.exe understands.</summary>
public sealed record ScanPlan(bool NeedsScheduledTask, string Describe, string Frequency, string StartTime, string Day)
{
    public static ScanPlan Manual() => new(false, "Only when you press Scan", "", "", "");
}

/// <summary>
/// Turns the Settings dropdown into a task definition. Pure on purpose, so the awkward part -
/// what the words mean - is tested without going anywhere near Task Scheduler.
///
/// Same shape as AppGeek's ScanSchedule, and the same three non-obvious choices apply, for the
/// same reasons: no /RU or /RP (registers for the current account, runs only when logged on,
/// never prompts for a password), /RL HIGHEST (the manifest requires administrator and the task
/// would otherwise stall behind a UAC prompt at 3am), and a flat task name with no folder
/// (schtasks will not create a folder, and a path into one that does not exist registers
/// nothing at all).
///
/// The one difference from AppGeek is what the task is allowed to do. AppGeek's scheduled task
/// scans and can be told to install. DriverGeek's scans and cannot: see InstallGate.
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
    /// The schtasks command line. Returns empty when the choice needs no task, which is also the
    /// signal to REMOVE any task already registered - switching to "Manually only" must not
    /// leave an orphan behind.
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
