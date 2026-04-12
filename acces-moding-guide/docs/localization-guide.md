# Localization Guide for Accessibility Mods

This guide describes how to implement multi-language localization for accessibility mods. Using a central utility ensures that all announcements can be easily translated and adapted to the user's preferred language.

---

## Core Principles

### 1. Automatic Language Detection
The mod should automatically detect the game language and adapt. No manual switching needed from the user's side if we can hook into the game's settings.

### 2. Fallback Chain
When a translation is missing:
1. Try the detected current language.
2. If not available, fallback to **English**.
3. If English is also missing, return the **Key** itself (useful for debugging).

### 3. Central Localization Class
Keep all translations in one place (`Loc.cs`). This avoids searching through different handler files when updating texts.

### 4. Simple API
Only two methods are in daily use:
- `Loc.Get("key")` - Retrieve a simple string.
- `Loc.Get("key", param1, param2)` - Retrieve a string with placeholders.

---

## Architecture

### File Structure
- `Loc.cs` - Central localization class.
- `Handler` classes - Use `Loc.Get()` for all announcements.
- `Main.cs` - Initializes the localization at startup.

---

## Implementation (Loc.cs)

The `Loc` utility supports both hardcoded dictionary-based translations (best for mod stability) and JSON-based translations (best for community contributions).

### Hardcoded Dictionary Approach
This is the recommended approach for core mod strings.

```csharp
private static void InitializeStrings()
{
    // General
    Add("mod_loaded",
        en: "[ModName] loaded. F1 for help.",
        de: "[ModName] geladen. F1 für Hilfe.");

    // With placeholders: {0}, {1}, etc.
    Add("item_count",
        en: "{0} items",
        de: "{0} Gegenstände");
}
```

### JSON-based Translations
You can also load translations from an external JSON string, for example from a file in `UserData`.

**JSON Format:**
```json
{
  "translations": [
    { "key": "mod_loaded", "value": "Mod is ready!" },
    { "key": "help_title", "value": "Available Commands:" }
  ]
}
```

**Loading in Code:**
```csharp
string json = File.ReadAllText("path/to/lang_en.json");
Loc.LoadFromJson("en", json);
```

---

## Step-by-Step Integration

### Step 1: Hook into Game Language
Every Unity game handles language differently. You need to find the "Source of Truth" for the current language.

**Common Patterns:**
- `Language.getAlias()` -> returns "en", "de", etc.
- `PlayerPrefs.GetString("language")`
- `Application.systemLanguage` (Unity's built-in detection)

Adapt the `GetGameLanguage()` method in `Loc.cs` to use your game's specific system.

### Step 2: Initialize at Startup
In your mod's `Main.OnInitializeMelon()` or equivalent:

```csharp
public override void OnInitializeMelon()
{
    Loc.Initialize();
    // ...
}
```

### Step 3: Localize Announcements
Replace hardcoded strings in your handlers with `Loc.Get()`.

**Before:**
```csharp
ScreenReader.Say("Inventory opened. 5 items.");
```

**After:**
```csharp
ScreenReader.Say(Loc.Get("inventory_opened", itemCount));
```

---

## Best Practices

### 1. Naming Conventions
Use clear prefixes for keys to keep them organized:
- `inv_opened`, `inv_closed`
- `shop_buy_success`, `shop_no_money`
- `settings_verbosity_low`

### 2. Never Build Sentences Manually
Grammar varies wildly between languages. Never do this:
`string s = Loc.Get("you_have") + count + Loc.Get("items");`

**Always use placeholders:**
`string s = Loc.Get("item_count", count);`
The translation for `item_count` would be `"{0} items"` in English and `"{0} Gegenstände"` in German.

### 3. Short and Concise
Screen readers take time to read. Keep announcements short. "Inventory, 5 items" is better than "You have successfully opened your inventory and it contains 5 items."

---

## Checklist

- [ ] `GetGameLanguage()` adapted to the specific game.
- [ ] `Loc.Initialize()` called in `Main`.
- [ ] Fallback language (English) is fully populated.
- [ ] No hardcoded strings left in handler classes.
- [ ] Tested by switching the game language (if possible).
