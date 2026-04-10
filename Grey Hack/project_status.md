# Project Status: GreyHackAccess

## Project Info

- **Game:** Grey Hack
- **Engine:** Unity 2022.3.9f1
- **Architecture:** 64-bit
- **Mod Loader:** BepInEx 5.4.23.5
- **Runtime:** net472 (CLR 4.0, Mono)
- **Game directory:** C:\Users\bilal\Downloads\Gray Hack
- **User experience level:** Little/None
- **User game familiarity:** Not at all

## Setup Progress

- [x] Experience level determined
- [x] Game name and path confirmed
- [x] Game familiarity assessed
- [x] Game directory auto-check completed
- [x] Mod loader selected and installed (BepInEx)
- [x] Tolk DLLs in place
- [x] .NET SDK available (9.0.305)
- [x] Decompiler tool ready (ilspycmd 9.1)
- [x] Game code decompiled to `decompiled/` (2608 files, reverified 2026-04-08)
- [ ] Tutorial texts extracted
- [x] Language support decided (English only)
- [x] Project directory set up (csproj, Main.cs, etc.)
- [x] CLAUDE.md updated with project-specific values
- [x] First build successful (0 warnings, 0 errors)
- [x] "Mod loaded" announcement working in game (NVDA detected, F1/F12 confirmed working)

## Current Phase

**Phase:** Implementation
**Currently working on:** Phase 4 Browser Handler implementation complete. Ready for in-game testing.
**Blocked by:** Nothing

## Codebase Analysis Progress

### GATE: Tier 1 MUST be complete before Phase 2 (Framework)!

- [x] 1.1 Structure overview (namespaces, singletons) -> documented in game-api.md
- [x] 1.2 Input system - ALL game key bindings documented in game-api.md "Game Key Bindings"
- [x] 1.2 Input system - Safe mod keys identified and listed in game-api.md "Safe Mod Keys"
- [x] 1.3 UI system (base classes, text access patterns, Reflection needed?)
- [x] 1.4 State management decision -> documented in "Architecture Decisions" below
- [ ] 1.5 Localization: game's language system analyzed (only if multilingual)

### GATE: Relevant Tier 2 items MUST be done before implementing each feature!

- [ ] 1.6 Game mechanics (analyzed as needed per feature)
- [ ] 1.7 Status/feedback systems
- [ ] 1.8 Event system / Harmony patch points
- [ ] 1.9 Results documented in `docs/game-api.md`
- [ ] 1.10 Tutorial analysis (when relevant)

## Game Key Bindings (Original)

- (not yet documented - MUST be done before Phase 2)

## Implemented Features

- BiosMenu (main menu) keyboard navigation and screen reader announcements
- Boot sequence accessibility (BootUpHandler) - announces boot phases, first install form, errors, desktop ready
- Terminal accessibility (TerminalHandler) - announces output, focus, command history navigation
- Window focus accessibility (WindowFocusHandler) - announces window open, close, minimize, focus changes
- Task switcher accessibility (TaskSwitcherHandler) - announces Ctrl+Tab window cycling
- Desktop icon navigation (DesktopIconHandler) - Alt+D keyboard navigation of desktop icons
- Start menu accessibility (StartMenuHandler) - Alt+S keyboard navigation of start menu and Programs submenu
- File explorer accessibility (FileExplorerHandler) - auto-activates when file explorer focused, keyboard file/folder navigation
- Notification accessibility (NotificationHandler) - announces notifications via screen reader, Alt+N to repeat last
- Intro cutscene accessibility (IntroHandler) - announces intro phases, skip feedback, loading transition
- Terminal auto-focus (WindowFocusHandler) - auto-selects SelectableTerminal when terminal window opens/focuses, fixes post-tutorial focus loss
- Context menu accessibility (ContextMenuHandler) - Shift+F10 opens context menus in terminal/file explorer, Up/Down/Enter/Escape navigation
- Game over detail view (DialogHandler) - Left/Right navigates buttons, Enter on Show Details for Up/Down trace navigation
- Notepad accessibility (NotepadHandler) - announces file name on focus, Alt+R reads content, file loaded announcements
- Email client accessibility (MailHandler) - inbox navigation with Up/Down, Enter to read, R to reply, N to compose, Tab to switch inbox/outbox, Delete to delete
- Chat accessibility (ChatHandler) - Ctrl+Up/Down message history, Alt+Left/Right channel tabs, Alt+U user list, auto-announces new messages (configurable)
- Tutorial accessibility (TutorialHandler) - auto-reads tutorial pages, Enter to advance, Escape to skip, Space to re-read, pending action announcements, wrong action feedback
- Browser accessibility (BrowserHandler + 15 sub-handlers) - coordinator pattern with panel-specific sub-handlers for all HtmlBrowser panels: web pages, search, bank (login/register/account), shop, hack shop (tools/exploits), router config (ports/firewall/help), jobs, police reports, CCTV, ISP, cryptocurrency, CTF, device finder. PreBuy dialog handling for purchases.

