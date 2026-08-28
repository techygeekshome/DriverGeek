namespace DriverGeek.Services;

public static class AppPaths
{
    public static string DataFolder
    {
        get
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(root, "DriverGeek");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string SettingsFile => Path.Combine(DataFolder, "settings.json");
    public static string LogFile => Path.Combine(DataFolder, "drivergeek.log");

    /// <summary>Where a driver is exported before it is replaced.</summary>
    public static string DefaultBackupFolder
    {
        get
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(root, "DriverGeek", "backup");
        }
    }
}
