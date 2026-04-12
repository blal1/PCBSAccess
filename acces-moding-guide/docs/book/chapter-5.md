# Chapter 5: The Senses (Extraction & Reflection)

To make a game accessible, your mod must be able to "see" what is happening on the screen. Since we are modifying a game from the outside, we use two primary "senses": **UITextExtractor** for visual elements and **ReflectionHelper** for internal logic.

## 1. UITextExtractor: Reading the Screen
Unity UI is typically built using components like `Text` or `TextMeshPro`. The `UITextExtractor` is a utility that crawls the game's hierarchy to find these components and return their text.

### The Problem with Buttons
Often, a button doesn't have a `Text` property itself; the text is in a "Child" object. Our `UITextExtractor` is designed to look through children to find the most likely label for a button.

```csharp
// Simple example of usage
GameObject currentButton = GetCurrentlyFocusedButton();
string label = UITextExtractor.GetText(currentButton);
ScreenReader.Say(label);
```

## 2. ReflectionHelper: Reading the Mind
Sometimes the information we need isn't on the screen at all—it's locked inside a "private" field in the game's code. **Reflection** is a C# feature that allows us to access these private fields.

### Why use a Helper?
Reflection is slow. If you look up a field name every frame, the game's performance will tank. Our `ReflectionHelper` solves this by **caching** the field information the first time it finds it.

```csharp
// Accessing a private "health" field in the Player class
int health = ReflectionHelper.GetField<int>(playerInstance, "currentHealth");
```

## 3. Defensive Programming (The Senses Must Be Robust)
The game's UI and internal code can change between updates. If your mod assumes an object exists and it's missing, the mod will crash.

**Rule:** Always check for null.
```csharp
var textComp = panel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
if (textComp == null) {
    DebugLogger.Log("Error: Could not find TextMeshPro on inventory panel.");
    return;
}
```

## 4. Combining the Senses
A high-quality announcement often combines both senses:
1.  Use `ReflectionHelper` to find the **Item ID**.
2.  Use the ID to look up the **Item Name** in the game's database.
3.  Use `UITextExtractor` to see if there's a **Price Tag** attached to the UI slot.
4.  Announce: "[Name], [Price] gold."

---
*Next: [Chapter 6: Reading the Matrix](chapter-6.md)*
