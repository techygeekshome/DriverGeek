using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class GateTests
{
    private const string Scsi = "{4d36e97b-e325-11ce-bfc1-08002be10318}";
    private const string Display = "{4d36e968-e325-11ce-bfc1-08002be10318}";

    private static DeviceDriver Ordinary() => new() { DeviceName = "Logitech USB Input Device" };
    private static DeviceDriver Storage() => new() { DeviceName = "Samsung NVMe Controller", ClassGuid = Scsi };

    private static InstallContext Good() => new()
    {
        SystemProtectionEnabled = true,
        RestorePointCreated = true,
        CurrentDriverExported = true,
        Unattended = false,
        ExplicitlyChosen = true,
        RunningElevated = true
    };

    public static void Run()
    {
        Check.Section("The install gate");

        Check.That("a normal device with everything in place is allowed",
            InstallGate.CanInstall(Ordinary(), Good()).Allowed);

        // The refusals, in the order the gate applies them.
        var storage = InstallGate.CanInstall(Storage(), Good());
        Check.That("a boot-critical device is refused outright", !storage.Allowed);
        Check.That("and the refusal explains why in plain words",
            storage.Reason.Contains("boot-critical") && storage.Reason.Contains("unbootable"));

        Check.That("a boot-critical device is refused even with everything else perfect",
            !InstallGate.CanInstall(Storage(), Good() with { RunningElevated = true }).Allowed);

        Check.That("an unattended run is refused",
            !InstallGate.CanInstall(Ordinary(), Good() with { Unattended = true }).Allowed);
        Check.That("an unticked device is refused",
            !InstallGate.CanInstall(Ordinary(), Good() with { ExplicitlyChosen = false }).Allowed);
        Check.That("running unelevated is refused",
            !InstallGate.CanInstall(Ordinary(), Good() with { RunningElevated = false }).Allowed);

        var noProtection = InstallGate.CanInstall(Ordinary(), Good() with { SystemProtectionEnabled = false });
        Check.That("System Protection off is refused", !noProtection.Allowed);
        Check.That("and says the install was refused rather than risked",
            noProtection.Reason.Contains("refused rather than risked"));

        Check.That("a restore point that did not happen is refused",
            !InstallGate.CanInstall(Ordinary(), Good() with { RestorePointCreated = false }).Allowed);
        Check.That("a driver export that did not happen is refused",
            !InstallGate.CanInstall(Ordinary(), Good() with { CurrentDriverExported = false }).Allowed);

        // Boot-critical is checked first, so it is the reason reported even when something
        // fixable is also wrong.
        var both = InstallGate.CanInstall(Storage(), Good() with { SystemProtectionEnabled = false });
        Check.That("boot-critical is reported ahead of a fixable problem",
            both.Reason.Contains("boot-critical"));

        Check.Section("Warnings that are not refusals");

        Check.That("a display driver warns about the screen going black",
            InstallGate.WarningFor(new DeviceDriver { ClassGuid = Display }).Contains("black"));
        Check.Equal("an ordinary device has nothing to warn about",
            "", InstallGate.WarningFor(Ordinary()));
    }
}
