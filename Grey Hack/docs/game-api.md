# Grey Hack - Game API Documentation

## Overview

- **Game:** Grey Hack
- **Engine:** Unity 2022.3.9f1
- **Runtime:** net472 (Mono, CLR 4.0)
- **Architecture:** 64-bit
- **Developer:** Loading Home

---

## 1. Singleton Access Points

- **PlayerClient.Singleton** (PlayerClient.cs) - Main network client, handles all server RPCs, TCP connection
- **RedGlobal.Singleton** (RedGlobal.cs) - Global network/world state, game loading
- **XmlGlobal.CacheText** (XmlGlobal.cs) - Global config, file system generation, localization
- **TranslationSystem.Singleton** (TranslationSystem.cs) - Text translation/localization
- **CacheDevices** (CacheDevices.cs) - Caches network device data, manages computer locks
- **TraceSystem** (TraceSystem.cs) - Network tracing mechanics
- **BlockchainSystem** (BlockchainSystem.cs) - Cryptocurrency/blockchain operations
- **Notifications** (Notifications.cs) - Notification display system
- **TaskSwitcher** (TaskSwitcher.cs) - Window/task switching UI
- **MouseCursor** (MouseCursor.cs) - Cursor management
- **Icons** (Icons.cs) - Icon resource management
- **PoolPrefabs.Singleton** (PoolPrefabs.cs) - Object pooling for performance
- **Clock** (Clock.cs) - Game time management
- **GlobalChat** (GlobalChat.cs) - Global chat system
- **GlobalCctv** (GlobalCctv.cs) - CCTV camera surveillance system
- **ServerMap** (ServerMap.cs) - Network topology/server map
- **NetworksUI** (NetworksUI.cs) - Network interface UI
- **GuildPanel** (GuildPanel.cs) - Guild UI panel

### Scene Flow

- **"Intro" scene**: Logo animation (Intro.cs), press Escape to skip
  - Intro.Awake: Sets quality/fps, hides cursor
  - Intro.ShowCursor: Starts cursor blink animation (~3s)
  - After cursor: animator.SetTrigger("StartAnim"), startedLogo = true
  - Escape only works after startedLogo = true
  - Escape: disables animator, fades logo, calls ForceStartGame (0.5s delay then loads Game scene)
  - Natural end: LoadGameScene.OnLogoIntroEnd() loads Game scene
- **"Game" scene**: Contains BiosMenu (main menu), then BootUp sequence, then Desktop
- BiosMenu is NOT a singleton - use Harmony patch on BiosMenu.Start to capture instance
- BootUp.cs handles BIOS animation, OS init, login, desktop loading

### Bootstrap Sequence

1. Intro scene: Intro.Awake fires, cursor animation plays
2. Logo animation starts (startedLogo = true), Escape now available
3. Logo ends naturally (LoadGameScene.OnLogoIntroEnd) or user skips with Escape
4. "Game" scene loads
5. BiosMenu.Start() - main menu appears (PLAY selected by default)
6. Player selects game mode and starts
7. BootUp.cs runs boot animation
8. Desktop loaded, game playable

---

## 2. Game Key Bindings (DO NOT override in mod!)

### Global Keys

- **Ctrl+Tab** - Task switcher (TaskSwitcher.cs)
- **Ctrl+C** - Copy selected text (Clipboard.cs)
- **F6** - Toggle Debug Reporter overlay (Reporter.cs)

### Terminal Keys (when terminal focused)

- **Tab** - Auto-complete command
- **Backspace** - Delete character before cursor
- **Delete** - Delete character at cursor
- **Home** - Move cursor to start of line
- **End** - Move cursor to end of line
- **Left/Right Arrow** - Move cursor
- **Up/Down Arrow** - Command history navigation
- **Enter / Keypad Enter** - Execute command
- **Ctrl+C** - Cancel running command
- **Ctrl+Shift+C** - Copy selected terminal text
- **Ctrl+Shift+V** - Paste into terminal
- **Escape** - Send Escape as input (special mode)
- **Left/Right Shift, Ctrl, Alt** - Send modifier as input (special mode)

