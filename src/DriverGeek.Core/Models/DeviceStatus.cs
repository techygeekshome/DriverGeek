namespace DriverGeek.Core.Models;

/// <summary>What DriverGeek is prepared to say about a device.</summary>
public enum DeviceStatus
{
    /// <summary>Windows Update has nothing newer. This is the answer for most devices, most of the time.</summary>
    Current,

    /// <summary>Windows Update has a newer driver and would offer it to you anyway.</summary>
    UpdateOffered,

    /// <summary>
    /// Windows Update has a newer driver and keeps it under Optional updates. This is the one
    /// worth a person's attention, because nothing else on the machine will mention it.
    /// </summary>
    UpdateHiddenAsOptional
}
