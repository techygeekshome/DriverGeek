namespace DriverGeek.Core.Services;

/// <summary>
/// The Windows Update Agent search strings.
///
/// This tiny class is the whole premise of DriverGeek, so it is worth spelling out.
/// IUpdateSearcher.Search takes a criteria string, and two of its fields matter here:
///
///   Type='Driver'   - driver updates rather than software updates.
///   BrowseOnly=1    - the flag Windows uses to mark an update OPTIONAL.
///
/// BrowseOnly is the interesting one. An update with BrowseOnly=1 is one Windows has, knows
/// applies to this machine, and will never offer you on the Windows Update page - it sits under
/// Settings, Windows Update, Advanced options, Optional updates, four clicks in. Most people
/// never look there. Surfacing those is the reason this app exists.
///
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

    /// <summary>
    /// Updates the user has explicitly hidden in Windows. DriverGeek never un-hides anything -
    /// this exists so the count can be shown and explained, not acted on.
    /// </summary>
    public const string HiddenDrivers = "IsInstalled=0 and Type='Driver' and IsHidden=1";

    public static string For(bool includeOptional)
        => includeOptional ? AllDrivers : OfferedDrivers;
}