### CCTV Camera Keys (BrowserCam.cs, when CCTV focused)

- **W / Up Arrow** - Pan camera up
- **A / Left Arrow** - Pan camera left
- **D / Right Arrow** - Pan camera right
- **S / Down Arrow** - Pan camera down

### File Explorer Keys (VentanaFinder.cs, when file explorer focused)

- **Alt+Up Arrow** - Navigate to parent folder
- **Alt+Left Arrow** - Navigate back in history
- **Alt+Right Arrow** - Navigate forward in history

### Other Context Keys

- **Escape** - Close chat window (ChatGuild.cs), cancel task switching
- **Delete** - Interrupt boot/return to menu (BootUp.cs)
- **Tab** - Advance to next field (TabInputField.cs)

### Axis/Button Inputs

- **"Submit"** (Enter) - Generic form submission (ChatGuild, HtmlBrowser, NetConnDialog)
- **"Horizontal"/"Vertical"** - Vehicle steering (TSSimpleCar.cs)

---

## 3. Safe Mod Keys

### Completely Free F-Keys

- **F1** (reserved: mod help)
- **F2, F3, F4, F5** - Free
- **F7, F8, F9, F10, F11** - Free
- **F12** (reserved: mod debug toggle)

### Free Modifier Combos

- **Alt+Any Key** - Free (Alt alone used in terminal special mode only)
- **Ctrl+Alt+Any Key** - Free
- **Shift+F1-F12** - Free
- **Ctrl+[0-9]** - Free
- **Alt+[0-9]** - Free

### Free in Menu Context (no terminal/CCTV)

- **Up/Down/Left/Right Arrow** - Free (used in terminal/CCTV only)
- **All number keys** - Free
- **Most letter keys** - Free (A/D/S/W used in CCTV only)

### Reserved for Accessibility Mod

- **F1**: Help
- **F12**: Toggle debug mode
- **Ctrl+F11**: Mod settings
- **Up/Down Arrow**: Menu navigation (in BiosMenu context), icon navigation (in desktop icon mode), file navigation (in file explorer), start menu navigation
- **Enter**: Activate item (in BiosMenu/desktop icon/file explorer/start menu context)
- **Backspace**: Go back (in BiosMenu context), go up folder (in file explorer), close submenu (in start menu)
- **Space**: Repeat announcement (in BiosMenu/desktop icon/file explorer/start menu context)
- **Alt+D**: Toggle desktop icon navigation
- **Alt+S**: Toggle start menu
- **Alt+Left/Right**: File explorer history back/forward
- **Alt+Home**: File explorer go to home directory
- **Shift+F10**: Context menu for selected file (in file explorer)
- **F2**: Rename file (in file explorer)
- **Delete**: Delete file (in file explorer)
- **Left/Right Arrow**: Navigate start menu submenus
- **Escape**: Exit desktop icon navigation, close start menu
- **Alt+N**: Repeat last notification

---

## 4. UI System

### Framework

- **Unity UI (uGUI)** with Canvas - primary UI framework
- **TextMeshPro (TMPro)** - all text rendering via TMP_Text
- **PowerUI** - HTML/CSS framework for in-game web browser
- **EnhancedScroller** - optimized scrollable lists (terminal, mail, chat, code editor)
- **No legacy OnGUI** usage for game UI

### Base Classes

- **uDialog** (UI.Dialogs namespace) - Universal dialog/window framework with resize, drag, minimize/maximize, animations. Public GO_* references for UI hierarchy.
- **Ventana** (Window) - Application window base extending MonoBehaviour + FeedBackInterface. Contains uDialog reference, clipboard, mouse cursor, theme.
- **SelectableTerminal** - Extends Unity Selectable for terminal focus callbacks.

### Text Access

