# TutorialHandler Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the multi-page interactive tutorial system accessible via screen reader — auto-read pages, keyboard navigation, pending action announcements, wrong action feedback.

**Architecture:** TutorialHandler detects when an AdvancedTutorial window is focused (same pattern as NotepadHandler). TutorialPatches uses Harmony to intercept page builds and wrong actions, forwarding events to TutorialHandler static methods. All strings go through Loc.Get().

**Tech Stack:** C# / net472, BepInEx 5, Harmony 2, Unity 2022.3, Tolk screen reader

**Spec:** `docs/superpowers/specs/2026-04-10-tutorial-handler-design.md`

---

### Task 1: Add Tutorial Localization Strings

**Files:**
- Modify: `Loc.cs:371` (before closing brace of InitializeStrings)

- [ ] **Step 1: Add tutorial strings to Loc.cs**

Open `Loc.cs` and add the following block after the Chat section (after line 370, before the closing `}`):

```csharp
            // Tutorial
            _english["tutorial_focused"] = "Tutorial. Page {0} of {1}.";
            _english["tutorial_refocused"] = "Tutorial. Page {0} of {1}.";
            _english["tutorial_action_required"] = "Action required: {0}";
            _english["tutorial_next_available"] = "Press Enter to continue.";
            _english["tutorial_wrong_action"] = "{0}";
            _english["tutorial_complete"] = "Tutorial complete.";
            _english["tutorial_help"] = "Tutorial. Enter to go to next page. Escape to skip tutorial. Space to re-read page. F1 for help.";
            _english["tutorial_pending_open_terminal"] = "Open the terminal.";
            _english["tutorial_pending_launch_pwd"] = "Type pwd in the terminal.";
            _english["tutorial_pending_launch_ls"] = "Type ls in the terminal.";
            _english["tutorial_pending_launch_cd"] = "Type cd in the terminal.";
            _english["tutorial_pending_launch_mkdir"] = "Type mkdir in the terminal.";
            _english["tutorial_pending_explorer_root"] = "Navigate to the root folder.";
            _english["tutorial_pending_explorer_bin"] = "Navigate to the bin folder.";
            _english["tutorial_pending_explorer_usrbin"] = "Navigate to usr bin.";
            _english["tutorial_pending_mail"] = "Check your mail.";
            _english["tutorial_pending_remote_conn"] = "Connect to a remote computer.";
            _english["tutorial_pending_trace_system"] = "Open the system log on the remote computer.";
            _english["tutorial_pending_remote_explorer"] = "Open the file explorer on the remote computer.";
            _english["tutorial_pending_create_mail_button"] = "Create a mail account.";
            _english["tutorial_pending_select_login_issues"] = "Select login issues.";
```

- [ ] **Step 2: Verify build compiles**

