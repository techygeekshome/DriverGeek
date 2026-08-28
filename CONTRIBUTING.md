# Contributing to DriverGeek

Thanks for considering it. This is a small project and contributions are genuinely welcome.

## Licensing — inbound equals outbound

DriverGeek is licensed under the **GNU General Public License v3.0**. By opening a pull request you
agree that your contribution is offered under that same licence. There is no contributor licence
agreement to sign and no paperwork — the project licence covers it, which is one of the reasons
the TechyGeeksHome apps moved to an OSI-approved licence in the first place.

Copyright in your own commits stays yours.

## Before you start

- **Open an issue first for anything substantial.** A quick conversation saves you writing
  something that does not fit the design.
- Small fixes — a typo, a crash, a wrong string — just send the pull request.

## House rules this project keeps

These are not negotiable, because they are what the app is for:

- **No telemetry, no analytics, no phoning home.**
- **No account, no sign-in, no paid tier.**
- **Nothing installs or deletes itself.** Every destructive action is explicitly chosen by the
  user, every time.
- **No bundled third-party offers**, ever.
- Third-party dependencies are added reluctantly, and never for something the framework already
  does.

## Code

- .NET 8, and the solution must build clean with `-warnaserror`.
- Keep logic that can be tested out of the UI layer, and add checks to the test project for it.
- Match the surrounding style rather than reformatting files you are passing through.

## Pull requests

- One change per pull request.
- Say what it does and why in the description. If it changes behaviour a user would notice, say
  what they will see.
- A pull request that touches `LICENSE` will be reviewed separately from the rest of the diff.
