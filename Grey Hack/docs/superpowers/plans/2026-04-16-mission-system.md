# Mission System Accessibility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Announce `PanelMission` contract dialog details via screen reader, provide Left/Right/Enter/Escape keyboard navigation for Accept/Decline, and add Alt+M to re-read the last accepted mission.

**Architecture:** A new `MissionHandler` owns all state (panel ref, announcement cache, Alt+M cache). Three Harmony patches on `PanelMission` forward lifecycle events to static entry points on the handler. The handler is registered in `Main.cs` alongside all other handlers.

**Tech Stack:** C# / .NET 4.7.2, BepInEx 5.4.23.5, HarmonyLib, Unity 2022.3

---

## File Map

- **Create:** `MissionHandler.cs` — state, announcements, input handling, Alt+M cache
- **Create:** `MissionPatches.cs` — Harmony patches on `PanelMission.AddMission`, `OnAceptar`, `OnCancelar`
- **Modify:** `Loc.cs` — add mission string keys
- **Modify:** `Main.cs` — add field, init, UpdateHandlers, AnnounceHelp, Alt+M hotkey

---

## Task 1: Add Loc Strings

**Files:**
- Modify: `Loc.cs` — insert after line containing `jobs_help`

- [ ] **Step 1: Open Loc.cs and find the insertion point**

Locate this line in `InitializeStrings()`:
```csharp
            _english["jobs_help"] = "Jobs. Up Down to browse missions. Enter to view details. Backspace to go back. Space to repeat.";
```

- [ ] **Step 2: Insert mission strings immediately after `jobs_help`**

Add these lines after `jobs_help` and before the `// Police` comment:
```csharp
            // Mission Panel
            _english["mission_panel"] = "{0}. Type: {1}. Reward: {2}. Difficulty: {3}. {4}. Left Right to choose, Enter to accept, Escape to decline.";
            _english["mission_panel_nodesc"] = "{0}. Type: {1}. Reward: {2}. Difficulty: {3}. Left Right to choose, Enter to accept, Escape to decline.";
            _english["mission_accepted"] = "Mission accepted.";
            _english["mission_declined"] = "Mission declined.";
            _english["mission_nav_accept"] = "Accept";
            _english["mission_nav_decline"] = "Decline";
            _english["mission_diff_easy"] = "Easy";
            _english["mission_diff_medium"] = "Medium";
            _english["mission_diff_hard"] = "Hard";
            _english["mission_active"] = "Active mission. {0}: {1}. {2}";
            _english["mission_none"] = "No active mission.";
            _english["mission_help"] = "Mission contract. Left Right to navigate Accept Decline. Enter to confirm. Escape to decline. Space to repeat.";
            _english["mission_type_tutorial"] = "Tutorial";
            _english["mission_type_credentials"] = "Steal credentials";
            _english["mission_type_academic"] = "Academic record";
            _english["mission_type_police"] = "Police record";
            _english["mission_type_destroy"] = "Destroy computer";
            _english["mission_type_stealfile"] = "Steal file";
            _english["mission_type_deletefile"] = "Delete file";
            _english["mission_type_findhacker"] = "Find hacker";
            _english["mission_type_findevidence"] = "Find evidence";
```

- [ ] **Step 3: Commit**

```bash
rtk git add Loc.cs && rtk git commit -m "feat: add mission panel loc strings"
```

---

## Task 2: Create MissionHandler

**Files:**
- Create: `MissionHandler.cs`

- [ ] **Step 1: Create the file**

Create `MissionHandler.cs` in the project root with this complete content:

