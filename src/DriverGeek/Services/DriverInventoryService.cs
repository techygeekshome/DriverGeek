using System.Management;
using DriverGeek.Core.Models;

namespace DriverGeek.Services;

/// <summary>
/// Reads every device on the machine and the driver it is running, from
/// Win32_PnPSignedDriver.
///
/// Two things about this class are deliberate.
///
/// First, it reads and nothing else. There is no code path here that changes anything, which is
/// what makes DriverGeek 1.0 safe to run on any machine without a second thought.
///
/// Second, one bad row must never take out the scan. A machine with 150 devices reliably has one
/// with a null property, an unparsable date or a WMI provider that throws on access, and losing
/// the whole inventory to it would be the difference between a working app and a useless one.
/// So every row is read defensively and a failure skips that device.
/// </summary>
public sealed class DriverInventoryService
{
    private const string Query =
        "SELECT DeviceName, Manufacturer, DeviceClass, ClassGuid, DriverVersion, DriverDate, " +
        "Signer, InfName, DeviceID, IsSigned FROM Win32_PnPSignedDriver";

    public IReadOnlyList<DeviceDriver> Read(bool includeAbsent = false)
    {
        var found = new List<DeviceDriver>(200);

        try
        {
            using var searcher = new ManagementObjectSearcher(new ObjectQuery(Query));
            using var results = searcher.Get();

            foreach (var row in results)
            {
                using var mo = (ManagementObject)row;
                var device = TryRead(mo);
                if (device is null) continue;
                if (!includeAbsent && string.IsNullOrWhiteSpace(device.DeviceName)) continue;
                found.Add(device);
            }
        }
        catch (ManagementException ex)
        {
            Log.Write($"Driver inventory failed: {ex.Message}");
        }

        return found
            .GroupBy(d => d.DeviceId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(d => d.ClassName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.DeviceName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static DeviceDriver? TryRead(ManagementObject mo)
    {
        try
        {
            var name = Text(mo, "DeviceName");
            if (string.IsNullOrWhiteSpace(name)) return null;

            return new DeviceDriver
            {
                DeviceName = name,
                Manufacturer = Text(mo, "Manufacturer"),
                ClassName = Text(mo, "DeviceClass"),
                ClassGuid = Text(mo, "ClassGuid"),
                DriverVersion = Text(mo, "DriverVersion"),
                DriverDate = Date(mo, "DriverDate"),
                Signer = Text(mo, "Signer"),
                InfName = Text(mo, "InfName"),
                DeviceId = Text(mo, "DeviceID"),
                IsPresent = true
            };
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    private static string Text(ManagementObject mo, string property)
    {
        try
        {
            return mo[property]?.ToString()?.Trim() ?? "";
        }
        catch (ManagementException)
        {
            return "";
        }
    }

    /// <summary>
    /// WMI dates are CIM_DATETIME - "20260612000000.000000+000". ManagementDateTimeConverter
    /// handles the well-formed ones and throws on the rest, which is common enough on driver
    /// dates to be worth catching rather than avoiding.
    /// </summary>
    private static DateTime? Date(ManagementObject mo, string property)
    {
        try
        {
            var raw = mo[property]?.ToString();
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return ManagementDateTimeConverter.ToDateTime(raw);
        }
        catch (ArgumentOutOfRangeException) { return null; }
        catch (FormatException) { return null; }
        catch (ManagementException) { return null; }
    }
}
