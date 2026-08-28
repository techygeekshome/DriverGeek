namespace DriverGeek.Tests;

/// <summary>
/// A deliberately tiny assertion harness, copied from AppGeek. DriverGeek takes no third-party
/// dependencies outside the UI framework, and that rule is worth more than the conveniences xunit
/// would add: every package in the tree is a package a reviewer has to trust, and this project
/// asks people to trust it with administrator rights.
/// </summary>
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
