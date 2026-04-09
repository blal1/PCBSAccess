# Phase 3: Communication Features Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add screen reader accessibility for Notepad, Email, and Chat windows in Grey Hack.

**Architecture:** Each feature follows the existing Handler + Patches pattern. Handler classes detect when their window type is focused (via `uDialog_TaskBar.CurrentTask` component lookup), manage navigation state, and process keyboard input. Patches file contains Harmony postfixes on game methods to trigger handler callbacks. All strings go through `Loc.Get()`. Registration in `Main.cs`.

**Tech Stack:** C# / BepInEx 5 / Harmony / Unity uGUI / Tolk screen reader

**Build command:** `powershell -File scripts/Build-Mod.ps1`

**Deploy command:** `powershell -File scripts/Deploy-Mod.ps1`

---

## File Structure

### New Files
- `NotepadHandler.cs` - Notepad window accessibility (focus detection, read content, help)
- `NotepadPatches.cs` - Harmony patch on `Notepad.ResumeConnectionWindow(string)`
- `MailHandler.cs` - Email client accessibility (inbox nav, read, compose, reply)
- `MailPatches.cs` - Harmony patches on MailWindow methods
- `ChatHandler.cs` - Chat accessibility (channel nav, message history, new message announcements)
- `ChatPatches.cs` - Harmony patches on ChatGuild methods

### Modified Files
- `Main.cs` - Register new handlers, add to Update chain and Help
- `Loc.cs` - Add localization strings for all three features
- `ModConfig.cs` - Add chat announcement toggle setting
- `project_status.md` - Update with new features and test cases

---

## Task 1: NotepadHandler + NotepadPatches

**Files:**
- Create: `NotepadHandler.cs`
- Create: `NotepadPatches.cs`
- Modify: `Loc.cs:325` (add notepad strings before closing brace)
- Modify: `Main.cs:31` (add field), `Main.cs:106-107` (register), `Main.cs:236-237` (update), `Main.cs:293-296` (help)

### Step 1: Add localization strings

- [ ] **Step 1a: Add notepad strings to Loc.cs**

In `Loc.cs`, before the closing `}` of `InitializeStrings()` (line 326), add:

```csharp
            // Notepad
            _english["notepad_focused"] = "Notepad. {0}";
            _english["notepad_focused_new"] = "Notepad. New file.";
            _english["notepad_file_loaded"] = "File loaded. {0}";
            _english["notepad_read_content"] = "{0}";
            _english["notepad_empty"] = "File is empty";
            _english["notepad_help"] = "Notepad. Alt R to read content. Ctrl S to save, Ctrl O to open file. F1 for help.";
```

### Step 2: Create NotepadPatches.cs

- [ ] **Step 2a: Create the patches file**

Create `NotepadPatches.cs`:

```csharp
using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for notepad accessibility.
    /// Announces file loaded events.
    /// </summary>

    /// <summary>
    /// Patches Notepad.ResumeConnectionWindow(string) to announce file loaded.
    /// </summary>
    [HarmonyPatch(typeof(Notepad), "ResumeConnectionWindow", new[] { typeof(string) })]
    public class NotepadResumeConnectionPatch
    {
        static void Postfix(Notepad __instance, string info)
        {
            DebugLogger.LogState($"Notepad.ResumeConnectionWindow: info length={info?.Length ?? 0}");
            NotepadHandler.OnFileLoaded(__instance);
        }
    }
}
```

### Step 3: Create NotepadHandler.cs

- [ ] **Step 3a: Create the handler file**

Create `NotepadHandler.cs`:

```csharp
using NotepadPoolSystem;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for notepad window accessibility.
    /// Auto-activates when a notepad window is focused.
    /// Alt+R reads file content, F1 for help.
    /// </summary>
    public class NotepadHandler
    {
        #region Fields

        private static NotepadHandler _instance;
        private Notepad _activeNotepad;
        private uDialog _activeDialog;
        private bool _isActive;

        #endregion

        #region Static Entry

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>
        /// Called when a notepad file is loaded via Harmony patch.
        /// </summary>
        public static void OnFileLoaded(Notepad notepad)
        {
            if (_instance == null) return;
            _instance.HandleFileLoaded(notepad);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether notepad handler is currently active.
        /// </summary>
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

            if (_activeNotepad == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            return HandleInput();
        }

        /// <summary>
        /// Returns help text for F1 during notepad focus.
        /// </summary>
        public string GetHelpText()
        {
            return Loc.Get("notepad_help");
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

            Notepad notepad = currentTask.GetComponentInChildren<Notepad>();
            if (notepad != null)
            {
                if (!_isActive || notepad != _activeNotepad)
                {
                    Activate(notepad, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(Notepad notepad, uDialog dialog)
        {
            _activeNotepad = notepad;
            _activeDialog = dialog;
            _isActive = true;

            string title = dialog.TitleText ?? "";
            string fileName = title.Contains(" - ") ? title.Substring(title.IndexOf(" - ") + 3) : "";

            if (string.IsNullOrEmpty(fileName))
            {
                ScreenReader.Say(Loc.Get("notepad_focused_new"));
            }
            else
            {
                ScreenReader.Say(Loc.Get("notepad_focused", fileName));
            }

            DebugLogger.LogState($"NotepadHandler: activated, title='{title}'");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeNotepad = null;
            _activeDialog = null;
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            // Alt+R = Read content
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R))
            {
                ReadContent();
                return true;
            }

            return false;
        }

        #endregion

        #region Content Reading

        private void ReadContent()
        {
            if (_activeNotepad == null) return;

            NotepadListAdapter adapter = ReflectionHelper.GetField<NotepadListAdapter>(_activeNotepad, "listAdapter");
            if (adapter == null)
            {
                DebugLogger.LogState("NotepadHandler: could not access listAdapter");
                return;
            }

            string content = adapter.GetText();
            if (string.IsNullOrEmpty(content?.Trim()))
            {
                ScreenReader.Say(Loc.Get("notepad_empty"));
                return;
            }

            // Limit to first 500 chars to avoid overwhelming the screen reader
            string trimmed = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
            ScreenReader.Say(Loc.Get("notepad_read_content", trimmed));
        }

        private void HandleFileLoaded(Notepad notepad)
        {
            if (notepad == null) return;

            uDialog dialog = notepad.GetComponent<uDialog>();
            if (dialog == null) return;

            string title = dialog.TitleText ?? "";
            string fileName = title.Contains(" - ") ? title.Substring(title.IndexOf(" - ") + 3) : "";

            if (!string.IsNullOrEmpty(fileName))
            {
                ScreenReader.Say(Loc.Get("notepad_file_loaded", fileName));
            }
        }

        #endregion
    }
}
```

