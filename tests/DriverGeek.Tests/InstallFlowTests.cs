using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class InstallFlowTests
{
    private static InstallReport Ok(string name, bool reboot = false) => new()
    {
        DeviceName = name, Stage = InstallStage.Done, Succeeded = true, RebootRequired = reboot
    };

    private static InstallReport Refused(string name) => new()
    {
        DeviceName = name, Stage = InstallStage.Refused, Succeeded = false
    };

    public static void Run()
    {
        Check.Section("Restore points Windows will not take twice");

        var now = new DateTime(2026, 8, 29, 18, 0, 0);

        Check.That("no restore point at all never counts",
            !RestorePointPolicy.CountsAsRecent(null, now));
        Check.That("one from an hour ago counts",
            RestorePointPolicy.CountsAsRecent(now.AddHours(-1), now));
        Check.That("one from just inside the day counts",
            RestorePointPolicy.CountsAsRecent(now.AddHours(-23), now));
        Check.That("one from two days ago does not",
            !RestorePointPolicy.CountsAsRecent(now.AddDays(-2), now));
        Check.That("a clock that says the future does not count either",
            !RestorePointPolicy.CountsAsRecent(now.AddHours(1), now));
        Check.That("the note says which point is being used",
            RestorePointPolicy.ReusedNote(now).Contains("18:00"));

        Check.Section("What the status line says after installing");

        Check.Equal("nothing chosen", "Nothing was installed.", InstallSummary.For([]));
        Check.Equal("one that worked", "1 driver installed.", InstallSummary.For([Ok("Mouse")]));
        Check.Equal("two that worked", "2 drivers installed.", InstallSummary.For([Ok("Mouse"), Ok("Net")]));
        Check.Equal("one that did not", "The driver was not installed.", InstallSummary.For([Refused("Mouse")]));
        Check.Equal("none of several", "None of the 2 drivers were installed.",
            InstallSummary.For([Refused("Mouse"), Refused("Net")]));
        Check.Equal("a mixture", "1 installed, 1 not.", InstallSummary.For([Ok("Mouse"), Refused("Net")]));
        Check.That("a restart is mentioned when Windows asks for one",
            InstallSummary.For([Ok("Display", reboot: true)]).Contains("restart"));
        Check.That("and is not mentioned when it does not",
            !InstallSummary.For([Ok("Mouse")]).Contains("restart"));

        Check.Section("Whether the machine was touched");

        Check.That("a refusal changed nothing", Refused("Mouse").NothingChanged);
        Check.That("a failed restore point changed nothing",
            new InstallReport { Stage = InstallStage.RestorePoint }.NothingChanged);
        Check.That("a failed export changed nothing",
            new InstallReport { Stage = InstallStage.Export }.NothingChanged);
        Check.That("a failed download changed nothing",
            new InstallReport { Stage = InstallStage.Download }.NothingChanged);
        Check.That("a failure during the install itself might have",
            !new InstallReport { Stage = InstallStage.Install }.NothingChanged);
        Check.That("and a finished install certainly did", !Ok("Mouse").NothingChanged);
    }
}
