using DriverGeek.Core.Models;

namespace DriverGeek.Core.Services;

/// <summary>
/// Decides whether a device has anything worth reporting. The only thing that makes a device
/// actionable is a newer driver from Windows Update; age on its own is reported as a fact.
/// </summary>
public static class StalenessPolicy
{
    /// <summary>
    /// The highest-versioned applicable update, or null when nothing applies. An update whose
    /// version cannot be parsed is not treated as an upgrade.
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
    /// Hardware ID is the reliable match; model plus manufacturer is a fallback for updates that
    /// carry no hardware ID.
    /// </summary>
    public static bool Matches(DeviceDriver device, DriverUpdate update)
    {
        if (!string.IsNullOrWhiteSpace(update.DriverHardwareId) &&
            !string.IsNullOrWhiteSpace(device.DeviceId) &&
            device.DeviceId.Contains(update.DriverHardwareId, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrWhiteSpace(update.DriverModel)) return false;
        if (!device.DeviceName.Equals(update.DriverModel, StringComparison.OrdinalIgnoreCase)) return false;

        // Model names are not unique, so require the manufacturer to agree as well.
        if (string.IsNullOrWhiteSpace(update.DriverManufacturer) ||
            string.IsNullOrWhiteSpace(device.Manufacturer)) return false;

        return device.Manufacturer.Equals(update.DriverManufacturer, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A display note for the driver's age. Not a warning.</summary>
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