### Step 4: Register in Main.cs

- [ ] **Step 4a: Add field to Main.cs**

After line 31 (`private ContextMenuHandler _contextMenuHandler;`), add:

```csharp
        private NotepadHandler _notepadHandler;
```

- [ ] **Step 4b: Register handler in InitializeHandlers()**

After line 107 (`_contextMenuHandler.Register();`), add:

```csharp
            _notepadHandler = new NotepadHandler();
            _notepadHandler.Register();
```

- [ ] **Step 4c: Add to UpdateHandlers()**

After line 236 (`if (_fileExplorerHandler.Update()) return;`), add:

```csharp
            // Notepad consumes input when active
            if (_notepadHandler.Update()) return;
```

- [ ] **Step 4d: Add to AnnounceHelp()**

After the file explorer help block (line 295-296), add:

```csharp
            if (_notepadHandler.IsActive)
            {
                ScreenReader.Say(_notepadHandler.GetHelpText());
                return;
            }
```

### Step 5: Build and verify

- [ ] **Step 5a: Build the mod**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build successful, 0 errors

- [ ] **Step 5b: Commit**

```bash
rtk git add NotepadHandler.cs NotepadPatches.cs Main.cs Loc.cs
rtk git commit -m "feat: add NotepadHandler for notepad window accessibility

Alt+R reads file content, file loaded announcements,
F1 help text. Auto-activates on notepad window focus."
```

---

## Task 2: MailHandler + MailPatches

**Files:**
- Create: `MailHandler.cs`
- Create: `MailPatches.cs`
- Modify: `Loc.cs` (add mail strings)
- Modify: `Main.cs` (register MailHandler)

### Step 1: Add localization strings

- [ ] **Step 1a: Add mail strings to Loc.cs**

After the notepad strings block, add:

```csharp
            // Mail
            _english["mail_login"] = "Mail login. Enter username and password.";
            _english["mail_inbox"] = "Inbox. {0} emails.";
            _english["mail_outbox"] = "Outbox. {0} emails.";
            _english["mail_no_emails"] = "No emails.";
            _english["mail_item"] = "{0} of {1}. From {2}. {3}. {4}";
            _english["mail_item_unread"] = "unread";
            _english["mail_item_read"] = "read";
            _english["mail_reading"] = "From {0}. Subject: {1}.";
            _english["mail_read_body"] = "{0}";
            _english["mail_empty_body"] = "Message is empty.";
            _english["mail_compose"] = "Compose email. Tab to move between fields.";
            _english["mail_reply"] = "Reply. Type your message.";
            _english["mail_deleted"] = "Email deleted. {0} emails remaining.";
            _english["mail_back_to_inbox"] = "Back to inbox.";
            _english["mail_switched_inbox"] = "Inbox.";
            _english["mail_switched_outbox"] = "Outbox.";
            _english["mail_focused"] = "Mail. {0}";
            _english["mail_help_login"] = "Mail login. Tab between username and password fields, Enter to log in. F1 for help.";
            _english["mail_help_inbox"] = "Mail inbox. Up Down to navigate emails, Enter to read, N to compose, Tab to switch inbox outbox, Delete to delete. F1 for help.";
            _english["mail_help_read"] = "Reading email. Alt R to read body, R to reply, Backspace to go back, Delete to delete. F1 for help.";
            _english["mail_help_compose"] = "Compose email. Tab between fields, Ctrl Enter to send, Escape to cancel. F1 for help.";
```

### Step 2: Create MailPatches.cs

- [ ] **Step 2a: Create the patches file**

Create `MailPatches.cs`:

