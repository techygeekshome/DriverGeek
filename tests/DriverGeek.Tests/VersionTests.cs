using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class VersionTests
{
    public static void Run()
    {
        Check.Section("Driver version parsing and comparison");

        Check.That("parses a normal four-part version", DriverVersion.TryParse("23.60.1.3", out var p) && p.Length == 4);
        Check.That("parses a three-part version", DriverVersion.TryParse("6.0.9622", out var q) && q.Length == 3);
        Check.That("parses a single number", DriverVersion.TryParse("10", out _));
        Check.That("refuses an empty string", !DriverVersion.TryParse("", out _));
        Check.That("refuses whitespace", !DriverVersion.TryParse("   ", out _));
        Check.That("refuses null", !DriverVersion.TryParse(null, out _));
        Check.That("refuses a version that starts with letters", !DriverVersion.TryParse("v23.60", out _));
        Check.That("stops at the first unusable part", DriverVersion.TryParse("23.60.beta.3", out var r) && r.Length == 2);
        Check.That("ignores a fifth part", DriverVersion.TryParse("1.2.3.4.5", out var s) && s.Length == 4);

        Check.Equal("higher major wins", 1, DriverVersion.Compare("24.0.0.0", "23.99.99.99"));
        Check.Equal("lower major loses", -1, DriverVersion.Compare("23.99.99.99", "24.0.0.0"));
        Check.Equal("identical versions are equal", 0, DriverVersion.Compare("1.2.3.4", "1.2.3.4"));
        Check.Equal("missing trailing parts count as zero", 0, DriverVersion.Compare("23.60", "23.60.0.0"));
        Check.Equal("compares numerically, not as text", 1, DriverVersion.Compare("1.10.0.0", "1.9.0.0"));
        Check.Equal("a big build number is not a string", 1, DriverVersion.Compare("6.0.9749.1", "6.0.9622.1"));

        Check.That("a newer version is newer", DriverVersion.IsNewer("23.80.0.9", "23.60.1.3"));
        Check.That("an equal version is not newer", !DriverVersion.IsNewer("23.60.1.3", "23.60.1.3"));
        Check.That("an older version is not newer", !DriverVersion.IsNewer("23.60.1.3", "23.80.0.9"));

        // The honesty rule: never claim an upgrade off the back of a string we did not understand.
        Check.That("unreadable candidate is never newer", !DriverVersion.IsNewer("Unknown", "23.60.1.3"));
        Check.That("unreadable installed version is never beaten", !DriverVersion.IsNewer("23.80.0.9", ""));
        Check.That("two unreadable versions compare equal", DriverVersion.Compare("x", "y") == 0);
    }
}
