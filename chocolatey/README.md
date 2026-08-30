# Chocolatey package for DriverGeek

The package source for `drivergeek` on the Chocolatey community repository lives here so that
`packageSourceUrl` in the nuspec points at something real.

## What the package does

It downloads `DriverGeekSetup.exe` from the GitHub release for the matching tag and verifies it against a
SHA-256 checksum, then runs it silently. **Nothing is embedded in the nupkg.**

Because nothing is embedded, this package must **not** contain `tools\VERIFICATION.txt`. That file
is only for packages that ship a binary inside the nupkg, and including one is exactly what the
Ultimate Settings Panel 8.0.0 submission was rejected for.

## Release checklist

1. Cut the GitHub release and let CI attach the artefacts.
2. Take the SHA-256 for `DriverGeekSetup.exe` from the release's own `SHA256SUMS.txt`.
3. Update in this folder: `<version>` in the nuspec, `url` and `checksum` in
   `tools/chocolateyinstall.ps1`, and `<releaseNotes>` to point at the new tag.
4. `choco pack`
5. Install locally from the built nupkg and check the shim works:
   `choco install drivergeek -s . -y`
6. `choco push drivergeek.<version>.nupkg --source https://push.chocolatey.org/`

Moderation is by hand and can take weeks. Do not repush the same version unless a moderator asks
for it.
