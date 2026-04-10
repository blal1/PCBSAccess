# TutorialHandler Design Spec

**Date:** 2026-04-10
**Feature:** Tutorial system accessibility
**Game classes:** AdvancedTutorial, ButtonTutoNext, Tutorial (Util), HelperTutorialTerm

## Overview

Make the multi-page interactive tutorial system accessible via screen reader. Tutorials open alongside programs (file explorer, terminal, mail, etc.) to teach the player game mechanics. Each tutorial has multiple pages with text/image content, optional Next buttons, and pending actions the player must complete.

## Activation

Auto-activates when an `AdvancedTutorial` window is focused. Uses the same pattern as NotepadHandler: checks `uDialog_TaskBar.CurrentTask` for an `AdvancedTutorial` component.

Deactivates when focus switches to another window. Re-announces page content when the tutorial window regains focus.

## Page Load Behavior

When a tutorial page loads (detected via Harmony patch on `ConstruyePagina`):

1. Announce page position: "Tutorial. Page 3 of 8."
2. Read all text items on the page, skipping image items
3. Strip rich text tags (HTML-like tags) and `[ACTION]`/`[/ACTION]` markers from text
4. If the page has a pending action (not NONE), append: "Action required: [action description]"
5. If the page has a Next button (`showNextButton` is true), append: "Press Enter to continue."

Text content comes from `TMP_Text` components instantiated under `parentPages`. The handler reads these after `ConstruyePagina` completes.

## Keyboard Shortcuts

All shortcuts active only when the tutorial window is focused:

- **Enter**: Advance to next page. Invokes `ButtonTutoNext.OnNext()` if a Next button exists. If a pending action is incomplete, re-announces the action requirement.
- **Escape**: Skip tutorial. Calls `AdvancedTutorial.CloseTaskBar()` which triggers the game's skip confirmation dialog (handled by existing DialogHandler).
- **Space**: Re-read the full current page text (same content as auto-read on page load).
- **F1**: Announce tutorial help text.

## Pending Actions

Some pages require the player to complete an action before proceeding (e.g., open terminal, run `ls`, navigate to `/bin`).

- On page load, the pending action type is included in the announcement
- Enter with an incomplete action re-announces the requirement
- The game auto-advances the tutorial when the action is completed (calls `OnNext(true)`), which triggers a new `ConstruyePagina` call that the handler picks up

Pending action types and their descriptions:
- OPEN_TERMINAL: "Open the terminal"
- LAUNCH_PWD: "Type pwd in the terminal"
- LAUNCH_LS: "Type ls in the terminal"
- LAUNCH_CD: "Type cd in the terminal"
- LAUNCH_MKDIR: "Type mkdir in the terminal"
- EXPLORER_ROOT: "Navigate to the root folder"
- EXPLORER_BIN: "Navigate to the bin folder"
- EXPLORER_USRBIN: "Navigate to usr bin"
- MAIL: "Check your mail"
- REMOTE_CONN: "Connect to a remote computer"
- TRACE_SYSTEM: "Open the system log on the remote computer"
- REMOTE_EXPLORER: "Open the file explorer on the remote computer"
- CREATE_MAIL_BUTTON: "Create a mail account"
- SELECT_LOGIN_ISSUES: "Select login issues"

## Wrong Action Feedback

When the player performs the wrong action, the game calls `ShowWrongAction(string text)` which displays an error. A Harmony postfix patch on this method announces the stripped text via screen reader.

## Harmony Patches (TutorialPatches.cs)

- **`AdvancedTutorial.ConstruyePagina` postfix**: Notify handler that a new page was built. Handler reads the page content from the `parentPages` transform's TMP_Text components.
- **`AdvancedTutorial.ShowWrongAction` postfix**: Announce wrong action text via screen reader, stripping HTML tags.
- **`AdvancedTutorial.OnNext` postfix**: Optional - notify handler after a page advance to ensure state is synchronized.

## Localization Strings

- `tutorial_focused`: "Tutorial. Page {0} of {1}."
- `tutorial_action_required`: "Action required: {0}"
- `tutorial_next_available`: "Press Enter to continue."
- `tutorial_wrong_action`: "{0}"
- `tutorial_help`: "Tutorial. Enter to go to next page. Escape to skip tutorial. Space to re-read page. F1 for help."
- `tutorial_complete`: "Tutorial complete."
- `tutorial_pending_descriptions`: Map of PendingAction enum to human-readable descriptions (see Pending Actions section above)

## Integration with Main.cs

- Add `_tutorialHandler` field
- Register in `InitializeHandlers()`
- Add `_tutorialHandler.Update()` call in `UpdateHandlers()` — should be high priority (before file explorer, notepad, etc.) since the tutorial window overlays other programs
- Add F1 help check in `AnnounceHelp()`

## Files to Create/Modify

New files:
- `TutorialHandler.cs` — main handler
- `TutorialPatches.cs` — Harmony patches

Modified files:
- `Main.cs` — register handler, add to update loop and help
- `Loc.cs` — add tutorial strings

## Edge Cases

- Tutorial with only 1 page: show "Page 1 of 1", Enter closes if no pending action
- Last page reached: game unlinks the tutorial from the program window. Handler should announce "Tutorial complete." if detected.
- Skip terminal tutorial: game has a special flow that skips to a summary page. Handler just reads whatever page is shown.
- Multiple tutorials: only one AdvancedTutorial can exist at a time (game enforces this in `IShowTutorial`).
