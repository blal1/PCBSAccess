# Project Status: PCBSAccess

## Project Info

- **Game:** PC Building Simulator
- **Developer:** The Irregular Corporation
- **Engine:** Unity 2018.4.16f1 (Mono runtime)
- **Architecture:** 64-bit
- **Mod Loader:** BepInEx 5.x x64
- **Runtime:** net472
- **Game directory:** C:\Users\bilal\Downloads\PC Building Simulator
- **Decompiled source:** PCBS_Data\Managed\Assembly-CSharp-firstpass\ (main game code)
- **User experience level:** Little/None
- **Languages:** English + French

## Current Phase

**Phase 3 — COMPLETE** (64 features, all tests passed 2026-05-01)
**Phase 4 — DLC** (71 features — 6 more added 2026-05-02; pending in-game tests with DLC)

Next step: Phase 4 DLC (optional — see below). Core game fully covered.

## Setup (all done)

- BepInEx 5.x x64 installed
- Tolk.dll + nvdaControllerClient64.dll in game root
- .NET SDK 10.0.201
- ILSpy CLI 10.0.0.8330
- Game code decompiled — PCBS_Data\Managed\
- Build script: scripts\Build-Mod.ps1

## Architecture Decisions

- **Decompiled source:** Assembly-CSharp-firstpass\ — NOT Assembly-CSharp\ (shaders only)
- **State management:** InputRouter + IInputContext stack. Handlers push/pop context; topmost handles keys.
- **Reflection:** 1,119 [SerializeField] fields across 228 files — almost all UI fields private. ReflectionHelper used throughout.
- **Text components:** Mixed `UnityEngine.UI.Text` and `TextMeshProUGUI`. Check using statements per class.
- **Input:** Rewired for game actions. `Input.GetKeyDown(KeyCode.*)` for mod hotkeys only.
- **Localization:** I2 Localization. `LocalizationManager.CurrentLanguage` → "English" or "French".

## Key Bindings (Mod Hotkeys)

- S: Career status (cash, kudos, rating, date, upcoming events)
- T: Re-read tutorial task
- J: Re-read current job
- W: Re-read last workshop tooltip
- I: Re-read current inventory item
- F: Re-read current save slot
- N: Cycle job objectives
- H: Re-read HWInfo summary
- B: Re-read benchmark (OCCT priority over 3DMark)
- P: Re-read PC stats
- E: Re-read current email
- G: Re-read current shop item
- Numpad7: Read live HW Monitor sensors
- F1: Context help
- F11: List open OS windows
- F12: Toggle debug mode

## Implemented Features (64 total, all passed)

### Framework (Phase 2)
- ScreenReader (Tolk), DebugLogger, Loc (EN+FR), InputRouter, IInputContext, IHelpContext, ReflectionHelper, ButtonHelper, Main entry point, Build-Mod.ps1

### Phase 3 Handlers

