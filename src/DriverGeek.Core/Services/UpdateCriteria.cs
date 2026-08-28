namespace DriverGeek.Core.Services;

/// <summary>
/// Criteria strings for IUpdateSearcher.Search. Type='Driver' selects driver updates rather than
/// software ones; BrowseOnly=1 is the flag Windows uses to mark an update optional, meaning it
/// applies to this machine but is never offered on the Windows Update page.
/// Reference: learn.microsoft.com/windows/win32/api/wuapi/nf-wuapi-iupdatesearcher-search
/// </summary>
public static class UpdateCriteria
{
    /// <summary>Every driver update that applies and is not installed or hidden.</summary>
    public const string AllDrivers = "IsInstalled=0 and Type='Driver' and IsHidden=0";

    /// <summary>Only the ones Windows keeps under Optional updates.</summary>
    public const string OptionalDrivers = "IsInstalled=0 and Type='Driver' and IsHidden=0 and BrowseOnly=1";

    /// <summary>Only the ones Windows Update would offer on its own.</summary>
    public const string OfferedDrivers = "IsInstalled=0 and Type='Driver' and IsHidden=0 and BrowseOnly=0";

    /// <summary>Updates the user has hidden in Windows. Counted for display only, never acted on.</summary>
    public const string HiddenDrivers = "IsInstalled=0 and Type='Driver' and IsHidden=1";

    public static string For(bool includeOptional)
        => includeOptional ? AllDrivers : OfferedDrivers;
}
