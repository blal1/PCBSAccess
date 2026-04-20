# PC Building Simulator - Game API Documentation

## Overview

- **Game:** PC Building Simulator
- **Developer:** The Irregular Corporation
- **Engine:** Unity 2018.4.16f1 (Mono runtime)
- **Architecture:** 64-bit
- **Mod Loader:** BepInEx 5.x x64 (net472)
- **Decompiled source:** PCBS_Data\Managed\Assembly-CSharp-firstpass\ (main game code)
- **NOTE:** Assembly-CSharp\ has only 71 files (shaders, Rewired demos) — NOT the game. Real code is in Assembly-CSharp-firstpass\

---

## 1. Singleton Access Points

### Main Game Controller
- `GameController.instance` / `GameController.Get()` — both work
  - `GameController.Get().GetCurrentGameState()` — returns `GameState` enum
  - `GameController.Get().m_gameMode` — returns `GameMode` enum
  - `GameController.Get().SetCurrentState(State state)` — state machine entry
  - `GameController.Get().GoToPreviousState()` — back navigation
  - `GameController.Get().ShowTutorial(GameTutorial step)` — trigger tutorial step

### Career / Player Data
- `CareerStatus.Get()` (field: `CareerStatus.s_instance`) — career progression
  - `CareerStatus.Get().GetCash()` — player money (int)
  - `CareerStatus.Get().GetKudos()` — reputation/kudos points (int)
  - `CareerStatus.Get().GetStarRating()` — shop star rating (float)
  - `CareerStatus.Get().WantsTutorial(GameTutorial step)` — tutorial state check
  - NOTE: m_cash, m_kudos, m_starRating are on the private inner CareerStatus.State — use Get methods above

### Central UI Access
- `CommonUI.s_instance` — singleton; holds references to all main UI panels
  - `CommonUI.inGameMenu` — `InGameMenu`
  - `CommonUI.saveMenu` — `SaveLoadMenu`
  - `CommonUI.optionsMenu` — `OptionsMenu`
  - `CommonUI.workshopSelectionMenu` — `WorkshopSelectionMenu`
  - `CommonUI.keyBindings` — `KeyBindingMenu`
  - `CommonUI.messageBox` — `MessageBox`
  - `CommonUI.tutorialUI` — `TutorialUI`
  - `CommonUI.debugMenu` — `DebugMenu`
  - `CommonUI.backButton` — back button (active/inactive per state)

### Workshop
- `WorkshopController.Get()` (field: `WorkshopController.s_instance`)

### Parts / Data
- `PartsDatabase.s_instance` — all part definitions
  - `PartsDatabase.IsReady` — whether parts are loaded

### Other Singletons
- `InputModule.s_instance` — custom input module (extends PointerInputModule)
- `MusicManager.s_instance` — music playback
- `SoundPlayer.instance` — sound effects
- `ToolTips.s_instance` — tooltip display
- `LoadingScreenManager.Instance` — extends `Singleton<LoadingScreenManager>`
- `Singleton<ScreenFade>.Instance` — screen fade control

### Singleton Patterns
Two patterns used:
- `PCBS.Singleton<T>` — persistent across scenes
- `FuturLab.SceneOnlySingleton<T>` — destroyed on scene change, check `.InstanceExists` before use

---

## 2. Game Key Bindings (DO NOT override in mod!)

