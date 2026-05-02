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

## 11. Tablet / OS App Classes (Phase 3B–4 Implementation Targets)

### ReviewApp (ReviewApp.cs)
- Displays company name, overall star rating, list of recent reviews
- **Key fields:** `m_companyName (Text)`, `m_nReviews (Text)`, `m_starRating (StarRating)`, `m_reviewContainer (Transform)`
- **Review items:** `ReviewEntry` children — `m_date (Text)`, `m_from (Text)`, `m_body (Text)`, `m_rating (StarRating)`
- **Best patch point:** `ReviewApp.Awake()` Postfix (all text fields set by end of Awake via `UpdateReviews()`)
- **Accessible data:** `CareerStatus.Get().GetBusinessName()`, `CareerStatus.Get().GetStarRating()`, `CareerStatus.Get().GetReviews()`
- **IReview interface:** `GetStars() (float)`, `GetDay() (int)`, `GetFrom() (string)`, `GetReview() (string)`
- **Navigation:** Up/Down through ReviewEntry children in m_reviewContainer; first item auto-announced on open
- **Announce:** company name, star rating, N reviews. Per entry: X of Y. From. Body. Date. Stars.

### RankApp (RankApp.cs)
- GPU/CPU performance ranking chart — shows rank, part name, score bar
- **Key fields:** `m_scrollRect (ScrollRect)`, `m_search (InputField)`, `m_resultCount (Text)`, `m_rows (private List<RankAppRow>)`
- **Row class RankAppRow:** `m_rank (Text)`, `m_name (Text)`, `m_score (Text)` — all public fields
- **Heading class RankAppHeading:** `m_title (Text)` — used for "GPU" / "CPU" category labels
- **Best patch point:** `RankApp.Start()` Postfix — list is populated via `RefreshList()` at Start
- **Navigation:** rows are children of `m_scrollRect.content` — collect via `GetComponentsInChildren<RankAppRow>()`; headings via `GetComponentsInChildren<RankAppHeading>()`
- **Announce:** "Rank chart. X entries." then per row: "Rank N. Name. Score S."
- **Key binding note:** Numpad2 assigned for re-read

### VirusScanApp (VirusScanApp.cs)
- Simulated virus scan — states: STANDARD, IN_PROGRESS, DIRTY, CLEAN
- **Key fields (all public):** `m_title (Text)`, `m_message (Text)`, `m_buttonText (Text)`, `m_button (Button)`, `m_progressBar (GameObject)`, `m_progress (Image)`
- **Button action:** `OnStart()` — starts scan (fires `StartScan()` which starts `ScanProcess()` coroutine)
- **Best patch points:**
  - `VirusScanApp.SetVisuals(State visuals)` Postfix — private, fires on every state change; `__0` parameter = State enum (int: 0=STANDARD, 1=IN_PROGRESS, 2=DIRTY, 3=CLEAN)
  - OR poll `m_title.text` on open to read current state
- **Announce on open:** read `m_title.text + ". " + m_message.text`. On Start click: "Scanning..." On complete DIRTY: title + message. On CLEAN: "No viruses found."
- **Enter binding:** invoke `m_button.onClick` when button is active

### MarketApp (MarketApp.cs)
- Price graph for part categories over time — visual only, not interactive
- **Key fields:** `m_min (Text)`, `m_max (Text)`, `m_today (Text)`, `m_start (Text)`, `m_keyEntries (List<Toggle>)`
- **Key entries:** each `Toggle` has a child `Text` with `e.m_uiName` (category name) and `toggle.isOn` (visible/hidden)
- **Best patch point:** `MarketApp.Start()` Postfix — all data initialized; `UpdateGraph()` is private
- **Accessible data:** `PartsDatabase.MarketCategories()` returns all entries; `CareerStatus.Get().GetMarketValue(key, day)` gives today's price
- **Announce:** read today's date from `m_today.text`; for each visible (isOn) toggle: category name + current market value + trend (compare today vs yesterday)
- **No navigation needed** — read-only data display; one-shot announcement on open

### LightingApp (LightingApp.cs)
- LED color/effect configurator — complex multi-component editor
- **Key fields:** `m_lightList (ScrollRect)`, `m_r/g/b (InputField)`, `m_effect (Dropdown)`, `m_speed/m_offset (Slider)`
- **Row class LightingRow:** `m_name (Text)`, `m_toggle (Toggle)`, `m_colour (Image)` — Init sets `m_name.text = id + " " + index`
- **Best patch point:** `LightingApp.Start()` Postfix — light list built at Start
- **Navigation:** Up/Down through LightingRow children of `m_lightList.content`
- **Select action:** simulate toggle via `row.m_toggle.isOn = !row.m_toggle.isOn`; invokes `app.OnSelectionChanged()`
- **Apply:** `OnApply()` — public method on LightingApp
- **Announce:** "Lighting. N LEDs. Up Down to navigate." Per row: "X of Y. [name]. [RGB values]."
- **Complexity note:** Color picker is purely visual (HSV picker image); only expose RGB text fields + effect dropdown + sliders

