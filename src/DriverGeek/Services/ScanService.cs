using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Services;

/// <summary>One device, with whatever Windows Update has for it attached.</summary>
public sealed record ScannedDevice(DeviceDriver Device, DriverUpdate? Update, DeviceStatus Status)
{
    public DriverRisk Risk => DriverClass.RiskOf(Device.ClassGuid, Device.ClassName);
    public bool IsBootCritical => Risk == DriverRisk.BootCritical;
}

public sealed record ScanResult(
    IReadOnlyList<ScannedDevice> Devices,
    IReadOnlyList<DriverUpdate> Updates,
    string? Error)
{
    public int DeviceCount => Devices.Count;
    public int UpdateCount => Devices.Count(d => d.Status != DeviceStatus.Current);
    public int OptionalCount => Devices.Count(d => d.Status == DeviceStatus.UpdateHiddenAsOptional);
    public int UnsignedCount => Devices.Count(d => !d.Device.IsSigned);
}

/// <summary>Matches the installed driver inventory against the Windows Update search results.</summary>
public sealed class ScanService
{
    private readonly DriverInventoryService _inventory = new();
    private readonly WindowsUpdateDriverService _updates = new();

    public ScanResult Run(AppSettings settings, IProgress<string>? progress = null)
    {
        progress?.Report("Reading the driver behind every device on this PC\u2026");
        var devices = _inventory.Read(settings.IncludeAbsentDevices);

        progress?.Report($"{devices.Count} devices read. Asking Windows Update what it has for them \u2014 " +
                         "this is the slow part and can take a few minutes.");
        var updates = _updates.Search(settings.IncludeOptionalUpdates, out var error);

        progress?.Report("Matching what Windows Update offered against what is installed\u2026");

        var scanned = devices
            .Select(d => new ScannedDevice(d, StalenessPolicy.BestUpdateFor(d, updates),
                                           StalenessPolicy.StatusFor(d, updates)))
            .ToList();

        Log.Write($"Scan: {devices.Count} devices, {updates.Count} driver updates from Windows Update" +
                  (error is null ? "" : $" (search error: {error})"));

        return new ScanResult(scanned, updates, error);
    }
}
