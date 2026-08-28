@echo off
setlocal
echo Building DriverGeek...
dotnet build DriverGeek.sln -c Release || exit /b 1
echo.
echo Running checks...
dotnet run --project tests\DriverGeek.Tests -c Release --no-build || exit /b 1
echo.
echo Publishing the portable build...
dotnet publish src\DriverGeek\DriverGeek.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\portable || exit /b 1
echo.
echo Done. publish\portable\DriverGeek.exe