- All visible text: `TMP_Text.text` (public property)
- Input fields: `TMP_InputField.text`
- Button labels: `button.GetComponentInChildren<TMP_Text>().text`
- Read from game localization: `TranslationSystem.Singleton.GetText(id, language)`

### Theme System

- **UI_Theme** (UI_Theme.cs) - 42 color properties applied dynamically
- **Apariencia** (Apariencia.cs) - Appearance/theming panel

### Focus/Selection

- Unity EventSystem-based focus management
- IPointerEnterHandler/IPointerExitHandler for hover
- ISelectHandler/IDeselectHandler for selection state
- TabInputField.cs manages focus cycling with `List<Selectable>`
- uDialog: `FocusOnClick = true`, `FocusOnShow = true`

### Tooltip System

- **Tooltip.cs** - Uses EventTrigger (PointerEnter/PointerExit), TMP_Text, configurable wait time (0.15s default)
- Text from `XmlGlobal.GetTexto(textID)` or direct string

### Key UI Panels

- **BiosMenu** - Main menu (PLAY, GRAPHICS, LANGUAGE, AUDIO, CREDITS, EXIT)
- **BootUp** - Boot sequence animation
- **MenuInicio** - In-game right-click menu (launches apps from /usr/bin)
- **Terminal** - Terminal emulator (extends Ventana)
- **HtmlBrowser** - Web browser with PowerUI rendering
- **NetworksUI** - Network panel (WiFi/Ethernet toggles, network list)
- **SettingsWindow** - System settings (hardware, accounts, appearance, mouse, streaming mode)
- **Mapa** - Network map visualization
- **GuildPanel** - Guild management
- **CTFPanel** - CTF events
- **PanelMission** - Missions
- **Notepad** - Text editor
- **MailWindow** - Email client

---

## 5. Boot Sequence (BootUp.cs)

### Entry Point

- `BootUp.Iniciar(PlayerComputer pc, bool isFirstInstall, bool isGameOver, string checkInicio, int termSafePID)` called from `PlayerClientMethods.PlayerLoadClientRpc()`
- Main animation coroutine: `AnimScreen()`

### Boot Phases (Normal Boot)

1. **BIOS screen** (~4-5s) - `backToMenuAvail = true`, Delete key can interrupt
   - 1.5s initial delay
   - BIOS header text (partesIni[0]): "Modular BIOS v10.05PE, An Energy Star Ally"
   - BIOS variant (partesIni[1]): "MIW1M/BIW2M BIOS 2.6"
   - CPU name (partesIni[2]): "Main Processor : [CPU_NAME]" from pc.GetHardware().cpus[0].name
   - Memory test (partesIni[3]): "Memory Testing: " then animated count to maxNum K
   - PnP extension (partesIni[4]): "Modular Plug and Play BIOS Extension v2.0B"
2. **OS init lines** (~1-2s) - text from `textIniOs` TextAsset, line-by-line with 0.005-0.05s delays
3. **Autologin** (~1s) - "Autologin enabled\nPlease wait..." with typewriter effect (Teletipo coroutine)
4. **Desktop load** - `ResumeBoot()` -> `AnimLoadDesktop()`:
   - Desktop fade-in animation (panelDesktop Animator, "Iniciar" bool)
   - Icons appear staggered (0.05-0.1s each)
   - Network auto-connect (Ethernet/WiFi)
   - Tutorial dialog if first time
   - Boot screen deactivated

### Boot Scenarios

- **Normal**: BIOS -> OS init -> autologin -> desktop
- **First install** (isFirstInstall=true): BIOS -> OS init -> InstallOS form -> progress bar -> desktop
- **Safe mode** (checkInicio has error): BIOS -> OS init -> terminal with error message
- **Game over** (isGameOver=true): Shows DesktopMessageUser overlay with Exit button
- **Delete interrupt**: "Interrupting boot... Entering BIOS... Disconnect" -> returns to menu

### Key Fields

