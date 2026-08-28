using DriverGeek.Core.Models;

namespace DriverGeek.Core.Services;

/// <summary>
/// What DriverGeek is willing to call a problem.
///
/// This is the honesty rule of the whole application, in one class. Every paid tool in this
/// category counts an old driver as a "found issue", because a scan that reports nothing wrong
/// cannot sell you a fix. A 2019 driver for a 2019 chipset that works is not a problem, and
/// DriverGeek will not say it is.
///
/// So there is exactly one thing that makes a device actionable: Windows Update has a NEWER
/// driver for it. Age on its own is reported as a fact, never as a finding.
/// </summary>
public static class StalenessPolicy
{
    /// <summary>
    /// Picks the best available update for a device, or null when nothing applies.
    /// "Best" means the highest version we can actually parse; an update whose version we cannot
    /// read is not treated as an upgrade.
    /// </summary>
    public static DriverUpdate? BestUpdateFor(DeviceDriver device, IEnumerable<DriverUpdate> updates)
    {
        DriverUpdate? best = null;

        foreach (var candidate in updates)
        {
            if (!Matches(device, candidate)) continue;
            if (!DriverVersion.IsNewer(candidate.DriverVersion, device.DriverVersion)) continue;
            if (best is null || DriverVersion.IsNewer(candidate.DriverVersion, best.DriverVersion))
                best = candidate;
        }

        return best;
    }

    public static DeviceStatus StatusFor(DeviceDriver device, IEnumerable<DriverUpdate> updates)
    {
        var best = BestUpdateFor(device, updates);
        if (best is null) return DeviceStatus.Current;
        return best.IsOptional ? DeviceStatus.UpdateHiddenAsOptional : DeviceStatus.UpdateOffered;
    }

    /// <summary>
    /// Is this update for this device? Hardware ID is the reliable answer; the model and
    /// manufacturer pair is a fallback for updates that do not carry one.
    /// </summary>
    public static bool Matches(DeviceDriver device, DriverUpdate update)
    {
        if (!string.IsNullOrWhiteSpace(update.DriverHardwareId) &&
            !string.IsNullOrWhiteSpace(device.DeviceId) &&
            device.DeviceId.Contains(update.DriverHardwareId, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrWhiteSpace(update.DriverModel)) return false;
        if (!device.DeviceName.Equals(update.DriverModel, StringComparison.OrdinalIgnoreCase)) return false;

        // A model name alone is not enough - "Wireless Adapter" is not unique. Require the
        // manufacturer to agree too, when both sides state one.
        if (string.IsNullOrWhiteSpace(update.DriverManufacturer) ||
            string.IsNullOrWhiteSpace(device.Manufacturer)) return false;

        return device.Manufacturer.Equals(update.DriverManufacturer, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A plain-English age note. This is deliberately NOT a warning: it is shown next to the
    /// driver date and says nothing about whether anything should be done.
    /// </summary>
    public static string AgeNote(DateTime? driverDate, DateTime today)
    {
        if (driverDate is null) return "";
        var years = (int)((today - driverDate.Value).TotalDays / 365.25);
        return years switch
        {
            < 0 => "",
            0 => "this year",
            1 => "a year old",
            _ => $"{years} years old"
        };
    }
}
