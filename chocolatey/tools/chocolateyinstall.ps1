$ErrorActionPreference = 'Stop'

# DriverGeek ships an Inno Setup installer. The package downloads it from the GitHub release for the
# matching tag and verifies it against a SHA-256 checksum rather than embedding the binary. Because
# nothing is embedded, this package must NOT contain a tools\VERIFICATION.txt - that file is only
# for packages that ship a binary inside the nupkg, and including one is what the USP 8.0.0
# submission was rejected for.
$packageArgs = @{
  packageName    = 'drivergeek'
  fileType       = 'exe'
  url            = 'https://github.com/techygeekshome/DriverGeek/releases/download/v1.1.2/DriverGeekSetup.exe'
  checksum       = 'b2afc746124b57bf954603b4ca76462defc254fed40a8461f9f12e1b040f0960'
  checksumType   = 'sha256'
  silentArgs     = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-'
  validExitCodes = @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
