namespace DriverGeek.Core.Services;

/// <summary>
/// Comparing Windows driver versions.
///
/// A driver version is four dotted numbers - 23.60.1.3 - but the strings that reach us are not
/// reliably that. WMI hands back an empty string for some inbox devices, vendors ship three parts
/// or five, and a few put letters in. Every one of those has to compare without throwing, because
/// this runs across every device on the machine and one odd string must not take out the scan.
///
/// Deliberate: a version we cannot parse is never treated as older than one we can. Saying
/// "you are out of date" on the strength of a string we did not understand is the exact
/// dishonesty this app exists to avoid.
/// </summary>
public static class DriverVersion
{
    public const int MaxParts = 4;

    /// <summary>Parses as many leading numeric parts as it can. Returns false on anything unusable.</summary>
    public static bool TryParse(string? text, out int[] parts)
    {
        parts = [];
        if (string.IsNullOrWhiteSpace(text)) return false;

        var raw = text.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (raw.Length == 0) return false;

        var got = new List<int>(MaxParts);
        foreach (var piece in raw)
        {
            if (got.Count == MaxParts) break;
            if (!int.TryParse(piece, out var n) || n < 0) break;
            got.Add(n);
        }

        if (got.Count == 0) return false;
        parts = got.ToArray();
        return true;
    }

    /// <summary>
    /// -1, 0 or 1 the usual way. Missing trailing parts count as zero, so 23.60 == 23.60.0.0.
    /// </summary>
    public static int Compare(string? left, string? right)
    {
        var okLeft = TryParse(left, out var a);
        var okRight = TryParse(right, out var b);

        if (!okLeft && !okRight) return 0;
        if (!okLeft) return -1;
        if (!okRight) return 1;

        for (var i = 0; i < MaxParts; i++)
        {
            var x = i < a.Length ? a[i] : 0;
            var y = i < b.Length ? b[i] : 0;
            if (x != y) return x < y ? -1 : 1;
        }
        return 0;
    }

    /// <summary>
    /// True only when we understood BOTH strings and the candidate really is higher. An
    /// unparsable version on either side answers false - see the note at the top of the class.
    /// </summary>
    public static bool IsNewer(string? candidate, string? installed)
    {
        if (!TryParse(candidate, out _) || !TryParse(installed, out _)) return false;
        return Compare(candidate, installed) > 0;
    }
}
