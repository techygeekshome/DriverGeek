namespace DriverGeek.Core.Models;

/// <summary>A driver update Windows Update has for this machine.</summary>
public sealed class DriverUpdate
{
    public string Title { get; init; } = "";
    public string DriverManufacturer { get; init; } = "";
    public string DriverModel { get; init; } = "";
    public string DriverClass { get; init; } = "";
    public string DriverVersion { get; init; } = "";
    public DateTime? DriverDate { get; init; }

    /// <summary>The hardware ID this update applies to. Used to match it to a device.</summary>
    public string DriverHardwareId { get; init; } = "";

    /// <summary>Download size in bytes, as Windows reports it.</summary>
    public long SizeBytes { get; init; }

    /// <summary>Windows Update's BrowseOnly flag: the update is filed under Optional. See UpdateCriteria.</summary>
    public bool IsOptional { get; init; }

    public string UpdateId { get; init; } = "";
}
