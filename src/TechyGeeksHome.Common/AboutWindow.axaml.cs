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
        GitHubProfileButton.Click += (_, _) => AppInfo.OpenUrl(GitHubProfileUrl);
        FamilyHubButton.Click += (_, _) => AppInfo.OpenUrl(Family.HubUrl);
    }

    /// <summary>The whole range on GitHub, for the button that squares the grid off.</summary>
    private const string GitHubProfileUrl = "https://github.com/techygeekshome";

    /// <summary>
    /// Renders the rest of the range as buttons, with this app removed from its own list.
    ///
    /// Every button carries the app's name and opens ITS PAGE ON THE WEBSITE, not its
    /// repository - someone reading an About box wants the product, not the source.
    ///
    /// The grid is two columns, so an odd number of apps would leave a gap. When that
    /// happens the GitHub profile button fills it; when the count is even it drops below
    /// instead, full width and in the accent colour, the way the Ko-fi button sits above.
    /// </summary>
    private void BuildFamilyList(string ownRepo)
    {
        var others = Family.Others(ownRepo);
        var oddCount = others.Count % 2 == 1;

        for (var i = 0; i < others.Count; i++)
        {
            FamilyGrid.Children.Add(FamilyButton(others[i].Name, others[i].ProductUrl, i));
        }

        if (oddCount)
        {
            FamilyGrid.Children.Add(
                FamilyButton("All our code on GitHub", GitHubProfileUrl, others.Count));
            GitHubProfileButton.IsVisible = false;
        }
    }

    /// <summary>
    /// One cell of the range grid, styled like the Website and Product page buttons above it.
    /// The margin alternates so the gutter between the columns matches the rows.
    /// </summary>
    private static Button FamilyButton(string text, string url, int index)
    {
        var button = new Button
        {
            Content = text,
            Classes = { "ghost" },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
            Margin = index % 2 == 0
                ? new Avalonia.Thickness(0, 0, 4, 8)
                : new Avalonia.Thickness(4, 0, 0, 8)
        };

        button.Click += (_, _) => AppInfo.OpenUrl(url);
        return button;
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
