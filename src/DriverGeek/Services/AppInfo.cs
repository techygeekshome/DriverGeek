using System.Reflection;

namespace DriverGeek.Services;

/// <summary>Product names and links.</summary>
public static class AppInfo
{
    public const string Name = "DriverGeek";
    public const string By = "TechyGeeksHome";
    public const string ProductUrl = "https://techygeekshome.info/drivergeek/";
    public const string RepoUrl = "https://github.com/techygeekshome/DriverGeek";
    public const string DonateUrl = "https://ko-fi.com/techygeekshome";
    public const string LicenceName = "GNU General Public License v3.0";

    public static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version is { } v
            ? $"{v.Major}.{v.Minor}.{v.Build}"
            : "1.0.0";
}
