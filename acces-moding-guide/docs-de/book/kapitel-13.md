# Kapitel 13: Der letzte Feinschliff

Ein Mod ist "fertig", wenn er robust und leistungsstark ist und Fehler korrekt verarbeitet. Dieses Kapitel behandelt die letzten Schritte beim Polieren Ihres Accessibility-Mods.

## 1. Leistungsoptimierung (Performance)
Ihr Mod sollte das Spiel nicht zum Ruckeln bringen.
*   **Vermeiden Sie Garbage Collection:** Vermeiden Sie in `Update()`-Schleifen das Erstellen neuer Strings oder Listen. Verwenden Sie stattdessen vorhandene wieder.
*   **Drosseln Sie aufwendige Prüfungen:** Wenn Sie die Szene nach interaktiven Objekten durchsuchen, tun Sie dies nicht 60 Mal pro Sekunde. Einmal alle 0,5 Sekunden ist für die Barrierefreiheit völlig ausreichend.
*   **Caching ist der Schlüssel:** Cachen Sie Verweise auf `GameObjects` und `Components` in Ihren `OnEnable`- oder `Awake`-Methoden.

## 2. "Lautlose Verschlechterung" (Silent Degradation) vermeiden
Das schlimmste Ergebnis für einen blinden Spieler ist, wenn der Mod lautlos aufhört zu funktionieren.
*   **Sichtbar (hörbar) scheitern:** Wenn Ihr Mod ein kritisches UI-Element nicht findet, kündigen Sie es an!
*   **Alles protokollieren:** Verwenden Sie Ihren `DebugLogger`, um unerwartete Nullwerte oder Reflection-Fehler aufzuzeichnen.

## 3. Robuste Fehlerbehandlung
Umschließen Sie Ihre Reflection- und Spiel-API-Aufrufe bei Bedarf mit `try-catch`-Blöcken.
```csharp
try {
    var gold = ReflectionHelper.GetField<int>(player, "gold");
} catch (Exception e) {
    DebugLogger.Log($"Kritischer Fehler: Gold konnte nicht gelesen werden. {e.Message}");
    ScreenReader.Say("Fehler beim Lesen des Goldes.");
}
```

## 4. Die Mod-Konfigurationsdatei
Ermöglichen Sie es den Spielern, ihr Erlebnis anzupassen. Verwenden Sie die Vorlage `ModConfig.cs`, um Einstellungen bereitzustellen für:
*   **Ausführlichkeit (Verbosity):** (Niedrig/Normal/Hoch), um zu steuern, wie viele Informationen angesagt werden.
*   **Lautstärke:** Für alle benutzerdefinierten Sounds oder Pieptöne.
*   **Tasten:** Erlauben Sie den Spielern, mod-spezifische Tasten (wie F1-F4) neu zu belegen.

### Profi-Tipp: Der Abschlusstest
Testen Sie Ihren Mod bei ausgeschaltetem Bildschirm. Dies wird alle verborgenen Abhängigkeiten von visuellem Feedback aufdecken und sicherstellen, dass Ihr Mod ein wirklich unabhängiges Erlebnis bietet.

---
*Weiter: [Kapitel 14: Vertrieb & Community](kapitel-14.md)*
