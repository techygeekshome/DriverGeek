namespace DriverGeek.Services;

/// <summary>
/// A single-file log, kept small. It records what was scanned and what was found, which is what
/// a user needs when something looks wrong, and nothing that identifies the machine beyond the
/// device names already on screen. It is never uploaded anywhere.
/// </summary>
public static class Log
{
    private const long MaxBytes = 512 * 1024;
    private static readonly object Gate = new();

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                var path = AppPaths.LogFile;
                if (File.Exists(path) && new FileInfo(path).Length > MaxBytes)
                    File.Delete(path);

                File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
        }
        catch (IOException)
        {
            // Logging must never be the reason a scan fails.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
