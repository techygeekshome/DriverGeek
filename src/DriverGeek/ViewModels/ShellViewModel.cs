using System.Collections.ObjectModel;
using Avalonia.Threading;
using DriverGeek.Core.Models;
using DriverGeek.Core.Services;
using DriverGeek.Services;

namespace DriverGeek.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
    private readonly SettingsService _settings = new();
    private readonly ScanService _scan = new();
    private readonly InstallRunner _installer;

    private string _page = "Drivers";
    private bool _busy;
    private string _statusLine = "Not scanned yet.";
    private string? _searchError;
    private bool _rebootRequired;

    public ShellViewModel()
    {
        _settings.Load();
        _installer = new InstallRunner(_settings);

        ScanCommand = new RelayCommand(() => _ = ScanAsync(), () => !Busy);
        InstallCommand = new RelayCommand(() => _ = InstallAsync(), () => !Busy && SelectedCount > 0);
        RestartElevatedCommand = new RelayCommand(RestartElevated);
        ShowDrivers = new RelayCommand(() => Page = "Drivers");
        ShowUpdates = new RelayCommand(() => Page = "Updates");
        ShowSettings = new RelayCommand(() => Page = "Settings");
        Settings = new SettingsViewModel(_settings);
    }

    public ObservableCollection<DeviceRowViewModel> Devices { get; } = [];
    public ObservableCollection<UpdateRowViewModel> Updates { get; } = [];
    public SettingsViewModel Settings { get; }

    public RelayCommand ScanCommand { get; }
    public RelayCommand InstallCommand { get; }
    public RelayCommand RestartElevatedCommand { get; }
    public RelayCommand ShowDrivers { get; }
    public RelayCommand ShowUpdates { get; }
    public RelayCommand ShowSettings { get; }

    public string BrandName => AppInfo.Name;
    public string BrandBy => "by " + AppInfo.By;
    public string VersionText => AppInfo.Version + " · portable";

    public string Page
    {
        get => _page;
        set
        {
            if (!Set(ref _page, value)) return;
            Raise(nameof(IsDrivers));
            Raise(nameof(IsUpdates));
            Raise(nameof(IsSettings));
        }
    }

    public bool IsDrivers => Page == "Drivers";
    public bool IsUpdates => Page == "Updates";
    public bool IsSettings => Page == "Settings";

    public bool Busy
    {
        get => _busy;
        private set
        {
            if (!Set(ref _busy, value)) return;
            ScanCommand.RaiseCanExecuteChanged();
            InstallCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusLine { get => _statusLine; private set => Set(ref _statusLine, value); }

    /// <summary>
    /// Lets the window put something in the status line that did not come from a scan - the
    /// update check is the only caller. The setter stays private so nothing else can.
    /// </summary>
    public void SetStatus(string message) => StatusLine = message;
    public string? SearchError { get => _searchError; private set => Set(ref _searchError, value); }
    public bool HasSearchError => !string.IsNullOrEmpty(SearchError);

    public int DeviceCount => Devices.Count;
    public int UpdateCount => Updates.Count;
    public int OptionalCount => Updates.Count(u => u.IsOptional);
    public int UnsignedCount => Devices.Count(d => d.IsUnsigned);

    // --- installing ---------------------------------------------------------------------------

    /// <summary>Everything that has happened during this run of installs, newest last.</summary>
    public ObservableCollection<string> InstallLog { get; } = [];

    public bool HasInstallLog => InstallLog.Count > 0;

    public int SelectedCount => Updates.Count(u => u.IsSelected);
    public bool AnySelected => SelectedCount > 0;

    public string InstallButtonText => SelectedCount switch
    {
        0 => "Install ticked drivers",
        1 => "Install 1 ticked driver",
        var n => $"Install {n} ticked drivers"
    };

    public bool IsElevated => Elevation.IsElevated();
    public bool NeedsElevation => !IsElevated;

    public bool RebootRequired { get => _rebootRequired; private set => Set(ref _rebootRequired, value); }

    private void RestartElevated()
    {
        if (!Elevation.Relaunch()) return;

        if (Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private void OnSelectionChanged()
    {
        Raise(nameof(SelectedCount));
        Raise(nameof(AnySelected));
        Raise(nameof(InstallButtonText));
        InstallCommand.RaiseCanExecuteChanged();
    }

    private async Task InstallAsync()
    {
        var chosen = Updates.Where(u => u is { IsSelected: true, CanBeInstalled: true }).ToList();
        if (chosen.Count == 0) return;

        Busy = true;
        InstallLog.Clear();
        RebootRequired = false;
        Raise(nameof(HasInstallLog));

        var reports = new List<InstallReport>();

        try
        {
            foreach (var row in chosen)
            {
                StatusLine = $"Working on {row.DeviceName}…";
                Note($"— {row.DeviceName}");

                var progress = new Progress<string>(line =>
                {
                    row.Progress = line;
                    Note("   " + line);
                });

                var report = await Task.Run(() => _installer.Run(row.Device, row.Update, progress));
                reports.Add(report);

                row.Progress = report.Message;
                Note("   " + report.Message);

                if (report.RebootRequired) RebootRequired = true;
                if (report.Succeeded) row.IsSelected = false;
            }

            StatusLine = InstallSummary.For(reports);
        }
        catch (Exception ex)
        {
            Log.Write("Install run failed: " + ex);
            StatusLine = "The install stopped early. See drivergeek.log for the reason.";
            Note("   The install stopped early. See drivergeek.log for the reason.");
        }
        finally
        {
            Busy = false;
            OnSelectionChanged();
        }
    }

    private void Note(string line)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            InstallLog.Add(line);
            Raise(nameof(HasInstallLog));
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                InstallLog.Add(line);
                Raise(nameof(HasInstallLog));
            });
        }
    }

    private async Task ScanAsync()
    {
        Busy = true;
        SearchError = null;
        InstallLog.Clear();
        Raise(nameof(HasInstallLog));
        StatusLine = "Reading devices and asking Windows Update… this can take a few minutes.";

        try
        {
            // Off the UI thread: the online Windows Update search can take minutes, and a
            // blocked UI thread is what Windows reports as "not responding".
            var result = await Task.Run(() => _scan.Run(_settings.Current));

            Devices.Clear();
            foreach (var d in result.Devices) Devices.Add(new DeviceRowViewModel(d));

            foreach (var old in Updates) old.SelectionChanged -= OnSelectionChanged;
            Updates.Clear();
            foreach (var d in result.Devices.Where(x => x.Status != DeviceStatus.Current && x.Update is not null))
            {
                var row = new UpdateRowViewModel(d.Device, d.Update!);
                row.SelectionChanged += OnSelectionChanged;
                Updates.Add(row);
            }

            SearchError = result.Error;
            StatusLine = result.Error is null
                ? $"{result.DeviceCount} devices · {result.UpdateCount} updates · " +
                  $"{result.OptionalCount} of them hidden by Windows as optional"
                : "Devices read, but Windows Update could not be searched.";
        }
        catch (Exception ex)
        {
            Log.Write("Scan failed: " + ex);
            SearchError = "The scan could not finish. Nothing was changed on this machine. " +
                          "See drivergeek.log for the reason.";
            StatusLine = "The scan stopped early.";
        }
        finally
        {
            Busy = false;
            RebootRequired = false;
            Raise(nameof(DeviceCount));
            Raise(nameof(UpdateCount));
            Raise(nameof(OptionalCount));
            Raise(nameof(UnsignedCount));
            Raise(nameof(HasSearchError));
            OnSelectionChanged();
        }
    }
}