- `textoScreen` (TMP_Text) - main text display during boot
- `backToMenuAvail` (bool) - Delete key availability (true during BIOS only)
- `panelDesktop` (Animator) - desktop fade-in
- `panelInstall` (Transform) - first-install panel (has InstallOS component)
- `imagenes[]` - boot screen images
- `fondoNegro` (Image) - black background
- `mostrarSecuencia` (bool) - show boot sequence flag

### Coroutines

- `AnimScreen()` - main boot animation
- `MemoryTesting()` - RAM count animation
- `Teletipo()` - typewriter effect
- `InterruptBoot()` - Delete key interrupt
- `AnimLoadDesktop()` - final desktop load
- `MostrarDesktopIcons()` - staggered icon appearance

### Delete Key Interrupt

- Available only when `backToMenuAvail == true` (during BIOS phase)
- Checked in Update(): `Input.GetKeyDown(KeyCode.Delete)`
- Stops both `animMemTest` and `animBoot` coroutines
- Runs `InterruptBoot()` -> `PlayerClient.Singleton.Disconnect()`

---

## 5b. First Install (InstallOS.cs + WindowFirstLogin.cs)

### Flow

1. `InstallOS.ShowUserConfig()` -> 1s delay -> opens uDialog with WindowFirstLogin
2. WindowFirstLogin has 4 TMP_InputField + 1 Button:
   - `inputUser` (auto-focused on open)
   - `inputPcName`
   - `inputPass`
   - `inputConfirmPass`
   - `nextButton` (also responds to Enter key via OnGUI)
3. `OnClickNext()` validates, then calls `InstallOS.ShowAnimInstall(user, pcName, pass)`
4. Login dialog closes, progress bar dialog opens (AnimInstallBar)
5. Progress bar fills to 100%, calls `FinishInstall()` -> sends CrearPlayerPcServerRpc to server

### Validation (OS.IsValidNewPlayerCredentials)

- All three fields required (not empty)
- Username cannot be "root", contain "admin", or contain "guest"
- Max 15 chars each for username, password, computer name
- Username and password must be alphanumeric
- Password must match confirm password

### Error Display

- `OS.ShowError(msg)` creates ErrorWindow uDialog on HelperComputerCanvas.Singleton.rectCanvasErrors

---

## 5c. Game Over (DesktopMessageUser.cs)

- `Iniciar(forceCursorVisible, message)` activates overlay with text message
- Text accessed via: `desktopMessage.GetComponentInChildren<VerticalLayoutGroup>().GetComponentInChildren<TMP_Text>().text`
- `OnExit()` button calls `PlayerClient.Singleton.Disconnect()`

---

## 5d. Terminal System (Terminal.cs)

### Architecture

- `Terminal` extends `Ventana` (base window class)
- `SelectableTerminal` (Unity Selectable) manages terminal focus via `OnSelect`/`OnDeselect`
- `TerminalListAdapter` (OSA-based scroller) handles text display and input
- `TerminalListItemModel` - one per line, stores `.line` (text), `.isCaret`, `.isPassword`
- No singleton - multiple terminals possible. Use `FindObjectsOfType<Terminal>()`

### Key Fields (Terminal.cs)

- `listAdapter` (TerminalListAdapter, protected) - text display/input handler
- `historialComandos` (List<string>, private) - command history
- `indiceHistorial` (int, private) - current history position
- `currentFolder` (FileSystem.Carpeta, protected) - current working directory
- `promptEnabled` (bool, protected) - whether prompt is active
- `isPasswordMode` (bool, private) - password input mode
- `pwd` (string, private) - prompt string (e.g. "user@machine:~$ ")
- `netCommandActivos` (List<NetComandoActivo>, protected) - active remote connections

### Key Methods

- `PrintLinea(string linea, bool subLastLine, bool pendingPrompt)` - display output line
- `ProcesaLinea(KeyCode keyCode = KeyCode.None)` - process input line (private)
- `AddTexto(string texto, ...)` - add text to terminal display (protected virtual)
- `GetActiveUser()` - current user name
- `GetCurrentFolder()` - current working directory
- `GetPID()` - terminal process ID
- `IsRemoteConnection()` - whether connected to remote machine
- `GetComandoActivo()` - active remote connection info
- `SetPromptEnabled(bool)` - enable/disable prompt

