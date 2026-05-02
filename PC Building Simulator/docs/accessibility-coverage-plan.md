# PCBSAccess — Full Accessibility Coverage Plan

**Game:** PC Building Simulator  
**Mod:** PCBSAccess (BepInEx 5.x, net472, Unity 2018.4.16f1)  
**Date:** 2026-04-23  
**Purpose:** Complete inventory of every screen, app, and mechanic requiring accessibility work,
with designed keyboard shortcuts, patch points, and implementation priority.

---

## Keyboard Shortcut Master Map

### All hotkeys (implemented, all tests passed 2026-05-01)

| Key | Action | Handler |
|-----|--------|---------|
| S | Career status (cash, kudos, rating, date, upcoming events) | Main |
| T | Re-read current tutorial task | HowToBuildAPCHandler |
| J | Re-read current job email | CareerJobHandler |
| W | Re-read last workshop tooltip | WorkshopBuildHandler |
| I | Re-read current inventory item | InventoryHandler |
| F | Re-read current save slot | SaveLoadMenuHandler |
| N | Cycle job objectives | JobStatusHandler |
| H | Re-read hardware info summary | HWInfoHandler |
| E | Re-read current email | EmailAppHandler |
| G | Re-read current shop item | ShopHandler |
| B | Re-read last benchmark / OCCT result | ThreedMarkHandler / OCCTHandler |
| P | Re-read last PC stats summary | PCStatsHandler |
| F1 | Context help / main menu help | InputRouter / MainMenuHandler |
| F11 | List open OS windows | OSHandler |
| F12 | Toggle debug mode | Main |
| Numpad7 | Read live HW Monitor sensors | HWMonitorHandler |
| Up / Down | Navigate lists | Most handlers |
| Home / End | First / last item | Most handlers |
| Left / Right | Notes: prev/next note; GPU Tuner: adjust param | NotesAppHandler / GPUTunerHandler |
| Enter | Activate / select | Most handlers |
| Space | Toggle (filters, freebuild upgrades, lighting) | SearchFiltersPanelHandler etc. |
| Tab / Shift+Tab | Options menu navigation | OptionsMenuHandler |

**Conflict notes:**
- Arrow keys are Rewired "X"/"Y" actions — rebindable. In BIOS, game uses
  `PCBSInput.m_increaseBios` / `PCBSInput.m_decreaseBios`. Do NOT intercept
  these in BiosHandler; only READ what the game changes by patching value-change
  methods (Postfix). No custom nav inside BIOS — the game's own input handles it.
- Tab is `PCBSInput.m_inventory` — rebindable. Safe in menus only (OptionsMenuHandler
  already uses Tab; it works because OptionsMenu disables game input while open).
- F9, F10, F11 are not used by the game; safe for mod.
- Numpad2–9 not used by the game; safe for mod.

---

## Feature Coverage Table

### Legend
- **Status:** Done ✓ | In progress ~ | Not started ✗
- **Priority:** P1 = critical (play-blocking) | P2 = high | P3 = medium | P4 = low
- **Patch type:** Postfix | Prefix | Poll (activeSelf) | Event (static delegate)

---

## TIER 1 — Virtual PC OS Layer

### OS-1: OS Desktop & Program Launcher
- **Class:** `OS`
- **Status:** ✓ (Feature 39 — OSHandler; all tests passed 2026-05-01)
- **Priority:** P1
- **What to announce:**
  - OS startup: "Operating system started."
  - OS shutdown: "Operating system shut down."
  - Program launched: "[Name] opened."
  - Program closed: "[Name] closed."
  - New email badge: "New email received."
  - Start menu opened: "Start menu. [count] programs. Up Down to navigate, Enter to open."
  - Start menu item focused: "[program name]."
- **Patch points:**
  - `OS.Launch(OSProgramDesc)` Postfix → announce program name
  - `OS.CloseWindow(WindowFrame)` Postfix → announce close
  - `OS.OnOpenStartMenu()` Postfix → announce start menu + items
  - `OS.OnStartup(ComputerSave)` Postfix → "OS started"
  - `OS.OnShutdown()` Postfix → "OS shut down"
  - Private `OnNewEmail()` Postfix → "New email received"
- **Navigation:** Up/Down in start menu icon list (if grid); Enter to launch
- **F-key:** F11 → "Running: [list of open programs]."
- **Key fields:** `m_icons` (program grid), `m_startMenu` (start menu panel), `m_taskBar`

### OS-2: TaskBar / Window Focus
- **Class:** `TaskBar`, `WindowFrame`
- **Status:** ✗
- **Priority:** P2
- **What to announce:**
  - Window title when app gains focus (already handled per-app via OnGainFocus patches)
  - Taskbar item list on F11 (part of OSHandler)
