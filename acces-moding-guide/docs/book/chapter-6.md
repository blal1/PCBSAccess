# Chapter 6: Reading the Matrix

Reverse engineering—the process of understanding someone else's code—is the most challenging and rewarding part of modding. For a blind modder, this is not a visual process. It is a search-and-compare mission.

## 1. The Decompiled Mindset
When you open a game's DLL in a decompiler (like `ilspycmd` or `dnSpy`), you are looking at a reconstruction of the original C#. It will be messy. Variable names like `k__BackingField` are common. Don't panic.

## 2. Navigating Without Sight
Use your IDE's search and "Go to Definition" features. These are your primary eyes.

### The Search Strategy
1.  **Search for Strings:** If you see "Inventory" on the screen, search for that exact word in the decompiled code. You might find a class named `UI_Inventory` or a constant like `STRING_INVENTORY_TITLE`.
2.  **Search for Singletons:** Look for `static [ClassName] instance` or `public static [ClassName] i`. Games often use singletons for the "Manager" classes that control the game state.
3.  **Search for Prefixes:** Use `grep` to list all class names. Look for common Unity patterns: `Manager`, `Controller`, `UI`, `Screen`, `Panel`.

## 3. The "Matrix" Technique: Comparing Live vs. Static
Use a tool like **UnityExplorer** (if you have a sighted helper or can navigate its text-based menus) to see the live hierarchy of `GameObjects`.
*   If UnityExplorer says a button is named `btn_Accept`, search for `btn_Accept` in the code.
*   Find where that button is assigned.
*   Look for the `OnClick` listener. This will lead you directly to the code that executes when the button is pressed.

## 4. Identifying the "Update" Loop
Most Unity logic happens in the `Update()` method. In a decompiler, look for classes that inherit from `MonoBehaviour`.
*   Search for `void Update()`.
*   Inside `Update()`, look for `Input.GetKeyDown`. This tells you which keys the game is listening for.

### Pro Tip: Documentation is Key
Every time you find a class or method name, document it in your `game-api.md`.
```markdown
## Inventory System
- Class: `UI_Inventory_Manager`
- Singleton: `UI_Inventory_Manager.i`
- Open Method: `void Show()`
- Selected Item Field: `private int _currentIndex`
```
This "API Map" is the most valuable asset you will build.

---
*Next: [Chapter 7: Identifying Game Logic](chapter-7.md)*
