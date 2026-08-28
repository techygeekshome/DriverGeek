using DriverGeek.Core.Models;
using DriverGeek.Core.Services;

namespace DriverGeek.Tests;

public static class StalenessTests
{
    private static DeviceDriver Device(string name, string version, string hwid = "", string maker = "", string cls = "")
        => new()
        {
            DeviceName = name,
            DriverVersion = version,
            DeviceId = hwid,
            Manufacturer = maker,
            ClassGuid = cls
        };

    private static DriverUpdate Update(string model, string version, bool optional = false,
                                       string hwid = "", string maker = "")
        => new()
        {
            DriverModel = model,
            DriverVersion = version,
            IsOptional = optional,
            DriverHardwareId = hwid,
            DriverManufacturer = maker
        };

    public static void Run()
    {
        Check.Section("What counts as a problem");

        var wifi = Device("Intel Wi-Fi 6E AX211 160MHz", "23.60.1.3",
            hwid: @"PCI\VEN_8086&DEV_51F0", maker: "Intel Corporation");

        // The headline behaviour: an optional update is found and correctly labelled.
        var optional = Update("Intel Wi-Fi 6E AX211 160MHz", "23.80.0.9", optional: true,
            hwid: @"PCI\VEN_8086&DEV_51F0");
        Check.Equal("an optional update is surfaced as such",
            DeviceStatus.UpdateHiddenAsOptional, StalenessPolicy.StatusFor(wifi, [optional]));

        var offered = Update("Intel Wi-Fi 6E AX211 160MHz", "23.80.0.9", hwid: @"PCI\VEN_8086&DEV_51F0");
        Check.Equal("an ordinary update is surfaced as offered",
            DeviceStatus.UpdateOffered, StalenessPolicy.StatusFor(wifi, [offered]));

        // The honesty rule, stated as a test: age alone is never a finding.
        var ancient = Device("AMD SMBus", "5.12.0.38");
        Check.Equal("a 2023 driver with nothing newer is Current",
            DeviceStatus.Current, StalenessPolicy.StatusFor(ancient, []));
        Check.Equal("an age note is a fact, not a warning",
            "3 years old", StalenessPolicy.AgeNote(new DateTime(2023, 7, 31), new DateTime(2026, 8, 28)));
        Check.Equal("a driver from this year says so",
            "this year", StalenessPolicy.AgeNote(new DateTime(2026, 3, 1), new DateTime(2026, 8, 28)));
        Check.Equal("no date means no note", "", StalenessPolicy.AgeNote(null, new DateTime(2026, 8, 28)));

        // Not an upgrade.
        Check.Equal("an equal version is not an update",
            DeviceStatus.Current, StalenessPolicy.StatusFor(wifi, [Update("Intel Wi-Fi 6E AX211 160MHz", "23.60.1.3", hwid: @"PCI\VEN_8086&DEV_51F0")]));
        Check.Equal("an older version is not an update",
            DeviceStatus.Current, StalenessPolicy.StatusFor(wifi, [Update("Intel Wi-Fi 6E AX211 160MHz", "22.10.0.1", hwid: @"PCI\VEN_8086&DEV_51F0")]));

        Check.Section("Matching an update to a device");

        Check.That("hardware ID matches", StalenessPolicy.Matches(wifi, optional));
        Check.That("a different hardware ID does not match",
            !StalenessPolicy.Matches(wifi, Update("Something else", "9.9.9.9", hwid: @"PCI\VEN_10DE&DEV_2786")));

        // A model name on its own is not unique - "Wireless Adapter" is on a hundred machines.
        var vague = Device("Wireless Adapter", "1.0.0.0", maker: "Realtek");
        Check.That("model alone is not enough without a manufacturer",
            !StalenessPolicy.Matches(vague, Update("Wireless Adapter", "2.0.0.0")));
        Check.That("model plus manufacturer is enough",
            StalenessPolicy.Matches(vague, Update("Wireless Adapter", "2.0.0.0", maker: "Realtek")));
        Check.That("a different manufacturer does not match",
            !StalenessPolicy.Matches(vague, Update("Wireless Adapter", "2.0.0.0", maker: "Intel Corporation")));

        Check.Section("Picking between several updates");

        var many = new[]
        {
            Update("Intel Wi-Fi 6E AX211 160MHz", "23.70.0.1", hwid: @"PCI\VEN_8086&DEV_51F0"),
            Update("Intel Wi-Fi 6E AX211 160MHz", "23.80.0.9", optional: true, hwid: @"PCI\VEN_8086&DEV_51F0"),
            Update("Intel Wi-Fi 6E AX211 160MHz", "23.65.0.0", hwid: @"PCI\VEN_8086&DEV_51F0"),
        };
        Check.Equal("the highest version wins", "23.80.0.9", StalenessPolicy.BestUpdateFor(wifi, many)?.DriverVersion);
        Check.That("nothing applicable returns null", StalenessPolicy.BestUpdateFor(wifi, []) is null);

        var unreadable = new[] { Update("Intel Wi-Fi 6E AX211 160MHz", "latest", hwid: @"PCI\VEN_8086&DEV_51F0") };
        Check.That("an unreadable update version is not offered as an upgrade",
            StalenessPolicy.BestUpdateFor(wifi, unreadable) is null);
    }
}
