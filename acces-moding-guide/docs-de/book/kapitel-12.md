# Kapitel 12: Native Lokalisierung einhaken

Während es in `Kapitel 11` um die Strings Ihres *eigenen* Mods ging, muss Ihr Mod auch die Strings des *Spiels* lesen (wie Gegenstandsnamen, Questbeschreibungen und Dialoge).

## 1. Die Sprachdatenbank des Spiels
Die meisten Unity-Spiele verfügen über einen `LocalizationManager` oder eine `LanguageDatabase`, in der alle Texte des Spiels gespeichert sind. Ihr Ziel ist es, herauszufinden, wo diese Daten gespeichert sind.

### Häufige Speicherorte
*   **Ein Dictionary oder Map:** Suchen Sie nach `Dictionary<string, string>` in den Lokalisierungsklassen.
*   **Eine Tabelle oder CSV:** Viele Spiele laden ihre Texte aus einem `Resources`-Ordner oder einer externen Datei.
*   **Eine Übersetzungsmethode:** Suchen Sie nach einer Methode wie `GetLocalizedString(string key)` oder `Translate(string key)`.

## 2. Übersetzte Spieltexte lesen
Sobald Sie die Übersetzungsmethode des Spiels gefunden haben, können Sie sie verwenden, um den Namen jedes Objekts in der vom Spieler gewählten Sprache abzurufen.

```csharp
// Beispiel: Übersetzen eines Gegenstandsnamens
string localizedName = GameLocalization.Get(item.ID);
ScreenReader.Say(localizedName);
```

## 3. Umgang mit benutzerdefinierter Formatierung
Einige Spiele verwenden benutzerdefinierte "Tags" in ihren Texten, wie `<color=red>Feuerschwert</color>`. Bevor Sie diesen Text an den Screenreader senden, müssen Sie diese Tags entfernen.
```csharp
string cleanText = Regex.Replace(gameText, "<.*?>", string.Empty);
```

## 4. Mod und Spiel synchronisieren
Stellen Sie immer sicher, dass Ihr `Loc.CurrentLanguage` mit dem Spiel synchronisiert ist. Wenn der Spieler die Sprache des Spiels im Einstellungsmenü ändert, sollte Ihr Mod dies erkennen und seine eigene Sprache sofort ebenfalls ändern.

### Warum das lebenswichtig ist
Wenn ein Spieler ein deutsches Spiel mit einem englischen Accessibility-Mod spielt, wird der Sprachmix verwirrend sein. Die Synchronisation beider stellt ein nahtloses, professionelles Erlebnis sicher.

---
*Weiter: [Kapitel 13: Der letzte Feinschliff](kapitel-13.md)*
