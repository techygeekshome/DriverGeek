namespace DriverGeek.Services;

/// <summary>
/// Runs a piece of work on its own single-threaded apartment. The Windows Update Agent's
/// installer is happiest called that way, and a thread pool thread is not one.
/// </summary>
public static class Sta
{
    public static T Run<T>(Func<T> work)
    {
        T result = default!;
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try { result = work(); }
            catch (Exception ex) { failure = ex; }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();

        if (failure is not null) throw failure;
        return result;
    }
}
