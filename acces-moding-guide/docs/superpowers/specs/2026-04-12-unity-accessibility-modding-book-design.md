# Design Spec: Unity Accessibility Modding Book

**Date:** 2026-04-12
**Topic:** A comprehensive, chapter-based book for developing accessibility mods for Unity games.
**Format:** Structured Markdown "Book" with a central Table of Contents and detailed chapters.

## 1. Objective
To produce a cohesive, long-form educational resource that guides a developer from absolute zero to building professional-grade accessibility mods for Unity games. Unlike a collection of loose guides, this "Book" will follow a progressive learning path.

## 2. Book Structure (Table of Contents)

### Part I: The Foundation
*   **Chapter 1: The Modding Ecosystem** - Introduction to Unity, BepInEx, MelonLoader, and the role of Tolk as a bridge to screen readers.
*   **Chapter 2: Setting Up Your Laboratory** - Detailed environment setup, installing the .NET SDK, configuring CLI tools, and preparing native binaries (Tolk.dll).

### Part II: The Architecture (The Modular Framework)
*   **Chapter 3: Designing the Nervous System** - Implementation of the `ScreenReader` wrapper, handling speech interruptions vs. queuing.
*   **Chapter 4: The Brain (State Management)** - Using `AccessStateManager` to manage context-aware input and "Escape" stacks for nested menus.
*   **Chapter 5: The Senses (Extraction & Reflection)** - Building `UITextExtractor` and `ReflectionHelper` to programmatically "read" the game's internal state.

### Part III: The Art of Discovery (Reverse Engineering)
*   **Chapter 6: Reading the Matrix** - Techniques for navigating decompiled code with `dnSpy/ILSpy` without sight.
*   **Chapter 7: Identifying Game Logic** - How to find the "Source of Truth" for game variables (health, gold, inventory) and identifying hookable methods.

### Part IV: Implementation & Patterns
*   **Chapter 8: Mastering Menus** - Standardized navigation patterns ("X of Y", grid coordinates, contextual key-hints).
*   **Chapter 9: Living in the World** - Spatialized audio, proximity beeps, and world exploration techniques for blind players.
*   **Chapter 10: Advanced Interception** - Deep dive into Harmony Patches (Prefix, Postfix, Transpiler) and the `AccessTools` utility.

### Part V: Going Global
*   **Chapter 11: The Polyglot Mod** - Designing the `Loc` utility for multi-language support.
*   **Chapter 12: Hooking Native Localization** - Syncing the mod with the game's internal translation system (LocalizationManager).

### Part VI: Deployment & Beyond
*   **Chapter 13: The Finishing Touches** - Performance optimization (caching references vs. values), error handling, and avoiding "Silent Degradation."
*   **Chapter 14: Distribution & Community** - Packaging, publishing on Nexus/Thunderstore, and building a community around accessibility modding.

## 3. Implementation Strategy
*   **Single Entry Point:** A `BOOK_OF_ACCESSIBILITY.md` file will serve as the root Table of Contents, linking to each chapter.
*   **Modular Chapters:** Each chapter will be a separate file in `docs/book/chapter-X.md`.
*   **Code-Integrated:** Chapters will include complete, copy-pasteable snippets from the refined `templates/` folder.
*   **Bilingual Support:** The book will be available in both English and German (in `docs-de/book/`).

## 4. Success Criteria
*   The developer can follow the book linearly to build a fully functional, localized accessibility mod.
*   The book covers both BepInEx and MelonLoader loaders.
*   The architecture taught is modular and resistant to game updates.
*   The book provides clear "Mental Models" for blind modders to navigate codebases.