```csharp
using MissionConfig;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handles PanelMission contract dialog accessibility.
    /// Announces mission details on open and provides keyboard navigation
    /// for Accept and Decline. Alt+M re-reads the last accepted mission.
    /// </summary>
    public class MissionHandler
    {
        #region Fields

        private static MissionHandler _instance;

        private PanelMission _panel;
        private int _buttonIndex; // 0 = Accept, 1 = Decline

        // Panel announcement state (populated in Activate, used for Space repeat)
        private string _panelTitle;
        private string _panelType;
        private string _panelDifficulty;
        private string _panelReward;
        private string _panelDescription;

        // Active mission cache (updated on accept, read by Alt+M)
        private string _cachedTitle;
        private string _cachedType;
        private string _cachedDescription;

        #endregion

        #region Static Entry Points

        /// <summary>Called from MissionPatches when PanelMission.AddMission runs.</summary>
        public static void OnMissionPanelOpened(PanelMission panel, DirectMission mission)
        {
            _instance?.Activate(panel, mission);
        }

        /// <summary>Called from MissionPatches Prefix when PanelMission.OnAceptar runs.</summary>
        public static void OnMissionAccepted()
        {
            _instance?.HandleAccepted();
        }

        /// <summary>Called from MissionPatches Prefix when PanelMission.OnCancelar runs.</summary>
        public static void OnMissionDeclined()
        {
            _instance?.HandleDeclined();
        }

        /// <summary>Registers this instance as the active singleton.</summary>
        public void Register()
        {
            _instance = this;
        }

        #endregion

        #region Properties

        /// <summary>True when a PanelMission dialog is open and being tracked.</summary>
        public bool IsActive => _panel != null && _panel.gameObject.activeInHierarchy;

        #endregion

        #region Public Methods

        /// <summary>Called every frame from Main.UpdateHandlers(). Returns true if input was consumed.</summary>
        public bool Update()
        {
            if (!IsActive)
            {
                _panel = null;
                return false;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _buttonIndex = _buttonIndex == 0 ? 1 : 0;
                AnnounceCurrentButton();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_buttonIndex == 0)
                    _panel.OnAceptar();
                else
                    _panel.OnCancelar();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _panel.OnCancelar();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnouncePanel();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Announces the current panel state (if open) or the last accepted mission.
        /// Bound to Alt+M in Main.cs.
        /// </summary>
        public void AnnounceActiveMission()
        {
            if (IsActive)
            {
                AnnouncePanel();
                return;
            }

            if (!string.IsNullOrEmpty(_cachedTitle))
                ScreenReader.Say(Loc.Get("mission_active", _cachedType, _cachedTitle, _cachedDescription));
            else
                ScreenReader.Say(Loc.Get("mission_none"));
        }

        /// <summary>Returns F1 help text for the mission panel.</summary>
        public string GetHelpText() => Loc.Get("mission_help");

        #endregion

        #region Private Methods

        private void Activate(PanelMission panel, DirectMission mission)
        {
            _panel = panel;
            _buttonIndex = 0;

            _panelTitle = panel.titleLabel != null ? panel.titleLabel.text : string.Empty;
            _panelDescription = panel.contentMission != null ? panel.contentMission.text : string.Empty;
            _panelReward = panel.rewardText != null ? panel.rewardText.text : string.Empty;
            _panelType = GetTypeLabel(mission.missionType);
            _panelDifficulty = GetDifficultyLabel(mission.rep);

            AnnouncePanel();
            DebugLogger.LogState($"MissionHandler: activated '{_panelTitle}' type={mission.missionType} rep={mission.rep}");
        }

        private void HandleAccepted()
        {
            _cachedTitle = _panelTitle;
            _cachedType = _panelType;
            _cachedDescription = _panelDescription;
            ScreenReader.Say(Loc.Get("mission_accepted"));
            _panel = null;
            DebugLogger.LogState($"MissionHandler: accepted '{_cachedTitle}'");
        }

        private void HandleDeclined()
        {
            ScreenReader.Say(Loc.Get("mission_declined"));
            _panel = null;
            DebugLogger.LogState("MissionHandler: declined");
        }

        private void AnnouncePanel()
        {
            if (string.IsNullOrEmpty(_panelDescription))
                ScreenReader.Say(Loc.Get("mission_panel_nodesc", _panelTitle, _panelType, _panelReward, _panelDifficulty));
            else
                ScreenReader.Say(Loc.Get("mission_panel", _panelTitle, _panelType, _panelReward, _panelDifficulty, _panelDescription));
        }

        private void AnnounceCurrentButton()
        {
            string label = _buttonIndex == 0
                ? Loc.Get("mission_nav_accept")
                : Loc.Get("mission_nav_decline");
            ScreenReader.Say(label);
        }

        private static string GetTypeLabel(TypeMissionDirect type)
        {
            switch (type)
            {
                case TypeMissionDirect.Tutorial:        return Loc.Get("mission_type_tutorial");
                case TypeMissionDirect.Credentials:     return Loc.Get("mission_type_credentials");
                case TypeMissionDirect.AcademicRecord:  return Loc.Get("mission_type_academic");
                case TypeMissionDirect.PoliceRecord:    return Loc.Get("mission_type_police");
                case TypeMissionDirect.DestroyComputer: return Loc.Get("mission_type_destroy");
                case TypeMissionDirect.StealFile:       return Loc.Get("mission_type_stealfile");
                case TypeMissionDirect.DeleteFile:      return Loc.Get("mission_type_deletefile");
                case TypeMissionDirect.FindHacker:      return Loc.Get("mission_type_findhacker");
                case TypeMissionDirect.FindEvidence:    return Loc.Get("mission_type_findevidence");
                default:                                return Loc.Get("unknown");
            }
        }

        private static string GetDifficultyLabel(int rep)
        {
            if (rep <= 0) return Loc.Get("mission_diff_easy");
            if (rep == 1) return Loc.Get("mission_diff_medium");
            return Loc.Get("mission_diff_hard");
        }

        #endregion
    }
}
```

