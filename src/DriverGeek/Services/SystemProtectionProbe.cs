using Microsoft.Win32;

namespace DriverGeek.Services;

/// <summary>
/// Is System Protection on for the system drive?
///
/// DriverGeek 1.0 installs nothing, so this is not a gate yet - it is a read-only check the
/// Settings page uses to tell the user whether the machine is ready for the install path when it
/// arrives. Finding out on the day you first need a restore point is finding out too late.
/// </summary>
public static class SystemProtectionProbe
{
    private const string Key = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";

    public static bool? IsEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(Key);
            if (key is null) return null;

            // RPSessionInterval > 0 means restore points are being taken. DisableSR = 1 is the
            // explicit off switch and wins when both are present.
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
        false => "Turned off. When the install path arrives in 1.1, a driver install will be refused rather " +
                 "than risked until this is switched back on.",
        _ => "Could not be read on this machine."
    };
}
