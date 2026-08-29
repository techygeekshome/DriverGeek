using TechyGeeksHome.Common;

namespace DriverGeek;

/// <summary>
/// Everything the shared About window and update check need to know about this app. One place
/// to edit when the product page moves or a dependency changes.
///
/// Note the alias: this app already has its own <c>DriverGeek.Services.AppInfo</c> for the strings
/// the main window binds to, so the shared type is named in full rather than imported.
/// </summary>
public static class AppMetadata
{
    public static readonly TechyGeeksHome.Common.AppInfo Info = new()
    {
        Name = "DriverGeek",
        Tagline = "Free driver inventory for Windows",
        Description =
            "Lists every driver Windows knows about on this machine and asks Windows Update whether newer ones exist, including the optional driver updates Windows files away and never offers. It reports; it installs nothing.",
        GitHubOwner = "techygeekshome",
        GitHubRepo = "DriverGeek",
        ProductUrl = "https://techygeekshome.info/drivergeek/",
        WebsiteUrl = "https://techygeekshome.info",
        DonateUrl = "https://ko-fi.com/techygeekshome",
        IconUri = "avares://DriverGeek/Assets/drivergeek.png",
        LicenceLine =
            "GPL-3.0. Free to use, including at work. No paid tier, no subscription, no upsell.",
        Credits = new[]
        {
            new Credit("Avalonia", "MIT", "https://avaloniaui.net/")
        }
    };
}
