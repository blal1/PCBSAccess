# Lokalisierung für Accessibility-Mods - Anleitung

Diese Anleitung beschreibt, wie du eine Mehrsprachen-Lokalisierung für Accessibility-Mods implementierst. Ein zentrales System stellt sicher, dass alle Ansagen leicht übersetzt und an die bevorzugte Sprache des Nutzers angepasst werden können.

---

## Grundprinzipien

### 1. Automatische Spracherkennung
Der Mod sollte die Spielsprache automatisch erkennen und sich anpassen. Ein manuelles Umschalten durch den Nutzer sollte nicht nötig sein, wenn wir uns an die Spieleinstellungen hängen können.

### 2. Fallback-Kette
Wenn eine Übersetzung fehlt:
1. Versuche die erkannte aktuelle Sprache.
2. Falls nicht verfügbar, nutze **Englisch** als Fallback.
3. Falls auch Englisch fehlt, gib den **Key** selbst zurück (hilfreich beim Debugging).

### 3. Zentrale Lokalisierungsklasse
Alle Übersetzungen an einem Ort (`Loc.cs`). Das erspart das Suchen in verschiedenen Handler-Dateien bei Textänderungen.

### 4. Einfache API
Nur zwei Methoden im Alltag:
- `Loc.Get("key")` - Einfachen String abrufen.
- `Loc.Get("key", param1, param2)` - String mit Platzhaltern abrufen.

---

## Architektur

### Dateistruktur
- `Loc.cs` - Zentrale Lokalisierungsklasse.
- `Handler`-Klassen - Nutzen `Loc.Get()` für alle Ansagen.
- `Main.cs` - Initialisiert die Lokalisierung beim Start.

---

## Implementierung (Loc.cs)

Die `Loc`-Utility unterstützt sowohl fest im Code definierte (Dictionary-basiert) als auch JSON-basierte Übersetzungen.

### Dictionary-Ansatz (Empfohlen)
Dies ist der empfohlene Weg für die Kern-Strings des Mods, um Abhängigkeiten zu minimieren.

```csharp
private static void InitializeStrings()
{
    // Allgemein
    Add("mod_loaded",
        en: "[ModName] loaded. F1 for help.",
        de: "[ModName] geladen. F1 für Hilfe.");

    // Mit Platzhaltern: {0}, {1}, etc.
    Add("item_count",
        en: "{0} items",
        de: "{0} Gegenstände");
}
```

### JSON-basierte Übersetzungen
Du kannst Übersetzungen auch aus einem externen JSON-String laden, z. B. aus einer Datei im `UserData`-Ordner.

**JSON-Format:**
```json
{
  "translations": [
    { "key": "mod_loaded", "value": "Mod ist bereit!" },
    { "key": "help_title", "value": "Verfügbare Befehle:" }
  ]
}
```

**Laden im Code:**
```csharp
string json = File.ReadAllText("pfad/zu/lang_de.json");
Loc.LoadFromJson("de", json);
```

---

## Schritt für Schritt Integration

### Schritt 1: Spielsprache finden
Jedes Unity-Spiel speichert die Sprache anders. Du musst die "Quelle der Wahrheit" finden.

**Typische Muster:**
- `Language.getAlias()` -> gibt "en", "de", etc. zurück.
- `PlayerPrefs.GetString("language")`
- `Application.systemLanguage` (Eingebaute Unity-Erkennung)

Passe die Methode `GetGameLanguage()` in `Loc.cs` entsprechend an.

### Schritt 2: Initialisierung beim Start
In `Main.OnInitializeMelon()` oder äquivalent:

```csharp
public override void OnInitializeMelon()
{
    Loc.Initialize();
    // ...
}
```

### Schritt 3: Ansagen lokalisieren
Ersetze hartcodierte Strings in deinen Handlern durch `Loc.Get()`.

**Vorher:**
```csharp
ScreenReader.Say("Inventar geöffnet. 5 Gegenstände.");
```

**Nachher:**
```csharp
ScreenReader.Say(Loc.Get("inventory_opened", itemCount));
```

---

## Best Practices

### 1. Namenskonventionen
Nutze klare Präfixe für Keys:
- `inv_opened`, `inv_closed`
- `shop_buy_success`, `shop_no_money`
- `settings_verbosity_low`

### 2. Sätze niemals manuell zusammenbauen
Die Grammatik variiert stark zwischen Sprachen. Tu das niemals:
`string s = Loc.Get("you_have") + count + Loc.Get("items");`

**Nutze immer Platzhalter:**
`string s = Loc.Get("item_count", count);`
Die Übersetzung für `item_count` wäre `"{0} items"` auf Englisch und `"{0} Gegenstände"` auf Deutsch.

### 3. Kurz und prägnant
Screenreader brauchen Zeit zum Vorlesen. Halte Ansagen kurz. "Inventar, 5 Gegenstände" ist besser als "Du hast erfolgreich dein Inventar geöffnet und es enthält 5 Gegenstände."

---

## Checkliste

- [ ] `GetGameLanguage()` an das Spiel angepasst.
- [ ] `Loc.Initialize()` wird in `Main` aufgerufen.
- [ ] Fallback-Sprache (Englisch) ist vollständig befüllt.
- [ ] Keine hartcodierten Strings mehr in Handler-Klassen.
- [ ] Getestet durch Umstellen der Spielsprache.