## Pending Tests

### BiosMenu (all passed)

- [x] Build the mod and verify it compiles
- [x] Start the game and check if "GreyHackAccess loaded" is announced
- [x] Launch game: hear "Grey Hack. Press Escape to skip intro." during logo
- [x] Press Escape: hear "Main Menu. 6 options. Up Down to navigate, Enter to select." then "1 of 6: PLAY"
- [x] Up/Down arrows navigate menu items with position announcements
- [x] Enter on PLAY: transitions to game mode selection (Online / Single Player)
- [x] Enter on game mode: transitions to play options (toggles + Start Game)
- [x] Enter on toggles: flips checkbox, announces checked/unchecked
- [x] Backspace: goes back one layer with announcement
- [x] Space: repeats current item
- [x] F1 in menu: announces menu-specific help
- [x] Start single player: hear "Starting single player. Loading."
- [x] Start multiplayer: hear "Connecting to server" and status updates

### Boot Sequence (all passed)

- [x] Start single player game: hear "Booting up. Press Delete to cancel and return to menu."
- [x] Wait for boot to finish: hear "Desktop loaded."
- [x] F1 during boot: hear boot-specific help
- [x] Delete during BIOS phase: boot cancels, returns to menu
- [x] Start with Wipe Computer checked: hear "Booting up. First time setup will begin after boot."
- [x] First install form: hear "First time setup. Enter your username, computer name, and password."
- [x] Tab between fields: each field name announced (Username, Computer name, Password, Confirm password)
- [x] Submit with empty fields: hear "Error: Username, password and computer name can't be empty"
- [x] Submit with valid data: hear "Installing. Please wait." then "Installation complete."
- [x] Error dialogs announced via screen reader (any OS.ShowError)

### Terminal Handler (all passed)

- [x] Boot into desktop: terminal opens and hear "Terminal. [user] at [path]"
- [x] Type a command (e.g. `ls`) and press Enter: hear the output
- [x] Type `whoami` and press Enter: hear the username
- [x] Press Up arrow: hear recalled command from history
- [x] Press Down arrow: hear next command in history
- [x] Press F1 while terminal focused: hear terminal help text
- [x] Long output (e.g. `cat` a big file): hear summarized output, not hang
- [x] Run a command that takes time: no output announced until result arrives
- [x] Ctrl+C to cancel command: hear "^C"
- [x] Click away from terminal then back: hear focus announcement again

### Window Focus Handler (all passed)

- [x] Boot to desktop: hear "Terminal, opened" when initial terminal appears
- [x] Open a second program (e.g. right-click desktop, open file manager): hear "[title], opened"
- [x] Click between two open windows: hear the focused window's title
- [x] Minimize a window: hear "[title], minimized"
- [x] Close a window: hear "[title], closed"
- [x] F1 when a non-terminal window is focused: hear "In [title]. Ctrl Tab to switch windows."
- [x] No double announcements when window opens (should hear "opened" once, not "opened" then title again)

