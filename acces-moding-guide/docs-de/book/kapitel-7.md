# Kapitel 7: Spiellogik identifizieren

Sobald Sie die UI-Klassen identifiziert haben, müssen Sie die "Quelle der Wahrheit" finden – die tatsächliche Logik, die den Spielzustand verfolgt. Ein `HealthBar`-UI-Element ist nur eine visuelle Darstellung; der tatsächliche Gesundheitswert ist in einer Klasse wie `PlayerStatus` oder `Stats` gespeichert.

## 1. Die "Quelle der Wahrheit" finden
Suchen Sie nach dem Code, der die UI *aktualisiert*.

1.  Finden Sie die `Text`-Komponente für die Gesundheitsanzeige im Code.
2.  Schauen Sie nach, welche Variable diesem Text zugewiesen wird.
3.  Verfolgen Sie diese Variable zurück zu ihrem Ursprung.

**Beispiel:**
Wenn `txt_Health.text = player.CurrentHP.ToString()` im UI-Code steht, dann ist `player.CurrentHP` Ihre Quelle der Wahrheit.

## 2. Hookbare Methoden (Die Harmony-Ziele)
Harmony funktioniert am besten, wenn Sie sich in Methoden einklinken, die eine klare Zustandsänderung darstellen.

*   **Vermeiden Sie Polling, wenn möglich:** Anstatt jeden Frame `if (menu.isOpen)` zu prüfen, finden Sie die Methode `menu.Show()` und legen Sie einen **Postfix**-Patch darauf.
*   **Aktionsmethoden:** Suchen Sie nach Methoden wie `PickupItem()`, `TakeDamage()`, `LevelUp()`, `OnDialogueStart()`. Dies sind perfekte Auslöser für Ansagen.

## 3. Den Spielfluss verstehen
Identifizieren Sie die "Manager"-Klassen. Die meisten Unity-Spiele haben:
*   `GameManager`: Der übergeordnete Zustand (Im Spiel, Pausiert, Game Over).
*   `UIManager`: Verwalter für das Öffnen und Schließen aller Panels.
*   `InputManager`: Zentralisiert die Tastatur-/Maushandhabung.

Wenn Sie den `UIManager` finden, können Sie sich oft in seine zentrale `OpenPanel(string name)`-Methode einklinken, um das Öffnen *jedes* Menüs im gesamten Spiel mit einem einzigen Patch zu erkennen.

## 4. Die "Reflection"-Abkürzung
Wenn Sie die benötigten Daten finden, diese aber als `private` markiert sind, versuchen Sie nicht, den Code des Spiels zu ändern. Verwenden Sie Ihren `ReflectionHelper`.
```csharp
// Wir haben die Player-Klasse gefunden, aber currentGold ist privat.
// Nutzen wir unseren Helper, um es sicher zu lesen.
int gold = ReflectionHelper.GetField<int>(playerInstance, "currentGold");
```

### Zusammenfassung: Der Modder-Workflow
1.  **Erkennen** des Ereignisses (via Harmony Patch).
2.  **Sammeln** der Daten (via Reflection oder öffentlicher API).
3.  **Übersetzen** in Text (via Lokalisierung).
4.  **Ansagen** für den Spieler (via ScreenReader).

---
*Weiter: [Kapitel 8: Menüs meistern](kapitel-8.md)*
