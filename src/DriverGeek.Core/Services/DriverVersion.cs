namespace DriverGeek.Core.Services;

/// <summary>
/// Compares Windows driver version strings. WMI reports empty versions for some inbox devices,
/// and vendors ship three, five or non-numeric parts, so nothing here throws. A version that
/// cannot be parsed is never treated as older than one that can.
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

    /// <summary>-1, 0 or 1. Missing trailing parts count as zero, so 23.60 == 23.60.0.0.</summary>
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

    /// <summary>True only when both strings parse and the candidate is higher.</summary>
    public static bool IsNewer(string? candidate, string? installed)
    {
        if (!TryParse(candidate, out _) || !TryParse(installed, out _)) return false;
        return Compare(candidate, installed) > 0;
    }
}
