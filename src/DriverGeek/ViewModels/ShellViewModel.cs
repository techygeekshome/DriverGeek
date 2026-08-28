using System.Collections.ObjectModel;
using DriverGeek.Core.Models;
using DriverGeek.Services;

namespace DriverGeek.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
    private readonly SettingsService _settings = new();
    private readonly ScanService _scan = new();

    private string _page = "Drivers";
    private bool _busy;
    private string _statusLine = "Not scanned yet.";
    private string? _searchError;

    public ShellViewModel()
    {
        _settings.Load();
        ScanCommand = new RelayCommand(() => _ = ScanAsync(), () => !Busy);
        ShowDrivers = new RelayCommand(() => Page = "Drivers");
        ShowUpdates = new RelayCommand(() => Page = "Updates");
        ShowSettings = new RelayCommand(() => Page = "Settings");
        Settings = new SettingsViewModel(_settings);
    }

    public ObservableCollection<DeviceRowViewModel> Devices { get; } = [];
    public ObservableCollection<UpdateRowViewModel> Updates { get; } = [];
    public SettingsViewModel Settings { get; }

    public RelayCommand ScanCommand { get; }
    public RelayCommand ShowDrivers { get; }
    public RelayCommand ShowUpdates { get; }
    public RelayCommand ShowSettings { get; }

    public string BrandName => AppInfo.Name;
    public string BrandBy => "by " + AppInfo.By;
    public string VersionText => AppInfo.Version + " · portable";
    public string NetworkPromise => AppInfo.NetworkPromise;

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
        private set { if (Set(ref _busy, value)) ScanCommand.RaiseCanExecuteChanged(); }
    }

    public string StatusLine { get => _statusLine; private set => Set(ref _statusLine, value); }
    public string? SearchError { get => _searchError; private set => Set(ref _searchError, value); }
    public bool HasSearchError => !string.IsNullOrEmpty(SearchError);

    public int DeviceCount => Devices.Count;
    public int UpdateCount => Updates.Count;
    public int OptionalCount => Updates.Count(u => u.IsOptional);
    public int UnsignedCount => Devices.Count(d => d.IsUnsigned);

    private async Task ScanAsync()
    {
        Busy = true;
        SearchError = null;
        StatusLine = "Reading devices and asking Windows Update… this can take a few minutes.";

        try
        {
            // Off the UI thread: the online Windows Update search can take minutes, and a
            // blocked UI thread is what Windows reports as "not responding".
            var result = await Task.Run(() => _scan.Run(_settings.Current));

            Devices.Clear();
            foreach (var d in result.Devices) Devices.Add(new DeviceRowViewModel(d));

            Updates.Clear();
            foreach (var d in result.Devices.Where(x => x.Status != DeviceStatus.Current && x.Update is not null))
                Updates.Add(new UpdateRowViewModel(d.Device, d.Update!));

            SearchError = result.Error;
            StatusLine = result.Error is null
                ? $"{result.DeviceCount} devices · {result.UpdateCount} updates · " +
                  $"{result.OptionalCount} of them hidden by Windows as optional · nothing was installed"
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
            Raise(nameof(DeviceCount));
            Raise(nameof(UpdateCount));
            Raise(nameof(OptionalCount));
            Raise(nameof(UnsignedCount));
            Raise(nameof(HasSearchError));
        }
    }
}
