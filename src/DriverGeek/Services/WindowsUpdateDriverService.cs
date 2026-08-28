using System.Runtime.InteropServices;
using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Services;

/// <summary>
/// Searches the Windows Update Agent for driver updates. The COM is late-bound through IDispatch
/// so that no WUApiLib interop assembly or generated wrapper is needed in the tree. Search only:
/// nothing here downloads or installs.
/// </summary>
public sealed class WindowsUpdateDriverService
{
    /// <summary>Search Windows Update for driver updates. Returns an empty list if the agent is unavailable.</summary>
    public IReadOnlyList<DriverUpdate> Search(bool includeOptional, out string? error)
    {
        error = null;
        var found = new List<DriverUpdate>();

        object? session = null;
        object? searcher = null;

        try
        {
            var type = Type.GetTypeFromProgID("Microsoft.Update.Session");
            if (type is null)
            {
                error = "The Windows Update service is not available on this machine.";
                return found;
            }

            session = Activator.CreateInstance(type);
            if (session is null)
            {
                error = "The Windows Update service could not be started.";
                return found;
            }

            Set(session, "ClientApplicationID", "TechyGeeksHome DriverGeek");

            searcher = Call(session, "CreateUpdateSearcher");
            if (searcher is null)
            {
                error = "The Windows Update searcher could not be created.";
                return found;
            }

            // Online rather than the local cache: slower, but the only way to see updates that
            // have appeared since the last sync.
            Set(searcher, "Online", true);

            var result = Call(searcher, "Search", UpdateCriteria.For(includeOptional));
            if (result is null) return found;

            var updates = Get(result, "Updates");
            if (updates is null) return found;

            var count = Convert.ToInt32(Get(updates, "Count") ?? 0);
            for (var i = 0; i < count; i++)
            {
                var update = Call(updates, "Item", i);
                if (update is null) continue;
                var parsed = TryRead(update);
                if (parsed is not null) found.Add(parsed);
                Release(update);
            }

            Release(updates);
            Release(result);
        }
        catch (COMException ex)
        {
            // 0x8024402C and friends: no network, WSUS misconfigured, service disabled.
            error = "Windows Update could not be reached. " + Friendly(ex);
            Log.Write($"WUA search failed: 0x{ex.HResult:X8} {ex.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            error = "Windows would not allow a driver search from this account.";
        }
        catch (Exception ex)
        {
            // Late binding turns a missing member, a bad type or a failed marshal into a plain
            // exception rather than a COMException. None of them are worth losing the app over:
            // the device list is still useful without the update search.
            error = "The driver search could not be completed on this machine.";
            Log.Write("WUA search failed: " + ex);
        }
        finally
        {
            Release(searcher);
            Release(session);
        }

        return found;
    }

    private static DriverUpdate? TryRead(object update)
    {
        try
        {
            var title = Get(update, "Title")?.ToString() ?? "";
            var identity = Get(update, "Identity");
            var id = identity is null ? "" : Get(identity, "UpdateID")?.ToString() ?? "";
            Release(identity);

            long size = 0;
            try { size = Convert.ToInt64(Get(update, "MaxDownloadSize") ?? 0L); }
            catch (InvalidCastException) { /* some updates report no size */ }
            catch (OverflowException) { }

            var browseOnly = false;
            try { browseOnly = Convert.ToBoolean(Get(update, "BrowseOnly") ?? false); }
            catch (InvalidCastException) { }

            // IWindowsDriverUpdate fields. A non-driver update reports none of them.
            var maker = Get(update, "DriverManufacturer")?.ToString() ?? "";
            var model = Get(update, "DriverModel")?.ToString() ?? "";
            var cls = Get(update, "DriverClass")?.ToString() ?? "";
            var hwid = Get(update, "DriverHardwareID")?.ToString() ?? "";
            var ver = Get(update, "DriverVerVersion")?.ToString() ?? "";

            DateTime? date = null;
            var rawDate = Get(update, "DriverVerDate");
            if (rawDate is DateTime dt) date = dt;

            return new DriverUpdate
            {
                Title = title,
                UpdateId = id,
                SizeBytes = size,
                IsOptional = browseOnly,
                DriverManufacturer = maker,
                DriverModel = model,
                DriverClass = cls,
                DriverHardwareId = hwid,
                DriverVersion = ver,
                DriverDate = date
            };
        }
        catch (Exception ex)
        {
            // A single update that will not answer is dropped, not fatal.
            Log.Write("Skipped an update that could not be read: " + ex.Message);
            return null;
        }
    }

    private static string Friendly(COMException ex) => (uint)ex.HResult switch
    {
        0x8024402C => "That usually means no internet connection, or a proxy in the way.",
        0x80244022 => "The update service is busy or temporarily unavailable.",
        0x8024001E => "The update service was stopped while searching.",
        _ => "Windows reported: " + ex.Message.Trim()
    };

    // --- late-bound COM helpers -------------------------------------------------------------

    private static object? Get(object target, string name) =>
        target.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, target, null);

    private static void Set(object target, string name, object value) =>
        target.GetType().InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, target, [value]);

    // IUpdateCollection.Item is a parameterised property rather than a method, and asking for it
    // as a method returns DISP_E_MEMBERNOTFOUND. Passing both flags lets either shape answer.
    private static object? Call(object target, string name, params object[] args) =>
        target.GetType().InvokeMember(
            name,
            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.GetProperty,
            null, target, args);

    private static void Release(object? o)
    {
        try
        {
            if (o is not null && Marshal.IsComObject(o))
                Marshal.ReleaseComObject(o);
        }
        catch (ArgumentException)
        {
            // Already released.
        }
    }
}
