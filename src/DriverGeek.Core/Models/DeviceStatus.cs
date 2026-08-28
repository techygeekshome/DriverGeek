namespace DriverGeek.Core.Models;

/// <summary>The scan result for a device.</summary>
public enum DeviceStatus
{
    /// <summary>Windows Update has nothing newer.</summary>
    Current,

    /// <summary>Windows Update has a newer driver and offers it normally.</summary>
    UpdateOffered,

    /// <summary>Windows Update has a newer driver but keeps it under Optional updates.</summary>
    UpdateHiddenAsOptional
}
