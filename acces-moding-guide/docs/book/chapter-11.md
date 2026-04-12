# Chapter 11: The Polyglot Mod

A true accessibility mod should be usable by people across the globe. In this chapter, we build the `Loc` utility, which enables multi-language support from day one.

## 1. The Localization Architecture
Never hardcode strings in your mod. Instead, use "Keys" that point to a dictionary of translations.
`ScreenReader.Say("inventory_opened")`

## 2. Using the Loc Utility
The `Loc.cs` template provides a simple way to manage these strings.

### Adding Translations
```csharp
private static void InitializeStrings() {
    Add("inv_opened", 
        en: "Inventory Opened", 
        de: "Inventar geöffnet");
    
    // With placeholders
    Add("item_count", 
        en: "{0} items", 
        de: "{0} Gegenstände");
}
```

### Retrieving Translations
```csharp
ScreenReader.Say(Loc.Get("item_count", 5)); // "5 items"
```

## 3. Automatic Language Detection
Your mod should automatically detect the player's game language.
1.  Identify the game's `LocalizationManager`.
2.  Read its current language setting.
3.  Set `Loc.CurrentLanguage` to match.

## 4. The Fallback Chain
If a translation is missing for the current language, the `Loc` utility should automatically fall back to English. If English is also missing, it should return the key itself (e.g., `"!!MISSING:inv_opened!!"`) so you can easily spot it during testing.

### Pro Tip: Placeholders for Grammar
Different languages use different word orders. Never build a sentence by adding strings together like `Loc.Get("you_have") + count + Loc.Get("items")`. This will break in many languages.

**Always use placeholders:** `Loc.Get("item_count", count)`. This allows translators to move the position of the `{0}` to match their language's grammar.

---
*Next: [Chapter 12: Hooking Native Localization](chapter-12.md)*
