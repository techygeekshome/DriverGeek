using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class ClassTests
{
    public static void Run()
    {
        Check.Section("Device class risk");

        const string scsi = "{4d36e97b-e325-11ce-bfc1-08002be10318}";
        const string display = "{4d36e968-e325-11ce-bfc1-08002be10318}";
        const string printer = "{4d36e979-e325-11ce-bfc1-08002be10318}";

        Check.Equal("a storage controller is boot-critical", DriverRisk.BootCritical, DriverClass.RiskOf(scsi));
        Check.Equal("a display adapter is awkward, not fatal", DriverRisk.Awkward, DriverClass.RiskOf(display));
        Check.Equal("a printer is ordinary", DriverRisk.Ordinary, DriverClass.RiskOf(printer));

        Check.That("boot-critical is recognised without braces",
            DriverClass.IsBootCritical("4d36e97b-e325-11ce-bfc1-08002be10318"));
        Check.That("boot-critical is recognised in upper case",
            DriverClass.IsBootCritical(scsi.ToUpperInvariant()));

        // Class names are localised, so the GUID has to win when both are present.
        Check.Equal("the GUID beats a misleading class name",
            DriverRisk.BootCritical, DriverClass.RiskOf(scsi, "Printer"));
        Check.Equal("a German class name still resolves by GUID",
            DriverRisk.Awkward, DriverClass.RiskOf(display, "Grafikkarte"));

        // ...and the name is only a fallback when there is no usable GUID.
        Check.Equal("falls back to the class name when the GUID is missing",
            DriverRisk.BootCritical, DriverClass.RiskOf(null, "SCSIAdapter"));
        Check.Equal("falls back when the GUID is not a GUID",
            DriverRisk.BootCritical, DriverClass.RiskOf("not-a-guid", "DiskDrive"));
        Check.Equal("an unknown class with no GUID is ordinary",
            DriverRisk.Ordinary, DriverClass.RiskOf(null, "Fingerprint"));
        Check.Equal("nothing at all is ordinary", DriverRisk.Ordinary, DriverClass.RiskOf(null, null));

        Check.That("chipset System class is boot-critical",
            DriverClass.IsBootCritical("{4d36e97d-e325-11ce-bfc1-08002be10318}"));
        Check.That("network is not boot-critical",
            !DriverClass.IsBootCritical("{4d36e972-e325-11ce-bfc1-08002be10318}"));
    }
}
