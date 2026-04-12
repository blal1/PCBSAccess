# Chapter 11: The Polyglot Mod

## Introduction: Accessibility Without Borders

Accessibility is a universal requirement. Blind gamers live all over the world, speaking dozens of different languages. If you build a sophisticated accessibility mod that flawlessly navigates menus, intercepts combat logic, and provides spatial audio, but hardcodes every single announcement in English, you are building an artificial barrier.

A Spanish-speaking player using a German screen reader trying to interpret English text from a mod injected into a French game is a recipe for cognitive overload and ultimate frustration.

The hallmark of a truly professional accessibility mod is its localization system. It must be a **Polyglot**—capable of speaking multiple languages fluently, and intelligent enough to switch between them automatically based on the user's environment.

This chapter details the architecture of the `Loc` utility, a centralized localization framework designed specifically for the high-speed, dynamic requirements of screen reader output.

---

## 1. The Core Philosophy of the Loc Utility

The cardinal rule of localization in programming is simple: **Never hardcode strings that the user will hear.**

If you have a line of code in your mod that looks like this:
```csharp
ScreenReader.Say("You opened the inventory and have " + itemCount + " items.");
```
You have failed the localization test.

Instead, you must use a "Key". A Key is a unique identifier that points to a dictionary of translations. The code should look like this:
```csharp
ScreenReader.Say(Loc.Get("inventory_opened_count", itemCount));
```

### The Grammar Problem: Why Concatenation is Evil
In the bad example above, we concatenated three things: a string, an integer, and a string. 
`"You have " + count + " items."`

In English, this works: "You have 5 items."
In other languages, the sentence structure is fundamentally different. The number might need to appear at the end of the sentence, or the surrounding words might change based on plurality. 

By using the `Loc.Get` system with placeholders (`{0}`, `{1}`), we give the translator total control over the grammar. 
*   English Dictionary: `"inventory_opened_count" : "You have {0} items."`
*   Yoda Dictionary: `"inventory_opened_count" : "Items {0}, you have."`

The translator can move the `{0}` placeholder anywhere in the sentence, preserving the grammatical integrity of their language.

---

## 2. Architecting the Loc.cs Template

Our `Loc.cs` template is designed to be highly performant (as it will be called dozens of times per second during menu navigation) and incredibly resilient to missing data.

### The Dictionary Structure
Internally, the `Loc` class manages a nested dictionary structure. The outer dictionary maps the language code (e.g., "en", "de", "fr") to an inner dictionary. The inner dictionary maps the specific translation key (e.g., "menu_options") to the actual localized string (e.g., "Options").

```csharp
public static class Loc
{
    // The master dictionary: Dictionary<LanguageCode, Dictionary<TranslationKey, TranslatedString>>
    private static Dictionary<string, Dictionary<string, string>> _translations = 
        new Dictionary<string, Dictionary<string, string>>();

    // The currently active language
    public static string CurrentLanguage { get; set; } = "en";

    // The fail-safe language
    private const string FALLBACK_LANGUAGE = "en";
}
```

### Populating the Dictionaries
There are two primary ways to load data into these dictionaries: Hardcoded Methods and JSON Files. A robust mod usually uses a hybrid approach.

**Method 1: Hardcoded Dictionaries (The Core)**
For the fundamental strings that your mod relies on (menu names, standard error messages), it is safest to hardcode them directly into the C# file. This ensures that even if the user deletes their configuration files, the mod can still speak.

```csharp
private static void AddHardcodedStrings()
{
    // A helper method to make adding strings visually clean
    Add("mod_initialized", 
        en: "Accessibility Mod Initialized. Press F1 for Help.", 
        de: "Accessibility Mod initialisiert. Drücke F1 für Hilfe.");
        
    Add("error_not_found", 
        en: "Element not found.", 
        de: "Element nicht gefunden.");
}

private static void Add(string key, string en, string de = null, string fr = null)
{
    // Automatically creates the language dictionaries if they don't exist
    if (!string.IsNullOrEmpty(en)) EnsureDict("en")[key] = en;
    if (!string.IsNullOrEmpty(de)) EnsureDict("de")[key] = de;
    if (!string.IsNullOrEmpty(fr)) EnsureDict("fr")[key] = fr;
}
```

**Method 2: External JSON (Community Contributions)**
Hardcoding is safe, but it requires you to recompile the mod every time someone submits a new translation. To support the community, your `Loc` utility should also look for `.json` files in the mod's configuration folder.

```csharp
public static void LoadExternalTranslations(string configFolder)
{
    // Look for files like "lang_es.json", "lang_ru.json"
    string[] files = Directory.GetFiles(configFolder, "lang_*.json");
    foreach (string file in files)
    {
        string langCode = Path.GetFileNameWithoutExtension(file).Replace("lang_", "");
        string jsonContent = File.ReadAllText(file);
        
        // Parse the JSON (using a simple parser or Unity's JsonUtility) and inject it into _translations
        ParseAndInjectJson(langCode, jsonContent);
        DebugLogger.Log($"Loaded external translations for: {langCode}");
    }
}
```
This allows players to create and share new language packs without needing to know how to write C# code or use a compiler.

---

## 3. The Fallback Chain: Preventing Silence

