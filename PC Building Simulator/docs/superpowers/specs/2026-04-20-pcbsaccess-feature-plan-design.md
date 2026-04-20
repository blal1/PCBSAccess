# PCBSAccess — Feature Plan Design

**Date:** 2026-04-20
**Project:** PCBSAccess — Accessibility mod for PC Building Simulator
**Phase:** 1.5 — Feature Planning

---

## Goal

Make PC Building Simulator fully playable for a blind screen reader user. The game must be navigable from main menu through career mode: accept jobs, diagnose and repair PCs, order parts, build PCs, collect rewards, grow the shop.

---

## Approach

Framework-first (Phase 2), then features one per session in priority order (Phase 3+).

All shared infrastructure is built once before any feature work begins. Each feature is a self-contained Handler class that plugs into the framework.

---

## Phase 2 — Framework

Six files, all created in one session before any feature work.

- **`Main.cs`** — BepInEx plugin entry point. Applies Harmony patches. Runs hotkey polling in `Update()`. Initializes all framework classes on plugin load.
- **`ScreenReader.cs`** — Tolk wrapper. Single method: `Speak(string text, bool interrupt = true)`. Interrupt = true cuts off previous speech. All announcements go through here.
- **`DebugLogger.cs`** — File logger. Active only when F12 debug mode is on. Zero overhead otherwise. Used for null warnings and unexpected state.
- **`Loc.cs`** — Localization for mod-added labels. Initialized from `LocalizationManager.CurrentLanguage` (I2 Localization). Supports English (default) and French. `Loc.Get(key)` returns the right string. Game text (tutorial body, email content, part names) comes from the game directly — Loc only covers labels the mod adds.
- **`AccessStateManager.cs`** — Tracks which handler is active. Prevents two handlers fighting over arrow keys and Enter. Handlers register/deregister themselves. Not used for global hotkeys (F1, F12).
- **`ReflectionHelper.cs`** — Cached reflection for private `[SerializeField]` fields. Used whenever a needed field is private. Cache prevents repeated reflection overhead.

---

## Phase 3 — Features (Priority Order)

### 1. HowToBuildAPC Handler

**What:** Announces each guided step message in the "How to Build a PC" tutorial mode. This is a linear, guided assembly mode — the safest way for the user to learn the game.

**Trigger:** Harmony patch on `HBPCState` step advancement.

**Announces:** The localized step message (`HBPCStep.m_message`) when each step begins.

**Format:** `"{step message}"`

---

### 2. Main Menu Handler

**What:** Announces the currently selected button as the user navigates the main menu.

**Trigger:** Harmony patch on `MainMenu` navigation (selection change events).

**Announces:** Button label when selection changes.

**Format:** `"{button label}"`

---

### 3. Tutorial Popup Handler

**What:** Reads tutorial popups automatically the moment they appear anywhere in the game.

**Trigger:** Harmony postfix on `TutorialUI.Init()`. Fires after Init sets `m_title.text` and `m_body.text`, so real localized strings are already loaded.

**Announces:** Title then body.

**Format:** `"{title}. {body}"`

**Note:** `m_title` and `m_body` are public fields — no Reflection needed.

---

### 4. Email / Notification Handler

**What:** Auto-announces job emails and delivery notifications the moment they arrive — no key press needed.

**Trigger:** Harmony patch on email arrival and notification display methods.

**Announces:** Subject + body of incoming email or notification.

**Format:** `"{subject}. {body}"`

---

### 5. Career Status Handler

**What:** On-demand hotkey reads cash, kudos, and star rating. Works in any game state.

**Trigger:** F1 key press (polled in `Main.Update()`).

**Announces:** All three stats in one announcement.

**Format:** `"Cash: {amount}. Kudos: {kudos}. Rating: {stars} stars"`

**Labels** ("Cash:", "Kudos:", "Rating:", "stars") go through `Loc.Get()` for French support.

---

### 6. Inventory Handler

**What:** Auto-announces full part details when the selected item changes in the inventory panel.

**Trigger:** Harmony patch on inventory selection change.

**Announces:** Full part details — name, brand, all specs, quantity owned.

**Format:** `"{name}. {brand}. {spec1}, {spec2}, ... {quantity} in stock"`

---

### 7. Shop Handler

**What:** Auto-announces full part details when browsing the in-game parts shop.

**Trigger:** Harmony patch on shop selection change.

**Announces:** Full part details including price and delivery time.

**Format:** `"{name}. {brand}. {spec1}, {spec2}, ... Price: {price}. Delivery: {days} days"`

---

### 8. Build Mode Handler

**What:** Guides the user through PC assembly — announces slot names, what's installed, and what the current job requires.

**Trigger:** Harmony patches on build mode entry, slot selection change, part install/uninstall events.

**Announces:** On slot focus: slot name, installed part (or "empty"), job requirement for that slot. On install/uninstall: confirmation of action.

**Format (slot):** `"{slot name}. Installed: {part or empty}. Required: {requirement or none}"`
**Format (action):** `"{part name} installed" / "{part name} removed"`

---

## Architecture

### Handler Pattern

```
Game event
    → Harmony patch fires
    → Handler reads game state (public fields or ReflectionHelper)
    → Handler calls ScreenReader.Speak(text, interrupt: true)
    → Handler uses Loc.Get(key) for any mod-added labels
```

Handlers do not call each other. No shared mutable state between handlers. Each handler manages its own Harmony patches.

### AccessStateManager

Tracks which handler is currently active. Handlers call `AccessStateManager.SetActive(this)` on entry and `AccessStateManager.SetActive(null)` on exit. Used to prevent two handlers competing for the same input. Global hotkeys (F1, F12) bypass AccessStateManager and always work.

### Announcement interrupt policy

All `ScreenReader.Speak()` calls use `interrupt: true` by default. This cuts off previous speech when a new selection is made — matching standard screen reader behavior.

### Error handling

- Null game objects → `DebugLogger.Log("warn: ...")`, no crash, no announcement
- No try-catch in normal handler code
- Try-catch only around Reflection calls and Tolk (external library)

---

## Localization

`Loc.cs` covers only mod-added labels. All game text is read directly from game fields.

| Key | English | French |
|---|---|---|
| `mod_loaded` | Accessibility mod loaded | Mod d'accessibilité chargé |
| `debug_on` | Debug mode on | Mode débogage activé |
| `debug_off` | Debug mode off | Mode débogage désactivé |
| `cash` | Cash | Argent |
| `kudos` | Kudos | Kudos |
| `rating` | Rating | Évaluation |
| `stars` | stars | étoiles |
| `installed` | Installed | Installé |
| `required` | Required | Requis |
| `empty` | empty | vide |
| `none` | none | aucun |

---

## Hotkeys

| Key | Action | State |
|---|---|---|
| F1 | Read career status (cash, kudos, rating) | Any |
| F12 | Toggle debug mode | Any |

Additional hotkeys allocated per feature as needed. All from confirmed safe mod keys (game-api.md).

---

## Feature Dependencies

- All features depend on Phase 2 framework being complete
- No feature depends on another feature
- Features can be built and tested independently in any order after Phase 2

---

## Not in Scope (this mod)

- Overclocking UI reader (Tier C — unlock-gated, complex BIOS UI)
- Auction house reader (Tier C)
- Water cooling mode (Tier C)
- Peripherals/decorations reader (Tier C)

Tier C features can be added later once core game loop is fully accessible.
