using System.Management;
using DriverGeek.Core.Models;

namespace DriverGeek.Services;

/// <summary>
/// Reads every device and its current driver from Win32_PnPSignedDriver. Rows are read
/// defensively: a null property or a provider that throws skips that device rather than losing
/// the whole inventory.
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
        catch (Exception ex)
        {
            // An unhealthy WMI service throws COMException and UnauthorizedAccessException as
            // readily as ManagementException. Whatever it is, report no devices rather than fail.
            Log.Write("Driver inventory failed: " + ex);
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
        catch (Exception)
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
        catch (Exception)
        {
            return "";
        }
    }

    /// <summary>
    /// WMI dates are CIM_DATETIME, e.g. "20260612000000.000000+000". The converter throws on
    /// malformed values, which is common enough on driver dates to be worth catching.
    /// </summary>
    private static DateTime? Date(ManagementObject mo, string property)
    {
        try
        {
            var raw = mo[property]?.ToString();
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return ManagementDateTimeConverter.ToDateTime(raw);
        }
        catch (Exception) { return null; }
    }
}