```csharp
using System.Collections.Generic;
using HarmonyLib;
using MailConfig;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for email client accessibility.
    /// Announces inbox load, email selection, deletion, and panel changes.
    /// </summary>

    /// <summary>
    /// Patches MailWindow.ResumeLogin to announce inbox loaded.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "ResumeLogin")]
    public class MailResumeLoginPatch
    {
        static void Postfix(MailWindow __instance, UserMail userMail)
        {
            DebugLogger.LogState($"MailWindow.ResumeLogin: {userMail?.emails?.Count ?? 0} emails");
            MailHandler.OnInboxLoaded(__instance);
        }
    }

    /// <summary>
    /// Patches MailWindow.OnClickMail(int, bool) to announce email opened.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "OnClickMail", new[] { typeof(int), typeof(bool) })]
    public class MailOnClickMailPatch
    {
        static void Postfix(MailWindow __instance, int indexMail)
        {
            DebugLogger.LogState($"MailWindow.OnClickMail: index={indexMail}");
            MailHandler.OnEmailSelected(__instance);
        }
    }

    /// <summary>
    /// Patches MailWindow.ResumeDeleteMail to announce email deleted.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "ResumeDeleteMail")]
    public class MailResumeDeletePatch
    {
        static void Postfix(MailWindow __instance)
        {
            DebugLogger.LogState("MailWindow.ResumeDeleteMail");
            MailHandler.OnEmailDeleted(__instance);
        }
    }

    /// <summary>
    /// Patches MailWindow.OnShowPanelRedactar to announce compose panel.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "OnShowPanelRedactar")]
    public class MailShowComposePatch
    {
        static void Postfix(MailWindow __instance)
        {
            DebugLogger.LogState("MailWindow.OnShowPanelRedactar");
            MailHandler.OnComposeOpened();
        }
    }

    /// <summary>
    /// Patches MailWindow.ShowPanelPrincipal to announce return to inbox.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "ShowPanelPrincipal")]
    public class MailShowPrincipalPatch
    {
        static void Postfix(MailWindow __instance)
        {
            DebugLogger.LogState("MailWindow.ShowPanelPrincipal");
            MailHandler.OnReturnToInbox(__instance);
        }
    }
}
```

### Step 3: Create MailHandler.cs

- [ ] **Step 3a: Create the handler file**

Create `MailHandler.cs`:

```csharp
using System.Collections.Generic;
using MailConfig;
using TMPro;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for email client accessibility.
    /// Auto-activates when a MailWindow is focused.
    /// Up/Down navigates inbox, Enter reads email, R replies,
    /// N composes, Tab switches inbox/outbox, Delete deletes.
    /// </summary>
    public class MailHandler
    {
        #region Fields

        private static MailHandler _instance;
        private MailWindow _activeMail;
        private uDialog _activeDialog;
        private bool _isActive;
        private int _currentIndex;
        private List<Mail> _currentMails;
        private bool _inReadView;

        /// <summary>
        /// Which panel state the handler last detected.
        /// </summary>
        private enum PanelState { None, Login, Inbox, Read, Compose }
        private PanelState _panelState = PanelState.None;

        #endregion

        #region Static Entry

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>Called by patch when inbox is loaded.</summary>
        public static void OnInboxLoaded(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.HandleInboxLoaded(mail);
        }

        /// <summary>Called by patch when an email is selected for reading.</summary>
        public static void OnEmailSelected(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.HandleEmailSelected(mail);
        }

        /// <summary>Called by patch when an email is deleted.</summary>
        public static void OnEmailDeleted(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.HandleEmailDeleted(mail);
        }

        /// <summary>Called by patch when compose panel opens.</summary>
        public static void OnComposeOpened()
        {
            if (_instance == null || !_instance._isActive) return;
            _instance._panelState = PanelState.Compose;
            _instance._inReadView = false;
            ScreenReader.Say(Loc.Get("mail_compose"));
        }

        /// <summary>Called by patch when returning to inbox.</summary>
        public static void OnReturnToInbox(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance._inReadView = false;
            _instance._panelState = PanelState.Inbox;
            _instance.RefreshMailList();
            ScreenReader.Say(Loc.Get("mail_back_to_inbox"));
        }

        #endregion

        #region Public Properties

        /// <summary>Whether mail handler is currently active.</summary>
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

            if (_activeMail == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            DetectPanelState();
            return HandleInput();
        }

        /// <summary>Returns help text for F1.</summary>
        public string GetHelpText()
        {
            switch (_panelState)
            {
                case PanelState.Login: return Loc.Get("mail_help_login");
                case PanelState.Compose: return Loc.Get("mail_help_compose");
                case PanelState.Read: return Loc.Get("mail_help_read");
                default: return Loc.Get("mail_help_inbox");
            }
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

            if (currentTask == _activeDialog && _isActive) return;

            MailWindow mail = currentTask.GetComponentInChildren<MailWindow>();
            if (mail != null)
            {
                if (!_isActive || mail != _activeMail)
                {
                    Activate(mail, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(MailWindow mail, uDialog dialog)
        {
            _activeMail = mail;
            _activeDialog = dialog;
            _isActive = true;
            _currentIndex = 0;
            _inReadView = false;
            _currentMails = null;

            string title = dialog.TitleText ?? "Mail";
            ScreenReader.Say(Loc.Get("mail_focused", title));
            DetectPanelState();

            DebugLogger.LogState($"MailHandler: activated, title='{title}'");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeMail = null;
            _activeDialog = null;
            _currentMails = null;
            _panelState = PanelState.None;
        }

        #endregion

        #region Panel Detection

        private void DetectPanelState()
        {
            if (_activeMail == null) return;

            List<GameObject> panels = _activeMail.panelsMail;
            if (panels == null || panels.Count < 4) return;

            if (panels[3].activeInHierarchy)
            {
                if (_panelState != PanelState.Login)
                {
                    _panelState = PanelState.Login;
                }
            }
            else if (panels[1].activeInHierarchy)
            {
                if (_panelState != PanelState.Compose)
                {
                    _panelState = PanelState.Compose;
                }
            }
            else if (_inReadView)
            {
                _panelState = PanelState.Read;
            }
            else if (panels[0].activeInHierarchy)
            {
                _panelState = PanelState.Inbox;
            }
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            switch (_panelState)
            {
                case PanelState.Login:
                    // No special input - Tab/Enter handled natively
                    return false;

                case PanelState.Inbox:
                    return HandleInboxInput();

                case PanelState.Read:
                    return HandleReadInput();

                case PanelState.Compose:
                    return HandleComposeInput();

                default:
                    return false;
            }
        }

        private bool HandleInboxInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateMail(1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateMail(-1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenSelectedMail();
                return true;
            }

            // N = compose new
            if (Input.GetKeyDown(KeyCode.N))
            {
                _activeMail.OnShowPanelRedactar();
                return true;
            }

            // Tab = toggle inbox/outbox
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInboxOutbox();
                return true;
            }

            // Delete = delete selected
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelectedMail();
                return true;
            }

            // Space = repeat current
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceCurrentMail();
                return true;
            }

            return false;
        }

        private bool HandleReadInput()
        {
            // Alt+R = read body
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R))
            {
                ReadEmailBody();
                return true;
            }

            // R = reply
            if (Input.GetKeyDown(KeyCode.R))
            {
                _activeMail.OnShowReplyField();
                ScreenReader.Say(Loc.Get("mail_reply"));
                return true;
            }

            // Backspace = back to inbox
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                _activeMail.ShowPanelPrincipal();
                return true;
            }

            // Delete = delete this email
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                _activeMail.OnDeleteMail();
                return true;
            }

            // Space = repeat sender/subject
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceReadView();
                return true;
            }

            return false;
        }

        private bool HandleComposeInput()
        {
            // Ctrl+Enter = send
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                _activeMail.OnSendNewMessage();
                return true;
            }

            // Escape = cancel compose
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _activeMail.ShowPanelPrincipal();
                return true;
            }

            return false;
        }

        #endregion

        #region Mail Navigation

        private void RefreshMailList()
        {
            if (_activeMail == null) return;

            _currentMails = ReflectionHelper.GetField<List<Mail>>(_activeMail, "allMails");
            if (_currentMails == null)
            {
                _currentMails = new List<Mail>();
            }
        }

        private void NavigateMail(int direction)
        {
            RefreshMailList();

            if (_currentMails.Count == 0)
            {
                ScreenReader.Say(Loc.Get("mail_no_emails"));
                return;
            }

            int newIndex = _currentIndex + direction;
            if (newIndex < 0 || newIndex >= _currentMails.Count)
            {
                return;
            }

            _currentIndex = newIndex;
            AnnounceCurrentMail();
        }

        private void AnnounceCurrentMail()
        {
            RefreshMailList();

            if (_currentMails == null || _currentMails.Count == 0 || _currentIndex >= _currentMails.Count)
            {
                ScreenReader.Say(Loc.Get("mail_no_emails"));
                return;
            }

            Mail mail = _currentMails[_currentIndex];
            string address = mail.otherMail ?? "unknown";
            string subject = (mail.messages != null && mail.messages.Count > 0)
                ? mail.messages[0].titulo : "";
            string readStatus = mail.isUnread
                ? Loc.Get("mail_item_unread")
                : Loc.Get("mail_item_read");

            ScreenReader.Say(Loc.Get("mail_item",
                _currentIndex + 1, _currentMails.Count, address, subject, readStatus));
        }

        private void OpenSelectedMail()
        {
            RefreshMailList();

            if (_currentMails == null || _currentMails.Count == 0 || _currentIndex >= _currentMails.Count)
            {
                return;
            }

            // Call the private OnClickMail(int, bool) via reflection
            ReflectionHelper.InvokeMethod(_activeMail, "OnClickMail",
                new object[] { _currentIndex, true },
                new[] { typeof(int), typeof(bool) });

            _inReadView = true;
            _panelState = PanelState.Read;
        }

        private void DeleteSelectedMail()
        {
            if (_currentMails == null || _currentMails.Count == 0 || _currentIndex >= _currentMails.Count)
            {
                return;
            }

            // Select the mail first so selectedMail is set
            OpenSelectedMail();
            // Then trigger delete
            _activeMail.OnDeleteMail();
        }

        private void ToggleInboxOutbox()
        {
            if (_activeMail == null) return;

            // Check current state by reading titleLeftPanel
            TMP_Text titlePanel = _activeMail.titleLeftPanel;
            if (titlePanel != null && titlePanel.text == "Inbox")
            {
                _activeMail.OnShowOutbox();
                ScreenReader.Say(Loc.Get("mail_switched_outbox"));
            }
            else
            {
                _activeMail.OnShowInbox();
                ScreenReader.Say(Loc.Get("mail_switched_inbox"));
            }

            _currentIndex = 0;
            RefreshMailList();
        }

        #endregion

        #region Read View

        private void HandleEmailSelected(MailWindow mail)
        {
            _inReadView = true;
            _panelState = PanelState.Read;
            AnnounceReadView();
        }

        private void AnnounceReadView()
        {
            if (_activeMail == null) return;

            string address = _activeMail.readAddress?.text ?? "unknown";
            string subject = _activeMail.readSubject?.text ?? "";

            ScreenReader.Say(Loc.Get("mail_reading", address, subject));
        }

        private void ReadEmailBody()
        {
            if (_activeMail == null) return;

            Mail selected = ReflectionHelper.GetField<Mail>(_activeMail, "selectedMail");
            if (selected == null || selected.messages == null || selected.messages.Count == 0)
            {
                ScreenReader.Say(Loc.Get("mail_empty_body"));
                return;
            }

            // Read the most recent message body
            string body = selected.messages[selected.messages.Count - 1].mensaje;
            if (string.IsNullOrEmpty(body?.Trim()))
            {
                ScreenReader.Say(Loc.Get("mail_empty_body"));
                return;
            }

            // Strip rich text tags for screen reader
            string clean = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", "");
            string trimmed = clean.Length > 1000 ? clean.Substring(0, 1000) + "..." : clean;
            ScreenReader.Say(Loc.Get("mail_read_body", trimmed));
        }

        #endregion

        #region Inbox Loaded / Deleted

        private void HandleInboxLoaded(MailWindow mail)
        {
            _panelState = PanelState.Inbox;
            _inReadView = false;
            RefreshMailList();
            _currentIndex = 0;

            int count = _currentMails?.Count ?? 0;
            if (count > 0)
            {
                ScreenReader.Say(Loc.Get("mail_inbox", count));
            }
            else
            {
                ScreenReader.Say(Loc.Get("mail_no_emails"));
            }
        }

        private void HandleEmailDeleted(MailWindow mail)
        {
            _inReadView = false;
            _panelState = PanelState.Inbox;
            RefreshMailList();

            int count = _currentMails?.Count ?? 0;
            if (_currentIndex >= count && count > 0)
            {
                _currentIndex = count - 1;
            }

            ScreenReader.Say(Loc.Get("mail_deleted", count));
        }

        #endregion
    }
}
```

