using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.ViewModels;

public sealed class UpdateRowViewModel(DeviceDriver device, DriverUpdate update)
{
    public string DeviceName => device.DeviceName;
    public string Maker => string.IsNullOrWhiteSpace(update.DriverManufacturer)
        ? device.Manufacturer
        : update.DriverManufacturer;

    public string Change =>
        $"{(string.IsNullOrWhiteSpace(device.DriverVersion) ? "not reported" : device.DriverVersion)} → {update.DriverVersion}";

    public string Size => update.SizeBytes > 0 ? ByteSize.Format(update.SizeBytes) : "";

    public bool IsOptional => update.IsOptional;

    public string Label => update.IsOptional ? "HIDDEN AS OPTIONAL" : "OFFERED BY WINDOWS";
    public string LabelBrush => update.IsOptional ? "Teal" : "BlueLight";

    public bool IsBootCritical => DriverClass.IsBootCritical(device.ClassGuid, device.ClassName);

    /// <summary>
    /// Shown on every row in 1.0, because 1.0 installs nothing at all. It is not a placeholder -
    /// it is the honest state of the application.
    /// </summary>
    public string ActionNote => IsBootCritical
        ? "Boot-critical. DriverGeek will not install this in any version — use the manufacturer's installer."
        : "DriverGeek 1.0 reports updates and does not install them.";
}
