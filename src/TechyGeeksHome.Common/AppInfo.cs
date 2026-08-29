using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace TechyGeeksHome.Common;

/// <summary>A third-party component we want to credit in the About window.</summary>
public sealed record Credit(string Name, string Licence, string Url);

/// <summary>
/// Everything the shared About window and update check need to know about the app hosting them.
/// Fill one of these in once per application and pass it to <see cref="AboutWindow"/> and
/// <see cref="UpdateChecker"/>.
/// </summary>
public sealed class AppInfo
{
    public required string Name { get; init; }
    public required string Tagline { get; init; }
    public required string Description { get; init; }

    /// <summary>GitHub owner, e.g. "techygeekshome". Used for the update check and the repo link.</summary>
    public required string GitHubOwner { get; init; }

    /// <summary>GitHub repository name, e.g. "PDFGeek".</summary>
    public required string GitHubRepo { get; init; }

    /// <summary>The product's own page on the website.</summary>
    public required string ProductUrl { get; init; }

    public string WebsiteUrl { get; init; } = "https://techygeekshome.info";

    /// <summary>The standard TechyGeeksHome donation page. Same for every app in the range.</summary>
    public string DonateUrl { get; init; } = "https://ko-fi.com/techygeekshome";

    public string Publisher { get; init; } = "TechyGeeksHome";

    /// <summary>
    /// Optional Avalonia resource URI for the app icon, e.g. "avares://PDFGeek/Assets/pdfgeek.png".
    /// When set, the About window shows the real icon instead of a text monogram.
    /// </summary>
    public string? IconUri { get; init; }
    public string LicenceLine { get; init; } = "Free to use, including at work. No paid tier, ever.";

    public IReadOnlyList<Credit> Credits { get; init; } = Array.Empty<Credit>();

    public string RepositoryUrl => $"https://github.com/{GitHubOwner}/{GitHubRepo}";
    public string IssuesUrl => $"{RepositoryUrl}/issues";
    public string ReleasesUrl => $"{RepositoryUrl}/releases";

    /// <summary>
    /// The running version, read from the assembly so it can never drift from the build.
    /// </summary>
    public static Version CurrentVersion
    {
        get
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            return assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        }
    }

    public static string CurrentVersionText
    {
        get
        {
            var v = CurrentVersion;
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }

    /// <summary>Opens a URL in the user's default browser. Never throws.</summary>
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // A missing browser association is not worth crashing an About window over.
        }
    }
}