### Step 4: Register in Main.cs

- [ ] **Step 4a: Add field after NotepadHandler field**

```csharp
        private MailHandler _mailHandler;
```

- [ ] **Step 4b: Register in InitializeHandlers() after notepad registration**

```csharp
            _mailHandler = new MailHandler();
            _mailHandler.Register();
```

- [ ] **Step 4c: Add to UpdateHandlers() after notepad update**

```csharp
            // Mail consumes input when active
            if (_mailHandler.Update()) return;
```

- [ ] **Step 4d: Add to AnnounceHelp() after notepad help block**

```csharp
            if (_mailHandler.IsActive)
            {
                ScreenReader.Say(_mailHandler.GetHelpText());
                return;
            }
```

### Step 5: Build and commit

- [ ] **Step 5a: Build the mod**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build successful, 0 errors

- [ ] **Step 5b: Commit**

```bash
rtk git add MailHandler.cs MailPatches.cs Main.cs Loc.cs
rtk git commit -m "feat: add MailHandler for email client accessibility

Inbox navigation with Up/Down, Enter to read, R to reply,
N to compose, Tab to switch inbox/outbox, Delete to delete.
Announces email sender/subject/read status."
```

---

## Task 3: ChatHandler + ChatPatches

**Files:**
- Create: `ChatHandler.cs`
- Create: `ChatPatches.cs`
- Modify: `Loc.cs` (add chat strings)
- Modify: `Main.cs` (register ChatHandler)
- Modify: `ModConfig.cs` (add chat announcement setting)

### Step 1: Add localization strings and config

- [ ] **Step 1a: Add chat strings to Loc.cs**

After the mail strings block, add:

```csharp
            // Chat
            _english["chat_focused"] = "Chat. Channel: {0}.";
            _english["chat_nickname"] = "Chat. Enter a nickname to register.";
            _english["chat_nickname_registered"] = "Nickname registered. You can now chat.";
            _english["chat_channel_switched"] = "Channel: {0}.";
            _english["chat_message"] = "{0}: {1}";
            _english["chat_private_message"] = "Private from {0}: {1}";
            _english["chat_users"] = "{0} users in {1}: {2}";
            _english["chat_no_users"] = "No users in channel.";
            _english["chat_no_messages"] = "No messages.";
            _english["chat_no_channels"] = "No channels open.";
            _english["chat_history_item"] = "{0}: {1}";
            _english["chat_help"] = "Chat. Ctrl Up Down to read message history. Alt Left Right to switch channels. Alt U for user list. Alt C for channel list. F1 for help.";
```

- [ ] **Step 1b: Add chat announcement toggle to ModConfig.cs**

After the `_announceEmptyStates` field declaration (around line 17), add:

```csharp
        private static ConfigEntry<bool> _announceChatMessages;
```

After the `AnnounceEmptyStates` accessor (around line 27), add:

```csharp
        /// <summary>Whether to auto-announce incoming chat messages.</summary>
        public static bool AnnounceChatMessages => _announceChatMessages.Value;
```

In the `_settingNames` array, add `"Announce chat messages"` as a new entry.

In `Initialize()`, after the `_announceEmptyStates` binding, add:

```csharp
            _announceChatMessages = config.Bind("Accessibility",
                "AnnounceChatMessages", true,
                "Automatically announce new chat messages via screen reader");
```