### MusicPlayerApp (MusicPlayerApp.cs)
- In-game music player for soundtrack / user files / internet radio
- **Key fields:** `m_trackList (ScrollRect)`, `m_trackName (Text)`, `m_play/m_pause (Button)`, `m_shuffle/m_loop/m_mute (Toggle)`, `m_volume (Slider)`
- **Row prefab:** `MusicPlayerTrackRow` — has track number + name
- **Best patch points:**
  - `MusicPlayerApp.PlayTrack(int i, string name)` — private, fires when track changes. `m_currentTrack` field has formatted "Track N: name"
  - `MusicManager.m_onPlay` event (Action<int, string>) — fired by MusicManager directly
- **Controls:** OnPlayPause(), OnPrev(), OnNext() — all public
- **Announce:** on track change: "Now playing: [track name]." Play/pause/prev/next via hotkeys mapped

### OS Desktop (OS.cs)
- Virtual PC operating system — desktop icons + taskbar + window management
- **Key fields:** `m_icons (private List<ProgramIcon>)`, `m_taskBar (TaskBar)`, `m_startMenu (private StartMenu)`
- **ProgramIcon:** `m_text (Text)` = app name; `m_onClick (private Action)` = custom action or launch; `m_desc (private OSProgramDesc)` = program descriptor
- **OSProgramDesc:** `m_id (string)`, `m_uiName (string, computed)` = localized name via `("OSProgramName/" + m_id).Localized()`
- **Best patch point:** `OS.OnStartup(ComputerSave computer)` Postfix — desktop icons built here; announces "Desktop. N apps."
- **Desktop navigation:** Up/Down through `m_icons` list; Enter = double-click via `GetComponentInParent<OS>().Launch(m_desc)` or invoke `m_onClick`
- **Note:** `ProgramIcon.OnClick()` requires TWO clicks (double-click pattern: first click sets highlight, second within 0.5s launches). Skip this — call `Launch()` directly.
- **Window management:** `OS.Launch(OSProgramDesc desc)` opens app. `m_taskBar.m_running` = open windows (via TaskBarItem list — private).
- **TaskBar:** `m_time (Text)` — clock. `TaskBarItem`: `m_name (Text)`, `GetWindow() (WindowFrame)`
- **Start menu:** opened via `OS.OnOpenStartMenu()`. Items are `StartMenuItem` children: `m_name (Text)`, `OnClick()` launches app.
- **Events:** `OS.m_gainFocus` (Action) — fires when OS gains focus

### BIOS (Bios.cs)
- System BIOS — System/CPU/RAM/Settings tabs with adjustable settings
- **Key fields:** `m_tabContainer (GameObject)`, `m_settingsContainer (GameObject)`, `m_help (Text)`, `m_date (Text)`, `m_time (Text)`, `m_prompt (GameObject)`, `m_promptText (Text)`, `m_promptYes/No (Button)`
- **Setting class BiosSetting:** `m_name (Text)`, `m_fixedValue (Text)`, `m_value (Text)`, `m_plus/m_minus (Button)` (adjust), `m_click (Button)` (action items)
- **Best patch point:** `Bios.OnEnable()` Postfix — BIOS opened; `OnTab(int tab)` private — tab switched
- **Tabs:** 0=System, 1=CPU (if overclockable), 2=RAM (if RAM installed), 3=Settings
- **Tab buttons:** children of `m_tabContainer` — collect via `GetComponentsInChildren<Button>()`
- **Settings:** children of `m_settingsContainer` — collect via `GetComponentsInChildren<BiosSetting>()`
- **BiosSetting fields:** `m_name.text` = localized setting name; `m_fixedValue.text` or `m_value.text` = current value; `m_plus/m_minus` visible = adjustable; `m_click` visible = clickable action
- **Navigation:** Up/Down through BiosSetting rows; Left/Right on adjustable rows = m_plus/m_minus click; Enter on action rows = m_click click
- **Input capture note:** BIOS uses `PCBSInput.m_increaseBios`/`m_decreaseBios` (Rewired held actions) — we cannot capture those. Only handle Up/Down/Enter/Left/Right from our mod keys.
- **Confirm prompt:** `m_prompt.activeSelf` = prompt open; Enter = `m_promptYes.onClick`, Escape = `m_promptNo.onClick`
- **Announce on open:** manufacturer + current tab name + N settings
- **Assign:** Numpad3 for re-read current setting