| Handler | What it does |
|---------|-------------|
| HowToBuildAPCHandler | Tutorial popups, checklist, T re-read |
| MainMenuHandler | Button nav (Up/Down/Enter) |
| OptionsMenuHandler | Tab nav, Left/Right adjust, Enter toggle |
| CareerJobHandler | Email inbox nav, Enter accept/collect, J re-read |
| WorkshopBuildHandler | Tooltip announce, mode switch announce, W re-read |
| InventoryHandler | Item nav, category switch, specs announce, I re-read |
| SaveLoadMenuHandler | Save slot nav, F re-read |
| JobStatusHandler | Objective panel, N cycles, met/not-met state |
| ShopHandler | Item nav, detail, checkout cart, G re-read, Numpad1 re-read |
| InGameMenuHandler | Pause menu nav |
| MessageBoxHandler | Dialog title+body, Enter/Escape |
| PartInstallHandler | "Installed/Removed: [name]" on state transitions |
| DeliveryHandler | Delivery arrival poll, manifest collect announce |
| DaySummaryHandler | Date+cash+kudos on open, button nav |
| JobResultHandler | Result screen: from/subject/stars/review, Enter dismiss |
| WorkshopSelectionHandler | Workshop carousel nav |
| PCPowerHandler | Power on/off events |
| WillItRunHandler | Program list nav, detail with 5 categories |
| PCBayHandler | Buy/sell tab, item nav, auction action, Enter |
| HWInfoHandler | All rows on open, H re-read |
| EmailAppHandler | Inbox nav, full body on select, E re-read |
| CareerEventHandler | Level up, new review, inventory updated events |
| ThreedMarkHandler | Started/results announce, B re-read |
| OCCTHandler | Start/stop/finish sensors, B re-read |
| PCStatsHandler | Build summary rows, Enter dismiss, P re-read |
| ObjectiveHandler | Auto-announce on objective completion |
| AddProgramHandler | Program list nav, install progress, restart prompt |
| ReviewAppHandler | Company/rating/top-3 reviews on open |
| RankAppHandler | Rank list nav, entry announce |
| VirusScanAppHandler | State changes, Enter to scan/dismiss, F1 re-read |
| MarketAppHandler | All categories + day values on open |
| OSHandler | Desktop boot, launch, start menu, F11 open windows |
| BiosHandler | Tab cycle, setting nav, value adjust, confirm prompt |
| MusicPlayerAppHandler | Track list nav, play/pause/track announce |
| LightingAppHandler | LED group nav, Space toggle, apply confirm |
| AchievementHandler | Unlock announce by name |
| WalkingStateHandler | "Entered/Left workshop." on state transitions |
| RenameCompanyHandler | Current name on open, new name on apply |
| NotesAppHandler | Compact view: Left/Right note nav, title+body, F1 re-read |
| SelectWallpaperHandler | Tab announce, apply confirm |
| FreebuildOptionsHandler | Upgrade list nav, Space toggle, name+state announce |
| WorkshopNavigatorHandler | Tab targets, Space hold actions, orientation, HBPC mode |
| WorkingOnComputerStateHandler | "Working on [PC]." / "Left PC." |
| PCEmergencyShutdownHandler | BSOD reason announce (6 types) |
| CalendarHandler | Day events on SetFocusDay, 1.5s startup cooldown |
| WindowFocusHandler | OS window focus via taskbar (1s dedup) |
| GPUTunerHandler | Up/Down params, Left/Right adjust, Enter apply, R reset |
| HWMonitorHandler | Sensor cache on Start, Numpad7 live read |
| ManifestHandler | Delivery manifest item nav (Up/Down/Home/End) |
| KeyBindingMenuHandler | Binding list nav, Enter redefine, re-read on assign |
| WhatsNewPopUpHandler | Popup read on OnEnable, Enter/Escape dismiss |
| SearchFiltersPanelHandler | Filter nav, Space toggle/expand, active count update |
| SwapPeripheralStateHandler | Peripheral slot type + current peripheral announce |
| Career status (Main) | S key: cash+kudos+rating+date+upcoming events |

## Phase 4 — DLC (Optional)

| DLC | Status | Notes |
|-----|--------|-------|
| DLC-1: Esports emails | ✓ Covered | EsportsEmail appears in standard inbox — EmailAppHandler handles it |
| DLC-2: IT Support TikkIT tablet | ✓ TikkITHandler | Board nav (Left/Right cols, Up/Down cards), popup detail, positive/negative actions, new job announce |
| IT Support email body | ✓ EmailAppHandler enhanced | ITSupportEmailRowJob detected; reads staff name, primary info, secondary info, deadline, notes via Reflection |
| IT Support level on S key | ✓ Main.AnnounceCareerStatus | LevelProgression.GetZeroIndexedLevel → "Level: N." appended when in IT Support / FuturLab DLC mode |
| Esports Messenger | ✓ MessengerHandler | SetConversationID Postfix reads last 5 messages; OnBackButtonPressed announces list count + unread; Tick detects new incoming messages |
| IT Support lift floors | ✓ ITSupportLiftHandler | MoveToNewFloor Prefix reads OfficeFloor lookup → localized name via ITSupportFloors; falls back to formatted enum name; ExitLiftControllerState says "Arrived." |
| Esports LikedIn team selection | ✓ LikedInHandler | OnStartShowing reads league likes + all team entries (name, likes required, message); TeamHasBeenChosen announces chosen team |
| DLC tablet shops | ✓ TabletShopHandler | OnAppOpened/OnAppClosed + PollState; Up/Down/Home/End nav through m_stock items; each item: name, price, stock count, availability |
| DLC-3: Water cooling piping | ✓ Covered | WorkshopNavigatorHandler already handles Piping mode |

## Known Issues

None.

## Pending Tests

All tests passed 2026-05-01.

## References

- `docs/game-api.md` — class names, method names, field names, singleton access
- `docs/accessibility-coverage-plan.md` — full feature inventory with tier/priority
- `docs/ACCESSIBILITY_MODDING_GUIDE.md` — code patterns
- `docs/technical-reference.md` — BepInEx, Harmony, Tolk
- `docs/unity-reflection-guide.md` — Reflection patterns
- `docs/localization-guide.md` — Loc.cs usage
- `docs/menu-accessibility-patterns.md` — IInputContext patterns
- `scripts/Build-Mod.ps1` — build + deploy
