@echo off
setlocal
REM  build.cmd            - build, run the checks, publish the portable build
REM  build.cmd installer  - the above, then compile the Inno Setup installer
REM
REM  The installer target needs Inno Setup 6. CI installs it when the runner image does not
REM  already have it; on a workstation, get it from jrsoftware.org and let it use the default
REM  install location.

set APPNAME=DriverGeek
set TESTPROJ=tests\DriverGeek.Tests
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

echo Building %APPNAME%...
dotnet build %APPNAME%.sln -c Release || exit /b 1

echo.
echo Running checks...
dotnet run --project %TESTPROJ% -c Release --no-build || exit /b 1

echo.
echo Publishing the portable build...
dotnet publish src\%APPNAME%\%APPNAME%.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\app || exit /b 1

if /I not "%~1"=="installer" goto :done

echo.
echo Building the installer...
if not exist %ISCC% (
  echo Inno Setup 6 was not found at %ISCC%.
  echo Install it from jrsoftware.org, or run build.cmd with no argument to skip this step.
  exit /b 1
)
%ISCC% "installer\%APPNAME%.iss" || exit /b 1
echo.
echo Done. dist\%APPNAME%Setup.exe and publish\app\%APPNAME%.exe
goto :eof

:done
echo.
echo Done. publish\app\%APPNAME%.exe
echo Run "build.cmd installer" to build the installer as well.