### WindowFrame (WindowFrame.cs)
- Generic OS window container for all tablet apps
- **Key fields:** `m_title (Text)`, `m_icon (Image)`, `IsMinimised/IsMaximised (bool props)`
- **Methods:** `OnTaskBar()` (toggle minimize), `OnClose()` → calls `OS.CloseWindow(this)`
- **Events:** `OnSizeChanged (Action)` — fires when window resized/minimized/maximized

### StartMenu (StartMenu.cs)
- OS start menu — list of installed programs + shutdown/restart
- **Items:** `StartMenuItem` children — `m_name (Text)`, `OnClick()` = launch
- **Actions:** `OnShutdown()`, `OnRestart()` — public methods
- **Patch point:** no dedicated open method; `StartMenu` instantiated by `OS.OnOpenStartMenu()`. Patch `OS.OnOpenStartMenu()`.

### PermissionDeniedApp (PermissionDeniedApp.cs)
- Shown when app cannot run on current PC (specs too low)
- Low priority — just announce "Application cannot run: specs too low."

---

## 12. Career Events (Static Delegates on CareerStatus)

**All confirmed from CareerStatus.cs decompile:**
- `CareerStatus.s_onLevelUp` — `Action<int>` — zero-indexed level (implemented)
- `CareerStatus.s_onNewReview` — `Action` — new review received (implemented)
- `CareerStatus.s_onDayEnd` — `Action` — day ended (day cycle complete)
- `CareerStatus.s_onStockChange` — `Action` — stock/inventory changed (shop restock)
- `CareerStatus.s_onInventoryUpdated` — `Action` — player inventory changed
- `CareerStatus.s_onBusinessNameChanged` — `Action` — business name changed
- `CareerStatus.s_updateCalenderEvent` — `Action` — calendar day updated
- NOTE: No s_onCashChange event exists. Cash changes must be detected via Harmony patch on `CareerStatus.AddCash(int amount)`.

**Useful CareerStatus getter methods:**
- `CareerStatus.Get().GetCash()` — int
- `CareerStatus.Get().GetKudos()` — int
- `CareerStatus.Get().GetStarRating()` — float
- `CareerStatus.Get().GetBusinessName()` — string
- `CareerStatus.Get().GetReviews()` — `List<IReview>`
- `CareerStatus.Get().GetMarketValue(string key, int day)` — float percentage value (100=normal)
- `CareerStatus.Get().GetMarketTrend(MarketEntry cat, int day)` — `ComponentMarket.TrendStrength`
- `CareerStatus.Get().GetTrends(int day)` — `IEnumerable<TrendStrength>` all categories
- `CareerStatus.Get().GetToday()` — int (day number)
- `CareerStatus.Get().GetCalendar()` — Calendar; `GetDateString(day, format)`

**Market trend system:**
- `ComponentMarket.Trend` enum: HighMarket, Rising, Recovering, LowMarket, Falling, Stable, Mixed
- `TrendStrength.m_trend` — the trend value
- `TrendStrength.m_cat.m_uiName` — category display name
- `PartsDatabase.MarketCategories()` — `IEnumerable<MarketEntry>` — all categories
- `MarketEntry`: `m_key (string)`, `m_uiName (string)`, `m_category (MarketCategory enum)`, `m_color (Color)`

---

## 13. Keyboard Shortcut Master Map

### Currently Assigned (Mod)
- F1: Career status / main menu help
- F2: Re-read tutorial task
- F3: Re-read current job
- F4 / Numpad0: Re-read workshop tooltip
- F5: Re-read inventory item
- F6: Re-read save slot
- F7: Cycle job objectives
- F8: Re-read HWInfo summary
- F9: Re-read OCCT sensors (if ran) / 3DMark score
- F10: Re-read PC Stats
- Numpad1: Re-read shop item
- Numpad5: Re-read Add Program item
- F12: Toggle debug mode

### Available for Next Features
- F11: OS desktop / BIOS summary re-read
- Numpad2: Re-read Rank chart item
- Numpad3: Re-read BIOS setting
- Numpad4: Re-read Review App entry
- Numpad6: Re-read Music Player track
- Numpad7: Re-read Lighting item
- Numpad8: Re-read Virus Scan status
- Numpad9: Re-read Market App summary

---

## 14. Debug / Accessibility Hooks (DebugVars.cs)

`DebugVars` is a static class with public fields that change game behaviour. These cannot be set from mod safely in release builds (game may not honour them), but they reveal what the game already supports internally:

- `DebugVars.s_fastWork (bool)` — screwing/installation animations run near-instantly; equivalent to ScrewUpgrade
- `DebugVars.s_installAllApps (bool)` — installs all OS apps automatically; skips AddProgramApp navigation entirely
- `DebugVars.s_showHiddenObjectives (bool)` — reveals hidden diagnostic objectives in JobStatus panel
- `DebugVars.s_showVisuals (bool)` — enables extra visual debug overlays