- **Patch points:**
  - `WindowFrame.Init(OSProgramDesc, bool)` Postfix → store program name for later use
  - `OS.BringToFront(WindowFrame)` Postfix → announce "[Name] focused"
- **Key fields:** `WindowFrame.GetProgramDesc().m_uiName`

---

## TIER 2 — Virtual PC Apps (Tablet OS)

### APP-1: ThreedMarkApp (3DMark Benchmark)
- **Class:** `ThreedMarkApp`
- **Status:** ✓ (Feature 30 — ThreedMarkHandler; all tests passed 2026-05-01)
- **Priority:** P2
- **What to announce:**
  - "3DMark benchmark started."
  - During benchmark: phase changes ("CPU test", "GPU test", "Combined test").
  - Results: "CPU score: [X]. GPU score: [Y]. Overall score: [Z]."
- **Patch points:**
  - `ThreedMarkApp.OnRun()` Postfix → "Benchmark started."
  - `ThreedMarkApp.CalcResults()` Postfix → read `m_gpuScore.text`, `m_cpuScore.text`, `m_score[].text`
  - `ThreedMarkApp.PlayVideo()` Postfix → announce phase (read visible page heading if any)
- **F-key:** F9 (context: ThreedMark active) → re-read last score
- **Key fields:** `m_score[]`, `m_gpuScore`, `m_cpuScore`, `m_startPage`, `m_benchmarkingPage`, `m_resultsPage`

### APP-2: OCCTApp (Stress Test)
- **Class:** `OCCTApp`
- **Status:** ✓ (Feature 31 — OCCTHandler; all tests passed 2026-05-01)
- **Priority:** P2
- **What to announce:**
  - "OCCT stress test started."
  - "OCCT test stopped."
  - On sensor tick: CPU temp, GPU temp, wattage (announced once per significant change, not every second).
  - Throttle detected: "CPU throttling detected."
- **Patch points:**
  - `OCCTApp.OnOn()` Postfix → "Stress test started."
  - `OCCTApp.OnOff()` Postfix → "Stress test stopped."
  - `OCCTApp.SetLoad(bool)` Postfix → if load = false and was throttling, announce
  - Poll sensor rows for CPU throttle text change (or patch the Func<string> callback by reading `m_status.text`)
- **F-key:** F9 (context: OCCT active) → re-read current status + sensor values
- **Key fields:** `m_status`, `m_duration`, sensor timing rows

### APP-3: AddProgramApp (Software Manager)
- **Class:** `AddProgramApp`
- **Status:** ✓ (Feature 34 — AddProgramHandler; all tests passed)
- **Priority:** P2
- **What to announce:**
  - Add/Remove tab active: "[count] programs. Up Down to navigate."
  - Program selected: "[program name]. [installed/not installed]."
  - Installing: "Installing [name]…"
  - Progress: "[N]% complete." (poll)
  - Done: "Installation complete. Restart required." / "Removed [name]."
  - Error: message text announced
- **Patch points:**
  - `AddProgramApp.SetMode(bool add)` Postfix → announce mode + populate list
  - `AddProgramApp.UpdateProgramList()` Postfix → collect items, announce count
  - `AddProgramApp.ShowProgressDialog()` Postfix → "Installing [name]…"
  - Poll progress bar text or percentage
  - `AddProgramApp.OnRestartYes()` / `OnRestartNo()` Postfix → announce result
- **Navigation:** Up/Down in program list; Enter to install/remove
- **F-key:** Numpad5 → re-read current program item
- **Key fields:** `m_message`, `m_actionName`, `m_installProgramName`, program list ScrollRect

