using DriverGeek.Core.Models;
using DriverGeek.Core.Services;
using DriverGeek.Services;

namespace DriverGeek.ViewModels;

/// <summary>One row on the Drivers page.</summary>
public sealed class DeviceRowViewModel(ScannedDevice scanned)
{
    public string DeviceName => scanned.Device.DeviceName;

    public string SubTitle
    {
        get
        {
            var maker = string.IsNullOrWhiteSpace(scanned.Device.Manufacturer) ? "Unknown" : scanned.Device.Manufacturer;
            var cls = string.IsNullOrWhiteSpace(scanned.Device.ClassName) ? "Device" : scanned.Device.ClassName;
            return $"{maker} · {cls}";
        }
    }

    public string Version => string.IsNullOrWhiteSpace(scanned.Device.DriverVersion)
        ? "not reported"
        : scanned.Device.DriverVersion;

    public string DriverDate => scanned.Device.DriverDate?.ToString("dd MMM yyyy") ?? "";

    public string AgeNote => StalenessPolicy.AgeNote(scanned.Device.DriverDate, DateTime.Today);

    public string StatusText => scanned.Status switch
    {
        DeviceStatus.UpdateOffered => "UPDATE",
        DeviceStatus.UpdateHiddenAsOptional => "OPTIONAL",
        _ => "CURRENT"
    };

    public string StatusBrush => scanned.Status switch
    {
        DeviceStatus.UpdateOffered => "BlueLight",
        DeviceStatus.UpdateHiddenAsOptional => "Teal",
        _ => "GreenLight"
    };

    public bool IsBootCritical => scanned.IsBootCritical;
    public bool IsUnsigned => !scanned.Device.IsSigned;

    public string NewVersion => scanned.Update?.DriverVersion ?? "";
}
