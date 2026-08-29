using System.Diagnostics;
using CoreSchedule = DriverGeek.Core.Services.ScanSchedule;
using DriverGeek.Services;

namespace DriverGeek.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly SettingsService settings;

    public SettingsViewModel(SettingsService store)
    {
        settings = store;
        OpenBackupFolder = new RelayCommand(ShowBackupFolder);
    }

    public RelayCommand OpenBackupFolder { get; }

    public IReadOnlyList<string> ScheduleOptions => CoreSchedule.Options;

    public string ScanSchedule
    {
        get => settings.Current.ScanSchedule;
        set
        {
            if (settings.Current.ScanSchedule == value) return;
            settings.Current.ScanSchedule = value;
            settings.Save();
            Raise();
            Raise(nameof(SchedulePreview));
        }
    }

    public string SchedulePreview
    {
        get
        {
            var plan = CoreSchedule.Parse(settings.Current.ScanSchedule);
            return plan.NeedsScheduledTask
                ? $"{plan.Describe}, registered with Windows Task Scheduler as \"{CoreSchedule.TaskName}\". " +
                  "It only scans; it can never install."
                : $"{plan.Describe}.";
        }
    }

    public bool IncludeOptionalUpdates
    {
        get => settings.Current.IncludeOptionalUpdates;
        set
        {
            if (settings.Current.IncludeOptionalUpdates == value) return;
            settings.Current.IncludeOptionalUpdates = value;
            settings.Save();
            Raise();
        }
    }

    public bool NotifyOnUpdates
    {
        get => settings.Current.NotifyOnUpdates;
        set
        {
            if (settings.Current.NotifyOnUpdates == value) return;
            settings.Current.NotifyOnUpdates = value;
            settings.Save();
            Raise();
        }
    }

    public bool IncludeAbsentDevices
    {
        get => settings.Current.IncludeAbsentDevices;
        set
        {
            if (settings.Current.IncludeAbsentDevices == value) return;
            settings.Current.IncludeAbsentDevices = value;
            settings.Save();
            Raise();
        }
    }

    public string SystemProtectionStatus => SystemProtectionProbe.Describe();

    public string BackupFolder => string.IsNullOrWhiteSpace(settings.Current.BackupFolder)
        ? AppPaths.DefaultBackupFolder
        : settings.Current.BackupFolder;

    private void ShowBackupFolder()
    {
        try
        {
            Directory.CreateDirectory(BackupFolder);
            Process.Start(new ProcessStartInfo(BackupFolder) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Write("Could not open the backup folder: " + ex.Message);
        }
    }
}