**CRITICAL: The game uses Rewired (not Unity's Input). Keys are user-rebindable.**
**Use `PCBSInput.m_action.GetDown()` pattern — never call `Input.GetKeyDown()` for game actions.**

### PCBSInput Rewired Actions (action names in parentheses)
- `PCBSInput.m_mouseX/Y` ("MouseX", "MouseY") — mouse movement
- `PCBSInput.m_menuX/Y` ("X", "Y") — menu navigation (arrow keys / d-pad)
- `PCBSInput.m_menuAction` ("MenuAction") — confirm / select (Enter / A on pad)
- `PCBSInput.m_menuBack` ("MenuBack") — back / cancel (Escape / B on pad)
- `PCBSInput.m_moveX/Y` ("Move X", "Move Y") — player movement (WASD)
- `PCBSInput.m_lookX/Y` ("Look X", "Look Y") — camera look (mouse)
- `PCBSInput.m_scroll` ("Scroll") — mouse wheel
- `PCBSInput.m_action` — primary interact (likely E)
- `PCBSInput.m_secondaryAction` — secondary interact (likely Q)
- `PCBSInput.m_power` — PC power button
- `PCBSInput.m_exit` — exit (Escape)
- `PCBSInput.m_menu` — menu toggle
- `PCBSInput.m_hideUI` — hide UI
- `PCBSInput.m_inventory` — inventory (likely Tab or I)
- `PCBSInput.m_assemblyMode` — assembly mode toggle
- `PCBSInput.m_disassemblyMode` — disassembly mode toggle
- `PCBSInput.m_cablingMode` — cabling mode toggle
- `PCBSInput.m_pipingMode` — piping/water cooling mode
- `PCBSInput.m_toggleJobStatus` — job status panel
- `PCBSInput.m_cameraPan` — camera pan (middle mouse)
- `PCBSInput.m_cameraRotate` — camera rotate (right mouse)
- `PCBSInput.m_zoom` ("Scroll") — zoom
- `PCBSInput.m_enterBios` / `m_enterBiosAlt` — enter BIOS
- `PCBSInput.m_decreaseBios` / `m_increaseBios` — BIOS navigation
- `PCBSInput.m_run` — run (Shift)
- `PCBSInput.m_dlcEndPhase` — end esports phase
- `PCBSInput.m_dlcUsePhone` — use phone (IT Support DLC)
- `PCBSInput.m_dlc2UseTablet` — use tablet (IT Support DLC 2)
- `PCBSInput.m_dlc2ToggleCustomisation` — toggle customisation

### Direct KeyCode use (hardcoded, not Rewired)
- **Backtick / BackQuote (`)** — debug console toggle (Consolation library)
- **Tab** — NotesApp note editor navigation
- **Space** — IT Support offsite state action (FuturLab\ITSupportGoOffsiteState.cs)
- **Escape** — back navigation in InputModule (line 491, 544)
- **F8** — screenshot (TakeScreenshot utility in Crosstales library)

### Mouse
- Left click: interact / select UI
- Right click: camera rotate / secondary action
- Middle click: camera pan
- Scroll wheel: zoom / scroll UI

---

## 3. Safe Mod Keys

### Reserved for Accessibility Mod
- **F12** — debug mode toggle (our mod)
- **F1** — help announcement (our mod)

### Available (not used by game)
- **F2, F3, F4, F5, F6, F7, F9, F10, F11** — available for feature hotkeys
- **Numpad 0–9** — available for quick-read actions
- **Insert, Home, End, Page Up, Page Down** — available (check for conflicts if using keyboard navigation mode)

### Use with caution
- **Tab** — used in NotesApp (safe in other contexts but test carefully)
- **Escape** — game uses for back/exit, our mod must NOT intercept it globally
- **F8** — used by TakeScreenshot library (avoid)

---

## 4. UI System

### Text Components — MIXED
The game uses BOTH Unity UI `Text` and `TextMeshProUGUI`. Check per class:
- Most UI classes: `using UnityEngine.UI` → `Text` component
- InputModule: `using TMPro` → TextMeshProUGUI
- Pattern to check: look at `using` statements in each file

### Private [SerializeField] Fields — REFLECTION REQUIRED
1,119 `[SerializeField]` occurrences across 228 files. Almost all UI field references are private.
Use `ReflectionHelper` for any UI field access. Example private fields seen:
- `Credits.m_title` (private Text)
- `Credits.m_names` (private Text)
- `DisableBestFitForAsianLanguage.Text` (private Text)
- MainMenu: `[SerializeField] private Text[] m_textsAffectedByColorTheme`

**Create ReflectionHelper early in Phase 2** — it will be needed for almost everything.

### UI Base Classes
No common base class. All UI classes extend `MonoBehaviour` directly.

### Key UI Classes
- `MainMenu` — title screen (active in main menu scene)
  - Has: `mainMenu` (GameObject), `menuCanvas`, `optionsMenu`, `loadMenu`, `workshopSelectionMenu`
  - Uses: `MenuContext m_context` for navigation buttons
  - Start: calls `GameController.Get().SetCurrentState(new UIState(...))`
- `OptionsMenu` — options/settings panel
- `InGameMenu` — in-game pause menu (accessible via CommonUI.inGameMenu)
- `SaveLoadMenu` — save/load (accessible via CommonUI.saveMenu)
- `KeyBindingMenu` — Rewired key rebinding menu
- `WorkshopSelectionMenu` — workshop/slot selection
- `MenuContext` — context buttons (exit, options) shared across menu screens
- `WindowFrame` — generic window frame UI
- `SearchFiltersPanel` — shop search filters

### UI Navigation / State Machine
- `GameController.Get().SetCurrentState(State)` — push new state
- `GameController.Get().GoToPreviousState()` — pop to previous state
- `GameState` enum values seen: `UI`, `WalkingState`, `DLC_ArenaState`, `DLC2_MothershipCustomisationState`, `DLC_UI_Onboarding`
- `CommonUI.backButton` — back button, visible/hidden per state
- `GameController.Get().BackInputButtonEnabled` — setter for back button availability

### Tooltip System
- `ToolTips.SetComponent(string action, PartInstanceContainer context)` — show component tooltip
- `ToolTips.SetConnector(string action, PartInstanceContainer context, PowerConnector connector)` — show connector tooltip
- `ToolTips.Clear()` — clear tooltips (called on UI state entry)
- `ToolTips.s_instance` — singleton access

---

## 5. Game Mechanics Overview

### Game Modes
- `GameMode.CAREER` — main career with jobs, reputation, tutorial
- `GameMode.FREEBUILD` — sandbox free building
- `GameMode.HOW_TO_BUILD_A_PC` — guided tutorial mode (HowToBuildAPC.cs)
- `GameMode.DLC_ITSUPPORT` — IT Support DLC (FuturLab)
- Check: `GameController.Get().m_gameMode`

### Career Data
- `CareerStatus.Get()` — access all career data
  - `m_cash` — player money
  - `m_kudos` — reputation points
  - `m_starRating` — shop rating
  - `m_businessName` — player's company name
  - `m_isHardMode` — difficulty flag

### Parts System
- `PartDesc` — part definition (type, brand, specs)
- `PartInstance` — installed/owned instance of a part
- `PartsDatabase.s_instance` — lookup all parts
- `PartDesc.Type` enum — CPU, GPU, RAM, etc.

---

## 6. Tutorial System

Tutorial is a key accessibility feature — user doesn't know the game.

- `PCBS.GameTutorial` — enum of tutorial steps
- `Tutorial.cs` — static class, checks tutorial conditions
  - `Tutorial.ShowVerboseControls()` — true if in CAREER and COLLECT_REWARD tutorial active
- `TutorialUI` — UI for tutorial popups (reference via `CommonUI.tutorialUI`)
- `PCBS.GameTutorialTools` — helper tools for tutorial
- `HowToBuildAPC.cs` — separate "How to Build a PC" tutorial mode class
- `GameController.Get().ShowTutorial(GameTutorial step)` — trigger tutorial step

**Tutorial mode entry:** `GameMode.HOW_TO_BUILD_A_PC` is the beginner tutorial. Started from MainMenu.

---

## 7. Localization (I2 Localization)

The game uses **I2 Localization** (`I2.Loc` namespace).

### Detecting Current Language
```csharp
using I2.Loc;

// Full language name — use this to detect language in Loc.cs
string lang = LocalizationManager.CurrentLanguage;
// Returns: "English", "French", "German", "Chinese (Simplified)", "Japanese", "Korean", etc.

// ISO language code
string code = LocalizationManager.CurrentLanguageCode;
// Returns: "en-US", "fr-FR", etc.
```

### Game's Localization Access Patterns
```csharp
// Pattern 1: Generated ScriptLocalization class (type-safe)
ScriptLocalization.Category.KEY

// Pattern 2: String extension method (runtime lookup)
"Category/KEY".Localized()

// Both work. Our mod uses its own Loc.cs system — we do NOT use these.
```

### For our Loc.cs
```csharp
// In Loc.cs Initialize():
string lang = LocalizationManager.CurrentLanguage;
switch (lang)
{
    case "French": _lang = Lang.Fr; break;
    default: _lang = Lang.En; break; // English is fallback
}
```

### Supported Languages Found
- English, French, German, Japanese, Chinese (Simplified), Korean (confirmed from HBPCInspectionLabel.cs)

---

## 8. Event Hooks for Harmony Patches

### Best Patch Points (discovered)

**Main Menu activation:**
- `MainMenu.Start()` — postfix to detect when main menu loads
- `MainMenu.OnEnable()` — fires when main menu becomes visible

**Game state changes:**
- `GameController.SetCurrentState(State state)` — postfix for any state change
  - Check `GameController.Get().GetCurrentGameState()` to know which state

**Career status changes (Tier 2 — fill when implementing):**
- TBD — patch CareerStatus setters for money/kudos changes

**Tutorial triggers:**
- `GameController.ShowTutorial(GameTutorial step)` — postfix to intercept tutorial display

---

## 9. Code Examples

### Check current game state
```csharp
var gc = GameController.Get();
if (gc.GetCurrentGameState() == GameState.UI)
{
    // In a UI screen
}
```

### Access career data
```csharp
var career = CareerStatus.Get();
int cash = career.m_cash;
int kudos = career.m_kudos;
```

### Access UI elements via Reflection (for private [SerializeField] fields)
```csharp
// Example: read private Text field from a class
var field = typeof(SomeUIClass).GetField("m_someText", 
    BindingFlags.NonPublic | BindingFlags.Instance);
Text text = (Text)field.GetValue(someInstance);
string content = text.text;
```

### Harmony patch example
```csharp
[HarmonyPatch(typeof(MainMenu), "Start")]
class MainMenuStartPatch
{
    static void Postfix(MainMenu __instance)
    {
        // Main menu loaded — initialize main menu handler
    }
}
```

### Detect language for Loc.cs
```csharp
using I2.Loc;
string lang = LocalizationManager.CurrentLanguage; // "English" or "French"
```

---

## 10. Known Issues and Workarounds

- The Rewired input system means standard `Input.GetKeyDown()` calls won't see game action presses. Only use `Input.GetKeyDown()` for our own mod keys (F1, F2, F12 etc.) that are NOT in Rewired's action list.
- `CommonUI.s_instance` may be null if accessed before the game scene loads. Always null-check.

---

## 11. Not Yet Analyzed (Tier 2 — fill before implementing each feature)

- [ ] PCBSInput default key assignments (run game and check KeyBindingMenu)
- [ ] Assembly/build interaction classes (ComponentPC, WorkStation, Case)
- [ ] Inventory panel internals (InventoryPanelSwitcher)
- [ ] Shop/parts browser UI classes
- [ ] Job/email system (Job, JobDesc, EmailApp)
- [ ] WorldInteraction action display system
- [ ] OptionsMenu slider/toggle access

---

## Change History

- **2026-04-20:** Full Tier 1 analysis completed from Assembly-CSharp-firstpass decompiled source