### Input Handling (OnGUI)

- Custom keyboard handling in `OnGUI()` - NOT TMP_InputField
- Only processes input when `listAdapter.IsFocus()` is true
- Up/Down arrows navigate `historialComandos` via `UpdateHistorialPrompt()`
- Enter executes via `ProcesaLinea()`
- Tab triggers `AutoCompletar()` (server-side autocomplete)
- Ctrl+C cancels command
- Ctrl+Shift+C/V for copy/paste

### Focus System

- `SelectableTerminal.OnSelect()` calls `listAdapter.SetFocus(true)` + `terminal.SetFocus()`
- `SelectableTerminal.OnDeselect()` calls `listAdapter.SetFocus(false)`
- `TerminalListAdapter.IsFocus()` returns current focus state
- Terminal.Update() and OnGUI() early-return when not focused

### Prompt String Format

- Local: `user@machine:path$ ` ($ for normal, # for root)
- Remote: `user@remoteMachine:path$ `
- FTP: `ftp> `
- Home dir shortened: `/home/user` -> `~`

### Confirmed Patchable Methods (Terminal)

- `Terminal.PrintLinea(string, bool, bool)` - public, output announcement
- `Terminal.UpdateHistorialPrompt()` - private, history navigation (use Harmony field injection for historialComandos/indiceHistorial)
- `SelectableTerminal.OnSelect(BaseEventData)` - public override, focus gained
- `SelectableTerminal.OnDeselect(BaseEventData)` - public override, focus lost

---

## 5e. Window Management (uDialog + uDialog_TaskBar)

### Window Lifecycle

- `uDialog.Show(bool, bool)` - shows window, calls `Focus()` if `FocusOnShow=true`, updates taskbar
- `uDialog.Close(bool, bool, bool)` - hides window, removes from taskbar (unless minimize=true)
- `uDialog.Minimize()` - hides window but stays in taskbar as Inactive
- `uDialog.Maximize()` - toggles between maximized and original size
- `uDialog.Focus()` - brings to front via `SetAsLastSibling()`, calls `GO_TaskBar.SetFocusedTask(this)`

### Taskbar (uDialog_TaskBar)

- `Tasks` (List<uDialog>) - all open windows
- `CurrentTask` (uDialog) - currently focused window
- `AddTask(uDialog, bool)` - add window, auto-focuses it, updates display
- `RemoveTask(uDialog)` - remove window from taskbar
- `SetFocusedTask(uDialog)` - set currently focused window
- Task states: Active (open, not focused), Focused (current), Inactive (minimized)

### Window Title Access

- `dialog.TitleText` (public string field) - window title
- `dialog.GO_TitleText` (Text component) - title UI element
- Titles are set by each application (e.g. "Terminal", "File Explorer", "Trash")

### Confirmed Patchable Methods (Window Management)

- `uDialog_TaskBar.AddTask(uDialog, bool)` - public, window opened
- `uDialog_TaskBar.RemoveTask(uDialog)` - public, window closed
- `uDialog_TaskBar.SetFocusedTask(uDialog)` - public, focus changed
- `uDialog.Minimize()` - public, window minimized

## 5f. Task Switcher (TaskSwitcher.cs)

### How It Works

- `TaskSwitcher.Singleton` - global singleton
- Ctrl+Tab opens switcher, loads program list from taskbar
- Each Ctrl+Tab while held cycles to next (`SwitchToProgram`)
- Release Ctrl activates selection (`HidePrograms(toTask: true)`)
- Escape cancels (`HidePrograms(toTask: false)`)

### Key Fields (all private)

- `taskItems` (List<TaskSwitcherItem>) - items in the switcher
- `indexTask` (int) - currently selected index
- `show` (bool) - whether switcher is open

### Confirmed Patchable Methods

- `TaskSwitcher.LoadPrograms()` - private, switcher opened
- `TaskSwitcher.SwitchToProgram()` - private, cycled to next item
- `TaskSwitcher.HidePrograms(bool toTask)` - private, switcher closed (Prefix needed - clears items)

## 5g. Desktop Icons (DesktopFinder + IconoVentana)

### Desktop Icon Storage

- `DesktopFinder` extends `VentanaFinder`
- Icons are children of `contenido` GameObject
- `GetObjetosActuales()` returns List<GameObject> of icon objects
- Each has `IconoVentana` component with `GetNombre()`, `GetTipoOpenFile()`, `GetFichero()`

### Icon Activation

- `IconoVentana.OnClickFichero()` - single click selects, double click opens
- Double-click behavior depends on `TipoOpenFile`: Folder opens explorer, Text opens notepad/code editor, Binary runs program

### No Native Keyboard Navigation

- Desktop icons are mouse-only in base game
- Mod adds Alt+D keyboard navigation mode

## 5h. Start Menu (MenuInicio.cs)

### Architecture

- `MenuInicio.Singleton` - global singleton
- Triggered by clicking the main menu button on taskbar
- `OnClickOpenMenu()` activates menu, sends `OpenPanelToolsServerRpc` to server for /usr/bin contents
- `OnHideMainMenu()` closes menu and all submenus

### Menu Items (fixed)

- **Programs** (has `Flecha` arrow child) - opens submenu with installed programs
- **Preferences** - calls `OnClickPreferences()`, launches Settings.exe
- **Help** - calls `OnClickHelp()`, launches Manual.exe
- **Reboot** - calls `OnClickReboot()`, shows reboot dialog
- **Shutdown** - calls `OnClickShutdown()`, shows shutdown dialog

### Programs Submenu

- `OnShowPanelTool(Transform panelTool)` - populates submenu from `binFolder` (received via `ResumePanelShow`)
- Items are `ItemMainMenu` components with `textoItem` (TMP_Text) and `OnClick()` method
- `ItemMainMenu.OnClick()` calls `InternalBash.Singleton.ProcesaLinea(textoItem.text, ...)` to launch the program
- `binFolder` received async via `ResumePanelShow(byte[] zipUsrBin)` from server

### Closing Behavior

- Auto-closes when clicked outside (Update checks `!hasFocus && MouseButtonUp`)
- `OnHideMainMenu()` hides menu, clears submenus, disables highlights

## 5i. File Explorer (VentanaFinder.cs)

### Architecture

- `VentanaFinder` extends `Ventana` (base window class)
- `DesktopFinder` extends `VentanaFinder` (desktop-specific, NOT a file explorer window)
- `Ventana.dialogo` (protected uDialog) - the window dialog reference
- Files stored in `objetosActuales` (List<GameObject>), each has `IconoVentana` component

### Navigation

- `barraDir` (TMP_Text, public) - shows current path
- `PulsadoUp()` - navigate to parent folder
- `PulsadoBack()` - go back in history
- `PulsadoForward()` - go forward in history
- `PulsadoHome()` - go to home directory
- `EntrarCarpeta(Carpeta)` - enter a folder (sends server request)
- `UpdateWindow(Carpeta, string, bool)` - called when server responds with folder contents

### File Operations (via ContextualMenu + OpcionContextual)

- `InteractableContextual` base class on IconoVentana provides right-click context menu
- `ContextualMenu.OpenMenu(List<Opciones>)` creates menu buttons
- `OpcionContextual.OptionSelected()` executes the chosen action
- Available options: Open, Copy, Paste, Cut, Delete, DeleteDef, Rename, NewFolder, Properties, EmptyTrash

### Confirmed Patchable Methods (File Explorer)

- `VentanaFinder.UpdateWindow(Carpeta, string, bool)` - public virtual, folder contents updated

---

## 6. Game Mechanics

(Analyzed as needed per feature)

---

## 6. Status and Notifications

- **Notifications** singleton - system notification display
- **Tooltip** system - hover-based contextual info
- **connect_text** (BiosMenu) - connection status during multiplayer join

---

## 7. Audio System

(Not yet analyzed)

---

## 8. Save and Load

- **Database** - SQLite via Mono.Data.Sqlite
- **PlayerPrefs** - Settings persistence (VSync, FPS, language, server rules, etc.)
- **ConfigOS** - OS configuration (saved networks, etc.)

---

## 9. Event Hooks for Harmony Patches

### Confirmed Patchable Methods

- **BiosMenu.Start()** (private) - Menu creation, capture instance
- **BiosMenu.SelectOption(MenuOptions)** (private) - Panel switch
- **BiosMenu.OnOptionSelected(int)** (public) - Button click handler
- **BootUp.Iniciar()** - Boot sequence start, capture instance + params (isFirstInstall, isGameOver, checkInicio)
- **BootUp.ResumeBoot()** - Desktop loading phase start
- **WindowFirstLogin.Start()** - First install form opened, capture instance
- **WindowFirstLogin.OnClickNext()** - Form submitted (postfix: check validation errors)
- **InstallOS.FinishInstall()** - Install complete
- **AnimInstallBar.OnCompleteInstall()** - Progress bar done
- **DesktopMessageUser.Iniciar()** - Game over message shown
- **OS.ShowError()** - Error dialog created (capture error text)
- **PlayerClientMethods.PlayerLoadClientRpc()** - Game load trigger

### Notes

- Harmony can patch private methods by string name
- BiosMenu is destroyed on game start (Destroy(base.gameObject))
- Unity's null check handles destroyed object detection

---

## 10. Localization

- **TranslationSystem.Singleton** - Game's translation system
- Languages: English, Spanish, + Steam Workshop custom languages
- BiosMenu uses **MenuBiosTextIDs** components to auto-translate UI text
- Method: `TranslationSystem.Singleton.GetText(id, language)`
- Language stored in `PlayerPrefs["Translation"]`

---

## 11. Code Examples

### Reading button text from BiosMenu

```csharp
string label = biosMenu.btnOptions[i].GetComponentInChildren<TMP_Text>().text;
```

### Checking toggle state

```csharp
bool isChecked = biosMenu.wipeComputer.isOn;
```

### Invoking a button click

```csharp
biosMenu.btnOptions[i].onClick.Invoke();
```

---

## 12. Known Issues and Workarounds

- BiosMenu is NOT a singleton - must be captured via Harmony patch on Start
- BiosMenu animations (animGameModes) may delay UI element visibility after mode selection
- wipeWorld toggle visibility depends on game mode (active only in Single Player)

---

## 13. Not Yet Analyzed

- [x] Structure overview (namespaces, singletons)
- [x] Input system (key bindings)
- [x] UI system (base classes, panels)
- [ ] State management (decision pending - simple booleans for now)
- [ ] Game mechanics (hacking, networking, terminal)
- [ ] Status/feedback systems (partial)
- [ ] Event system / Harmony patch points (partial)
- [ ] Tutorial system

---

## Change History

- **2026-04-04**: Initial placeholder created during setup
- **2026-04-04**: Tier 1 analysis complete - singletons, input system, UI system, key bindings documented
- **2026-04-04**: Boot sequence analysis complete - BootUp.cs, InstallOS.cs, WindowFirstLogin.cs, DesktopMessageUser.cs
- **2026-04-05**: Terminal system analysis complete - Terminal.cs, SelectableTerminal.cs, TerminalListAdapter.cs, Ventana.cs
- **2026-04-05**: Window management analysis complete - uDialog.cs, uDialog_TaskBar.cs, Ventana.cs
- **2026-04-05**: Task switcher and desktop icon analysis complete - TaskSwitcher.cs, IconoVentana.cs, DesktopFinder.cs
- **2026-04-08**: Post-update reverification (2608 files). PrintOutput removed from Terminal. VentanaFinder now has native Alt+arrow navigation. All other APIs unchanged.
