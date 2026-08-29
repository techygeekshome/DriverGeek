using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TechyGeeksHome.Common;

public enum UpdateStatus
{
    UpToDate,
    UpdateAvailable,
    NoReleasesYet,
    CouldNotCheck
}

public sealed record UpdateResult(
    UpdateStatus Status,
    string CurrentVersion,
    string? LatestVersion,
    string? ReleaseUrl,
    string Message);

/// <summary>
/// Asks GitHub's public releases API whether there is a newer tag than the running build.
///
/// This is the only network call any TechyGeeksHome tool makes, and it is worth being precise
/// about what it does: a single unauthenticated GET to api.github.com. It sends no identifiers,
/// no file names, no usage data and no telemetry of any kind - GitHub sees an IP address and a
/// user-agent string, exactly as it would if the user opened the releases page in a browser.
/// It never downloads or installs anything; finding an update just offers to open the page.
///
/// Note for anyone editing this: do NOT add ConfigureAwait(false). Callers are UI code and
/// need the continuation back on the dispatcher thread. Doing otherwise crashed the app.
/// </summary>
public static class UpdateChecker
{
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"TechyGeeksHome-Updater/{AppInfo.CurrentVersionText}");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    public static async Task<UpdateResult> CheckAsync(AppInfo app, CancellationToken cancellationToken = default)
    {
        var current = AppInfo.CurrentVersionText;
        var url = $"https://api.github.com/repos/{app.GitHubOwner}/{app.GitHubRepo}/releases/latest";

        try
        {
            using var response = await Http.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new UpdateResult(UpdateStatus.NoReleasesYet, current, null, app.ReleasesUrl,
                    "No releases have been published yet.");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return new UpdateResult(UpdateStatus.CouldNotCheck, current, null, app.ReleasesUrl,
                    "GitHub is rate limiting update checks right now. Try again later.");

            if (!response.IsSuccessStatusCode)
                return new UpdateResult(UpdateStatus.CouldNotCheck, current, null, app.ReleasesUrl,
                    $"Could not reach GitHub ({(int)response.StatusCode}).");

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var tag = root.TryGetProperty("tag_name", out var tagElement) ? tagElement.GetString() : null;
            var page = root.TryGetProperty("html_url", out var pageElement) ? pageElement.GetString() : null;
            page ??= app.ReleasesUrl;

            if (string.IsNullOrWhiteSpace(tag))
                return new UpdateResult(UpdateStatus.CouldNotCheck, current, null, page,
                    "GitHub did not return a version number.");

            if (!TryParseVersion(tag, out var latest))
                return new UpdateResult(UpdateStatus.CouldNotCheck, current, tag, page,
                    $"Could not read the latest version ({tag}).");

            var running = AppInfo.CurrentVersion;
            var latestText = $"{latest.Major}.{latest.Minor}.{latest.Build}";

            return Normalise(latest) > Normalise(running)
                ? new UpdateResult(UpdateStatus.UpdateAvailable, current, latestText, page,
                    $"Version {latestText} is available. You have {current}.")
                : new UpdateResult(UpdateStatus.UpToDate, current, latestText, page,
                    $"You are running the latest version ({current}).");
        }
        catch (TaskCanceledException)
        {
            return new UpdateResult(UpdateStatus.CouldNotCheck, current, null, app.ReleasesUrl,
                "The update check timed out.");
        }
        catch (Exception ex)
        {
            return new UpdateResult(UpdateStatus.CouldNotCheck, current, null, app.ReleasesUrl,
                $"Could not check for updates: {ex.Message}");
        }
    }

    /// <summary>Accepts "v1.2.3", "1.2.3", "release-1.2" and similar.</summary>
    public static bool TryParseVersion(string tag, out Version version)
    {
        version = new Version(0, 0, 0, 0);

        var span = tag.AsSpan();
        var start = 0;
        while (start < span.Length && !char.IsDigit(span[start])) start++;
        if (start >= span.Length) return false;

        var end = start;
        while (end < span.Length && (char.IsDigit(span[end]) || span[end] == '.')) end++;

        var cleaned = span[start..end].ToString().Trim('.');
        return cleaned.Length > 0 && Version.TryParse(
            cleaned.Contains('.') ? cleaned : cleaned + ".0", out version!);
    }

    /// <summary>Treats unset build/revision components as zero so 1.2 and 1.2.0.0 compare equal.</summary>
    private static Version Normalise(Version v)
        => new(v.Major, v.Minor, Math.Max(v.Build, 0), Math.Max(v.Revision, 0));
}