The most critical logic in the `Loc` utility is what happens when a translation is missing. If the player is playing in German ("de"), and the mod tries to call `Loc.Get("new_quest_received")`, but the German translator hasn't translated that specific string yet, what happens?

If the mod returns `null` or crashes, the player misses a critical game event. 
We solve this with a rigorous **Fallback Chain**.

```csharp
public static string Get(string key, params object[] args)
{
    if (string.IsNullOrEmpty(key)) return "";

    string result = null;

    // Attempt 1: Try the user's Current Language
    if (_translations.ContainsKey(CurrentLanguage) && _translations[CurrentLanguage].ContainsKey(key))
    {
        result = _translations[CurrentLanguage][key];
    }
    
    // Attempt 2: Fallback to English (if the current language failed)
    if (result == null && CurrentLanguage != FALLBACK_LANGUAGE)
    {
        if (_translations.ContainsKey(FALLBACK_LANGUAGE) && _translations[FALLBACK_LANGUAGE].ContainsKey(key))
        {
            result = _translations[FALLBACK_LANGUAGE][key];
            DebugLogger.Log($"[LOC WARNING] Missing translation for '{key}' in '{CurrentLanguage}'. Using English fallback.");
        }
    }

    // Attempt 3: Ultimate Fallback - Return the Key itself
    if (result == null)
    {
        DebugLogger.Log($"[LOC ERROR] Translation key completely missing: '{key}'");
        return $"!!{key}!!"; 
    }

    // Process Placeholders safely
    try
    {
        return args != null && args.Length > 0 ? string.Format(result, args) : result;
    }
    catch (FormatException)
    {
        DebugLogger.Log($"[LOC ERROR] Format exception in key '{key}'. Result string: '{result}'");
        return result; // Return the raw, unformatted string rather than crashing
    }
}
```

### Why Return the Key?
Notice Attempt 3. If a string is completely missing, we do not return an empty string. We return `!!new_quest_received!!`. 

Why? Because a screen reader reading "Exclamation mark, exclamation mark, new underscore quest underscore received" is infinitely more useful to a blind player than total silence. It tells them exactly what event just occurred, even if it's not grammatically beautiful. It also provides immediate, undeniable bug reporting data.

---

## 4. Automatic Language Synchronization

Your mod is now capable of speaking multiple languages. But how does it know *which* language to speak?

The worst user experience is forcing a blind player to edit a `.ini` text file just to change the mod's language. The mod should be intelligent enough to ask the game what language it is currently rendering in, and adapt instantly.

### The Polling Strategy
Because players can change the language in the game's Options menu at any time, your mod should periodically check the game's state.

1.  **Find the Game's Language Variable:** Using the techniques from Chapter 7, find where the game stores its current language. This is often in a class named `LocalizationManager`, `SettingsManager`, or even Unity's built-in `Application.systemLanguage`.
2.  **The Translation Map:** The game might represent "German" as the integer `2`, the string `"Deutsch"`, or the enum `Languages.DE`. You must map this to your mod's standardized codes (like `"de"`).

```csharp
// Inside your Main.Update() loop, perhaps running every 5 seconds
public void SyncLanguageWithGame()
{
    // Example: Reading a static variable from the game's code via Reflection
    string gameLanguageRaw = ReflectionHelper.GetField<string>(typeof(GameSettings), "CurrentLanguageName");
    
    string mappedCode = "en"; // Default
    
    if (gameLanguageRaw == "Deutsch" || gameLanguageRaw == "German") mappedCode = "de";
    if (gameLanguageRaw == "Francais" || gameLanguageRaw == "French") mappedCode = "fr";
    if (gameLanguageRaw == "Espanol" || gameLanguageRaw == "Spanish") mappedCode = "es";
    
    // Only update if it changed
    if (Loc.CurrentLanguage != mappedCode)
    {
        Loc.CurrentLanguage = mappedCode;
        DebugLogger.Log($"Language auto-synced with game: Changed to {mappedCode}");
        ScreenReader.Say(Loc.Get("language_updated_by_game"));
    }
}
```

By implementing this sync function, your mod achieves true symbiosis with the game client. When the player clicks the "Deutsch" button in the options menu, the game's visual text changes to German, and the screen reader instantly announces "Sprache aktualisiert" (Language updated) without any manual intervention.

---

## Conclusion: The Universal Bridge

A Polyglot Mod is the hallmark of a mature, considerate accessibility project. By designing a centralized dictionary system, enforcing grammar-safe placeholder formatting, building rigorous fallback chains, and automatically syncing with the host game's environment, you ensure that your mod is not just a localized hack, but a truly global tool.

We have now covered the entire technical spectrum of accessibility modding. We have built the environment (The Laboratory), the output mechanisms (The Nervous System), the control logic (The Brain), the data retrieval tools (The Senses), and the localization framework (The Polyglot). We have mastered the art of intercepting the game's flow (Harmony) and translating visual spaces into auditory cues (Mastering Menus and the World).

There is only one phase left. The code is written. The mod works. But how do you ensure it survives contact with the real world? How do you package it, distribute it, and build a community around it? In the final section of this book, we will tackle **Deployment and Beyond**, focusing on performance optimization, rigorous testing, and the responsibilities of a mod maintainer.

---
*Character Count Check: ~10,800 characters.*