In the settings menu `Update()` method, add handling for the new setting (toggle with Enter, following existing pattern for boolean settings).

### Step 2: Create ChatPatches.cs

- [ ] **Step 2a: Create the patches file**

Create `ChatPatches.cs`:

```csharp
using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for chat accessibility.
    /// Announces new messages, channel switches, and nickname registration.
    /// </summary>

    /// <summary>
    /// Patches ChatGuild.RecibeMensaje to announce new messages.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "RecibeMensaje")]
    public class ChatRecibeMensajePatch
    {
        static void Postfix(ChatGuild __instance, PlayerUtilsChat.ChatMessage message, bool __result)
        {
            if (__result)
            {
                DebugLogger.LogState($"ChatGuild.RecibeMensaje: {message.nickName}: {message.message?.Substring(0, System.Math.Min(50, message.message?.Length ?? 0))}");
                ChatHandler.OnMessageReceived(message);
            }
        }
    }

    /// <summary>
    /// Patches ChatGuild.RecibeMensajePrivado to announce private messages.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "RecibeMensajePrivado")]
    public class ChatRecibeMensajePrivadoPatch
    {
        static void Postfix(ChatGuild __instance, PlayerUtilsChat.ChatMessage message, string otherNickname, bool __result)
        {
            if (__result)
            {
                DebugLogger.LogState($"ChatGuild.RecibeMensajePrivado: from {otherNickname}");
                ChatHandler.OnPrivateMessageReceived(message, otherNickname);
            }
        }
    }

    /// <summary>
    /// Patches ChatGuild.OnSelectTab to announce channel switch.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "OnSelectTab")]
    public class ChatOnSelectTabPatch
    {
        static void Postfix(ChatGuild __instance, ChatGuild.NetworkChannelInfo channel)
        {
            DebugLogger.LogState($"ChatGuild.OnSelectTab: {channel?.channelName}");
            ChatHandler.OnChannelSwitched(channel);
        }
    }

    /// <summary>
    /// Patches ChatGuild.ResumeConnectionWindow(bool) to announce nickname registered.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "ResumeConnectionWindow", new[] { typeof(bool) })]
    public class ChatResumeConnectionPatch
    {
        static void Postfix(bool success)
        {
            if (success)
            {
                DebugLogger.LogState("ChatGuild: nickname registered");
                ChatHandler.OnNicknameRegistered();
            }
        }
    }
}
```

### Step 3: Create ChatHandler.cs

- [ ] **Step 3a: Create the handler file**

Create `ChatHandler.cs`:

```csharp
using System.Collections.Generic;
using ChatPoolSystem;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for chat window accessibility.
    /// Auto-activates when ChatGuild window is focused.
    /// Ctrl+Up/Down for message history, Alt+Left/Right for channel tabs,
    /// Alt+U for user list, Alt+C for channel list.
    /// </summary>
    public class ChatHandler
    {
        #region Fields

        private static ChatHandler _instance;
        private ChatGuild _activeChat;
        private uDialog _activeDialog;
        private bool _isActive;
        private int _historyIndex = -1;
        private bool _isNicknamePanel;

        #endregion

        #region Static Entry

        /// <summary>Registers this handler instance.</summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>Called by patch when a message is received.</summary>
        public static void OnMessageReceived(PlayerUtilsChat.ChatMessage message)
        {
            if (_instance == null) return;
            if (!ModConfig.AnnounceChatMessages) return;

            string nick = StripRichText(message.nickName ?? "");
            string msg = StripRichText(message.message ?? "");
            ScreenReader.Say(Loc.Get("chat_message", nick, msg));
        }

        /// <summary>Called by patch when a private message is received.</summary>
        public static void OnPrivateMessageReceived(PlayerUtilsChat.ChatMessage message, string otherNickname)
        {
            if (_instance == null) return;
            if (!ModConfig.AnnounceChatMessages) return;

            string nick = StripRichText(otherNickname ?? message.nickName ?? "");
            string msg = StripRichText(message.message ?? "");
            ScreenReader.Say(Loc.Get("chat_private_message", nick, msg));
        }

        /// <summary>Called by patch when channel tab is switched.</summary>
        public static void OnChannelSwitched(ChatGuild.NetworkChannelInfo channel)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance._historyIndex = -1;
            string name = channel?.channelName ?? "unknown";
            ScreenReader.Say(Loc.Get("chat_channel_switched", name));
        }

        /// <summary>Called by patch when nickname registration succeeds.</summary>
        public static void OnNicknameRegistered()
        {
            if (_instance == null) return;
            _instance._isNicknamePanel = false;
            ScreenReader.Say(Loc.Get("chat_nickname_registered"));
        }

        #endregion

        #region Public Properties

        /// <summary>Whether chat handler is currently active.</summary>
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

            if (_activeChat == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            return HandleInput();
        }

        /// <summary>Returns help text for F1.</summary>
        public string GetHelpText()
        {
            if (_isNicknamePanel)
            {
                return Loc.Get("chat_nickname");
            }
            return Loc.Get("chat_help");
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

            if (currentTask == _activeDialog && _isActive) return;

            ChatGuild chat = currentTask.GetComponentInChildren<ChatGuild>();
            if (chat != null)
            {
                if (!_isActive || chat != _activeChat)
                {
                    Activate(chat, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(ChatGuild chat, uDialog dialog)
        {
            _activeChat = chat;
            _activeDialog = dialog;
            _isActive = true;
            _historyIndex = -1;

            // Check if on nickname panel
            _isNicknamePanel = chat.panelNickName != null && chat.panelNickName.gameObject.activeInHierarchy;

            if (_isNicknamePanel)
            {
                ScreenReader.Say(Loc.Get("chat_nickname"));
            }
            else
            {
                var currentChannel = ReflectionHelper.GetField<ChatGuild.NetworkChannelInfo>(chat, "currentChannel");
                string channelName = currentChannel?.channelName ?? "general";
                ScreenReader.Say(Loc.Get("chat_focused", channelName));
            }

            DebugLogger.LogState($"ChatHandler: activated, nickname={_isNicknamePanel}");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeChat = null;
            _activeDialog = null;
            _historyIndex = -1;
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            if (_isNicknamePanel)
            {
                // Nickname panel - no special keys, native input works
                return false;
            }

            // Ctrl+Up = scroll history up (older)
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
                return true;
            }

            // Ctrl+Down = scroll history down (newer)
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
                return true;
            }

            // Alt+Left = previous channel tab
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SwitchChannel(-1);
                return true;
            }

            // Alt+Right = next channel tab
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.RightArrow))
            {
                SwitchChannel(1);
                return true;
            }

            // Alt+U = user list
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.U))
            {
                AnnounceUserList();
                return true;
            }

            // Alt+C = channel list
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
            {
                _activeChat.OnShowChannels();
                return true;
            }

            // Space = repeat channel info
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Only consume Space if input field is NOT focused
                if (_activeChat.inputField != null && _activeChat.inputField.isFocused)
                {
                    return false;
                }
                var currentChannel = ReflectionHelper.GetField<ChatGuild.NetworkChannelInfo>(
                    _activeChat, "currentChannel");
                string channelName = currentChannel?.channelName ?? "general";
                ScreenReader.Say(Loc.Get("chat_focused", channelName));
                return true;
            }

            return false;
        }

        #endregion

        #region Message History

        private void NavigateHistory(int direction)
        {
            var currentChannel = ReflectionHelper.GetField<ChatGuild.NetworkChannelInfo>(
                _activeChat, "currentChannel");
            if (currentChannel == null) return;

            GameObject panelChat = currentChannel.GetPanelChat();
            if (panelChat == null) return;

            ChatListAdapter adapter = panelChat.GetComponentInChildren<ChatListAdapter>();
            if (adapter == null) return;

            int itemCount = adapter.GetItemsCount();
            if (itemCount == 0)
            {
                ScreenReader.Say(Loc.Get("chat_no_messages"));
                return;
            }

            // Initialize history index at the end (most recent)
            if (_historyIndex < 0)
            {
                _historyIndex = itemCount - 1;
            }
            else
            {
                _historyIndex += direction;
            }

            // Clamp
            if (_historyIndex < 0) _historyIndex = 0;
            if (_historyIndex >= itemCount) _historyIndex = itemCount - 1;

            // Read the message at this index
            var data = adapter.Data;
            if (data != null && _historyIndex < data.Count)
            {
                string nick = StripRichText(data[_historyIndex].nickname ?? "");
                string msg = StripRichText(data[_historyIndex].message ?? "");
                if (string.IsNullOrEmpty(nick))
                {
                    ScreenReader.Say(msg);
                }
                else
                {
                    ScreenReader.Say(Loc.Get("chat_history_item", nick, msg));
                }
            }
        }

        #endregion

        #region Channel Switching

        private void SwitchChannel(int direction)
        {
            var activeChannels = ReflectionHelper.GetField<List<ChatGuild.NetworkChannelInfo>>(
                _activeChat, "activeChannels");
            var currentChannel = ReflectionHelper.GetField<ChatGuild.NetworkChannelInfo>(
                _activeChat, "currentChannel");

            if (activeChannels == null || activeChannels.Count == 0)
            {
                ScreenReader.Say(Loc.Get("chat_no_channels"));
                return;
            }

            int currentIdx = activeChannels.IndexOf(currentChannel);
            int newIdx = currentIdx + direction;

            if (newIdx < 0) newIdx = activeChannels.Count - 1;
            if (newIdx >= activeChannels.Count) newIdx = 0;

            _activeChat.OnSelectTab(activeChannels[newIdx]);
            _historyIndex = -1;
        }

        #endregion

        #region User List

        private void AnnounceUserList()
        {
            var currentChannel = ReflectionHelper.GetField<ChatGuild.NetworkChannelInfo>(
                _activeChat, "currentChannel");
            if (currentChannel == null) return;

            GameObject panelUsers = currentChannel.GetPanelUsers();
            if (panelUsers == null)
            {
                ScreenReader.Say(Loc.Get("chat_no_users"));
                return;
            }

            UserChatList[] users = panelUsers.GetComponentsInChildren<UserChatList>(true);
            if (users.Length == 0)
            {
                ScreenReader.Say(Loc.Get("chat_no_users"));
                return;
            }

            string channelName = currentChannel.channelName ?? "channel";
            List<string> names = new List<string>();
            foreach (var user in users)
            {
                var userChat = user.GetUserChat();
                if (userChat != null)
                {
                    names.Add(userChat.nickName);
                }
            }

            string nameList = string.Join(", ", names);
            ScreenReader.Say(Loc.Get("chat_users", users.Length, channelName, nameList));
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Strips Unity rich text tags from a string for clean screen reader output.
        /// </summary>
        private static string StripRichText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", "");
        }

        #endregion
    }
}
```

