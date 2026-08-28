namespace DriverGeek.Core.Services;

/// <summary>Sizes the way a person reads them. One decimal place, and never "0.0 KB".</summary>
public static class ByteSize
{
    private static readonly string[] Units = ["bytes", "KB", "MB", "GB", "TB"];

    public static string Format(long bytes)
    {
        if (bytes < 0) return "";
        if (bytes == 0) return "0 bytes";
        if (bytes < 1024) return bytes == 1 ? "1 byte" : $"{bytes} bytes";

        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < Units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return value >= 100
            ? $"{Math.Round(value)} {Units[unit]}"
            : $"{value.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)} {Units[unit]}";
    }
}
