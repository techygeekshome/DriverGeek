using Microsoft.Win32;

namespace DriverGeek.Services;

/// <summary>Read-only check of whether System Protection is on for the system drive.</summary>
public static class SystemProtectionProbe
{
    private const string Key = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";

    public static bool? IsEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(Key);
            if (key is null) return null;

            // RPSessionInterval > 0 means restore points are being taken, but DisableSR = 1 is
            // the explicit off switch and wins when both are present.
            if (key.GetValue("DisableSR") is int disabled && disabled == 1) return false;
            if (key.GetValue("RPSessionInterval") is int interval) return interval > 0;

            return null;
        }
        catch (System.Security.SecurityException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
        catch (IOException) { return null; }
    }

    public static string Describe() => IsEnabled() switch
    {
        true => "On for the system drive. A restore point can be created before a driver is replaced.",
        false => "Turned off. A driver install is refused rather than risked until this is switched back on.",
        _ => "Could not be read on this machine."
    };
}