- [ ] **Step 2: Build to verify no compile errors**

```powershell
powershell -File scripts/Build-Mod.ps1
```

Expected: `Build succeeded. 0 Error(s)`

If errors appear, fix them before continuing.

- [ ] **Step 3: Commit**

```bash
rtk git add MissionHandler.cs && rtk git commit -m "feat: add MissionHandler for mission contract dialog"
```

---

## Task 3: Create MissionPatches

**Files:**
- Create: `MissionPatches.cs`

- [ ] **Step 1: Create the file**

Create `MissionPatches.cs` in the project root with this complete content:

```csharp
using HarmonyLib;
using MissionConfig;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for mission panel accessibility.
    /// Captures PanelMission lifecycle events and forwards to MissionHandler.
    /// </summary>

    /// <summary>
    /// Fires after AddMission sets up the dialog content.
    /// Passes the fully-populated PanelMission and mission data to MissionHandler.
    /// </summary>
    [HarmonyPatch(typeof(PanelMission), "AddMission")]
    public class PanelMissionAddMissionPatch
    {
        static void Postfix(PanelMission __instance, DirectMission mission)
        {
            DebugLogger.LogState($"PanelMission.AddMission: type={mission.missionType} rep={mission.rep}");
            MissionHandler.OnMissionPanelOpened(__instance, mission);
        }
    }

    /// <summary>
    /// Fires before the accept logic runs, so MissionHandler can cache and announce
    /// while the panel ref is still valid.
    /// </summary>
    [HarmonyPatch(typeof(PanelMission), "OnAceptar")]
    public class PanelMissionOnAceptarPatch
    {
        static void Prefix()
        {
            DebugLogger.LogState("PanelMission.OnAceptar called");
            MissionHandler.OnMissionAccepted();
        }
    }

    /// <summary>
    /// Fires before cancel/close so MissionHandler can announce and clear state.
    /// </summary>
    [HarmonyPatch(typeof(PanelMission), "OnCancelar")]
    public class PanelMissionOnCancelarPatch
    {
        static void Prefix()
        {
            DebugLogger.LogState("PanelMission.OnCancelar called");
            MissionHandler.OnMissionDeclined();
        }
    }
}
```

- [ ] **Step 2: Build to verify no compile errors**

```powershell
powershell -File scripts/Build-Mod.ps1
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
rtk git add MissionPatches.cs && rtk git commit -m "feat: add MissionPatches for PanelMission lifecycle"
```

---

## Task 4: Wire Into Main.cs

**Files:**
- Modify: `Main.cs` — 4 changes: field, init, hotkey, UpdateHandlers, AnnounceHelp

- [ ] **Step 1: Add the field**

In `Main.cs`, in the `#region Fields` block, after the `_translationWindowHandler` line, add:

