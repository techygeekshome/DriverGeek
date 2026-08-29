namespace DriverGeek.Core.Services;

/// <summary>
/// Windows refuses to take a second System Restore point within
/// SystemRestorePointCreationFrequency minutes of the last one - 24 hours out of the box.
/// DriverGeek does not change that setting behind your back. When Windows refuses for that
/// reason and there is already a point from inside the window, that point is what an install
/// would roll back to, so it counts and the install goes ahead. Anything older does not.
/// </summary>
public static class RestorePointPolicy
{
    public static readonly TimeSpan Window = TimeSpan.FromHours(24);

    public static bool CountsAsRecent(DateTime? newest, DateTime now)
    {
        if (newest is null) return false;
        var age = now - newest.Value;
        return age >= TimeSpan.Zero && age <= Window;
    }

    /// <summary>The line shown when an existing point is used instead of a new one.</summary>
    public static string ReusedNote(DateTime when) =>
        $"Windows would not take a second restore point today, so the one from " +
        $"{when:HH:mm} on {when:dd MMM} will be used instead.";
}