### Task Switcher (all passed)

- [x] Ctrl+Tab with 2+ windows open: hear "Task switcher. [N] windows. [title]"
- [x] Ctrl+Tab again (while still held): hear next window title with position
- [x] Release Ctrl: hear "Switched to [title]"
- [x] Ctrl+Tab then Escape: hear "Task switcher cancelled"
- [x] Ctrl+Tab with 1 window: hear "Task switcher. 1 windows. [title]"

### Desktop Icon Navigation (all passed)

- [x] Alt+D on desktop: hear "Desktop icons. [N] items. Up Down to navigate..."
- [x] Down arrow: hear next icon name with type and position
- [x] Up arrow: hear previous icon name
- [x] Wrap past last icon: hear "First item" then first icon
- [x] Wrap before first icon: hear "Last item" then last icon
- [x] Enter on an icon: hear "Opening [name]", program/folder opens
- [x] Escape: hear "Desktop icons closed"
- [x] Space: repeats current icon announcement
- [x] F1 during icon nav: hear desktop icon help
- [x] Alt+D when no icons: hear "No desktop icons"

### Start Menu

- [x] Alt+S on desktop: hear "Start menu. [N] items. Up Down to navigate..."
- [x] Down arrow: hear next item with position
- [x] Up arrow: hear previous item
- [x] Wrap past last: hear "First item" then first item
- [x] Wrap before first: hear "Last item" then last item
- [x] Enter on Programs: hear "Programs. [N] items..." then first program
- [x] Down/Up in Programs submenu: navigate programs
- [x] Enter on a program: hear "Launching [name]", program opens
- [x] Left arrow in submenu: hear "Back to start menu" and return to main items
- [x] Enter on Preferences: hear "Opening Preferences", settings window opens
- [x] Enter on Reboot/Shutdown: respective action triggers
- [x] Escape: hear "Start menu closed"
- [x] Space: repeats current item
- [x] F1 during menu: hear start menu help
- [x] Menu auto-closes if clicked outside

### File Explorer

- [x] Open file explorer (double-click folder on desktop): hear "File explorer. [path]. [N] items."
- [x] Down arrow: hear next file with type and position
- [x] Up arrow: hear previous file
- [x] Wrap past last: hear "First item" then first file
- [x] Wrap before first: hear "Last item" then last file
- [x] Enter on folder: hear "Opening folder [name]", navigates into it
- [x] Enter on text file: hear "Opening [name]", opens in notepad/editor
- [x] Enter on program: hear "Opening [name]", launches program
- [x] Backspace: hear "Going up", navigates to parent folder
- [x] Alt+Left: hear "Going back", goes back in history
- [x] Alt+Right: hear "Going forward"
- [x] Alt+Home: hear "Going home", goes to home directory
- [x] Space: repeats current file announcement
- [x] F1 during file explorer: hear file explorer help
- [x] Switch to another window and back: file explorer re-announces
- [x] Shift+F10 on a file: hear context menu options
- [x] F2 on a file: hear "Renaming [name]", input field activated
- [x] Delete on a file: hear "Deleting [name]", file moves to trash

### Notification Handler (all passed)

- [x] Play until a notification appears (e.g. connect to multiplayer, receive mail): hear the notification text
- [x] Warning notification: hear "Warning: [message]"
- [x] Mail notification: hear "Mail: [message]"
- [x] Info notification: hear just the message text (no prefix)
- [x] Alt+N after a notification: hear "Last notification: [message]"
- [x] Alt+N with no notifications yet: hear "No notifications yet"
- [x] Multiple notifications: Alt+N repeats only the most recent one

### Context Menu Handler (all passed)

