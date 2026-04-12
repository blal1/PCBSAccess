# Kapitel 11: Der polyglotte Mod

Ein echter Accessibility-Mod sollte von Menschen auf der ganzen Welt genutzt werden können. In diesem Kapitel bauen wir das `Loc`-Utility auf, das die Unterstützung mehrerer Sprachen vom ersten Tag an ermöglicht.

## 1. Die Lokalisierungs-Architektur
Hardcoden Sie niemals Zeichenfolgen (Strings) in Ihrem Mod. Verwenden Sie stattdessen "Keys" (Schlüssel), die auf ein Wörterbuch mit Übersetzungen verweisen.
`ScreenReader.Say("inventory_opened")`

## 2. Verwendung des Loc-Utilitys
Die Vorlage `Loc.cs` bietet eine einfache Möglichkeit, diese Zeichenfolgen zu verwalten.

### Übersetzungen hinzufügen
```csharp
private static void InitializeStrings() {
    Add("inv_opened", 
        en: "Inventory Opened", 
        de: "Inventar geöffnet");
    
    // Mit Platzhaltern
    Add("item_count", 
        en: "{0} items", 
        de: "{0} Gegenstände");
}
```

### Übersetzungen abrufen
```csharp
ScreenReader.Say(Loc.Get("item_count", 5)); // "5 Gegenstände"
```

## 3. Automatische Spracherkennung
Ihr Mod sollte automatisch die Spielsprache des Spielers erkennen.
1.  Identifizieren Sie den `LocalizationManager` des Spiels.
2.  Lesen Sie dessen aktuelle Spracheinstellung aus.
3.  Setzen Sie `Loc.CurrentLanguage` entsprechend.

## 4. Die Fallback-Kette
Wenn eine Übersetzung für die aktuelle Sprache fehlt, sollte das `Loc`-Utility automatisch auf Englisch zurückgreifen. Wenn Englisch ebenfalls fehlt, sollte es den Schlüssel selbst zurückgeben (z. B. `"!!MISSING:inv_opened!!" `), damit Sie dies beim Testen leicht erkennen können.

### Profi-Tipp: Platzhalter für die Grammatik
Verschiedene Sprachen verwenden unterschiedliche Wortstellungen. Bauen Sie niemals einen Satz zusammen, indem Sie Strings addieren wie `Loc.Get("you_have") + count + Loc.Get("items")`. Dies wird in vielen Sprachen falsch sein.

**Verwenden Sie immer Platzhalter:** `Loc.Get("item_count", count)`. Dies ermöglicht es Übersetzern, die Position des `{0}` so zu verschieben, dass sie der Grammatik ihrer Sprache entspricht.

---
*Weiter: [Kapitel 12: Native Lokalisierung einhaken](kapitel-12.md)*