Run: `scripts/Build-Mod.ps1`
Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
rtk git add Loc.cs && rtk git commit -m "feat: add tutorial localization strings"
```

---

### Task 2: Create TutorialHandler

**Files:**
- Create: `TutorialHandler.cs`

- [ ] **Step 1: Create TutorialHandler.cs**

```csharp
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for tutorial window accessibility.
    /// Auto-activates when an AdvancedTutorial window is focused.
    /// Enter to advance, Escape to skip, Space to re-read, F1 for help.
    /// </summary>
    public class TutorialHandler
    {
        #region Fields

        private static TutorialHandler _instance;
        private AdvancedTutorial _activeTutorial;
        private uDialog _activeDialog;
        private bool _isActive;
        private string _lastPageText = "";

        /// <summary>Map of PendingAction enum values to localization keys.</summary>
        private static readonly Dictionary<AdvancedTutorial.PendingAction, string> PendingActionKeys = new()
        {
            { AdvancedTutorial.PendingAction.OPEN_TERMINAL, "tutorial_pending_open_terminal" },
            { AdvancedTutorial.PendingAction.LAUNCH_PWD, "tutorial_pending_launch_pwd" },
            { AdvancedTutorial.PendingAction.LAUNCH_LS, "tutorial_pending_launch_ls" },
            { AdvancedTutorial.PendingAction.LAUNCH_CD, "tutorial_pending_launch_cd" },
            { AdvancedTutorial.PendingAction.LAUNCH_MKDIR, "tutorial_pending_launch_mkdir" },
            { AdvancedTutorial.PendingAction.EXPLORER_ROOT, "tutorial_pending_explorer_root" },
            { AdvancedTutorial.PendingAction.EXPLORER_BIN, "tutorial_pending_explorer_bin" },
            { AdvancedTutorial.PendingAction.EXPLORER_USRBIN, "tutorial_pending_explorer_usrbin" },
            { AdvancedTutorial.PendingAction.MAIL, "tutorial_pending_mail" },
            { AdvancedTutorial.PendingAction.REMOTE_CONN, "tutorial_pending_remote_conn" },
            { AdvancedTutorial.PendingAction.TRACE_SYSTEM, "tutorial_pending_trace_system" },
            { AdvancedTutorial.PendingAction.REMOTE_EXPLORER, "tutorial_pending_remote_explorer" },
            { AdvancedTutorial.PendingAction.CREATE_MAIL_BUTTON, "tutorial_pending_create_mail_button" },
            { AdvancedTutorial.PendingAction.SELECT_LOGIN_ISSUES, "tutorial_pending_select_login_issues" },
        };

        #endregion

        #region Static Entry

        /// <summary>Registers this handler instance.</summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>Called by Harmony patch when a tutorial page is built.</summary>
        public static void OnPageBuilt(AdvancedTutorial tutorial)
        {
            if (_instance == null) return;
            _instance.HandlePageBuilt(tutorial);
        }

        /// <summary>Called by Harmony patch when a wrong action is shown.</summary>
        public static void OnWrongAction(string text)
        {
            if (_instance == null) return;
            string clean = StripRichText(text);
            ScreenReader.Say(Loc.Get("tutorial_wrong_action", clean));
        }

        #endregion

        #region Public Properties

        /// <summary>Whether tutorial handler is currently active.</summary>
        public bool IsActive => _isActive;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update().
        /// Returns true if input was consumed.
        /// </summary>
        public bool Update()
        {
            CheckFocusedWindow();

            if (!_isActive) return false;

            if (_activeTutorial == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            return HandleInput();
        }

        /// <summary>Returns help text for F1 during tutorial focus.</summary>
        public string GetHelpText()
        {
            return Loc.Get("tutorial_help");
        }

        #endregion

        #region Focus Detection

        private void CheckFocusedWindow()
        {
            var taskbar = Object.FindObjectOfType<uDialog_TaskBar>();
            if (taskbar == null)
            {
                if (_isActive) Deactivate();
                return;
            }

            uDialog currentTask = taskbar.CurrentTask;
            if (currentTask == null)
            {
                if (_isActive) Deactivate();
                return;
            }

            if (currentTask == _activeDialog && _isActive)
            {
                return;
            }

            AdvancedTutorial tutorial = currentTask.GetComponentInChildren<AdvancedTutorial>();
            if (tutorial != null)
            {
                if (!_isActive || tutorial != _activeTutorial)
                {
                    Activate(tutorial, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(AdvancedTutorial tutorial, uDialog dialog)
        {
            _activeTutorial = tutorial;
            _activeDialog = dialog;
            _isActive = true;

            // Read page content on focus
            AnnouncePage(true);
            DebugLogger.LogState($"TutorialHandler: activated");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeTutorial = null;
            _activeDialog = null;
            _lastPageText = "";
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            // Enter = Next page
            if (Input.GetKeyDown(KeyCode.Return))
            {
                HandleNext();
                return true;
            }

            // Escape = Skip tutorial
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleSkip();
                return true;
            }

            // Space = Re-read page
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnouncePage(false);
                return true;
            }

            return false;
        }

        #endregion

        #region Page Reading

        private void HandlePageBuilt(AdvancedTutorial tutorial)
        {
            // Update reference in case it changed
            _activeTutorial = tutorial;

            // If we're active and focused on this tutorial, announce the new page
            if (_isActive)
            {
                AnnouncePage(true);
            }
        }

        private void AnnouncePage(bool includeHeader)
        {
            if (_activeTutorial == null) return;

            int currentPage = ReflectionHelper.GetPrivateFieldValue<int>(_activeTutorial, "pagina", 0);
            var pages = _activeTutorial.pages;
            int totalPages = pages != null ? pages.Count : 0;

            // Build announcement
            string announcement = "";

            if (includeHeader && totalPages > 0)
            {
                announcement = Loc.Get("tutorial_focused", currentPage + 1, totalPages);
            }

            // Read text content from parentPages transform
            string pageText = ReadPageText();
            if (!string.IsNullOrEmpty(pageText))
            {
                announcement += " " + pageText;
            }

            // Pending action
            var pendingAction = ReflectionHelper.GetPrivateFieldValue<AdvancedTutorial.PendingAction>(
                _activeTutorial, "pendingAction", AdvancedTutorial.PendingAction.NONE);

            if (pendingAction != AdvancedTutorial.PendingAction.NONE)
            {
                string actionDesc = GetPendingActionDescription(pendingAction);
                announcement += " " + Loc.Get("tutorial_action_required", actionDesc);
            }

            // Check if Next button exists (showNextButton on current page)
            if (totalPages > 0 && currentPage < totalPages)
            {
                var page = pages[currentPage];
                if (page.showNextButton)
                {
                    announcement += " " + Loc.Get("tutorial_next_available");
                }
            }

            _lastPageText = announcement.Trim();
            ScreenReader.Say(_lastPageText);
        }

        private string ReadPageText()
        {
            if (_activeTutorial == null) return "";

            Transform parentPages = _activeTutorial.parentPages;
            if (parentPages == null) return "";

            var texts = new List<string>();
            var tmpTexts = parentPages.GetComponentsInChildren<TMP_Text>();

            foreach (var tmp in tmpTexts)
            {
                if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;

                // Skip the ButtonTutoNext text (it just says "Next" or similar)
                if (tmp.GetComponentInParent<ButtonTutoNext>() != null) continue;

                string text = StripRichText(tmp.text);
                text = StripActionTags(text);

                if (!string.IsNullOrEmpty(text.Trim()))
                {
                    texts.Add(text.Trim());
                }
            }

            return string.Join(" ", texts);
        }

        #endregion

        #region Navigation

        private void HandleNext()
        {
            if (_activeTutorial == null) return;

            // Check if there's a pending action blocking progress
            var pendingAction = ReflectionHelper.GetPrivateFieldValue<AdvancedTutorial.PendingAction>(
                _activeTutorial, "pendingAction", AdvancedTutorial.PendingAction.NONE);

            if (pendingAction != AdvancedTutorial.PendingAction.NONE)
            {
                // Check if a Next button exists on this page
                int currentPage = ReflectionHelper.GetPrivateFieldValue<int>(_activeTutorial, "pagina", 0);
                var pages = _activeTutorial.pages;
                if (pages != null && currentPage < pages.Count && !pages[currentPage].showNextButton)
                {
                    // No Next button, pending action required
                    string actionDesc = GetPendingActionDescription(pendingAction);
                    ScreenReader.Say(Loc.Get("tutorial_action_required", actionDesc));
                    return;
                }
            }

            // Try to find and click the Next button
            ButtonTutoNext nextButton = _activeTutorial.parentPages?.GetComponentInChildren<ButtonTutoNext>();
            if (nextButton != null)
            {
                nextButton.OnNext();
            }
            else
            {
                // Last page or no button - check if this is the last page
                int currentPage = ReflectionHelper.GetPrivateFieldValue<int>(_activeTutorial, "pagina", 0);
                var pages = _activeTutorial.pages;
                if (pages != null && currentPage >= pages.Count - 1)
                {
                    ScreenReader.Say(Loc.Get("tutorial_complete"));
                }
            }
        }

        private void HandleSkip()
        {
            if (_activeTutorial == null) return;

            // Call CloseTaskBar which triggers the skip confirmation dialog
            _activeTutorial.CloseTaskBar();
        }

        #endregion

        #region Helpers

        private static string GetPendingActionDescription(AdvancedTutorial.PendingAction action)
        {
            if (PendingActionKeys.TryGetValue(action, out string key))
            {
                return Loc.Get(key);
            }
            return action.ToString();
        }

        /// <summary>Strips HTML/rich text tags from a string.</summary>
        private static string StripRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return Regex.Replace(text, "<.*?>", "");
        }

        /// <summary>Strips [ACTION]...[/ACTION] markers, keeping the inner text.</summary>
        private static string StripActionTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("[ACTION]", "").Replace("[/ACTION]", "");
        }

        #endregion
    }
}
```

- [ ] **Step 2: Verify build compiles**

Run: `scripts/Build-Mod.ps1`
Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
rtk git add TutorialHandler.cs && rtk git commit -m "feat: add TutorialHandler for tutorial accessibility"
```

