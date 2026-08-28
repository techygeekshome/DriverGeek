using System.Runtime.InteropServices;
using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Services;

/// <summary>
/// Asks Windows Update what driver updates it has for this machine.
///
/// This is the class the whole application is built around, so the mechanism is worth stating
/// plainly. The Windows Update Agent exposes IUpdateSearcher, whose criteria string supports
/// Type='Driver' as a first-class filter and, more usefully, BrowseOnly - the flag Windows uses
/// to mark an update OPTIONAL. An update with BrowseOnly=1 is one Windows has, knows applies to
/// this machine, and will not offer on the Windows Update page. It sits four clicks deep under
/// Advanced options, Optional updates. Surfacing those is the point of DriverGeek.
///
/// The COM is late-bound on purpose. Adding an interop assembly for WUApiLib would mean either a
/// third-party package or a generated wrapper in the tree, and this project's rule is that every
/// package is one a reviewer has to trust. Late binding through IDispatch costs a little
/// readability and adds no dependency at all.
///
/// Nothing here downloads or installs. Search only.
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

            // Online search: ask the service, do not just read the local cache. It is slower and
            // it is the only way to see updates that have appeared since the last sync.
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

            // IWindowsDriverUpdate carries the interesting fields. A non-driver update - which
            // should not appear given the criteria - simply reports none of them.
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
        catch (COMException)
        {
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

    private static object? Call(object target, string name, params object[] args) =>
        target.GetType().InvokeMember(name, System.Reflection.BindingFlags.InvokeMethod, null, target, args);

    private static void Release(object? o)
    {
        if (o is not null && Marshal.IsComObject(o))
            Marshal.ReleaseComObject(o);
    }
}
