using System.Management;
using DriverGeek.Core.Services;

namespace DriverGeek.Services;

/// <summary>The result of asking Windows for a restore point before a driver is replaced.</summary>
public sealed record RestorePointResult(bool Ok, bool Reused, DateTime? When, string Message);

/// <summary>
/// Creates a System Restore point through the SystemRestore WMI class - the same call
/// Checkpoint-Computer makes. Needs administrator rights and System Protection switched on for
/// the system drive; both are checked before anything gets this far.
/// </summary>
public static class RestorePointService
{
    private const string Scope = @"\\.\root\default";

    // RESTOREPOINTINFO values. 10 is DEVICE_DRIVER_INSTALL, which is what this is.
    private const uint DeviceDriverInstall = 10;
    private const uint BeginSystemChange = 100;

    public static RestorePointResult Create(string description)
    {
        var before = Newest();

        try
        {
            using var cls = new ManagementClass(new ManagementScope(Scope), new ManagementPath("SystemRestore"), null);
            using var args = cls.GetMethodParameters("CreateRestorePoint");

            args["Description"] = description;
            args["RestorePointType"] = DeviceDriverInstall;
            args["EventType"] = BeginSystemChange;

            using var outcome = cls.InvokeMethod("CreateRestorePoint", args, null);
            var code = Convert.ToUInt32(outcome?["ReturnValue"] ?? 1u);

            if (code == 0)
            {
                var made = Newest() ?? DateTime.Now;
                Log.Write($"Restore point created: {description}");
                return new RestorePointResult(true, false, made, $"Restore point created at {made:HH:mm}.");
            }

            // 1058 is the frequency limit: Windows already took one inside the window.
            var recent = Newest();
            if (RestorePointPolicy.CountsAsRecent(recent, DateTime.Now))
            {
                Log.Write($"Restore point refused (code {code}); reusing the one from {recent:u}.");
                return new RestorePointResult(true, true, recent, RestorePointPolicy.ReusedNote(recent!.Value));
            }

            Log.Write($"Restore point failed with code {code}.");
            return new RestorePointResult(false, false, null,
                $"Windows would not create a restore point (it returned {code}). Nothing has been changed.");
        }
        catch (ManagementException ex)
        {
            Log.Write("Restore point failed: " + ex.Message);
            return new RestorePointResult(false, false, null,
                "Windows would not create a restore point. " + ex.Message.Trim() + " Nothing has been changed.");
        }
        catch (UnauthorizedAccessException)
        {
            return new RestorePointResult(false, false, null,
                "Creating a restore point needs administrator rights. Nothing has been changed.");
        }
        catch (Exception ex)
        {
            Log.Write("Restore point failed: " + ex);
            return new RestorePointResult(false, false, null,
                "The restore point could not be created on this machine. Nothing has been changed.");
        }
    }

    /// <summary>When the most recent restore point was taken, or null when there are none to read.</summary>
    public static DateTime? Newest()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                new ManagementScope(Scope), new ObjectQuery("SELECT CreationTime FROM SystemRestore"));
            using var rows = searcher.Get();

            DateTime? newest = null;
            foreach (var row in rows)
            {
                using var mo = (ManagementObject)row;
                var raw = mo["CreationTime"]?.ToString();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                try
                {
                    var when = ManagementDateTimeConverter.ToDateTime(raw);
                    if (newest is null || when > newest) newest = when;
                }
                catch (ArgumentOutOfRangeException) { /* a row with a date WMI cannot parse */ }
                catch (FormatException) { }
            }

            return newest;
        }
        catch (Exception ex)
        {
            Log.Write("Could not read existing restore points: " + ex.Message);
            return null;
        }
    }
}