---

### Task 3: Create TutorialPatches

**Files:**
- Create: `TutorialPatches.cs`

The target methods `ConstruyePagina` and `ShowWrongAction` are both private, so we use `[HarmonyPatch]` with `MethodType` and `AccessTools`. The `AdvancedTutorial.Page` parameter for `ConstruyePagina` is a nested class.

- [ ] **Step 1: Create TutorialPatches.cs**

```csharp
using System.Text.RegularExpressions;
using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for tutorial window accessibility.
    /// Intercepts page builds and wrong action messages.
    /// </summary>

    /// <summary>
    /// Patches AdvancedTutorial.ConstruyePagina to announce new page content.
    /// ConstruyePagina is private, so we use HarmonyPatch with method name string.
    /// </summary>
    [HarmonyPatch(typeof(AdvancedTutorial), "ConstruyePagina")]
    public class TutorialPageBuiltPatch
    {
        static void Postfix(AdvancedTutorial __instance)
        {
            DebugLogger.LogState("AdvancedTutorial.ConstruyePagina postfix fired");
            TutorialHandler.OnPageBuilt(__instance);
        }
    }

    /// <summary>
    /// Patches AdvancedTutorial.ShowWrongAction to announce error feedback.
    /// ShowWrongAction is private, takes a string parameter.
    /// </summary>
    [HarmonyPatch(typeof(AdvancedTutorial), "ShowWrongAction")]
    public class TutorialWrongActionPatch
    {
        static void Postfix(string text)
        {
            string clean = Regex.Replace(text ?? "", "<.*?>", "");
            DebugLogger.LogState($"AdvancedTutorial.ShowWrongAction: '{clean}'");
            TutorialHandler.OnWrongAction(text);
        }
    }
}
```

