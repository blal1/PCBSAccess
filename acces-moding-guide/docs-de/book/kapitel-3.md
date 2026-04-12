# Kapitel 3: Das Nervensystem entwerfen

Im Accessibility-Modding ist das "Nervensystem" der Pfad von einem Ereignis zum Screenreader. Dieses Kapitel konzentriert sich darauf, wie Sie Ihren Mod mithilfe des `ScreenReader`-Wrappers effektiv "sprechen" lassen.

## 1. Das ScreenReader-Utility
Ihr Mod sollte `Tolk` niemals direkt aufrufen. Stattdessen verwenden wir einen `ScreenReader.cs`-Wrapper. Dies stellt sicher, dass:
1.  Fehler ordnungsgemäß behandelt werden.
2.  Ansagen leicht stummgeschaltet werden können.
3.  Der Mod nur versucht zu sprechen, wenn tatsächlich un Screenreader läuft.

### Wichtige Methoden
*   **`ScreenReader.Initialize()`**: Verbindet sich beim Start mit Tolk.
*   **`ScreenReader.Say(text, interrupt)`**: Die Hauptmethode für die Sprachausgabe.
    *   `interrupt = true`: Stoppt die vorherige Ansage sofort. Verwenden Sie dies für kritische Aktualisierungen (z. B. "Wenig Leben").
    *   `interrupt = false`: Reiht die Ansage in die Warteschlange ein, damit sie nach Abschluss der aktuellen Ansage vorgelesen wird. Verwenden Sie dies für Hinweise (z. B. "F1 drücken für Hilfe").
*   **`ScreenReader.Stop()`**: Schaltet den Screenreader sofort stumm. Verwenden Sie dies, wenn ein Menü geschlossen wird.

## 2. Best Practices für die Sprachausgabe
Screenreader-Nutzer verarbeiten Informationen linear. Wenn Ihr Mod zu wortreich ist, wird er zur Belastung.

*   **Fassen Sie sich kurz:** "3 von 10: Heiltrank" ist besser als "Du wählst gerade den dritten Gegenstand von zehn in deinem Inventar aus, nämlich einen Heiltrank."
*   **Verwenden Sie das Positionsmuster:** Geben Sie für Listen immer den Index und die Gesamtzahl an (z. B. "Gegenstand 5 von 20").
*   **Schweigen ist Gold:** Kündigen Sie keine Informationen an, die sich nicht geändert haben.

### Die Warteschlangen-Philosophie
Wenn Sie mehrere Informationen mitteilen möchten, verwenden Sie `SayQueued`:

```csharp
// Unterbrechen Sie die aktuelle Ansage mit der Hauptinformation
ScreenReader.Say("Inventar geöffnet");

// Reihen Sie die Zusatzinformation ein, damit sie nach Abschluss von "Inventar geöffnet" abgespielt wird
ScreenReader.Say("Insgesamt 15 Gegenstände. Pfeiltasten zum Navigieren.", false);
```

## 3. "Die Flut" vermeiden
Ein häufiger Fehler besteht darin, zu viele Ereignisse gleichzeitig anzukündigen. Wenn drei Feinde gleichzeitig sterben, kündigen Sie nicht jeden einzelnen an. Fassen Sie stattdessen zusammen: "3 Feinde besiegt."

### Checkpoint: Funktioniert es?
Testen Sie Ihre `ScreenReader.cs`, indem Sie eine Zeile in Ihre `Main.OnInitialize()` einfügen:
```csharp
ScreenReader.Say("Accessibility-Mod geladen und bereit.");
```
Wenn Sie dies hören, ist Ihre Brücke stabil!

---
*Weiter: [Kapitel 4: Das Gehirn (Statusverwaltung)](kapitel-4.md)*
