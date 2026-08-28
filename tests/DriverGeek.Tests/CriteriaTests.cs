using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class CriteriaTests
{
    public static void Run()
    {
        Check.Section("Windows Update search criteria");

        Check.That("all-drivers asks for drivers", UpdateCriteria.AllDrivers.Contains("Type='Driver'"));
        Check.That("all-drivers excludes installed", UpdateCriteria.AllDrivers.Contains("IsInstalled=0"));
        Check.That("all-drivers excludes hidden", UpdateCriteria.AllDrivers.Contains("IsHidden=0"));
        Check.That("all-drivers does not pin BrowseOnly", !UpdateCriteria.AllDrivers.Contains("BrowseOnly"));

        // BrowseOnly=1 is Windows' own flag for an optional update.
        Check.That("optional means BrowseOnly=1", UpdateCriteria.OptionalDrivers.Contains("BrowseOnly=1"));
        Check.That("offered means BrowseOnly=0", UpdateCriteria.OfferedDrivers.Contains("BrowseOnly=0"));
        Check.That("optional is still driver-only", UpdateCriteria.OptionalDrivers.Contains("Type='Driver'"));

        Check.Equal("including optional widens the search", UpdateCriteria.AllDrivers, UpdateCriteria.For(true));
        Check.Equal("excluding optional narrows it", UpdateCriteria.OfferedDrivers, UpdateCriteria.For(false));

        // Hidden updates are counted for display, never acted on.
        Check.That("hidden search is separate", UpdateCriteria.HiddenDrivers.Contains("IsHidden=1"));
    }
}
