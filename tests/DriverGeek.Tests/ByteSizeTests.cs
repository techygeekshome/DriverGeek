using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class ByteSizeTests
{
    public static void Run()
    {
        Check.Section("Sizes as people read them");

        Check.Equal("zero", "0 bytes", ByteSize.Format(0));
        Check.Equal("one byte is singular", "1 byte", ByteSize.Format(1));
        Check.Equal("a few bytes stay bytes", "512 bytes", ByteSize.Format(512));
        Check.Equal("a kilobyte", "1 KB", ByteSize.Format(1024));
        Check.Equal("a megabyte", "1 MB", ByteSize.Format(1024L * 1024));
        Check.Equal("one decimal place", "1.5 MB", ByteSize.Format(1024L * 1024 * 3 / 2));
        Check.Equal("no trailing zero", "2 MB", ByteSize.Format(1024L * 1024 * 2));
        Check.Equal("large values drop the decimal", "412 MB", ByteSize.Format(432013312L));
        Check.Equal("gigabytes", "1.5 GB", ByteSize.Format((long)(1024L * 1024 * 1024 * 1.5)));
        Check.Equal("a negative size is not a size", "", ByteSize.Format(-1));
    }
}