**Accessibility implication:** `s_showHiddenObjectives` could be exposed as a user-toggled accessibility option. If set to `true` before BIOS/job load, diagnostic objectives appear immediately without guessing. Check whether the game respects this field at runtime.

---

## 15. Upgrade Mechanics (InstallingPartState)

These upgrades are purchasable in-game and directly reduce motor precision requirements. Document them so handlers can detect when they are active:

- `PartDesc.AutoStandOffs` — standoffs auto-installed
- `PartDesc.ScrewUpgrade` — screws auto-fastened (no manual screwdriver steps)
- `PartDesc.AutoBuild` — parts snap without rotation alignment
- `PartDesc.InternalAutoConnect` — internal cables auto-connected
- `PartDesc.AutoConnect` / `PartDesc.QuickCable` — external cable auto-snap

**Check if upgrade active:**
```csharp
// CareerStatus tracks purchased upgrades
bool hasAutoConnect = CareerStatus.Get().HasUpgrade(PartDesc.AutoConnect);
```

---

## 16. Input System — CursorGravity and DirectUI

### CursorGravity (CursorGravity.cs)
- Magnetizes the software cursor toward clickable elements (IPointerClickHandler) when within radius
- Active in `InputModule` — part of the manette/gamepad cursor system
- `GravityPoint` component placed on buttons defines the exact snap target
- Mod should NOT interfere with CursorGravity — it already helps motor accessibility

### DirectUI (DirectUI.cs)
- Forces gamepad focus to first selectable element on panel activation
- `m_firstControl (GameObject)` — first element to select on open
- `m_rememberControl (bool)` — if true, returns to last-focused element when re-entering panel
- `DirectUISkipThis` — tag to exclude decorative elements from navigation loop
- `DirectUIAutoScrollOverride` — overrides auto-scroll target for a specific nav element
- Our handlers must be compatible: do NOT override `EventSystem.current.SetSelectedGameObject()` when DirectUI is already managing focus in a menu

### CustomNavigation (CustomNavigation.cs)
- Allows manual override of navigation order between UI elements
- `m_selectionPreference (List<GameObject>)` — explicit up/down/left/right targets
- Relevant when building handler nav loops: check if elements already have CustomNavigation before adding our own key handling

---

## 17. CalendarWidget API

- **Class:** `CalendarWidget`, `Calendar`, `CalendarDay`, `CalendarEvent`
- `CalendarWidget` — displays current month; prev/next month buttons
- `CalendarDay.m_today (bool)` — marks the current in-game day
- `CalendarDay` — each day cell; has list of `CalendarEvent` objects
- `CalendarEvent.GetDescription()` — returns localized event label ("Delivery", "Rent due", etc.)
- Event colours: `s_paymentCol`, `s_deliveryCol`, `s_eventCol` (static Color fields on CalendarEvent)
- `CareerStatus.Get().GetCalendar()` — returns `Calendar` instance
- `Calendar.GetDateString(int day, string format)` — formatted date string
- `CareerStatus.s_updateCalenderEvent` (Action) — fires on calendar update; subscribe here to detect day changes

---

## 11. Not Yet Analyzed (Tier 2 — fill before implementing each feature)

- [ ] PCBSInput default key assignments (run game and check KeyBindingMenu)
- [x] Assembly/build interaction classes — documented (WorkshopBuildHandler, PCBayHandler, HWInfoHandler)
- [x] Inventory panel internals — documented (InventoryHandler)
- [x] Shop/parts browser UI — documented (ShopHandler)
- [x] Job/email system — documented (CareerJobHandler, EmailAppHandler)
- [x] OptionsMenu slider/toggle access — documented (OptionsMenuHandler)
- [x] MarketApp / market categories — documented in sections 11 and 12
- [ ] CareerStatus.s_onCashChange — not confirmed to exist; use Harmony patch on AddCash() instead
- [x] DebugVars accessibility hooks — documented in section 14
- [x] CursorGravity / DirectUI — documented in section 16
- [x] CalendarWidget API — documented in section 17

---

## Change History

- **2026-04-20:** Full Tier 1 analysis completed from Assembly-CSharp-firstpass decompiled source
- **2026-04-24:** Phase 3B analysis — added sections 11–13: ReviewApp, RankApp, VirusScanApp, MarketApp, LightingApp, MusicPlayerApp, OS Desktop, BIOS, WindowFrame, StartMenu; career events; keyboard shortcut master map
- **2026-05-01:** Phase 3 complete analysis — added sections 14–17: DebugVars, upgrade mechanics, CursorGravity/DirectUI, CalendarWidget; coverage plan updated to reflect all 49 implemented features
