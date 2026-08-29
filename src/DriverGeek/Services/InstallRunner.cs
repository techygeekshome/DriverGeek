using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Services;

/// <summary>
/// One device, start to finish: check the gate, take a restore point, export what is there now,
/// then let Windows Update do the install. Every refusal happens before anything is changed.
/// </summary>
public sealed class InstallRunner(SettingsService settings)
{
    private readonly DriverInstallService _installer = new();

    public InstallReport Run(DeviceDriver device, DriverUpdate update, IProgress<string> progress)
    {
        var elevated = Elevation.IsElevated();
        var protection = SystemProtectionProbe.IsEnabled() == true;

        // Everything the gate can rule on before any work is done. The two flags that are not
        // known yet are passed as true so their refusals do not mask a real one.
        var preflight = InstallGate.CanInstall(device, new InstallContext
        {
            RunningElevated = elevated,
            SystemProtectionEnabled = protection,
            Unattended = false,
            ExplicitlyChosen = true,
            RestorePointCreated = true,
            CurrentDriverExported = true
        });

        if (!preflight.Allowed)
            return new InstallReport
            {
                DeviceName = device.DeviceName,
                Stage = InstallStage.Refused,
                Succeeded = false,
                Message = preflight.Reason
            };

        progress.Report("Taking a System Restore point…");
        var restore = RestorePointService.Create($"Before DriverGeek updated {Short(device.DeviceName)}");
        progress.Report(restore.Message);

        if (!restore.Ok)
            return new InstallReport
            {
                DeviceName = device.DeviceName,
                Stage = InstallStage.RestorePoint,
                Succeeded = false,
                Message = restore.Message
            };

        progress.Report("Saving a copy of the driver that is installed now…");
        var backupRoot = string.IsNullOrWhiteSpace(settings.Current.BackupFolder)
            ? AppPaths.DefaultBackupFolder
            : settings.Current.BackupFolder;

        var export = DriverExportService.Export(device, update, backupRoot);
        progress.Report(export.Message);

        if (!export.Ok)
            return new InstallReport
            {
                DeviceName = device.DeviceName,
                Stage = InstallStage.Export,
                Succeeded = false,
                Message = export.Message
            };

        // The real gate, now that both facts are known.
        var gate = InstallGate.CanInstall(device, new InstallContext
        {
            RunningElevated = elevated,
            SystemProtectionEnabled = protection,
            Unattended = false,
            ExplicitlyChosen = true,
            RestorePointCreated = restore.Ok,
            CurrentDriverExported = export.Ok
        });

        if (!gate.Allowed)
            return new InstallReport
            {
                DeviceName = device.DeviceName,
                Stage = InstallStage.Refused,
                Succeeded = false,
                Message = gate.Reason,
                BackupPath = export.Folder
            };

        // The Windows Update installer wants a single-threaded apartment, and the thread this
        // runs on is not one.
        var outcome = Sta.Run(() => _installer.Run(update.UpdateId, progress));

        Log.Write($"Install of {update.Title} for {device.DeviceName}: " +
                  $"{(outcome.Ok ? "ok" : "failed")} - {outcome.Message}");

        return new InstallReport
        {
            DeviceName = device.DeviceName,
            Stage = outcome.Ok ? InstallStage.Done
                : outcome.FailedBeforeInstalling ? InstallStage.Download : InstallStage.Install,
            Succeeded = outcome.Ok,
            RebootRequired = outcome.RebootRequired,
            Message = outcome.Message,
            BackupPath = export.Folder
        };
    }

    private static string Short(string name) =>
        string.IsNullOrWhiteSpace(name) ? "a device" : name.Length <= 48 ? name : name[..48].TrimEnd() + "…";
}