- [x] File explorer Shift+F10: hear option count and first option
- [x] Up/Down navigates options with position announcements
- [x] Enter selects option, menu closes
- [x] Escape closes menu, hear "Context menu closed"
- [x] Space repeats current option
- [x] Terminal Shift+F10: hear Copy/Paste context menu
- [x] Shift+F10 with no context: hear "No context menu available here"

### Game Over Detail View (deferred)

- [ ] Game over appears: hear title, message, button count
- [ ] Left/Right navigates buttons
- [ ] Enter on Show Details: hear trace count, Up/Down navigates traces
- [ ] Enter on Copy Log: hear "Trace log copied to clipboard"
- [ ] Enter on Close: disconnects

### Notepad Handler

- [x] Open a text file from file explorer: hear "Notepad. [filename]"
- [x] Alt+R: hear first ~500 chars of file content
- [x] Alt+R on empty file: hear "File is empty"
- [x] F1: hear notepad help
- [x] Switch away and back: hear focus announcement again
- [x] Ctrl+S: hear save notification via NotificationHandler

### Email Client (MailHandler)

- [x] Open Mail.exe: hear "Mail login" or auto-login then "Inbox. N emails."
- [x] Up/Down: navigate emails with position, sender, subject, read status
- [x] Enter: read email, hear "From [address]. Subject: [subject]."
- [x] Alt+R in read view: hear email body text
- [x] R in read view: hear "Reply. Type your message."
- [x] Backspace: back to inbox
- [x] N: compose panel opens, hear "Compose email."
- [x] Tab: toggle Inbox/Outbox, hear which view
- [x] Delete: delete email, hear confirmation
- [x] Space: repeat current announcement
- [x] F1: hear context-appropriate help

### Chat (ChatHandler)

- [x] Open Chat: hear "Chat. Enter a nickname" or "Chat. Channel: general."
- [x] Ctrl+Up/Down: scroll through message history, hear each message
- [x] Alt+Left/Right: switch channels, hear channel name
- [x] Alt+U: hear user count and names
- [x] Alt+C: channel list opens
- [x] New message arrives: hear "[nick]: [message]" automatically
- [x] Private message: hear "Private from [nick]: [message]"
- [x] Ctrl+F11 -> toggle chat announcements off -> messages no longer auto-announced
- [x] Space: repeat channel info
- [x] F1: hear chat help

### Tutorial Handler

- [x] Start a new single player game (with tutorials enabled): tutorial window opens alongside file explorer
- [x] Tutorial opens: hear "Tutorial. Page 1 of N." followed by page text
- [x] Enter: advances to next page, hear new page number and text
- [x] Space: re-reads current page text
- [x] Escape: hear skip tutorial confirmation dialog (handled by DialogHandler)
- [x] Page with pending action: hear "Action required: [description]"
- [x] Complete the pending action (e.g. open terminal): tutorial auto-advances, hear new page
- [x] Enter on page with pending action (not completed): hear the action requirement again
- [x] Wrong action performed: hear the error feedback text
- [x] Switch to another window and back to tutorial: hear page re-announced
- [x] F1: hear tutorial help text
- [x] Last page reached with no more pages: hear "Tutorial complete."

### Browser Handler

