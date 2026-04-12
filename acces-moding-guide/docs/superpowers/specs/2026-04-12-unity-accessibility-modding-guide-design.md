# Design Spec: Unity Accessibility Modding Guide (Modular Framework)

**Date:** 2026-04-12
**Topic:** Comprehensive guide for developing accessibility mods for Unity games using BepInEx and MelonLoader.
**Focus:** Screen reader support (Tolk), modular architecture, and localization.

## 1. Objective
To provide a professional-grade, architectural guide for creating accessibility mods that enable blind players to play Unity games using screen readers (NVDA, JAWS, etc.). The guide emphasizes "Playability, Not Simplification" and uses a modular framework to ensure maintainability and scalability.

## 2. Architecture: The Modular Framework
The guide will center on a decoupled architecture where game-specific logic is separated from reusable accessibility utilities.

### 2.1 Core Modules
- **`ScreenReader` (Tolk Wrapper):**
    - **Purpose:** Bridge to Windows screen readers.
    - **Features:** Initialization, `Say(text, interrupt)`, `SayQueued(text)`, `Stop()`, and `Shutdown()`.
    - **Dependency:** `Tolk.dll`, `nvdaControllerClient64.dll`.
- **`AccessStateManager`:**
    - **Purpose:** Tracks the active game context (e.g., `MainMenu`, `Inventory`, `Combat`).
    - **Features:** Prevents key conflicts by ensuring only the active "Handler" processes input. Supports an "Escape" stack for nested menus.
- **`ReflectionHelper`:**
    - **Purpose:** Safe access to private/internal game fields and methods.
    - **Features:** Cached field/method lookups to minimize performance impact.
- **`UITextExtractor`:**
    - **Purpose:** Extracts clean strings from Unity UI components (`Text`, `TextMeshProUGUI`, `Button`, `Toggle`, `Slider`).
    - **Features:** Hierarchy crawling, finding labels for buttons, and state description (e.g., "Toggle: On").
- **`DebugLogger`:**
    - **Purpose:** Diagnostic tool to log screen reader output to the console for developers.
    - **Features:** Conditional logging based on `DebugMode` config.

### 2.2 Handler Pattern
Each game feature (Inventory, Quest Log, Combat) gets its own `Handler` class.
- **Responsibilities:**
    - `IsOpen()`: Detect if the feature's UI is active.
    - `Update()`: Poll for state changes (selection, health, etc.).
    - `Navigate(direction)`: Custom keyboard navigation logic.
    - `AnnounceStatus()`: Context-specific status report (triggered by F-keys).

## 3. Integration & Tooling
The guide provides a dual-loader approach, ensuring code can be used with both BepInEx and MelonLoader.

### 3.1 Mod Loaders
- **BepInEx:** Instructions for `BaseUnityPlugin` setup, `[BepInPlugin]` attributes, and using `Bepinex.Logging`.
- **MelonLoader:** Instructions for `MelonMod` setup, `[assembly: MelonInfo]` attributes, and `MelonLogger`.
- **Unified Logic:** Patterns for sharing 90% of the codebase (Handlers/Utils) between both loaders.

### 3.2 Research Tools
- **UnityExplorer:** Live hierarchy inspection and component value tweaking.
- **Harmony:** Deep-dive into `[HarmonyPatch]` types (Prefix, Postfix, Transpiler) for method interception.
- **dnSpy/ILSpy:** Finding internal class names and logic structures in game DLLs.

## 4. Accessibility Patterns
Standardized ways to translate visual game elements into meaningful audio feedback.

### 4.1 UI & Navigation
- **List Navigation:** "X of Y: [Name]" (e.g., "3 of 12: Health Potion").
- **Grid Navigation:** "Row X, Col Y: [Name]".
- **Contextual Hints:** "Press F1 for help, Enter to use."
- **Throttling & Priority:** Only announce on change; interrupt for critical alerts (e.g., "Health Low!").

### 4.2 World Exploration
- **Proximity Audio:** Using beeps or spatialized sound for interactive objects.
- **Compass & Scanning:** Announcing cardinal directions and "scanning" the immediate area for points of interest.

## 5. Localization (Internationalization)
The guide addresses supporting multiple languages for both the mod's strings and the game's content.

### 5.1 Mod Localization (`Loc` Utility)
- **Pattern:** Using a `Loc` class with key-value pairs (English, German, etc.).
- **Storage:** JSON or embedded resources for easy translation by community members.

### 5.2 Game Integration
- **Hooking:** How to read the game's current language setting and use the game's own `LocalizationManager` to fetch item names/descriptions.
- **Fallback:** Logic for falling back to English if a translation is missing.

## 6. Implementation Strategy
The guide will be organized into the following chapters:
1. **Introduction & Core Philosophy**
2. **Environment Setup (BepInEx, MelonLoader, Tolk)**
3. **Research Phase (Decompiling & Exploring)**
4. **Building the Core Framework (Utilities)**
5. **Implementing Handlers (UI & Game Logic)**
6. **Advanced Patterns (World Exploration, Harmony Patches)**
7. **Localization & Community Distribution**

## 7. Success Criteria
- A modder can follow the guide to make a basic Unity UI screen accessible within 1 hour.
- The resulting mod code is modular and resistant to minor game updates.
- The mod works seamlessly with NVDA, JAWS, and other Tolk-supported screen readers.
