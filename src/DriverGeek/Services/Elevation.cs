using System.Diagnostics;
using System.Security.Principal;

namespace DriverGeek.Services;

/// <summary>
/// Scanning works fine as a normal user, so the app does not ask for administrator on startup.
/// Installing a driver does need it, and that is the one thing that asks.
/// </summary>
public static class Elevation
{
    public static bool IsElevated()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex)
        {
            Log.Write("Could not read the current account's rights: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Start the same executable again with the runas verb, so Windows shows the UAC prompt.
    /// Returns false when the user says No, which is not an error worth reporting as one.
    /// </summary>
    public static bool Relaunch()
    {
        try
        {
            var exe = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exe)) return false;

            Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true, Verb = "runas" });
            return true;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // 1223: the user clicked No on the UAC prompt.
            return false;
        }
        catch (Exception ex)
        {
            Log.Write("Could not restart with administrator rights: " + ex);
            return false;
        }
    }
}
