# Mission System Accessibility — Design Spec

**Date:** 2026-04-16
**Branch:** 001-matchmaking-screen
**Phase:** 5 (Mission System)

---

## Overview

Grey Hack missions are the main progression mechanic. Players browse available missions in the browser jobs panel, click one to open a mission contract dialog (`PanelMission`), then accept or decline. After accepting, they complete the objective in-game and send a report email to the mission client. Pass/fail feedback comes back via mail.

The jobs panel (browsing available missions) is already handled by `JobsSubHandler`. This spec covers:
- The `PanelMission` contract dialog (announce details, keyboard nav)
- Active mission tracking (Alt+M re-read after accepting)

---

## Architecture

### Files

- `MissionHandler.cs` — all state, announcements, input handling, caching
- `MissionPatches.cs` — Harmony patches forwarding to static entry points on `MissionHandler`

### Registration

- `Main.cs` `InitializeHandlers()` — create and register instance
- `Main.cs` `UpdateHandlers()` — call `MissionHandler.Update()`, returns true if input consumed
- `Main.cs` `AnnounceHelp()` — include mission panel help when panel is active

### Harmony Patches

| Method | Patch type | Handler entry point |
|---|---|---|
| `PanelMission.AddMission()` | Postfix | `MissionHandler.OnMissionPanelOpened(PanelMission, DirectMission)` |
| `PanelMission.OnAceptar()` | Prefix | `MissionHandler.OnMissionAccepted()` |
| `PanelMission.OnCancelar()` | Prefix | `MissionHandler.OnMissionDeclined()` |

### State

```
_panel               — ref to active PanelMission (null when closed)
_buttons             — list of 2 UnityEngine.UI.Button components
_buttonIndex         — 0 = Accept, 1 = Decline
_cachedTitle         — stored on accept for Alt+M
_cachedType          — TypeMissionDirect stored on accept
_cachedDescription   — contentMission.text stored on accept
```

---

## Announcements

### Panel opens (AddMission postfix)

Announce:
```
"Mission contract. [title]. Type: [type]. Reward: [reward]. Difficulty: [difficulty]. [description]. Left Right to choose, Enter to accept, Escape to decline."
```

- `[type]` — human-readable label from TypeMissionDirect (see table below)
- `[difficulty]` — rep 0 → "Easy", rep 1 → "Medium", rep 2+ → "Hard"
- `[reward]` — `rewardText.text` (already formatted as "$N" by game)
- `[description]` — `contentMission.text` (skip if empty)

### Mission type labels (Loc keys)

| TypeMissionDirect | Loc key | Display |
|---|---|---|
| Tutorial | mission_type_tutorial | Tutorial |
| Credentials | mission_type_credentials | Steal credentials |
| AcademicRecord | mission_type_academic | Academic record |
| PoliceRecord | mission_type_police | Police record |
| DestroyComputer | mission_type_destroy | Destroy computer |
| StealFile | mission_type_stealfile | Steal file |
| DeleteFile | mission_type_deletefile | Delete file |
| FindHacker | mission_type_findhacker | Find hacker |
| FindEvidence | mission_type_findevidence | Find evidence |

### Navigation

- Left/Right — cycle between "Accept" and "Decline", announce current selection
- Enter on Accept — announce "Mission accepted.", invoke `OnAceptar()` indirectly via button click
- Enter on Decline / Escape — announce "Mission declined.", invoke `OnCancelar()` indirectly
- Space — repeat full panel announcement

### After accepting

- Store title, type, description in cache
- Announce: "Mission accepted."
- Cache persists until a new mission is accepted

### Alt+M (global hotkey)

- Panel currently open → repeat full panel announcement
- Mission cached (previously accepted) → announce "Active mission. [type]: [title]. [description]."
- Nothing cached → announce "No active mission."

---

## Edge Cases

- Panel closed externally (X button): `Update()` detects `_panel == null` or `!activeInHierarchy`, clears state silently
- Buttons not found: log warning, Enter falls back to calling `OnAceptar()` / `OnCancelar()` via direct method call on `_panel`
- `contentMission.text` empty: omit description from announcement
- rep > 2: clamp to "Hard"
- New panel opens while one tracked: replace state, announce new mission
- Alt+M during open panel: show panel state (not cache)
- Accept second mission: overwrite cache silently

---

## Key Bindings

| Key | Context | Action |
|---|---|---|
| Left / Right | Mission panel open | Navigate Accept / Decline |
| Enter | Mission panel open | Activate selected button |
| Escape | Mission panel open | Decline and close |
| Space | Mission panel open | Repeat announcement |
| Alt+M | Global | Re-read active mission |
| F1 | Mission panel open | Help text |

---

## Test Checklist

- Open browser → jobs panel → Enter on a job → hear full mission announcement (title, type, reward, difficulty, description)
- Left arrow → hear "Decline"
- Right arrow → hear "Accept"
- Enter on Accept → hear "Mission accepted.", dialog closes
- Enter on Decline → hear "Mission declined.", dialog closes
- Escape → hear "Mission declined.", dialog closes
- Space → repeat full announcement
- F1 → hear mission panel help text
- After accepting, Alt+M anywhere → hear active mission summary
- Alt+M before accepting any mission → hear "No active mission."
- Accept second mission → Alt+M shows new mission
- Close panel via X button → no crash, state cleared, Alt+M shows previously accepted mission

---

## Out of Scope

- Real-time objective progress tracking (no client-side active mission list in game)
- Mission complete / fail detection (comes back via mail — already handled by MailHandler)
- TutorialMission type (handled separately by TutorialHandler)
