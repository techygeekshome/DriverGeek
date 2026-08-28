namespace DriverGeek.Core.Models;

/// <summary>One device and the driver it is currently running.</summary>
public sealed class DeviceDriver
{
    public string DeviceName { get; init; } = "";
    public string Manufacturer { get; init; } = "";
    public string ClassName { get; init; } = "";
    public string ClassGuid { get; init; } = "";

    /// <summary>The driver's own version string, as Windows reports it. May be empty.</summary>
    public string DriverVersion { get; init; } = "";

    /// <summary>The driver's date. Null when Windows does not report one.</summary>
    public DateTime? DriverDate { get; init; }

    /// <summary>Who signed the driver package, empty when it is unsigned.</summary>
    public string Signer { get; init; } = "";

    public bool IsSigned => !string.IsNullOrWhiteSpace(Signer);

    /// <summary>The published INF name, e.g. oem42.inf.</summary>
    public string InfName { get; init; } = "";

    public string DeviceId { get; init; } = "";

    /// <summary>False for devices that are present in the registry but not plugged in.</summary>
    public bool IsPresent { get; init; } = true;
}