- [ ] **Step 2: Verify build compiles**

Run: `scripts/Build-Mod.ps1`
Expected: 0 errors, 0 warnings

- [ ] **Step 3: Commit**

```bash
rtk git add TutorialPatches.cs && rtk git commit -m "feat: add TutorialPatches for Harmony hooks"
```

---

### Task 4: Register TutorialHandler in Main.cs

**Files:**
- Modify: `Main.cs`

Three changes needed: add field, register in InitializeHandlers, add to UpdateHandlers and AnnounceHelp.

- [ ] **Step 1: Add field declaration**

In `Main.cs`, after the `_chatHandler` field (line 34), add:

```csharp
        private TutorialHandler _tutorialHandler;
```

- [ ] **Step 2: Register in InitializeHandlers**

After the `_chatHandler.Register();` line (line 119), add:

```csharp
            _tutorialHandler = new TutorialHandler();
            _tutorialHandler.Register();
```

- [ ] **Step 3: Add to UpdateHandlers — high priority**

The tutorial handler should be checked early in `UpdateHandlers()` since it overlays other windows. Insert it after `_welcomeHandler.Update()` and before `_startMenuHandler.Update()`. After line 239 (`if (_welcomeHandler.Update()) return;`), add:

```csharp
            // Tutorial consumes input when active
            if (_tutorialHandler.Update()) return;
```

- [ ] **Step 4: Add to AnnounceHelp**

In the `AnnounceHelp()` method, add a tutorial check. Insert after the `_welcomeHandler` help check (after the closing brace on line 288), add:

```csharp
            if (_tutorialHandler.IsActive)
            {
                ScreenReader.Say(_tutorialHandler.GetHelpText());
                return;
            }
```

- [ ] **Step 5: Verify build compiles**

Run: `scripts/Build-Mod.ps1`
Expected: 0 errors, 0 warnings

- [ ] **Step 6: Commit**

```bash
rtk git add Main.cs && rtk git commit -m "feat: register TutorialHandler in Main.cs"
```

---

### Task 5: Build, Deploy, and Add Test Checklist

**Files:**
- Modify: `project_status.md`

- [ ] **Step 1: Build and deploy**

Run: `scripts/Build-Mod.ps1`
Then run: `scripts/Deploy-Mod.ps1`
Expected: 0 errors, DLL copied to plugins folder

- [ ] **Step 2: Add test checklist to project_status.md**

Add the following test section after the Chat test section in `project_status.md`:

```markdown
### Tutorial Handler

- [ ] Start a new single player game (with tutorials enabled): tutorial window opens alongside file explorer
- [ ] Tutorial opens: hear "Tutorial. Page 1 of N." followed by page text
- [ ] Enter: advances to next page, hear new page number and text
- [ ] Space: re-reads current page text
- [ ] Escape: hear skip tutorial confirmation dialog (handled by DialogHandler)
- [ ] Page with pending action: hear "Action required: [description]"
- [ ] Complete the pending action (e.g. open terminal): tutorial auto-advances, hear new page
- [ ] Enter on page with pending action (not completed): hear the action requirement again
- [ ] Wrong action performed: hear the error feedback text
- [ ] Switch to another window and back to tutorial: hear page re-announced
- [ ] F1: hear tutorial help text
- [ ] Last page reached with no more pages: hear "Tutorial complete."
```

- [ ] **Step 3: Update project_status.md current phase**

Update the "Currently working on" line to:
```
**Currently working on:** Tutorial Handler implementation complete. Ready for testing.
```

Update the Notes for Next Session to include tutorial testing info.

- [ ] **Step 4: Commit**

```bash
rtk git add project_status.md && rtk git commit -m "docs: add TutorialHandler test checklist to project status"
```