```csharp
        private MissionHandler _missionHandler;
```

- [ ] **Step 2: Register in InitializeHandlers()**

In `InitializeHandlers()`, after the `_translationWindowHandler` block:

```csharp
            _missionHandler = new MissionHandler();
            _missionHandler.Register();
```

- [ ] **Step 3: Add Alt+M hotkey**

In `ProcessHotkeys()`, after the Alt+N block (around line 221), add:

```csharp
            // Alt+M = Active mission
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
            {
                DebugLogger.LogInput("Alt+M", "Active mission");
                _missionHandler.AnnounceActiveMission();
                return true;
            }
```

- [ ] **Step 4: Add to UpdateHandlers()**

In `UpdateHandlers()`, after the `_dialogHandler.Update()` line and before `_welcomeHandler.Update()`, add:

```csharp
            // Mission panel consumes input when active
            if (_missionHandler.Update()) return;
```

The block should look like:
```csharp
            // Error/question dialogs consume input when active
            if (_dialogHandler.Update()) return;

            // Mission panel consumes input when active
            if (_missionHandler.Update()) return;

            // Welcome dialog consumes input when active
            if (_welcomeHandler.Update()) return;
```

- [ ] **Step 5: Add to AnnounceHelp()**

In `AnnounceHelp()`, after the `_dialogHandler.IsActive` block and before `_welcomeHandler.IsActive`, add:

```csharp
            if (_missionHandler.IsActive)
            {
                ScreenReader.Say(_missionHandler.GetHelpText());
                return;
            }
```

- [ ] **Step 6: Build**

```powershell
powershell -File scripts/Build-Mod.ps1
```

Expected: `Build succeeded. 0 Error(s)`

If the build fails, check that the field name `_missionHandler` matches exactly in all four locations.

- [ ] **Step 7: Deploy**

```powershell
powershell -File scripts/Deploy-Mod.ps1
```

- [ ] **Step 8: Commit**

```bash
rtk git add Main.cs && rtk git commit -m "feat: wire MissionHandler into main update loop and hotkeys"
```

---

## Task 5: In-Game Testing

Start the game and work through the test checklist. Report any failures back.

- [ ] **Open browser → jobs panel → press Enter on a job**

Expected: hear full announcement —
`"[title]. Type: [type]. Reward: $[amount]. Difficulty: [Easy/Medium/Hard]. [description]. Left Right to choose, Enter to accept, Escape to decline."`

- [ ] **Press Left arrow**

Expected: hear `"Decline"`

- [ ] **Press Right arrow**

Expected: hear `"Accept"`

- [ ] **Press Left again, then Enter (on Decline)**

Expected: hear `"Mission declined."`, dialog closes

- [ ] **Repeat: open a job, press Enter (default Accept), press Enter**

Expected: hear `"Mission accepted."`, dialog closes

- [ ] **After accepting, press Alt+M anywhere**

Expected: hear `"Active mission. [type]: [title]. [description]."`

- [ ] **Press Alt+M before accepting any mission (first boot)**

Expected: hear `"No active mission."`

- [ ] **Open a job, press Escape**

Expected: hear `"Mission declined."`, dialog closes

- [ ] **Open a job, press Space**

Expected: hear full announcement repeated

- [ ] **Open a job, press F1**

Expected: hear `"Mission contract. Left Right to navigate Accept Decline. Enter to confirm. Escape to decline. Space to repeat."`

- [ ] **Accept a second mission, then Alt+M**

Expected: hear second mission's details (cache overwritten)

- [ ] **Open a job, close via window X button (if available), then Alt+M**

Expected: no crash, Alt+M shows previously accepted mission

---

## Notes

- `OnAceptar` / `OnCancelar` are public methods on `PanelMission` — called directly from `Update()`, no button component lookup needed.
- Prefix patches on both methods fire before the dialog closes itself via `CloseTaskBar()`, so `_panelTitle` etc. are still valid when `HandleAccepted()` reads them.
- Alt+M is safe to call at any time — returns "No active mission." when nothing is cached.
- `_panel` becomes Unity-null when destroyed — `IsActive` check handles this correctly.
