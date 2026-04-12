# Kapitel 8: Menüs meistern

Menüs sind die häufigste Barriere für die Barrierefreiheit. In diesem Kapitel legen wir die Muster für eine standardisierte Navigation fest, um sicherzustellen, dass ein blinder Spieler jeden Bildschirm mit Leichtigkeit bedienen kann.

## 1. Standardisierte Navigationsmuster
Konsistenz ist Ihr bester Freund. Jeder Bildschirm sollte denselben Regeln folgen:

*   **Pfeiltasten:** Verwenden Sie diese für die Navigation innerhalb einer Liste oder eines Rasters.
*   **Positionsansage:** Verwenden Sie immer das Muster "X von Y" (z. B. "3 von 12: Heiltrank").
*   **Zustandsänderungen:** Wenn das Auswählen eines Gegenstands etwas anderes auf dem Bildschirm ändert (z. B. eine Beschreibung aktualisiert), kündigen Sie zuerst die Hauptinformation an, dann die Änderung.

## 2. Raster-Koordinaten (Grid Coordinates)
Kündigen Sie bei Inventaren oder Karten, die ein Raster (Zeilen und Spalten) verwenden, sowohl die Koordinate als auch den Namen an:
`"Zeile 1, Spalte 4: Rostiges Schwert"`

## 3. Kontextuelle Tasten-Hinweise
Wenn ein neues Panel geöffnet wird, kündigen Sie sofort die wichtigsten Tasten an.
`"Inventar geöffnet. Pfeiltasten zum Navigieren, Leertaste zum Benutzen, F2 für Beschreibung."`
**Hinweis:** Verwenden Sie `ScreenReader.SayQueued` für diese Hinweise, um den Haupttitel nicht zu unterbrechen.

## 4. Die Accessibility-Checkliste
Bevor Sie ein Menü als "barrierefrei" deklarieren, prüfen Sie diese Punkte:
*   [ ] Kann ich **jede** Schaltfläche mit der Tastatur erreichen?
*   [ ] Kündigt der Mod an, wenn das Menü **geöffnet** und **geschlossen** wird?
*   [ ] Kündigt er die **aktuelle Auswahl** an, während ich mich bewege?
*   [ ] Werden die **Escape**- und **Enter**-Tasten korrekt verarbeitet (über den `AccessStateManager`)?
*   [ ] Kann ich jederzeit eine **Statuszusammenfassung** mit einer dedizierten Taste (wie F-Tasten) erhalten?

### Implementierungsmuster: Der Handler
Jedes Menü sollte eine dedizierte `Handler`-Klasse haben.
```csharp
public class InventoryHandler : IHandler {
    private int _selectedIndex = 0;
    
    public void Navigate(int direction) {
        _selectedIndex += direction;
        // ... Logik für den Umbruch ...
        AnnounceSelection();
    }
    
    private void AnnounceSelection() {
        var item = GetItem(_selectedIndex);
        ScreenReader.Say($"{_selectedIndex + 1} von {total}: {item.Name}");
    }
}
```

Indem Sie diese Muster befolgen, stellen Sie sicher, dass ein Spieler, sobald er gelernt hat, wie man in einem Menü Ihres Mods navigiert, gelernt hat, wie man sie alle navigiert.

---
*Weiter: [Kapitel 9: In der Welt leben](kapitel-9.md)*
