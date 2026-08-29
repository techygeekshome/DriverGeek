using System.Reflection;
using System.Runtime.InteropServices;

namespace DriverGeek.Services;

public sealed record InstallOutcome(bool Ok, bool RebootRequired, bool FailedBeforeInstalling, string Message);

/// <summary>
/// Downloads and installs one driver package through the Windows Update Agent. The COM is
/// late-bound through IDispatch for the same reason the search is: no interop assembly in the
/// tree. One update at a time, by ID, because that is the only thing the UI ever asks for.
/// </summary>
public sealed class DriverInstallService
{
    // IUpdateDownloadResult / IInstallationResult ResultCode.
    private const int Succeeded = 2;
    private const int SucceededWithErrors = 3;

    public InstallOutcome Run(string updateId, IProgress<string>? progress)
    {
        if (!LooksLikeUpdateId(updateId))
            return new InstallOutcome(false, false, true, "That update no longer has a usable ID.");

        object? session = null;
        object? searcher = null;
        object? result = null;
        object? updates = null;
        object? update = null;
        object? batch = null;
        object? downloader = null;
        object? installer = null;

        try
        {
            var type = Type.GetTypeFromProgID("Microsoft.Update.Session");
            if (type is null)
                return new InstallOutcome(false, false, true, "The Windows Update service is not available.");

            session = Activator.CreateInstance(type);
            if (session is null)
                return new InstallOutcome(false, false, true, "The Windows Update service could not be started.");

            Set(session, "ClientApplicationID", "TechyGeeksHome DriverGeek");

            progress?.Report("Asking Windows Update for the package…");
            searcher = Call(session, "CreateUpdateSearcher");
            if (searcher is null)
                return new InstallOutcome(false, false, true, "The Windows Update searcher could not be created.");

            Set(searcher, "Online", true);
            result = Call(searcher, "Search", $"UpdateID='{updateId}'");
            updates = result is null ? null : Get(result, "Updates");
            var count = updates is null ? 0 : Convert.ToInt32(Get(updates, "Count") ?? 0);

            if (count == 0)
                return new InstallOutcome(false, false, true,
                    "Windows Update no longer offers this driver. Scan again to see what it has now.");

            update = Call(updates!, "Item", 0);
            if (update is null)
                return new InstallOutcome(false, false, true, "The update could not be read back.");

            AcceptEulaIfNeeded(update);

            var collType = Type.GetTypeFromProgID("Microsoft.Update.UpdateColl");
            batch = collType is null ? null : Activator.CreateInstance(collType);
            if (batch is null)
                return new InstallOutcome(false, false, true, "Windows Update would not accept the request.");

            Call(batch, "Add", update);

            progress?.Report("Downloading…");
            downloader = Call(session, "CreateUpdateDownloader");
            if (downloader is null)
                return new InstallOutcome(false, false, true, "The downloader could not be created.");

            Set(downloader, "Updates", batch);
            var downloadResult = Call(downloader, "Download");
            var downloadCode = downloadResult is null ? 0 : Convert.ToInt32(Get(downloadResult, "ResultCode") ?? 0);
            Release(downloadResult);

            if (downloadCode is not (Succeeded or SucceededWithErrors))
                return new InstallOutcome(false, false, true,
                    "The package would not download. Nothing has been changed.");

            progress?.Report("Installing. This can take a few minutes and the screen may flicker…");
            installer = Call(session, "CreateUpdateInstaller");
            if (installer is null)
                return new InstallOutcome(false, false, true, "The installer could not be created.");

            Set(installer, "Updates", batch);
            var installResult = Call(installer, "Install");
            var installCode = installResult is null ? 0 : Convert.ToInt32(Get(installResult, "ResultCode") ?? 0);
            var reboot = installResult is not null && Convert.ToBoolean(Get(installResult, "RebootRequired") ?? false);
            Release(installResult);

            return installCode switch
            {
                Succeeded => new InstallOutcome(true, reboot, false, "Installed."),
                SucceededWithErrors => new InstallOutcome(true, reboot, false,
                    "Installed, but Windows reported a problem along the way. Check the device in Device Manager."),
                _ => new InstallOutcome(false, reboot, false,
                    "Windows would not install the driver. The old one is still in place; " +
                    "the exported copy is there if it is not.")
            };
        }
        catch (COMException ex)
        {
            Log.Write($"Driver install failed: 0x{ex.HResult:X8} {ex.Message}");
            return new InstallOutcome(false, false, false, Friendly(ex));
        }
        catch (UnauthorizedAccessException)
        {
            return new InstallOutcome(false, false, true,
                "Windows would not allow a driver install from this account.");
        }
        catch (Exception ex)
        {
            Log.Write("Driver install failed: " + ex);
            return new InstallOutcome(false, false, false,
                "The install could not be completed. See drivergeek.log for the reason.");
        }
        finally
        {
            Release(installer);
            Release(downloader);
            Release(batch);
            Release(update);
            Release(updates);
            Release(result);
            Release(searcher);
            Release(session);
        }
    }

    private static void AcceptEulaIfNeeded(object update)
    {
        try
        {
            if (!Convert.ToBoolean(Get(update, "EulaAccepted") ?? true))
                Call(update, "AcceptEula");
        }
        catch (Exception ex)
        {
            // Driver packages rarely carry one. If reading it fails, the install will say so.
            Log.Write("Could not read the licence state: " + ex.Message);
        }
    }

    /// <summary>Update IDs are GUIDs. Anything else never reaches the search string.</summary>
    private static bool LooksLikeUpdateId(string id) =>
        !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out _);

    private static string Friendly(COMException ex) => (uint)ex.HResult switch
    {
        0x80240044 => "Windows will not install this driver without administrator rights.",
        0x8024402C => "Windows Update could not be reached - no connection, or a proxy in the way.",
        0x80240016 => "Windows Update is busy with another install. Let it finish and try again.",
        0x80070005 => "Windows refused the install: access denied.",
        _ => "Windows reported: " + ex.Message.Trim()
    };

    private static object? Get(object target, string name) =>
        target.GetType().InvokeMember(name, BindingFlags.GetProperty, null, target, null);

    private static void Set(object target, string name, object value) =>
        target.GetType().InvokeMember(name, BindingFlags.SetProperty, null, target, [value]);

    private static object? Call(object target, string name, params object[] args) =>
        target.GetType().InvokeMember(
            name, BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, target, args);

    private static void Release(object? o)
    {
        try
        {
            if (o is not null && Marshal.IsComObject(o)) Marshal.ReleaseComObject(o);
        }
        catch (ArgumentException) { }
    }
}