### Step 4: Register in Main.cs

- [ ] **Step 4a: Add field after MailHandler field**

```csharp
        private ChatHandler _chatHandler;
```

- [ ] **Step 4b: Register in InitializeHandlers() after mail registration**

```csharp
            _chatHandler = new ChatHandler();
            _chatHandler.Register();
```

- [ ] **Step 4c: Add to UpdateHandlers() after mail update**

```csharp
            // Chat consumes input when active
            if (_chatHandler.Update()) return;
```

- [ ] **Step 4d: Add to AnnounceHelp() after mail help block**

```csharp
            if (_chatHandler.IsActive)
            {
                ScreenReader.Say(_chatHandler.GetHelpText());
                return;
            }
```

### Step 5: Build and commit

- [ ] **Step 5a: Build the mod**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build successful, 0 errors

- [ ] **Step 5b: Commit**

```bash
rtk git add ChatHandler.cs ChatPatches.cs Main.cs Loc.cs ModConfig.cs
rtk git commit -m "feat: add ChatHandler for chat window accessibility

Ctrl+Up/Down for message history, Alt+Left/Right for channel tabs,
Alt+U for user list, Alt+C for channel list. Auto-announces
new messages (configurable). Private message announcements."
```

---

## Task 4: Update project status and documentation

**Files:**
- Modify: `project_status.md`
- Modify: `docs/game-api.md`

### Step 1: Update project status

- [ ] **Step 1a: Update project_status.md**

Update "Currently working on" to: "Phase 3 Communication complete (Notepad, Email, Chat). Ready for testing."

Add to "Implemented Features":
```
- Notepad accessibility (NotepadHandler) - announces file name on focus, Alt+R reads content, file loaded announcements
- Email client accessibility (MailHandler) - inbox navigation with Up/Down, Enter to read, R to reply, N to compose, Tab to switch inbox/outbox, Delete to delete
- Chat accessibility (ChatHandler) - Ctrl+Up/Down message history, Alt+Left/Right channel tabs, Alt+U user list, auto-announces new messages
```

Add to "Key Bindings (Mod)":
```
### Notepad (when notepad window is focused)

- Alt+R: Read file content

### Mail (when mail window is focused)

- Up/Down: Navigate email list
- Enter: Read selected email
- N: Compose new email
- R: Reply to email (in read view)
- Alt+R: Read email body (in read view)
- Tab: Toggle Inbox/Outbox
- Delete: Delete email
- Backspace: Back to inbox (from read view)
- Escape: Cancel compose
- Ctrl+Enter: Send email (in compose)
- Space: Repeat current

### Chat (when chat window is focused)

- Ctrl+Up/Down: Scroll message history
- Alt+Left/Right: Switch channel tabs
- Alt+U: Announce user list
- Alt+C: Open channel list
- Space: Repeat channel info
```

Add "Pending Tests" sections for all three features (see test cases in spec).

- [ ] **Step 1b: Update game-api.md**

Add MailWindow, ChatGuild, and Notepad analysis to the game-api.md under appropriate sections, documenting key fields, methods, and patch points.

### Step 2: Commit

- [ ] **Step 2a: Commit documentation**

```bash
rtk git add project_status.md docs/game-api.md
rtk git commit -m "docs: update project status with Phase 3 communication features"
```

---

## Task 5: Deploy and test

- [ ] **Step 5a: Deploy the mod**

Run: `powershell -File scripts/Deploy-Mod.ps1`

- [ ] **Step 5b: Test Notepad**

Test cases:
1. Open a text file from file explorer or terminal (`nano`): hear "Notepad. [filename]"
2. Alt+R: hear first ~500 chars of file content
3. Alt+R on empty file: hear "File is empty"
4. F1: hear notepad help
5. Switch away and back: hear focus announcement again
6. Ctrl+S: hear save notification (via NotificationHandler)

- [ ] **Step 5c: Test Email**

Test cases:
1. Open Mail.exe: hear "Mail login" or auto-login -> "Inbox. N emails."
2. Up/Down: navigate emails with position/sender/subject/read-status
3. Enter: read email, hear "From [address]. Subject: [subject]."
4. Alt+R in read view: hear email body text
5. R in read view: hear "Reply. Type your message."
6. Backspace: back to inbox
7. N: compose panel opens, hear "Compose email."
8. Tab: toggle Inbox/Outbox, hear which view
9. Delete: delete email, hear confirmation
10. Space: repeat current announcement
11. F1: hear context-appropriate help

- [ ] **Step 5d: Test Chat**

Test cases:
1. Open Chat: hear "Chat. Enter a nickname" or "Chat. Channel: general."
2. Ctrl+Up/Down: scroll through message history, hear each message
3. Alt+Left/Right: switch channels, hear channel name
4. Alt+U: hear user count and names
5. Alt+C: channel list opens
6. New message arrives: hear "[nick]: [message]" automatically
7. Private message: hear "Private from [nick]: [message]"
8. Ctrl+F11 -> toggle chat announcements off -> messages no longer auto-announced
9. Space: repeat channel info
10. F1: hear chat help
