using DriverGeek.Core.Models;

namespace DriverGeek.Core.Services;

/// <summary>The state of the machine at the moment an install is asked for.</summary>
public sealed record InstallContext
{
    /// <summary>System Protection is on for the system drive, so a restore point can be made.</summary>
    public bool SystemProtectionEnabled { get; init; }

    /// <summary>A restore point was actually created and confirmed.</summary>
    public bool RestorePointCreated { get; init; }

    /// <summary>The current driver was exported to disk and the export verified.</summary>
    public bool CurrentDriverExported { get; init; }

    /// <summary>True when this is a scheduled or otherwise unattended run.</summary>
    public bool Unattended { get; init; }

    /// <summary>True when the user ticked this specific device, in this run.</summary>
    public bool ExplicitlyChosen { get; init; }

    public bool RunningElevated { get; init; }
}

public sealed record GateResult(bool Allowed, string Reason)
{
    public static GateResult Allow() => new(true, "");
    public static GateResult Refuse(string reason) => new(false, reason);
}

/// <summary>
/// Whether a driver install may proceed.
///
/// Nothing in here is a setting. These are the conditions under which DriverGeek is willing to
/// replace a working driver at all, and they are checked in the order a person would care about
/// them - the most alarming refusal first.
///
/// DriverGeek 1.0 does not install anything; this gate and its tests exist so that 1.1 has the
/// safety rules already written and proven rather than bolted on next to the install button.
/// </summary>
public static class InstallGate
{
    public static GateResult CanInstall(DeviceDriver device, InstallContext ctx)
    {
        if (DriverClass.IsBootCritical(device.ClassGuid, device.ClassName))
            return GateResult.Refuse(
                "This is a boot-critical device. DriverGeek reports these and never installs over them - " +
                "a storage or chipset driver that goes wrong is an unbootable machine, not a failed update. " +
                "Use the manufacturer's own installer, and take a backup first.");

        if (ctx.Unattended)
            return GateResult.Refuse(
                "Driver installs are never run on a schedule or in the background. A scan may run unattended; " +
                "replacing a driver happens while you are at the machine and watching.");

        if (!ctx.ExplicitlyChosen)
            return GateResult.Refuse(
                "Nothing installs without being ticked. There is no 'update all' in DriverGeek.");

        if (!ctx.RunningElevated)
            return GateResult.Refuse("Installing a driver needs administrator rights.");

        if (!ctx.SystemProtectionEnabled)
            return GateResult.Refuse(
                "System Protection is turned off, so no restore point can be made. Turn it on for the system " +
                "drive and try again - the install is refused rather than risked.");

        if (!ctx.RestorePointCreated)
            return GateResult.Refuse("The restore point was not created. Nothing has been changed.");

        if (!ctx.CurrentDriverExported)
            return GateResult.Refuse(
                "The current driver could not be exported, so there would be nothing to put back. " +
                "Nothing has been changed.");

        return GateResult.Allow();
    }

    /// <summary>
    /// A device the user should be warned about before it is installed - not refused, but not
    /// waved through either. Losing the display or the network mid-install is recoverable and
    /// deeply unpleasant.
    /// </summary>
    public static string WarningFor(DeviceDriver device) =>
        DriverClass.RiskOf(device.ClassGuid, device.ClassName) switch
        {
            DriverRisk.Awkward =>
                "Your screen or input may go black for a few seconds while this installs, and the machine may " +
                "ask to restart. Save anything open first.",
            _ => ""
        };
}