### APP-4: ReviewApp (Customer Reviews)
- **Class:** `ReviewApp`
- **Status:** ✓ (Feature 35 — ReviewAppHandler; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:**
  - On open: "Reviews. [star rating] stars. [N] reviews. Up Down to navigate."
  - Each review: "[reviewer name]. [N] stars. [review text]."
  - New review event (static): "New review received. [N] stars."
- **Patch points:**
  - `ReviewApp.UpdateReviews()` Postfix → announce rating + count + first review
  - `CareerStatus.s_onNewReview` subscribe in Main → "New review. [rating] stars."
  - Navigation: collect review GameObjects from `m_reviewContainer`
- **Navigation:** Up/Down through review list
- **F-key:** Numpad2 → re-read current review
- **Key fields:** `m_companyName`, `m_nReviews`, `m_starRating`, `m_reviewContainer`

### APP-5: RankApp (Part Rankings)
- **Class:** `RankApp`
- **Status:** ✓ (Feature 36 — RankAppHandler; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:**
  - On open: "Part rankings. [category]."
  - Entry: "[rank]. [part name]. Quality: [N]%."
  - Search results: "[N] results for [term]."
- **Patch points:**
  - `RankApp.RefreshList()` Postfix → announce count + first entry
  - `RankApp.SelectResult(int)` Postfix → announce the selected rank entry
  - `RankApp.UpdateSeach(string)` Postfix → announce result count
- **Navigation:** Up/Down through rank list entries
- **F-key:** Numpad6 → re-read current rank entry
- **Key fields:** `m_resultCount`, category headers, `RankAppRow` items in scroll view

### APP-6: LightingApp (RGB/LED)
- **Class:** `LightingApp`
- **Status:** ✓ (Feature 42 — LightingAppHandler; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:**
  - On open: "Lighting. [N] components. Up Down to navigate."
  - Component selected: "[component name]. Color: [R,G,B]. Effect: [effect name]."
  - RGB changed: "Red [N]. Green [N]. Blue [N]."
  - Effect changed: "[effect name]."
  - Applied: "Lighting applied."
- **Patch points:**
  - `LightingApp.UpdateLightList()` Postfix → collect light rows, announce count
  - `LightingApp.OnSelectionChanged()` Postfix → announce selected component + color
  - `LightingApp.OnApply()` Postfix → "Lighting applied."
  - Patch RGB input field onChange or `OnApply` to read current values
- **Navigation:** Up/Down through light component list
- **Key fields:** `m_r`, `m_g`, `m_b` (InputField or Text), effect dropdown, speed/offset sliders

### APP-7: VirusScanApp (Antivirus)
- **Class:** `VirusScanApp`
- **Status:** ✓ (Feature 37 — VirusScanAppHandler; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:**
  - On open: "Virus scanner. [clean/scan required]."
  - Scan start: "Scanning…"
  - Progress (poll): "[N]% — [dirty count] issues found." (throttled, not every frame)
  - Complete: "Scan complete. Clean." or "Scan complete. [N] viruses found."
- **Patch points:**
  - `VirusScanApp.OnStart()` Postfix → "Scanning…"
  - `VirusScanApp.SetClean()` Postfix → "Scan complete. Clean."
  - Poll `m_message.text` for state transitions
- **Key fields:** `m_title`, `m_message`, `m_buttonText`, progress bar

### APP-8: MusicPlayerApp
- **Class:** `MusicPlayerApp`
- **Status:** ✓ (Feature 41 — MusicPlayerAppHandler; all tests passed 2026-05-01)
- **Priority:** P4
- **What to announce:**
  - Track started: "Now playing: [track name]."
  - Paused: "Paused."
  - Skip: "[track name]."
  - Volume changed: "Volume [N]%."
  - Shuffle/Loop/Mute toggled: "Shuffle on/off.", etc.
  - Tab switched: "[Soundtrack / User Files / Internet]."
- **Patch points:**
  - `MusicPlayerApp.OnPlayPause()` Postfix → announce state
  - `MusicPlayerApp.OnNext()` / `OnPrev()` Postfix → announce new track name
  - `MusicPlayerApp.SetVolume()` Postfix → announce level
  - `MusicPlayerApp.Shuffle()` / `Loop()` / `Mute()` Postfix → toggle state
  - `MusicPlayerApp.Populate()` Postfix → announce list count
- **F-key:** Numpad3 → re-read track name + state
- **Key fields:** `m_trackName`

### APP-9: NotesApp
- **Class:** `NotesApp`, `NotesAppNote`, `NotesAppNoteEditor`
- **Status:** ✓ (Feature 46 — NotesAppHandler; all tests passed 2026-05-01)
- **Priority:** P4
- **What to announce:**
  - On open: "Notes. [N] notes."
  - Note selected: "[note title]. [note content]."
  - Edit mode: "Editing: [title]."
- **Patch points:**
  - `NotesApp.Start()` Postfix → announce count
  - Patch note click/selection to announce content
- **Key fields:** Note title text, note content text

### APP-10: SelectWallpaperApp
- **Class:** `SelectWallpaperApp`
- **Status:** ✓ (Feature 47 — SelectWallpaperHandler; all tests passed 2026-05-01)
- **Priority:** P4
- **What to announce:**
  - Tab selected: "[Client/Custom/Workshop] wallpapers. [N] available."
  - Item selected: "[wallpaper name / file path]."
  - Applied: "Wallpaper applied."
- **Patch points:**
  - Tab selection methods Postfix → announce tab name + count
  - `SelectWallpaperApp.OnApplyCustomWallpaper()` Postfix → "Applied."
- **Key fields:** `m_currentFilePath`, wallpaper item lists

### APP-11: MarketApp (Part Price Graph)
- **Class:** `MarketApp`
- **Status:** ✓ (Feature 38 — MarketAppHandler; all tests passed 2026-05-01)
- **Priority:** P3
- **What announces:** All market categories + today's percentage value (one-shot on open)
- **Patch point:** `MarketApp.Start()` Postfix
- **Key fields:** `m_today (Text)`, `m_keyEntries (List<Toggle>)` — each Toggle child Text = category name
- **No navigation** — read-only data display

### APP-12: HWMonitor (Real-Time Hardware Monitor)
- **Class:** `HWMonitor`
- **Status:** ✓ (Feature 54 — HWMonitorHandler; Numpad7 reads all live sensors on demand)
- **Priority:** P3
- **Note:** Covered — HWMonitor.Start Postfix caches HWInfoRow list. Numpad7 announces live CPU/GPU temps and wattage from the dynamic Text fields.

---

## TIER 3 — BIOS

### BIOS-1: Bios Main Interface
- **Class:** `Bios`
- **Status:** ✓ (Feature 40 — BiosHandler; all tests passed 2026-05-01)
- **Priority:** P1
- **What to announce:**
  - Entry: "BIOS. Press Tab to switch tabs. Up Down to navigate settings."
  - Tab selected: "[System / CPU / RAM / Settings] tab."
  - Setting focused (navigating): "[setting name]. [current value]."
  - Setting help text: announce `m_help.text` (delayed, after navigation settles)
  - Value changed: "[setting name]: [new value]." (patch value-change methods)
  - Confirmation prompt: "[prompt text]. Enter for Yes, Escape for No."
  - Exit: "BIOS closed."
- **BIOS navigates with:** `PCBSInput.m_increaseBios` / `m_decreaseBios` (game input,
  not captured by mod). Mod patches the RESULT of changes (Postfix on value setters).
- **Patch points:**
  - `Bios.OnEnable()` Postfix → "BIOS. [tab count] tabs."
  - Tab button click handlers Postfix → announce tab name
  - `BiosSetting` value-change methods Postfix → announce new value
  - `Bios` prompt show Postfix → announce prompt text + hint
  - `Bios.OnPromptYes()` / `OnPromptNo()` Postfix → confirm/cancel message
- **F-key:** F9 → re-read current BIOS setting name + value
- **Key fields:** `m_help`, `m_date`, `m_time`, `m_promptText`, `BiosSetting` rows

### BIOS-2: BiosConfig Changes (Overclocking)
- **Class:** `BiosConfig`
- **Status:** ✗
- **Priority:** P2
- **What to announce:**
  - CPU clock: "CPU: [speed] GHz."
  - RAM clock: "RAM: [speed] MHz."
  - GPU clock: "GPU core: [speed] MHz. GPU memory: [speed] MHz."
  - XMP toggle: "XMP on/off."
- **Patch points:** Setters on `BiosConfig` fields (if any) or read from BiosSetting change events

---

## TIER 4 — Workshop / Career UI Panels

### WS-1: PCStats (Build Summary)
- **Class:** `PCStats`
- **Status:** ✓ (Feature 32 — PCStatsHandler; all tests passed 2026-05-01)
- **Priority:** P2
- **What to announce:**
  - On open: "[Job title / New Build case name]. Status: [status text]."
  - Benchmark: "Benchmark: [score]." or "Not benchmarked."
  - Value: "Invoice: [amount]." or "Resale value: [amount]."
  - Part list (queued): each part name + value.
  - Dismiss: Enter → `OnOK()`
- **Patch points:**
  - `PCStats.Init(ComputerSave)` Postfix → announce title + status + benchmark + value
  - Navigate part list with Up/Down
  - Poll `activeSelf` for close detection
- **F-key:** F10 → re-read build summary
- **Key fields:** `m_title`, `m_status`, `m_benchmark`, `m_value`, part list rows

### WS-2: DaySummary Extended (Financial Detail)
- **Class:** `DaySummary`
- **Status:** ✓ (basic: date + cash + kudos + buttons)
- **Priority:** P2 (enhancement)
- **Gap:** Current DaySummaryHandler announces cash total but NOT the
  line-by-line breakdown (revenue from jobs, expenses, profit/loss).
- **Enhancement:** After opening announcement, queue each accounting line:
  "Job income: [amount].", "Rent: [amount].", "Net: [amount]."
- **Patch points:** Read child `Text` rows inside the day summary panel after `activeSelf` true

### WS-3: Manifest (Delivery Tracking)
- **Class:** `Manifest`
- **Status:** ✓ (DeliveryHandler announces item count + item names on collect)
- **Priority:** P3 (enhancement)
- **Gap:** No Up/Down navigation through manifest items if user wants to review them.
- **Enhancement:** Track `Manifest` instance; add Up/Down to review individual manifest rows.

### WS-4: RenameCompany Dialog
- **Class:** `RenameCompany`
- **Status:** ✓ (Feature 45 — RenameCompanyHandler; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:**
  - On open: "Enter company name. Current: [name]."
  - On confirm: "Company name set to [name]."
- **Patch points:**
  - `RenameCompany` show Postfix (or poll `activeSelf`) → announce prompt + current name
  - `OnOK()` / confirm Postfix → announce new name
- **Key fields:** InputField for name entry, current name Text

### WS-5: FreebuildOptions
- **Class:** `FreebuildOptions`
- **Status:** ✓ (Feature 48 — FreebuildOptionsHandler; all tests passed 2026-05-01)
- **Priority:** P4
- **What to announce:** Options dialog toggles and values on open.
- **Patch points:** `FreebuildOptions` Init Postfix; toggle change handlers.

### WS-6: CalendarWidget (Workshop Calendar)
- **Class:** `CalendarWidget`, `Calendar`, `CalendarEvent`
- **Status:** ✗
- **Priority:** P4
- **What to announce:**
  - On open: current month + year
  - Day focused: date + events on that day (delivery due, rent due, etc.)
  - Month changed: new month + year
- **Notes from Gemini analysis:**
  - `CalendarEvent.GetDescription()` returns localized text ("Delivery", "Rent", etc.)
  - Events are colour-coded (s_paymentCol, s_deliveryCol) but text alternatives exist via GetDescription()
  - Current day has its own highlight (`m_today`)
- **Patch points:** `CalendarWidget` month-change Postfix; calendar day hover/select Postfix
- **Key fields:** `CalendarDay.m_today (bool)`, `CalendarEvent.GetDescription()`

### WS-7: PCTuner / GPUTuner (Overclocking Interface)
- **Class:** `PCTuner`, `GPUTuner`, `GPUTunerParam`
- **Status:** ✓ (Feature 53 — GPUTunerHandler covers in-game GPU Tuner app; PCTuner is a debug IMGUI overlay not accessible to players)
- **Priority:** P3
- **Note:** GPUTunerHandler patches GPUTuner.Start/OnApply/OnReset. Up/Down cycles 3 params, Left/Right adjusts, Enter applies, R resets. PCTuner is debug-only IMGUI — no player-facing UI to make accessible.
  - Units displayed with explicit labels (Hz, %) in separate Text components
- **Patch points:** `GPUTuner` param value-change Postfix; `AutoTuner` completion Postfix
- **Key fields:** `GPUTunerParam` rows, reset button, auto-tune button

---

## TIER 5 — Game Events (Non-UI)

### EVT-1: Level Up
- **Source:** `CareerStatus.s_onLevelUp(int level)` static event
- **Status:** ✓ (Feature 28 — CareerEventHandler; all tests passed 2026-05-01)
- **Priority:** P2
- **What to announce:** "Level up! Now level [N]."
- **Implementation:** Subscribe in `Main.Awake()`, announce via ScreenReader.

### EVT-2: New Review
- **Source:** `CareerStatus.s_onNewReview` static event
- **Status:** ✓ (Feature 29 — CareerEventHandler; all tests passed 2026-05-01)
- **Priority:** P2
- **What to announce:** "New review received. Rating: [N] stars."
- **Implementation:** Subscribe in `Main.Awake()`, read latest review via `CareerStatus.Get().GetReviews()`.

### EVT-3: Inventory Updated
- **Source:** `CareerStatus.s_onInventoryUpdated` static event
- **Status:** ✓ (Feature 49 — CareerEventHandler EVT-3; 2s debounce; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:** "Inventory updated. [N] items." (rate-limited — not every tiny change)
- **Implementation:** Subscribe + debounce (only announce if ≥1s since last fire).

### EVT-4: Day End Accounting
- **Source:** `CareerStatus.s_onDayEnd` static event
- **Status:** ✓ (partially — DaySummaryHandler handles the UI panel)
- **Gap:** The event fires before the UI opens; no gap issue.

### EVT-5: Achievement Unlocked
- **Source:** `Achievements.cs` — find the unlock callback
- **Status:** ✓ (Feature 43 — AchievementHandler; Prefix+Postfix on Achievements.SetProgress; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:** "Achievement unlocked: [achievement name]."
- **Implementation:** Patch `Achievements` unlock method Postfix.

---

## TIER 6 — Interaction States (In-World 3D)

### ST-1: Working On Computer (Building Mode)
- **Class:** `WorkingOnComputerState`
- **Status:** ✓ (WorkshopBuildHandler handles mode changes + tooltip announcements)
- **Gap:** No announcement when entering or leaving the work state.
- **Enhancement:** Patch `WorkingOnComputerState.OnEnter()` → "Working on [PC name]."
- **Patch point:** `WorkingOnComputerState.OnEnter()` Postfix

### ST-2: Objective Completion (In-Job Progress)
- **Source:** `Tutorial.OnAcceptObjective(Objective)` static callback
- **Status:** ✓ (Feature 33 — ObjectiveHandler; Prefix+Postfix on Objective.UpdateSatisifiedBy; all tests passed 2026-05-01)
- **Priority:** P2
- **What to announce:** "Objective complete: [name]." when an objective flips to met.
- **Implementation:** Patch the `Objective.CheckCompletion()` or subscribe to the completion event.
  Alternatively poll `JobStatus` objectives list for state changes in `JobStatusHandler.Update()`.

### ST-3: PC Diagnostic (Repair Jobs)
- **Class:** `WorkingOnComputerState` + `ComputerSave.CheckStatus()`
- **Status:** ✓ (covered by ObjectiveHandler — Feature 33)
- **Priority:** P2
- **Note:** Hidden diagnostic objectives are revealed progressively; ObjectiveHandler (Objective.UpdateSatisifiedBy Prefix+Postfix) announces each one when it transitions from hidden to visible. No separate diagnostic-complete announcement needed.

### ST-4: BIOS Entry (from Workshop)
- **Class:** State entered via `PCBSInput.m_enterBios`
- **Status:** ✗ (covered by BIOS-1 above)
- **Priority:** P1

### ST-5: Enter/Exit Workshop (Walking State)
- **Class:** `WorkshopState`, walking state transitions
- **Status:** ✓ (Feature 44 — WalkingStateHandler; patches WalkingState.OnStateEnter/OnStateExit; all tests passed 2026-05-01)
- **Priority:** P3
- **What to announce:** "Entered workshop." / "Left workshop."
- **Patch points:** Relevant `OnEnter()` / `OnExit()` on the walking state class.

---

## TIER 7 — DLC Content

### DLC-1: Esports DLC — Event / Email Announcements
- **Class:** `EsportsEmail`, `EsportsJob`
- **Status:** ✓ Covered — EsportsEmail implements the standard `IEmail` interface and appears in the normal inbox. EmailAppHandler reads from/subject/date/body for any IEmail, so esports emails are already announced without a dedicated handler.
- **Priority:** P3

### DLC-2: IT Support DLC — Job Tablet (TikkIT)
- **Class:** `TabletAppTikkIT`, `TabletTikkITBoard`, `TabletTikkITCard`, `TabletTikkITCardPopup`
- **Status:** ✓ (TikkITHandler — pending in-game test with IT Support DLC)
- **Priority:** P3
- **Coverage:**
  - Board open: announces total jobs + count per column.
  - Left/Right: cycle columns (Backlog, In Progress, Blocked, Completed, Nightshift).
  - Up/Down/Home/End: navigate cards within current column.
  - Enter: open focused card detail popup.
  - Popup: announces staff name, subject, request, objectives, payment, budget, and action buttons.
  - Popup Enter: positive action. N: negative action. Escape: close.
  - New card: "New IT job: [subject] from [name]."

### DLC-3: Water Cooling (FuturLab)
- **Class:** `PipeRouting`, `ConnectPipeState`
- **Status:** ✓ Covered — WorkshopNavigatorHandler handles Piping mode fully. Tab cycles pipe connectors, Enter connects. "Pipe routing mode." announced on mode switch via WorkshopBuildHandler.
- **Priority:** P3

---

## Implementation Phases (Recommended Order)

### Phase 3B — COMPLETE (all 22 features done)

| # | Feature | Status |
|---|---------|--------|
| 28 | EVT-1: Level up | ✓ passed 2026-05-01 |
| 29 | EVT-2: New review | ✓ passed 2026-05-01 |
| 30 | WS-1: PCStats | ✓ passed 2026-05-01 |
| 31 | APP-1: ThreedMarkApp | ✓ passed 2026-05-01 |
| 32 | APP-2: OCCTApp | ✓ passed 2026-05-01 |
| 33 | APP-3: AddProgramApp | ✓ tests passed |
| 34 | ST-2: Objective completion | ✓ passed 2026-05-01 |
| 35 | OS-1: OS Desktop + Launch | ✓ passed 2026-05-01 |
| 36 | BIOS-1: Bios main interface | ✓ passed 2026-05-01 |
| 37 | APP-4: ReviewApp | ✓ passed 2026-05-01 |
| 38 | APP-5: RankApp | ✓ passed 2026-05-01 |
| 39 | APP-6: LightingApp | ✓ passed 2026-05-01 |
| 40 | APP-7: VirusScanApp | ✓ passed 2026-05-01 |
| 41 | APP-11: MarketApp | ✓ passed 2026-05-01 |
| 42 | APP-8: MusicPlayerApp | ✓ passed 2026-05-01 |
| 43 | EVT-5: Achievement | ✓ passed 2026-05-01 |
| 44 | ST-5: Workshop enter/exit | ✓ passed 2026-05-01 |
| 45 | WS-4: RenameCompany | ✓ passed 2026-05-01 |
| 46 | APP-9: NotesApp | ✓ passed 2026-05-01 |
| 47 | APP-10: SelectWallpaperApp | ✓ passed 2026-05-01 |
| 48 | WS-5: FreebuildOptions | ✓ passed 2026-05-01 |
| 49 | EVT-3: Inventory updated | ✓ passed 2026-05-01 |

### Phase 3C — Remaining Gaps (not yet implemented)

| # | Feature | Class | Priority | Notes |
|---|---------|-------|----------|-------|
| 50 | OS-2: Window focus | WindowFrame, OS | P2 | ✓ Feature 52 — WindowFocusHandler; passed 2026-05-01 |
| 51 | ST-1: Enter working state | WorkingOnComputerState | P2 | ✓ Already implemented — WorkingOnComputerStateHandler |
| 52 | ST-3: PC emergency shutdown | VirtualComputer | P2 | ✓ Feature 50 — PCEmergencyShutdownHandler; passed 2026-05-01 |
| 53 | WS-2: DaySummary financial detail | DaySummary | P2 | DROPPED — DaySummary has no financial Text fields |
| 54 | WS-6: CalendarWidget | CalendarWidget, Calendar | P4 | ✓ Feature 51 — CalendarHandler; passed 2026-05-01 |
| 55 | WS-7: PCTuner / GPUTuner | GPUTuner | P3 | ✓ Feature 53 — GPUTunerHandler; PCTuner is debug-only IMGUI |
| 56 | APP-12: HWMonitor live | HWMonitor | P3 | ✓ Feature 54 — HWMonitorHandler; Numpad7 reads live sensors |
| 57 | ST-3: PC diagnostic faults | Objective / ObjectiveHandler | P2 | ✓ Covered — ObjectiveHandler announces each hidden objective as it becomes visible during diagnosis |

### Phase 4 — DLC Content

| # | Feature | Priority |
|---|---------|---------|
| 57 | DLC-1: Esports emails | P3 | ✓ Covered by EmailAppHandler (standard IEmail interface) |
| 58 | DLC-2: TikkIT tablet | P3 | ✓ TikkITHandler — board + card popup; pending DLC test |
| 59 | DLC-3: Water cooling piping | P3 | ✓ Covered by WorkshopNavigatorHandler (Piping mode) |

---

## Already Implemented (64 features)

| # | Handler | Coverage | Test status |
|---|---------|---------|------------|
| 1 | HowToBuildAPCHandler | Tutorial popups + checklist | Passed |
| 2 | MainMenuHandler | Main menu button navigation | Passed |
| 3 | OptionsMenuHandler | Options sliders/toggles/dropdowns | Passed |
| 4 | CareerJobHandler | Career email inbox navigation | Passed |
| 5 | WorkshopBuildHandler | Workshop tooltips + mode announcements | Passed |
| 6 | InventoryHandler | Inventory item navigation | Passed |
| 7 | SaveLoadMenuHandler | Save slot navigation | Passed |
| 8 | JobStatusHandler | Job objectives panel | Passed |
| 9 | ShopHandler | Shop browse, item detail, checkout | Passed |
| 10 | InGameMenuHandler | Pause menu navigation | Passed |
| 11 | MessageBoxHandler | Modal dialogs (OK / Yes-No) | Passed |
| 12 | PartInstallHandler | Part install/remove events | Passed |
| 13 | DeliveryHandler | Delivery arrival + manifest | Passed |
| 14 | DaySummaryHandler | Day summary screen + buttons | Passed |
| 15 | JobResultHandler | Job result screen (from/subject/stars) | Passed |
| 16 | WorkshopSelectionHandler | Workshop carousel navigation | Passed |
| 17 | PCPowerHandler | PC power on/off events | Passed |
| 18 | WillItRunHandler | WillItRun app (5 categories) | Passed |
| 19 | PCBayHandler | PCBay buy/sell navigation | Passed |
| 20 | HWInfoHandler | Hardware info app readout | Passed |
| 21 | EmailAppHandler | Tablet email app navigation | Passed |
| 22 | CareerEventHandler (EVT-1) | Level up announcement | Passed |
| 23 | CareerEventHandler (EVT-2) | New review announcement | Passed |
| 24 | ThreedMarkHandler | 3DMark results — score/CPU/GPU | Passed |
| 25 | OCCTHandler | OCCT stress test start/stop/sensors | Passed |
| 26 | PCStatsHandler | Build summary — title/status/benchmark/parts | Passed |
| 27 | ObjectiveHandler | Objective completion auto-announce | Passed |
| 28 | AddProgramHandler | Program install/remove + progress | Passed |
| 29 | ReviewAppHandler | Reviews app — company/rating/entries | Passed |
| 30 | RankAppHandler | Part ranking chart navigation | Passed |
| 31 | VirusScanAppHandler | Virus scanner state changes | Passed |
| 32 | MarketAppHandler | Market price readout (all categories) | Passed |
| 33 | OSHandler | OS startup/launch/start menu | Passed |
| 34 | BiosHandler | BIOS tabs/settings/navigate/value change | Passed |
| 35 | MusicPlayerAppHandler | Track announce + play/pause nav | Passed |
| 36 | LightingAppHandler | LED group list + select + apply | Passed |
| 37 | AchievementHandler | Achievement unlock detection | Passed |
| 38 | WalkingStateHandler | Enter/exit workshop state | Passed |
| 39 | RenameCompanyHandler | Company rename dialog | Passed |
| 40 | NotesAppHandler | Note nav in compact view | Passed |
| 41 | SelectWallpaperHandler | Wallpaper tab changes + apply confirm | Passed |
| 42 | FreebuildOptionsHandler | Upgrade toggle nav | Passed |
| 43 | CareerEventHandler (EVT-3) | Inventory updated (debounced) | Passed |
| 44 | PCEmergencyShutdownHandler | BSOD crash reason (6 types) | Passed |
| 45 | CalendarHandler | Day events on calendar focus | Passed |
| 46 | WindowFocusHandler | OS window brought to front | Passed |
| 47 | GPUTunerHandler | GPU overclocking nav + apply | Passed |
| 48 | HWMonitorHandler | Live sensors via Numpad7 | Passed |
| 49 | ManifestHandler | Delivery manifest item nav | Passed |
| 50 | KeyBindingMenuHandler | Key binding list nav + redefine | Passed |
| 51 | WhatsNewPopUpHandler | What's New popup read + dismiss | Passed |
| 52 | VirusScanAppHandler (rewrite) | Enter to scan + F1 re-read | Passed |
| 53 | NotesAppHandler (rewrite) | Note nav in compact view | Passed |
| 54 | FreebuildOptionsHandler (rewrite) | Upgrade toggle nav | Passed |
| 55 | WillItRunHandler (rewrite) | Program list nav + detail | Passed |
| 56 | SearchFiltersPanelHandler | Filter checkbox nav + toggle | Passed |
| 57 | SwapPeripheralStateHandler | Peripheral slot type announce | Passed |
| 58 | Career status enhanced | Date + upcoming events on S | Passed |

---

## Key Gaps / Risks

1. **BIOS navigation:** Game uses `PCBSInput.m_increaseBios`/`m_decreaseBios` which are
   Rewired actions (unknown physical keys). Mod cannot capture these safely. Strategy:
   patch the RESULT (value changed) via Postfix on BiosSetting value setters.

2. **OS.cs start menu grid:** Icons are laid out as a grid, not a list. Navigation
   approach: linearize the grid into a 1D list (row-major order), announce "[N] of [total]".

3. **LightingApp color picker:** The visual color picker (Hue/Sat wheel) has no text
   equivalent. Use RGB fields only. Announce R, G, B values separately.

4. **OCCTApp sensor throttle:** Sensor values update every frame. Use a cooldown (min 2s
   between repeated announcements of the same sensor) to avoid NVDA/JAWS flooding.

5. **ThreedMarkApp video playback:** The benchmark "plays a video" during the test.
   No meaningful state to announce during video. Announce phase start/end only.

6. **CareerStatus static events (s_onLevelUp, s_onNewReview):** These are Action delegates.
   Subscribe in `Main.Awake()` and unsubscribe in `Main.OnDestroy()`. Must null-check
   CareerStatus.Get() in the handler since it may not exist on non-career scenes.

