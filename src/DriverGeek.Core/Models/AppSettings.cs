using System.Text.Json.Serialization;

namespace DriverGeek.Core.Models;

public sealed class AppSettings
{
    // Scanning
    [JsonPropertyName("scanSchedule")] public string ScanSchedule { get; set; } = "Manually only";
    [JsonPropertyName("includeOptionalUpdates")] public bool IncludeOptionalUpdates { get; set; } = true;
    [JsonPropertyName("notifyOnUpdates")] public bool NotifyOnUpdates { get; set; } = true;
    [JsonPropertyName("includeAbsentDevices")] public bool IncludeAbsentDevices { get; set; }

    // Where an exported driver goes before anything replaces it.
    [JsonPropertyName("backupFolder")] public string BackupFolder { get; set; } = "";

    /// <summary>
    /// Deliberately absent from this file, because they are not settings and never will be:
    /// creating a restore point, exporting the current driver, and refusing boot-critical
    /// devices. They live in InstallGate, which has no way to be turned off.
    /// </summary>
    [JsonIgnore] public static string SafetyNote =>
        "Restore point, driver export and the boot-critical refusal are not configurable.";
}
