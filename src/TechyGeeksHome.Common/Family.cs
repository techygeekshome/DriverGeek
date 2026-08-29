// -----------------------------------------------------------------------------
//  THE TECHYGEEKSHOME FAMILY LIST
//
//  Canonical copy: PDFGeek repo, src/TechyGeeksHome.Common/Family.cs
//
//  The Geek range is spread across three UI frameworks - Avalonia (PDFGeek,
//  DiskGeek), WPF (AppGeek) and Go + WebView2 (Ultimate Settings Panel) - so
//  there is no single assembly every app can reference. This file is therefore
//  carried in each repo, byte-identical apart from the namespace on the line
//  below. It contains no framework types precisely so that stays true.
//
//  WHEN THE RANGE CHANGES: edit the canonical copy, then copy this file into
//  every other app repo. Do not edit one in isolation - that is exactly how the
//  WordPress plugins ended up with five different "our other plugins" lists.
//
//  Keep it in step with the hub page at techygeekshome.info/geek-tools/, which
//  is what a visitor sees. The blurbs below are that page's own wording.
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
            "CleanGeek",
            "Clears the caches, temporary files and update leftovers Windows keeps, and shows what is installed and what starts up.",
            "https://techygeekshome.info/cleangeek/",
            "CleanGeek"),

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
            "Ultimate Settings Panel",
            "250+ Windows settings, tools and commands in one searchable panel.",
            "https://techygeekshome.info/ultimate-settings-panel-online/",
            "Ultimate-Settings-Panel"),

        new FamilyApp(
            "Java 8 MSI Installers",
            "Oracle Java 8 extracted as MSIs, archived version by version, for silent deployment.",
            "https://techygeekshome.info/java-8-msi-installers/",
            null)
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
