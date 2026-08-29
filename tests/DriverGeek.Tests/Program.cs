using DriverGeek.Tests;

// Run with: dotnet run --project tests/DriverGeek.Tests -c Release
// Exit code 0 means everything passed; 1 fails the CI build.
//
// Everything under test is in DriverGeek.Core, which targets plain net8.0 and touches no Windows
// API, so the policy layer can be built and run on any machine.

VersionTests.Run();
ClassTests.Run();
CriteriaTests.Run();
StalenessTests.Run();
GateTests.Run();
InstallFlowTests.Run();
ScheduleTests.Run();
ByteSizeTests.Run();

return Check.Report();
