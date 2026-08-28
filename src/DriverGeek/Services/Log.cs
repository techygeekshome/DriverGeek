namespace DriverGeek.Services;

/// <summary>A single-file log, deleted and restarted once it grows past MaxBytes.</summary>
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
