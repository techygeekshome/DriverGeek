using DriverGeek.Tests;

// DriverGeek's test harness. Run it with:
//     dotnet run --project tests/DriverGeek.Tests -c Release
// Exit code 0 means everything passed; 1 means something did not, and CI fails the build.
//
// Everything under test here is in DriverGeek.Core, which targets plain net8.0 and touches no
// Windows API. That is deliberate: it means the whole policy layer - what counts as an update,
// what is boot-critical, what the install gate refuses - can be built and proven on any machine,
// including CI, rather than only on a developer's Windows box.

VersionTests.Run();
ClassTests.Run();
CriteriaTests.Run();
StalenessTests.Run();
GateTests.Run();
ScheduleTests.Run();
ByteSizeTests.Run();

return Check.Report();
