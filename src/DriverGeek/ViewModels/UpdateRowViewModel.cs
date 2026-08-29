using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.ViewModels;

public sealed class UpdateRowViewModel(DeviceDriver device, DriverUpdate update) : ObservableObject
{
    private bool _isSelected;
    private string _progress = "";

    public DeviceDriver Device => device;
    public DriverUpdate Update => update;

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

    /// <summary>Boot-critical devices have no tick box. Everything else does.</summary>
    public bool CanBeInstalled => !IsBootCritical;

    public bool IsSelected
    {
        get => _isSelected;
        set { if (Set(ref _isSelected, value)) SelectionChanged?.Invoke(); }
    }

    /// <summary>Raised so the shell can re-count what is ticked.</summary>
    public event Action? SelectionChanged;

    /// <summary>What is happening to this row right now, or how it finished. Empty until it is asked to install.</summary>
    public string Progress
    {
        get => _progress;
        set { if (Set(ref _progress, value)) Raise(nameof(HasProgress)); }
    }

    public bool HasProgress => !string.IsNullOrEmpty(Progress);

    public string ActionNote => IsBootCritical
        ? "Boot-critical. DriverGeek will not install this in any version — use the manufacturer's installer."
        : InstallGate.WarningFor(device) is { Length: > 0 } warning
            ? warning
            : "";

    public bool HasActionNote => ActionNote.Length > 0;
}
