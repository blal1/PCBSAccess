# Phase 3: Communication Features Design

## Overview

Three accessibility handlers for Grey Hack's communication systems: Notepad, Email, and Chat. Built in order of complexity (Notepad -> Email -> Chat).

## 3.3 NotepadHandler (Simple)

### Trigger

Detected when a focused window's title starts with "Notepad.exe". Activates via the existing WindowFocusHandler focus-change flow.

### Announcements

- **On focus:** "Notepad. [filename]" (parsed from uDialog.TitleText which is "Notepad.exe - /path")
- **On file loaded** (patch `Notepad.ResumeConnectionWindow(string)`): "File loaded. [filename]"
- **On file saved** (patch `Notepad.CloseConnection(string)` when msg is empty): notification already fires via NotificationHandler
- **Unsaved changes dialog:** already handled by DialogHandler

### Key Bindings (when notepad is focused)

- **F1:** Notepad help
- **Alt+R:** Read first ~500 characters of content via `listAdapter.GetText()`

### Patch Points

- `Notepad.ResumeConnectionWindow(string info)` - Postfix: file loaded
- No additional patches needed; save/close already covered by existing handlers

### Data Access

- `Notepad.listAdapter` (protected) - access via Reflection for `GetText()`
- `Notepad.rutaArchivo` (protected) - file path
- `uDialog.TitleText` - window title

---

## 3.1 MailHandler (Medium)

### Trigger

Detected when focused window title contains "Mail". Uses `MailWindow` component lookup on the focused window's GameObject.

### Panel States

MailWindow has 4 panels (`panelsMail` list):
- `[0]` MAIN - inbox/outbox mail list
- `[1]` WRITE - compose new email
- `[2]` OPTION/READ - reading a selected email
- `[3]` LOGIN - username/password login

Handler tracks which panel is active by checking `panelsMail[i].activeInHierarchy`.

### Login Panel

- Announce: "Mail login. Enter username and password."
- Tab between fields works natively
- F1: Mail help

### Inbox/Outbox Panel

- **On `ResumeLogin` (inbox loaded):** "Inbox. [N] emails." Auto-select first.
- **Up/Down:** Navigate `allMails` list. Announce: "[position] of [total]. From [address]. [subject]. [unread/read]."
- **Enter:** Open selected email for reading (triggers `OnClickMail`)
- **Tab:** Toggle between Inbox and Outbox views
- **N:** Open compose panel (triggers `OnShowPanelRedactar`)
- **Delete:** Delete selected email (triggers `OnDeleteMail`)
- **Space:** Repeat current email info
- **F1:** Mail help

### Read View

Entered after selecting an email. `panelsMail[2]` active, `panelsMail[0]` also active (split view).

- **On email selected:** "From [readAddress.text]. Subject: [readSubject.text]."
- **Alt+R:** Read message body text (from reply objects' TMP_Text)
- **R:** Open reply field. Announce "Reply. Type your message."
- **Backspace:** Return to inbox list (calls `ShowPanelPrincipal`)
- **Delete:** Delete this email
- **Space:** Repeat sender and subject

### Compose Panel

`panelsMail[1]` active.

- **On open:** "Compose email. Tab to move between fields."
- Tab cycles: To address -> Subject -> Body (native TabInputField)
- **Ctrl+Enter:** Send email
- **Escape:** Cancel compose, return to inbox

### Patch Points

- `MailWindow.ResumeLogin(UserMail, bool)` - Postfix: inbox loaded, announce count
- `MailWindow.OnClickMail(int, bool)` - Postfix: email selected for reading
- `MailWindow.ResumeDeleteMail(string)` - Postfix: email deleted confirmation
- `MailWindow.OnShowPanelRedactar()` - Postfix: compose panel opened
- `MailWindow.ShowPanelPrincipal()` - Postfix: returned to inbox/main view

### Data Access (via Reflection/Harmony)

- `allMails` (private List of Mail) - email list
- `selectedMail` (private Mail) - currently viewed email
- `userMail` (private UserMail) - logged-in user info
- `mainListAdapter` (public MailListAdapter) - list UI with Data property
- `readAddress` (public TMP_Text) - displayed sender
- `readSubject` (public TMP_Text) - displayed subject
- `panelsMail` (public List of GameObject) - panel references

---

## 3.2 ChatHandler (Complex)

### Trigger

Detected when focused window contains `ChatGuild` component. `ChatGuild.Singleton` is also available globally.

### Nickname Registration Panel

If `panelNickName` is active (first-time user):
- Announce: "Chat. Enter a nickname to register."
- Enter submits nickname
- F1: Chat help

### Main Chat Panel

- **On focus:** "Chat. Channel: [channelName]. [N] users."
- **Ctrl+Up/Ctrl+Down:** Scroll through message history in current channel's ChatListAdapter. Announce each message: "[nickname]: [message]"
- **Alt+U:** Announce user list: "[N] users in [channel]: [name1], [name2], ..."
- **Alt+Left/Alt+Right:** Switch between open channel tabs. Announce: "Channel: [name]."
- **Alt+C:** Open channel list panel for joining. Up/Down to navigate channels, Enter to join.
- **Escape:** Close chat (game-native)
- **Space:** Repeat current channel info
- **F1:** Chat help

### New Message Announcements

- On `RecibeMensaje`: announce "[nickname]: [message]" if chat window exists
- On `RecibeMensajePrivado`: announce "Private from [nickname]: [message]"
- **Default: ON.** Configurable via mod settings (Ctrl+F11). When off, messages are only spoken when manually scrolling history.
- Only announce if the message is in the currently selected channel (to avoid noise from background channels)

### Patch Points

- `ChatGuild.RecibeMensaje(PlayerUtilsChat.ChatMessage)` - Postfix: new message received
- `ChatGuild.RecibeMensajePrivado(PlayerUtilsChat.ChatMessage, string)` - Postfix: private message
- `ChatGuild.OnSelectTab(NetworkChannelInfo)` - Postfix: channel switched
- `ChatGuild.ResumeSendUsers(byte[], string)` - Postfix: user list updated
- `ChatGuild.ResumeConnectionWindow(bool)` - Postfix: nickname registered successfully

### Data Access

- `ChatGuild.Singleton` (public static) - singleton access
- `currentChannel` (private NetworkChannelInfo) - via Reflection
- `activeChannels` (private List) - via Reflection
- `inputField` (public TMP_InputField) - message input
- `panelNickName` (public RectTransform) - nickname panel visibility
- `panelChat` (public RectTransform) - chat panel visibility
- ChatListAdapter on each channel's panel - message history

---

## File Structure

Each feature = 1 Handler + 1 Patches file, following existing patterns:

- `NotepadHandler.cs` + `NotepadPatches.cs`
- `MailHandler.cs` + `MailPatches.cs`
- `ChatHandler.cs` + `ChatPatches.cs`

Registration in `Main.cs` following existing handler pattern.

## Key Bindings Summary (new)

- **Alt+R:** Read content (Notepad: file content, Mail read view: message body)
- **R:** Reply (Mail read view only)
- **N:** New/compose email (Mail inbox only)
- **Tab:** Toggle Inbox/Outbox (Mail inbox only)
- **Ctrl+Up/Ctrl+Down:** Scroll chat history
- **Alt+U:** User list (Chat)
- **Alt+Left/Alt+Right:** Switch chat tabs
- **Alt+C:** Channel list (Chat)

## Implementation Order

1. NotepadHandler + NotepadPatches (simplest, ~1 session)
2. MailHandler + MailPatches (medium, ~1-2 sessions)
3. ChatHandler + ChatPatches (complex, ~1-2 sessions)
