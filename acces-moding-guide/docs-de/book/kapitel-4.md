# Kapitel 4: Das Gehirn (Statusverwaltung)

Das "Gehirn" Ihres Accessibility-Mods ist das System, das genau verfolgt, wo sich der Spieler im Spiel befindet. Dies ist entscheidend, da dieselben Tasten (wie die Pfeiltasten) in einem Menü andere Funktionen haben können als in der Spielwelt.

## 1. Kontextsensitiver Input
Ohne Statusverwaltung könnte Ihr Mod versuchen, in einem Inventar zu navigieren, das gar nicht geöffnet ist. Wir lösen dies mithilfe des `AccessStateManager`.

### Das Kernkonzept
Der `AccessStateManager` verwaltet einen "Stack" (Stapel) von aktiven Kontexten.
1.  Wenn Sie ein Menü öffnen, **pushen** (schieben) Sie seinen Namen auf den Stapel.
2.  Wenn Sie es schließen, **poppen** (holen) Sie ihn wieder herunter.

## 2. Verwendung des AccessStateManager
Unser Framework bietet einen zentralen Manager, den Ihre Handler-Klassen verwenden können, um Interesse an Eingaben zu registrieren.

### Identifizieren des Kontexts
```csharp
// In Ihrer Update()-Schleife:
if (AccessStateManager.CurrentContext == "Inventory") {
    // Verarbeiten Sie Pfeiltasten nur, wenn das Inventar aktiv ist
    HandleInventoryNavigation();
}
```

## 3. Der "Escape"-Stack
Ein wichtiges Feature für Screenreader-Nutzer ist die "Zurück"- oder "Escape"-Funktionalität. Wenn mehrere Menüs geöffnet sind (z. B. ein Bestätigungs-Popup über einem Inventar), stellt der `AccessStateManager` sicher, dass durch Drücken von `Escape` zuerst das oberste Menü geschlossen wird.

### Implementieren des Stacks
Unsere `AccessStateManager.cs.template` enthält einen `EscapeStack`. Wenn ein Handler eine UI öffnet, sollte er sich selbst registrieren:
```csharp
AccessStateManager.PushContext("SubMenu", () => CloseSubMenu());
```
Wenn `Escape` gedrückt wird, führt der Manager automatisch die zuletzt registrierte "Schließen"-Aktion aus.

## 4. Ereignisgesteuerte Statusänderungen
Anstatt jeden Frame zu prüfen, ob ein Menü geöffnet ist, besteht die Best Practice darin, Ereignisse (Events) auszulösen. Der `AccessStateManager` kann ein `OnContextChanged`-Ereignis auslösen, auf das alle Ihre Handler hören. Dies ermöglicht es ihnen:
*   Ihren Auswahlindex zurückzusetzen.
*   Alle vorherigen Ansagen stummzuschalten.
*   Die erste Ansage für den neuen Kontext vorzubereiten.

### Zusammenfassung
Das "Gehirn" verhindert, dass Ihr Mod sich selbst unterbricht oder die falschen Aktionen ausführt. Durch die Verwendung eines zentralen Statusmanagers wird Ihr Mod zu einem zusammenhängenden System statt zu einer Sammlung konkurrierender Skripte.

---
*Weiter: [Kapitel 5: Die Sinne (Extraktion & Reflection)](kapitel-5.md)*
