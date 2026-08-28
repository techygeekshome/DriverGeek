using System.Text.Json;
using DriverGeek.Core.Models;

namespace DriverGeek.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public AppSettings Current { get; private set; } = new();

    public void Load()
    {
        try
        {
            var path = AppPaths.SettingsFile;
            if (!File.Exists(path)) return;
            Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings();
        }
        catch (JsonException ex)
        {
            // A corrupt settings file is not a reason to refuse to start.
            Log.Write($"settings.json could not be read, using defaults: {ex.Message}");
            Current = new AppSettings();
        }
        catch (IOException) { Current = new AppSettings(); }
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(AppPaths.SettingsFile, JsonSerializer.Serialize(Current, Options));
        }
        catch (IOException ex)
        {
            Log.Write($"settings.json could not be written: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Write($"settings.json could not be written: {ex.Message}");
        }
    }
}
