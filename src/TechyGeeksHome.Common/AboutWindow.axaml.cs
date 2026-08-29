using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace TechyGeeksHome.Common;

public partial class AboutWindow : Window
{
    private readonly AppInfo _app;
    private bool _checking;

    // Parameterless constructor exists only so the XAML previewer can load the window.
    public AboutWindow() : this(new AppInfo
    {
        Name = "TechyGeeksHome",
        Tagline = "Free software for Windows",
        Description = "Preview.",
        GitHubOwner = "techygeekshome",
        GitHubRepo = "PDFGeek",
        ProductUrl = "https://techygeekshome.info"
    })
    {
    }

    public AboutWindow(AppInfo app)
    {
        _app = app;
        InitializeComponent();

        Title = $"About {app.Name}";
        AppName.Text = app.Name;
        AppTagline.Text = app.Tagline;
        AppVersion.Text = $"Version {AppInfo.CurrentVersionText}  ·  {app.Publisher}";
        AppDescription.Text = app.Description;
        LicenceText.Text = app.LicenceLine;
        Monogram.Text = Monogram2(app.Name);
        TryShowIcon(app.IconUri);

        WebsiteButton.Click += (_, _) => AppInfo.OpenUrl(app.WebsiteUrl);
        ProductButton.Click += (_, _) => AppInfo.OpenUrl(app.ProductUrl);
        RepoButton.Click += (_, _) => AppInfo.OpenUrl(app.RepositoryUrl);
        IssuesButton.Click += (_, _) => AppInfo.OpenUrl(app.IssuesUrl);
        DonateButton.Click += (_, _) => AppInfo.OpenUrl(app.DonateUrl);

        CloseButton.Click += (_, _) => Close();
        CheckUpdatesButton.Click += async (_, _) => await CheckAsync();

        BuildFamilyList(app.GitHubRepo);
        FamilyHubButton.Click += (_, _) => AppInfo.OpenUrl(Family.HubUrl);

        if (app.Credits.Count == 0)
        {
            CreditsSection.IsVisible = false;
        }
        else
        {
            foreach (var credit in app.Credits)
            {
                var button = new Button
                {
                    Content = $"{credit.Name} — {credit.Licence}",
                    Background = Brushes.Transparent,
                    BorderThickness = default,
                    Foreground = new SolidColorBrush(Color.Parse("#9ca3af")),
                    Padding = new Avalonia.Thickness(0, 1),
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Cursor = new Cursor(StandardCursorType.Hand)
                };
                var url = credit.Url;
                button.Click += (_, _) => AppInfo.OpenUrl(url);
                CreditsList.Children.Add(button);
            }
        }
    }

    /// <summary>
    /// Renders the rest of the range, with this app removed from its own list.
    ///
    /// The list is data in <see cref="Family"/> rather than markup here, so adding a tool to
    /// the range is one edit rather than one edit per application. Every row opens the product
    /// page in the browser; nothing is downloaded and nothing phones home to build this - the
    /// list ships inside the executable.
    /// </summary>
    private void BuildFamilyList(string ownRepo)
    {
        foreach (var app in Family.Others(ownRepo))
        {
            var name = new TextBlock
            {
                Text = app.Name,
                FontSize = 12.5,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.Parse("#38bdf8"))
            };

            var blurb = new TextBlock
            {
                Text = app.Blurb,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.Parse("#9ca3af")),
                TextWrapping = TextWrapping.Wrap
            };

            var stack = new StackPanel { Spacing = 1 };
            stack.Children.Add(name);
            stack.Children.Add(blurb);

            var button = new Button
            {
                Content = stack,
                Background = Brushes.Transparent,
                BorderThickness = default,
                Padding = new Avalonia.Thickness(0, 5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            var url = app.ProductUrl;
            button.Click += (_, _) => AppInfo.OpenUrl(url);
            FamilyList.Children.Add(button);
        }
    }

    /// <summary>Swaps the placeholder monogram for the real app icon when one is supplied.</summary>
    private void TryShowIcon(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri)) return;
        try
        {
            IconImage.Source = new Bitmap(AssetLoader.Open(new Uri(uri)));
            IconImage.IsVisible = true;
            MonogramBadge.IsVisible = false;
        }
        catch
        {
            // Missing or malformed asset just leaves the monogram in place.
        }
    }

    /// <summary>Two-letter monogram for the badge: "PDFGeek" becomes "PG".</summary>
    private static string Monogram2(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "TG";

        // Split on the transition into the trailing capitalised word, e.g. PDF|Geek.
        for (var i = name.Length - 1; i > 0; i--)
        {
            if (!char.IsUpper(name[i])) continue;
            return $"{char.ToUpperInvariant(name[0])}{char.ToUpperInvariant(name[i])}";
        }

        return name.Length >= 2
            ? name[..2].ToUpperInvariant()
            : name.ToUpperInvariant();
    }

    /// <summary>
    /// Runs the update check and reports the outcome inline. Offers the releases page rather
    /// than downloading anything - the app never installs its own updates.
    /// </summary>
    public async Task CheckAsync()
    {
        if (_checking) return;
        _checking = true;
        CheckUpdatesButton.IsEnabled = false;
        UpdateStatusText.Text = "Checking…";
        UpdateStatusText.Foreground = new SolidColorBrush(Color.Parse("#9ca3af"));

        try
        {
            var result = await UpdateChecker.CheckAsync(_app);
            UpdateStatusText.Text = result.Message;

            if (result.Status == UpdateStatus.UpdateAvailable)
            {
                UpdateStatusText.Foreground = new SolidColorBrush(Color.Parse("#38bdf8"));
                CheckUpdatesButton.Content = "Open the download page";
                CheckUpdatesButton.Click += (_, _) => AppInfo.OpenUrl(result.ReleaseUrl ?? _app.ReleasesUrl);
            }
        }
        finally
        {
            CheckUpdatesButton.IsEnabled = true;
            _checking = false;
        }
    }
}
