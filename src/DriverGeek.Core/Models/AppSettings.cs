using System.Text.Json.Serialization;

namespace DriverGeek.Core.Models;

public sealed class AppSettings
{
    // Scanning
    [JsonPropertyName("scanSchedule")] public string ScanSchedule { get; set; } = "Manually only";
    [JsonPropertyName("includeOptionalUpdates")] public bool IncludeOptionalUpdates { get; set; } = true;
    [JsonPropertyName("notifyOnUpdates")] public bool NotifyOnUpdates { get; set; } = true;
    [JsonPropertyName("includeAbsentDevices")] public bool IncludeAbsentDevices { get; set; }

    // Where a driver is exported before it is replaced.
    [JsonPropertyName("backupFolder")] public string BackupFolder { get; set; } = "";

    /// <summary>Text for the Settings page. These safeguards live in InstallGate and are not configurable.</summary>
    [JsonIgnore] public static string SafetyNote =>
        "Restore point, driver export and the boot-critical refusal are not configurable.";
}
