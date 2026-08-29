<div align="center">

<img src="icons/drivergeek.png" alt="DriverGeek logo" width="96" height="96">

# DriverGeek

**See every driver on your PC — and the updates Windows Update keeps hidden.**

[![Build](https://github.com/techygeekshome/DriverGeek/actions/workflows/build.yml/badge.svg)](https://github.com/techygeekshome/DriverGeek/actions/workflows/build.yml)
[![Version](https://img.shields.io/github/v/release/techygeekshome/DriverGeek?label=version&color=4c9bff)](https://github.com/techygeekshome/DriverGeek/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078d4)](#getting-it-running)
[![License](https://img.shields.io/badge/License-GPL--3.0-blue)](LICENSE)
[![Made by TechyGeeksHome](https://img.shields.io/badge/made%20by-TechyGeeksHome-b191f2)](https://techygeekshome.info)
[![Support on Ko-fi](https://img.shields.io/badge/support-Ko--fi-ff5e5b)](https://ko-fi.com/techygeekshome)

[Download](#download--run) · [What it does](#what-it-does) · [Screenshots](#screenshots) · [What it refuses to do](#what-it-refuses-to-do) · [Build from source](#build-from-source) · [Licence](#licence)

</div>

---

Windows already knows about driver updates you have never been offered. They sit under
Settings → Windows Update → Advanced options → **Optional updates**, four clicks deep, and
almost nobody looks there. DriverGeek lists every device on the machine with the driver it is
actually running — version, date, provider, whether it is signed — and puts the updates Windows
is already holding for you on the same screen.

For everything else, it tells you what you have and links to the manufacturer's own support
page. It does not host drivers, it does not scrape vendor sites, and it does not ship a driver
pack.

## What it refuses to do

This category has a reputation, and it was earned. So, plainly:

- **No driver pack, no third-party driver hosting.** Updates come from Windows Update, which is
  WHQL-signed and already trusted by your machine. Anything else is a link to the vendor.
- **No "247 issues found!"** An old driver that works is not a problem. DriverGeek distinguishes
  *there is a newer driver available from Windows Update* from *this driver is old*, and it will
  tell you when the answer is that nothing needs doing.
- **No scan-then-pay.** There is no paid tier, no upsell and nothing withheld.
- **No telemetry, no account, no bundled offers.**
- **It never installs anything on its own.** No automatic mode, no scheduled installs.

## What it does

- 🔎 **Full driver inventory** — every device, its current driver version and date, the provider,
  and whether the driver is signed.
- ⬇️ **Surfaces optional driver updates** — the ones Windows Update has but does not offer you.
- 🏷️ **Honest staleness** — flags drivers with a newer version available, not merely old ones.
- 🔗 **Vendor links** — for hardware Windows Update does not cover, a direct link to the
  manufacturer's support page for that device.
- 📖 **1.0 reads and reports. It does not install.** Everything above is a read of your machine
  and a question put to Windows Update. Nothing is downloaded and nothing is changed.
- ℹ️ **About and Check for updates** — the same two buttons every TechyGeeksHome tool has, at the
  foot of the sidebar. The update check runs only when you press it.
- 🚩 **Marks what an install would refuse** — boot-critical and storage controller drivers are
  labelled on the list, because when the install path lands they will never be replaced.

### Where 1.0 stops

Shipping the reading half first is deliberate: it is most of the value with none of the risk.
When installing arrives it exports the current driver to disk first, refuses to run at all if
System Protection is off, takes a restore point, and does one device at a time — ticked by you.
There is no "update all", and there never will be one on a schedule.

## Screenshots

<div align="center">

**Drivers** — every device, its driver version and date, and whether it is signed.

<img src="docs/screenshots/drivers.png" alt="The Drivers screen, listing every device with its driver version, date and status" width="820">

**Updates** — what Windows Update is holding, including the ones it hides under Optional.

<img src="docs/screenshots/updates.png" alt="The Updates screen" width="820">

**Settings** — scanning options, and a plain statement of what 1.0 will not do.

<img src="docs/screenshots/settings.png" alt="The Settings screen" width="820">

</div>

## Download & run

**[⬇ Download DriverGeek 1.0](https://github.com/techygeekshome/DriverGeek/releases/latest)** — Windows 10 or 11, 64-bit.

| File | What it is | Size |
| --- | --- | --- |
| `DriverGeekSetup.exe` | Installer. Start-menu entry, uninstalls cleanly. | 29.2 MB |
| `DriverGeek-portable.exe` | One file. Run it from anywhere, install nothing. | 86.9 MB |
| `SHA256SUMS.txt` | Checksums for both, published with every release. | — |

Nothing else needs installing — .NET is inside the executable.

To verify what you downloaded, in PowerShell:

```powershell
Get-FileHash .\DriverGeekSetup.exe -Algorithm SHA256
```

and compare it against the line in `SHA256SUMS.txt`.

> **First run:** Windows may show a blue *"Windows protected your PC"* box. That is SmartScreen
> reacting to an executable it has not seen before, not a detection — DriverGeek is not code-signed,
> because a certificate costs more per year than this whole range earns. Click **More info** →
> **Run anyway**. The published SHA-256 is there so you never have to take that on trust.

## On the one network call

DriverGeek makes exactly one network request, and only when you press **Check for updates**: a
single unauthenticated GET to GitHub's public releases API, asking whether there is a newer tag
than the build you are running. It sends no machine identifier, no record of what you scanned
and no usage data — GitHub sees an IP address and a user agent, exactly as it would if you
opened the releases page in a browser. It never downloads or installs anything; if there is a
newer version it offers you the release page, and that is all.

Nothing is requested when the app starts. Open it, use it, close it, and it makes no network
connection at all.

## Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet build src/DriverGeek/DriverGeek.csproj -c Release
```

```powershell
dotnet publish src/DriverGeek/DriverGeek.csproj -c Release -r win-x64 --self-contained true
```

## Support & contributing

Found a bug or have a request? [Open an issue](https://github.com/techygeekshome/DriverGeek/issues)
or [get in touch](https://techygeekshome.info/contact/). Contributions are welcome — see
[CONTRIBUTING.md](CONTRIBUTING.md).

## ☕ Support

If DriverGeek saves you an afternoon, [buy us a coffee](https://ko-fi.com/techygeekshome). It is
never required and nothing is withheld without it.

## Licence

DriverGeek is free software under the **GNU General Public License v3.0** — see [LICENSE](LICENSE)
and [gnu.org](https://www.gnu.org/licenses/gpl-3.0.en.html). Anyone may use, modify and share it;
a distributed modification must publish its source under the same licence.

Free for everyone, including commercial use.

© 2026 TechyGeeksHome | Andrew Armstrong.

---

<div align="center">

Made with ❤️ by [**TechyGeeksHome**](https://techygeekshome.info)

[Website](https://techygeekshome.info) · [YouTube](https://www.youtube.com/channel/UCtEuFj1SMLiuRoucD1hv8dA) · [X](https://x.com/TechyGeeks1) · [Facebook](https://www.facebook.com/techygeeks.home) · [Instagram](https://www.instagram.com/andrewarmstrongtgh)

</div>
