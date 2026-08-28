namespace DriverGeek.Tests;

/// <summary>Minimal assertion harness, so the test project takes no third-party dependencies.</summary>
public static class Check
{
    private static int _passed;
    private static readonly List<string> Failures = new();
    private static string _section = "";

    public static void Section(string name)
    {
        _section = name;
        Console.WriteLine();
        Console.WriteLine(name);
        Console.WriteLine(new string('-', name.Length));
    }

    public static void That(string what, bool condition)
    {
        if (condition)
        {
            _passed++;
            Console.WriteLine($"  ok    {what}");
        }
        else
        {
            Failures.Add($"{_section} · {what}");
            Console.WriteLine($"  FAIL  {what}");
        }
    }

    public static void Equal(string what, object? expected, object? actual)
    {
        var same = Equals(expected, actual);
        if (!same)
            what += $"  (expected '{expected ?? "null"}', got '{actual ?? "null"}')";
        That(what, same);
    }

    public static int Report()
    {
        Console.WriteLine();
        if (Failures.Count == 0)
        {
            Console.WriteLine($"All {_passed} checks passed.");
            return 0;
        }

        Console.WriteLine($"{_passed} passed, {Failures.Count} FAILED:");
        foreach (var f in Failures) Console.WriteLine("  · " + f);
        return 1;
    }
}