- [ ] Open browser: hear panel announcement (e.g. "Search. Type query...")
- [ ] Visit a website: hear "Web page. N links."
- [ ] Up/Down on web page: navigate links with position
- [ ] Enter on link: activates link, panel switches
- [ ] Alt+Left/Right: browser back/forward announced
- [ ] Alt+Home: navigate to search home
- [ ] Alt+R: read current page content
- [ ] Navigate to bank login: hear "Bank login."
- [ ] Log in: hear "Logging in..." then account info
- [ ] Up/Down in bank account: browse transactions
- [ ] Open shop: hear "Shop. N items."
- [ ] Up/Down in shop: browse items with name, description, price
- [ ] Enter on item: PreBuy dialog announced with name, price
- [ ] Up/Down in PreBuy: cycle versions, Tab toggles source code
- [ ] Enter in PreBuy: buy, Escape to cancel
- [ ] Open hack shop: hear tools/exploits panel
- [ ] Alt+F: cycle library/filter
- [ ] Open router config: hear port forwarding rules or firewall rules
- [ ] Enter on port rule: edit mode with Tab field cycling
- [ ] N to add rule, Delete to remove
- [ ] Firewall: Left/Right for Allow/Deny, Alt+A for Any toggle
- [ ] Open jobs: hear mission count, Up/Down browse, Enter for details
- [ ] CCTV: hear camera info
- [ ] ISP, Currency, CTF, FindDevice panels: each announced on activation
- [ ] F1 in any browser panel: hear panel-specific help
- [ ] Space in browser: repeat current state

## Known Issues

- (none yet)

## Architecture Decisions

- Using BepInEx (community standard for Grey Hack modding)
- English only localization (single language, Loc.cs still used for string centralization)
- State management: simple boolean flags per handler (no AccessStateManager yet - only 1 handler)

## Key Bindings (Mod)

- F1: Help
- F12: Toggle debug mode
- Ctrl+F11: Mod settings
- Alt+D: Toggle desktop icon navigation
- Up/Down: Navigate icons (in icon nav mode)
- Enter: Open icon (in icon nav mode)
- Escape: Exit icon navigation
- Space: Repeat current icon
- Alt+S: Toggle start menu
- Alt+N: Repeat last notification
- Shift+F10: Open context menu (terminal, file explorer)

### Context Menu (when context menu is open)

- Up/Down: Navigate options
- Enter: Select option
- Escape: Close menu
- Space: Repeat current option

### Dialog (when dialog is active)

- Left/Right: Navigate buttons
- Enter: Activate button
- Escape: Dismiss (Cancel/No/OK)
- Up/Down: Navigate traces (game over detail view)

### File Explorer (when file explorer window is focused)

- Up/Down: Navigate files
- Enter: Open file or folder
- Backspace: Go up one folder
- Alt+Left: Go back in history
- Alt+Right: Go forward in history
- Alt+Home: Go to home directory
- Space: Repeat current file
- Shift+F10: Context menu for selected file
- F2: Rename selected file
- Delete: Delete selected file

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

### Browser (when browser window is focused)

- Alt+Left: Go back in browser history
- Alt+Right: Go forward in browser history
- Alt+Home: Go to search home page
- Alt+R: Read current page/panel content
- Space: Repeat current state
- Up/Down: Navigate links/items/rules (panel-dependent)
- Enter: Activate link/buy/edit (panel-dependent)
- Tab: Cycle fields (bank/router edit mode)
- N: Add new rule (router config)
- Delete: Remove rule (router config)
- Left/Right: Toggle Allow/Deny (firewall edit mode)
- Alt+A: Toggle Any (firewall edit mode)
- Alt+F: Cycle filter/library (shop/hack shop)
- Alt+P: Cycle permission filter (hack shop exploits)
- Backspace: Back from detail view (jobs/CTF)

## Notes for Next Session

- Phase 4 BrowserHandler + 15 sub-handlers implemented and deployed. Needs in-game testing.
- BrowserPanel mirror enum used because HtmlBrowser.SubPanelWebs is private. Values match exactly.
- Reflection used for: currentPanel, bankListAdapter, panelCustom (PowerUI), various item lists.
- PowerUI Document DOM used for extracting web page buttons (getElementsByClassName).
- PreBuy/PreBuyHardware dialogs handled at coordinator level, not in sub-handlers.
- Some game classes (ISPPanel, CTFPanel, DeviceManualUI) may have different field names at runtime - verify.
- Grey Hack is a hacking simulator - user doesn't know the game, so explain mechanics as we discover them.
