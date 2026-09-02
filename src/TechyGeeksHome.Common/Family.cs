// -----------------------------------------------------------------------------
//  THE TECHYGEEKSHOME FAMILY LIST
//
//  Canonical copy: PDFGeek repo, src/TechyGeeksHome.Common/Family.cs
//
//  The Geek range is spread across four stacks - Avalonia (PDFGeek, DiskGeek,
//  CutGeek, TranscribeGeek, SoundGeek, AuthGeek), WPF (AppGeek), Go + WebView2
//  (Ultimate Settings Panel) and Python (ReelGeek, ShortGeek) - so there is no
//  single assembly every app can reference. This file is therefore carried in
//  each .NET repo, byte-identical apart from the namespace on the line below.
//  It contains no framework types precisely so that stays true. ReelGeek and
//  ShortGeek are not .NET and carry the same list in their own source.
//
//  WHEN THE RANGE CHANGES: edit the canonical copy, then copy this file into
//  every other app repo. Do not edit one in isolation - that is exactly how the
//  WordPress plugins ended up with five different "our other plugins" lists.
//
//  Keep it in step with the hub page at techygeekshome.info/geek-tools/, which
//  is what a visitor sees. The blurbs below are that page's own wording.
//
//  APPLICATIONS ONLY. This list is rendered as a row of buttons in the About
//  window, one per app, each opening that app's own page on the website. Things
//  that are not our software - the Java 8 MSI archive, for one - do not belong
//  in it, however much they live on the same site.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace TechyGeeksHome.Common;

/// <summary>One product in the TechyGeeksHome range.</summary>
/// <param name="Name">Display name, exactly as the website writes it.</param>
/// <param name="Blurb">One line. What it does, not why it is good.</param>
/// <param name="ProductUrl">Its page on techygeekshome.info.</param>
/// <param name="RepoName">
/// The GitHub repository name, used to identify the running app so it can hide
/// itself from its own list. Null for anything that is not an application.
/// </param>
public sealed record FamilyApp(string Name, string Blurb, string ProductUrl, string? RepoName);

public static class Family
{
    /// <summary>The hub page listing everything, for the "see them all" link.</summary>
    public const string HubUrl = "https://techygeekshome.info/geek-tools/";

    /// <summary>
    /// Everything TechyGeeksHome makes, in the order the hub page lists it.
    /// </summary>
    public static readonly IReadOnlyList<FamilyApp> All = new[]
    {
        new FamilyApp(
            "AppGeek",
            "Scans everything installed, matches it against winget and shows what is out of date.",
            "https://techygeekshome.info/appgeek/",
            "AppGeek"),

        new FamilyApp(
            "AuthGeek",
            "Your two-factor codes on your own computer, in one encrypted file.",
            "https://techygeekshome.info/authgeek/",
            "AuthGeek"),

        new FamilyApp(
            "CleanGeek",
            "Clears the caches, temporary files and update leftovers Windows keeps, and shows what is installed and what starts up.",
            "https://techygeekshome.info/cleangeek/",
            "CleanGeek"),

        new FamilyApp(
            "CutGeek",
            "Cuts the background out of a photo on your own machine, at full resolution, with nothing to buy.",
            "https://techygeekshome.info/cutgeek/",
            "CutGeek"),

        new FamilyApp(
            "DiskGeek",
            "Find out exactly where the disk space went - treemap, duplicates and snapshots.",
            "https://techygeekshome.info/diskgeek/",
            "DiskGeek"),

        new FamilyApp(
            "DriverGeek",
            "Lists every driver on the machine and the updates Windows Update files under Optional and never offers.",
            "https://techygeekshome.info/drivergeek/",
            "DriverGeek"),

        new FamilyApp(
            "PDFGeek",
            "Merge, split, rotate, compress and convert PDFs entirely on your own machine.",
            "https://techygeekshome.info/pdfgeek/",
            "PDFGeek"),

        new FamilyApp(
            "ReelGeek",
            "Turns a folder of photos into a vertical edit cut to a beat grid, with movement on every shot.",
            "https://techygeekshome.info/reelgeek/",
            "ReelGeek"),

        new FamilyApp(
            "ShortGeek",
            "Turns one of your guides, an RSS feed or a bare idea into a narrated, captioned vertical short.",
            "https://techygeekshome.info/shortgeek/",
            "ShortGeek"),

        new FamilyApp(
            "SoundGeek",
            "Cleans up a recording and writes the copy beside it: background noise gone, mains hum gone, levels evened out.",
            "https://techygeekshome.info/soundgeek/",
            "SoundGeek"),

        new FamilyApp(
            "TranscribeGeek",
            "Turns audio and video into a transcript and subtitles, entirely offline.",
            "https://techygeekshome.info/transcribegeek/",
            "TranscribeGeek"),

        new FamilyApp(
            "Ultimate Settings Panel",
            "250+ Windows settings, tools and commands in one searchable panel.",
            "https://techygeekshome.info/ultimate-settings-panel-online/",
            "Ultimate-Settings-Panel")
    };

    /// <summary>
    /// The range with the running app removed, so AppGeek never advertises AppGeek.
    /// Matched on repository name because that is the one identifier an app knows
    /// about itself that never gets reworded.
    /// </summary>
    public static IReadOnlyList<FamilyApp> Others(string? ownRepoName) =>
        All.Where(a => !string.Equals(a.RepoName, ownRepoName, StringComparison.OrdinalIgnoreCase))
           .ToList();
}
