; DriverGeek installer
;
; Modelled on installer\PDFGeek.iss, which is the cleanest of the three existing scripts.
; Two things about this file are decisions rather than defaults, and both are explained where
; they appear: PrivilegesRequired, and the AppId.
;
; Build it locally with:  build.cmd installer
; CI builds it in .github\workflows\release.yml.

#define AppName        "DriverGeek"
#define AppSourceDir   "..\publish\app"
#define AppExeName     "DriverGeek.exe"
#define AppPublisher   "TechyGeeksHome"
#define AppURL         "https://techygeekshome.info/drivergeek/"
#define AppSupportURL  "https://github.com/techygeekshome/DriverGeek/issues"
#define AppUpdatesURL  "https://github.com/techygeekshome/DriverGeek/releases"
#define FirstYear      "2026"
#define CurrentYear    GetDateTimeString('yyyy', '', '')

; From 2027 onward show a range rather than only the current year, so the copyright reads
; 2026-2027 and so on, and never the odd looking 2026-2026.
#if CurrentYear == FirstYear
  #define CopyrightYears FirstYear
#else
  #define CopyrightYears FirstYear + "-" + CurrentYear
#endif

; Read straight off the executable that is about to be packaged, so the installer can never
; claim a different version from the thing inside it.
#define AppVersion GetVersionNumbersString(AppSourceDir + "\" + AppExeName)

#include "DriverGeek_languages.iss"

[Setup]
; NEVER regenerate this. Windows uses the AppId to tell an upgrade from a second parallel
; install; a new one means the next version installs alongside this one instead of over it.
AppId={{E1F5AC9E-3E9F-478D-BD69-40D637942B89}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppSupportURL}
AppUpdatesURL={#AppUpdatesURL}
AppCopyright=Copyright (C) {#CopyrightYears} {#AppPublisher}

VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} Setup

WizardStyle=modern
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName} {#AppVersion}
LicenseFile=..\LICENSE
SetupIconFile=..\icons\drivergeek.ico

OutputDir=..\dist
OutputBaseFilename={#AppName}Setup

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=auto
AllowNoIcons=yes

; The app's own manifest is asInvoker - it does not request administrator rights and does not
; need them for what it does. Installing it somewhere only an administrator can write would be
; pretending otherwise, so this is a per-user install with no UAC prompt. Anyone who wants it
; machine-wide can pass /ALLUSERS.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline dialog

Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0



; With more than one language available the wizard has to ask, and it has to ask every time
; rather than silently reusing whatever was picked last time on a shared machine. Detection
; starts from the Windows UI language, so most people never think about it.
ShowLanguageDialog=yes
UsePreviousLanguage=no
LanguageDetectionMethod=uilanguage

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopShortcut}"; GroupDescription: "{cm:Shortcuts}"

[Files]
Source: "{#AppSourceDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE";   DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; DestName: "README.md";   Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}";                        Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:WebSite}";                     Filename: "{#AppURL}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";                  Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; The application writes its settings and log under LocalAppData. They are left behind on
; purpose - an uninstall is not a request to throw away somebody's preferences - but the folder
; goes if it is empty.
Type: dirifempty; Name: "{localappdata}\{#AppName}"
