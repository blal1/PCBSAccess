# Chapter 12: Hooking Native Localization

While `Chapter 11` was about your *own* mod's strings, your mod also needs to read the *game's* strings (like item names, quest descriptions, and dialogues).

## 1. The Game's Language Database
Most Unity games have a `LocalizationManager` or `LanguageDatabase` that stores all game text. Your goal is to find where this data is stored.

### Common Locations
*   **A Dictionary or Map:** Search for `Dictionary<string, string>` in the localization classes.
*   **A Spreadsheet or CSV:** Many games load their text from a `Resources` folder or an external file.
*   **A Translation Method:** Search for a method like `GetLocalizedString(string key)` or `Translate(string key)`.

## 2. Reading Translated Game Text
Once you find the game's translation method, you can use it to fetch the name of any object in the player's chosen language.

```csharp
// Example: Translating an item name
string localizedName = GameLocalization.Get(item.ID);
ScreenReader.Say(localizedName);
```

## 3. Handling Custom Formatting
Some games use custom "Tags" in their text, like `<color=red>Fire Sword</color>`. Before sending this to the screen reader, you must strip these tags out.
```csharp
string cleanText = Regex.Replace(gameText, "<.*?>", string.Empty);
```

## 4. Syncing the Mod and the Game
Always ensure your `Loc.CurrentLanguage` is in sync with the game. If the player changes the game's language in the settings menu, your mod should detect this and change its own language immediately.

### Why this is vital
If a player plays a German game with an English accessibility mod, the mix of languages will be confusing. Syncing the two ensures a seamless, professional experience.

---
*Next: [Chapter 13: The Finishing Touches](chapter-13.md)*
