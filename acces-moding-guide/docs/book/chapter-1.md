# Chapter 1: The Modding Ecosystem

Before we write our first line of code, we must understand the environment we are working in. Accessibility modding is the art of injecting new behavior into existing software. For Unity games, this is made possible by a powerful stack of community-driven tools.

## 1. The Unity Engine
Unity is one of the most popular game engines in the world. It uses C# for its scripting logic. When a Unity game is "compiled," its code is turned into Intermediate Language (IL) and stored in DLL files (like `Assembly-CSharp.dll`).

## 2. Mod Loaders: BepInEx and MelonLoader
A "Mod Loader" is a specialized program that sits between your operating system and the game. Its primary job is to "inject" your mod's DLL into the game's memory space as it starts up.

*   **BepInEx:** The industry standard for many Unity mods. It is lightweight, stable, and highly compatible with many older games.
*   **MelonLoader:** A modern alternative that excels at handling "Il2Cpp" games (games where C# code was converted to C++ for performance).

In this book, we will support both, as they share the same underlying logic.

## 3. Harmony: The Master Interceptor
Harmony is a library included with both BepInEx and MelonLoader. It allows you to "patch" game methods at runtime. You can run your code *before* a game method (Prefix), *after* it (Postfix), or even replace the method entirely (Transpiler). This is how we detect when a menu opens or a player takes damage.

## 4. Tolk: The Bridge to the Screen Reader
The screen reader (NVDA, JAWS, etc.) is an external application. Your mod cannot talk to it directly. **Tolk** is a native C++ library that serves as a bridge. It detects which screen reader the player is using and sends your text to it.

### How the Chain Works:
1.  **The Game** triggers an event (e.g., a button is clicked).
2.  **Harmony** intercepts that event and calls your **Mod**.
3.  Your **Mod** identifies what happened and generates a text string.
4.  Your **Mod** sends that string to **Tolk**.
5.  **Tolk** speaks the text through the **Screen Reader**.

Understanding this chain is vital. If any link is broken—if the mod isn't loaded, if the patch doesn't trigger, or if the Tolk DLL is missing—the player experiences silence.

---
*Next: [Chapter 2: Setting Up Your Laboratory](chapter-2.md)*
