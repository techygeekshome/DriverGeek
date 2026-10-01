; DriverGeek installer - languages and custom messages
;
; Kept in its own file so a translation can be added or corrected without touching the
; installer script itself, which is the arrangement PDFGeek uses. Adding a language means
; two things and nothing else: a Name line under [Languages], and a block of messages
; under [CustomMessages] using that same name as the prefix.
;
; The Italian translation is by bovirus (github.com/bovirus).

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[CustomMessages]
english.CreateDesktopShortcut=Create a &desktop shortcut
english.Shortcuts=Shortcuts:
english.WebSite={#AppName} on the web
english.LaunchProgram=Run {#AppName}

italian.CreateDesktopShortcut=Crea collegamento programma sul &desktop
italian.Shortcuts=Collegamenti:
italian.WebSite=Sito web {#AppName}
italian.CreateQuickLaunchIcon=Crea collegamento programma nella &barra 'Avvio veloce'
italian.NameAndVersion={#AppName} {#AppVersion}
italian.LaunchProgram=Esegui {#AppName}
italian.AdditionalIcons=Collegamenti:
