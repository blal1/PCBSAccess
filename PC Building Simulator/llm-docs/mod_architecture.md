# PCBSAccess: Mod Architecture

This document describes the design and structure of the `PCBSAccess` accessibility mod.

## 1. Entry Point (`Main.cs`)
The mod uses **BepInEx** as its loader. The `Main` class inherits from `BaseUnityPlugin`.

### Lifecycle
- **`Awake()`**: Initializes the screen reader (`Tolk`), localization (`Loc`), and applies Harmony patches.
- **`OnSceneLoaded()`**: Resets all handlers when a new scene is loaded to prevent stale state.
- **`Update()`**: Polls global hotkeys and calls the `Update()` method of all active handlers.

## 2. Handler Pattern
To keep the codebase maintainable, each game screen or major mechanic is managed by a specific **Handler** class (e.g., `MainMenuHandler`, `BuildModeHandler`).

### Handler Characteristics
- **Static State**: Most handlers are static or managed as singletons.
- **`IsActive`**: A property that determines if the handler should currently process input or provide feedback.
- **`Update()`**: Called every frame from `Main.Update()` if the handler needs to poll for specific keys or state changes.
- **`Reset()`**: Called on scene load to clear any cached data.

## 3. Harmony Patching Strategy
The mod uses **Harmony** to hook into game methods and provide accessibility feedback.

### Patch Types
- **Prefix Patches**: Used to intercept input or actions before they happen.
- **Postfix Patches**: Used to trigger feedback (e.g., screen reader announcements) after a UI element is shown or a state changes.

### Safe Patching
The mod applies patches individually within a try-catch block in `Main.ApplyPatches()`. This ensures that if one patch fails (due to version mismatch or missing classes), the rest of the mod can still function.

## 4. Key Framework Components
- **`ScreenReader`**: A wrapper for the Tolk library to provide Text-to-Speech (TTS) output.
- **`Loc`**: A simple localization manager for mod-specific strings.
- **`ReflectionHelper`**: Utilities for accessing private fields and methods in the game's code.
- **`DebugLogger`**: A centralized logging system for troubleshooting.
- **`SoundFeedback`**: (Proposed) For providing non-verbal audio cues (beeps, etc.).
