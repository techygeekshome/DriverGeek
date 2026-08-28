namespace DriverGeek.Core.Services;

/// <summary>How much damage a bad driver in this class can do.</summary>
public enum DriverRisk
{
    /// <summary>An ordinary device. A bad driver is an annoyance.</summary>
    Ordinary,

    /// <summary>
    /// A bad driver here leaves the machine working but hard to fix from - no screen, no network,
    /// no keyboard. Allowed, but the user is told before it happens.
    /// </summary>
    Awkward,

    /// <summary>
    /// A bad driver here can stop Windows starting at all. DriverGeek reports these and never
    /// installs over them, in any version. This is not a setting.
    /// </summary>
    BootCritical
}

/// <summary>
/// Device setup classes, and which of them DriverGeek refuses to touch.
///
/// Matching is by the setup class GUID, because class NAMES are localised and a machine running
/// Windows in German reports "Netzwerkadapter". The GUIDs are fixed by Microsoft and are the same
/// on every install. Names are kept only as a fallback for the odd device that reports no GUID.
/// </summary>
public static class DriverClass
{
    // Storage and the buses underneath it. Get one of these wrong and the volume does not mount.
    private static readonly HashSet<string> BootCriticalGuids = new(StringComparer.OrdinalIgnoreCase)
    {
        "{4d36e97b-e325-11ce-bfc1-08002be10318}", // SCSIAdapter - includes NVMe and RAID controllers
        "{4d36e96a-e325-11ce-bfc1-08002be10318}", // HDC - IDE/ATA/ATAPI controllers
        "{4d36e967-e325-11ce-bfc1-08002be10318}", // DiskDrive
        "{71a27cdd-812a-11d0-bec7-08002be2092f}", // Volume
        "{533c5b84-ec70-11d2-9505-00c04f79deaf}", // VolumeSnapshot
        "{4d36e97d-e325-11ce-bfc1-08002be10318}", // System - chipset, host bridges, PCI root
    };

    private static readonly HashSet<string> AwkwardGuids = new(StringComparer.OrdinalIgnoreCase)
    {
        "{4d36e968-e325-11ce-bfc1-08002be10318}", // Display
        "{4d36e972-e325-11ce-bfc1-08002be10318}", // Net
        "{4d36e96b-e325-11ce-bfc1-08002be10318}", // Keyboard
        "{4d36e96f-e325-11ce-bfc1-08002be10318}", // Mouse
        "{745a17a0-74d3-11d0-b6fe-00a0c90f57da}", // HIDClass
    };

    private static readonly HashSet<string> BootCriticalNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "SCSIAdapter", "HDC", "DiskDrive", "Volume", "VolumeSnapshot", "System"
    };

    private static readonly HashSet<string> AwkwardNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Display", "Net", "Keyboard", "Mouse", "HIDClass"
    };

    public static DriverRisk RiskOf(string? classGuid, string? className = null)
    {
        var guid = Normalise(classGuid);
        if (guid is not null)
        {
            if (BootCriticalGuids.Contains(guid)) return DriverRisk.BootCritical;
            if (AwkwardGuids.Contains(guid)) return DriverRisk.Awkward;
            return DriverRisk.Ordinary;
        }

        var name = className?.Trim();
        if (string.IsNullOrEmpty(name)) return DriverRisk.Ordinary;
        if (BootCriticalNames.Contains(name)) return DriverRisk.BootCritical;
        if (AwkwardNames.Contains(name)) return DriverRisk.Awkward;
        return DriverRisk.Ordinary;
    }

    public static bool IsBootCritical(string? classGuid, string? className = null)
        => RiskOf(classGuid, className) == DriverRisk.BootCritical;

    /// <summary>Accepts a GUID with or without braces, in any case. Returns null if it is not one.</summary>
    private static string? Normalise(string? classGuid)
    {
        if (string.IsNullOrWhiteSpace(classGuid)) return null;
        var s = classGuid.Trim();
        if (!s.StartsWith('{')) s = "{" + s;
        if (!s.EndsWith('}')) s += "}";
        return Guid.TryParse(s, out _) ? s.ToLowerInvariant() : null;
    }
}
