using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using DriverGeek.ViewModels;
using TechyGeeksHome.Common;

namespace DriverGeek.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // InitializeComponent, NOT AvaloniaXamlLoader.Load. They are not interchangeable:
        // Load puts the XAML on the window but leaves every x:Name field null, so the two
        // buttons below were null and wiring them threw before the window was ever shown.
        // It compiled cleanly because the fields exist - they are simply never assigned.
        // That is what stopped 1.0.1 opening at all.
        InitializeComponent();

        // Belt and braces: a null here should cost the About button, not the application.
        try
        {
            AboutButton.Click += (_, _) => ShowAbout();
            CheckUpdatesButton.Click += async (_, _) => await CheckForUpdatesAsync();
        }
        catch (Exception ex)
        {
            DriverGeek.Services.Log.Write("Could not wire the About buttons: " + ex);
        }
    }

    private void ShowAbout()
    {
        try
        {
            new AboutWindow(AppMetadata.Info).ShowDialog(this);
        }
        catch (Exception ex)
        {
            Report("Could not open About: " + ex.Message);
        }
    }

    /// <summary>
    /// Asks GitHub whether there is a newer release. This is the only network call DriverGeek
    /// makes, and it only ever happens because someone pressed this button - which is why the
    /// sidebar says so rather than claiming nothing ever leaves the machine.
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        CheckUpdatesButton.IsEnabled = false;
        Report("Checking for updates…");
        try
        {
            var result = await UpdateChecker.CheckAsync(AppMetadata.Info);
            Report(result.Message);

            if (result.Status == UpdateStatus.UpdateAvailable)
            {
                TechyGeeksHome.Common.AppInfo.OpenUrl(result.ReleaseUrl ?? AppMetadata.Info.ReleasesUrl);
            }
        }
        catch (Exception ex)
        {
            Report("Could not check for updates: " + ex.Message);
        }
        finally
        {
            CheckUpdatesButton.IsEnabled = true;
        }
    }

    private void Report(string message)
    {
        if (DataContext is ShellViewModel vm)
        {
            vm.SetStatus(message);
        }
    }
}
