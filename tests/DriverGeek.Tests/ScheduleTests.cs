using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class ScheduleTests
{
    public static void Run()
    {
        Check.Section("Scheduled scanning");

        var daily = ScanSchedule.Parse("Daily at 03:00");
        Check.That("a daily choice needs a task", daily.NeedsScheduledTask);
        Check.Equal("and is daily", "DAILY", daily.Frequency);
        Check.Equal("at the time it says", "03:00", daily.StartTime);

        var weekly = ScanSchedule.Parse("Weekly on Sunday");
        Check.Equal("weekly is weekly", "WEEKLY", weekly.Frequency);
        Check.Equal("on the right day", "SUN", weekly.Day);

        Check.That("'Manually only' needs no task", !ScanSchedule.Parse("Manually only").NeedsScheduledTask);
        Check.That("'Every time DriverGeek starts' needs no task",
            !ScanSchedule.Parse("Every time DriverGeek starts").NeedsScheduledTask);
        Check.That("an unrecognised choice falls back to manual", !ScanSchedule.Parse("nonsense").NeedsScheduledTask);
        Check.That("null falls back to manual", !ScanSchedule.Parse(null).NeedsScheduledTask);

        var cmd = ScanSchedule.CreateCommand(daily, @"C:\Program Files\DriverGeek\DriverGeek.exe");
        Check.That("the command runs the scan switch", cmd.Contains("--scan"));
        Check.That("the command runs at the highest available level", cmd.Contains("/rl HIGHEST"));
        Check.That("the command overwrites an existing task", cmd.Contains("/f "));
        Check.That("the task name has no folder in it", !ScanSchedule.TaskName.Contains('\\'));

        // No /RU or /RP: the task runs as the logged-on user and needs no stored password.
        Check.That("no run-as user is set", !cmd.Contains("/ru "));
        Check.That("no password is ever passed", !cmd.Contains("/rp "));

        Check.Equal("no task means an empty command", "", ScanSchedule.CreateCommand(ScanPlan.Manual(), "x.exe"));
        Check.That("there is a delete command to clear an orphan",
            ScanSchedule.DeleteCommand().Contains("/delete"));

        Check.Equal("the options list is what Settings shows", 5, ScanSchedule.Options.Count);
    }
}
