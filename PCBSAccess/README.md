# PCBSAccess — Accessibility Mod for PC Building Simulator

A BepInEx mod that makes PC Building Simulator fully playable with a screen reader and keyboard only.
All menus, workshop actions, apps, and gameplay mechanics are accessible without a mouse.
Supports NVDA, JAWS, and Windows Narrator.
Supports English and French (automatically matches the game language setting).

64 features. All tests passed 2026-05-01.

---

## Requirements

- PC Building Simulator (Steam version, Unity 2018.4, 64-bit)
- BepInEx 5.x x64
- Tolk screen reader bridge: Tolk.dll and nvdaControllerClient64.dll
- No additional .NET installation required (game includes its runtime)

---

## Installation

**Step 1 — Install BepInEx 5.x x64:**

- Download BepInEx 5 (x64 build) from https://github.com/BepInEx/BepInEx/releases
- Extract the zip so that the BepInEx folder sits next to PCBS.exe in the game folder.
- Launch the game once to let BepInEx create its folders, then close it.

**Step 2 — Place Tolk DLLs in the game root folder (same folder as PCBS.exe):**

- Tolk.dll
- nvdaControllerClient64.dll

**Step 3 — Copy the mod DLL:**

- Copy PCBSAccess.dll into: BepInEx\plugins\

**Step 4 — Launch the game.**

Your screen reader will say "Accessibility mod loaded. Press S for status." when the mod is active.
If you hear nothing, check BepInEx\LogOutput.log for error messages.

---

## Language

The mod uses the same language as the game.
To switch the mod language, change the game language in Options.
English and French are supported.

---

## Global Hotkeys

These keys work from anywhere in the game.

- S — Career status: cash, kudos, star rating, today's date, and upcoming calendar events (rent, deliveries, deadlines).
- T — Re-read the current tutorial step (How to Build a PC mode only).
- J — Re-read the current job email (career mode).
- W — Re-read the last workshop tooltip.
- I — Re-read the current inventory item.
- F — Re-read the current save slot.
- N — Read the next job objective (cycles through all objectives).
- H — Re-read the last Hardware Info app summary.
- B — Re-read the last benchmark result. OCCT takes priority over 3DMark.
- P — Re-read the last PC stats / build summary.
- E — Re-read the current email (tablet email app).
- G — Re-read the current shop item.
- Numpad 7 — Read all live HW Monitor sensor values (CPU temps, GPU temp, wattage).
- F1 — Context-sensitive help for the active screen.
- F11 — List open windows on the virtual PC desktop.
- F12 — Toggle debug mode. When on, extra state info is written to BepInEx\LogOutput.log.

---

## Menus and Navigation

### Main Menu

- Down Arrow or Up Arrow — Navigate buttons.
- Enter — Activate the focused button.
- Announced on load with navigation hint.

### Options Menu

- Tab or Shift+Tab — Navigate controls.
- Left Arrow or Right Arrow — Adjust a slider or cycle a dropdown.
- Enter or Space — Toggle a checkbox or activate a button.
- Every value change is announced immediately.

### Key Bindings Menu

- Down Arrow or Up Arrow — Navigate bindings.
- Home or End — First or last.
- Enter — Start redefining the keyboard key for the focused action.
- Announced per binding: action name and current assigned key.
- After assigning a new key: the updated binding is re-read automatically.

### Save and Load Menu

- Down Arrow or Up Arrow — Navigate save slots.
- Home or End — First or last.
- Enter — Load or save.
- F — Re-read the current slot.

### Pause Menu

- Down Arrow or Up Arrow — Navigate buttons.
- Home or End — First or last.
- Enter — Activate.

### Message Boxes

- Enter — Confirm (Yes or OK).
- Escape — Cancel (No or Back).
- Title and body announced automatically.

### What's New Popup

- Announced automatically on game start if new content is present.
- Enter or Escape — Dismiss.

### Day Summary Screen

- Down Arrow or Up Arrow — Navigate buttons.
- Home or End — First or last.
- Enter — Activate.
- Date, cash, and kudos announced on open.
- Calendar shows upcoming events; use the calendar widget to hear individual day events.

### Job Result Screen

- Enter — Dismiss.
- Announced automatically: sender, subject, success or failure, labour, total, stars, and review text.

### Workshop Selection

- Down Arrow or Up Arrow — Navigate workshops.
- Home or End — First or last.
- Enter — Select.

---

## Career Mode

### Email Inbox (Career)

- Down Arrow or Up Arrow — Navigate emails.
- Home or End — First or last.
- Enter — Accept a job or collect a completed reward.
- Announced per email: sender, subject, labour, kudos, and available action. Full body text follows.
- J — Re-read the current email.
- Cash and kudos changes announced as they happen.

### Part Shop

- Down Arrow or Up Arrow — Browse items.
- Home or End — First or last.
- Enter — View item detail.
- G — Re-read the current item.
- Item count announced when a category is selected.
- Checkout: all cart items, total, and cash balance announced on open.
- Enter at checkout — Place the order.

### Search Filters (Shop and Inventory)

- Press the Filters button to open the panel.
- Down Arrow or Up Arrow — Navigate filter options.
- Home or End — First or last.
- Space or Enter — Toggle a checkbox filter or expand/collapse a category.
- Number of active filters announced after each change.
- Escape or the game's Back input — Close the filter panel.

### Part Inventory

- Down Arrow or Up Arrow — Navigate items.
- Home or End — First or last.
- Left Arrow or Right Arrow — Switch category.
- Enter or Space — Pick up the item for installation.
- I — Re-read the current item.
- Announced per item: name, condition, price, and specs.
- "Incompatible" announced if the part does not fit.

### Deliveries

- Arrival of a new delivery announced automatically.
- Delivery manifest: Down Arrow or Up Arrow to read each received part.
- Home or End — First or last.

### Calendar Events

- Open Day Summary and click a day in the calendar widget to hear its date and events.
- S key also reads the next 3 upcoming events automatically.

### Career Events

- Level up: "Level up! Now level N."
- New customer review: "New review received. Rating: X stars."
- Inventory change: "Inventory updated. N items." (2-second debounce).
- Achievement unlocked: "Achievement unlocked: [name]."

### Job Objectives

- N — Cycle through objectives one by one (text, met or not met, optional status).
- Objectives announced automatically when completed.
- Time-limited objectives include the deadline date and days remaining.

---

## Workshop — Building and Repairing PCs

### Entering and Leaving

- "Working on [PC name]." announced when entering the PC work state.
- "Left PC." announced on exit.
- "Entered workshop." / "Left workshop." announced on walking state transitions.

### Mode Announcements

Switching modes is announced automatically:
- Assembly mode: announced with the part currently in hand.
- Disassembly, Cabling, Piping, Thermal paste, and Clean modes: each announced.

### Navigating Targets

- Tab — Move to the next available target: slot, component, connector, pin.
- Shift+Tab — Move to the previous target.
- Target name and action announced on focus.

### Performing Actions

- Space (hold) — Perform the primary action on the focused target (install, remove, connect cable or pipe).
  Hold until the game completes the action. Releasing early cancels it.
- Enter (tap) — Short-press actions: open or close a latch or pin, apply thermal paste, power button.
- R — Flip a part 180 degrees during the orientation step.

### Peripheral Slot Swapping

When approaching a monitor, keyboard, mouse, headset, or microphone slot:
- Announced: "Swapping [type]. Current: [model name]." or "Swapping [type]. Slot empty."
- The inventory then opens filtered to that peripheral type.

### Installation Feedback

- "Installing: [part name]." — announced when installation begins.
- "Installed: [part name]." — announced when done.
- "Removed: [part name]." — announced when a part is removed.
- Emergency shutdown announced with reason: "Emergency shutdown: CPU overheating." etc.

### PC Power

- "PC powered on." / "PC powered off." announced automatically.

---

## How to Build a PC Tutorial

This is the beginner tutorial mode. The mod supports the entire tutorial with keyboard.

### Tutorial Popups

- Each step popup read automatically: title and body text.
- T — Re-read the current step at any time.

### Workshop Actions in Tutorial

Same keys as regular workshop: Tab, Space (hold), Enter, R.

### Visual Inventory

When the tutorial asks you to pick a part:

- Announced: "Visual inventory: N parts, N compatible."
- Down Arrow or Tab — Next part.
- Up Arrow — Previous part.
- Home or End — First or last.
- Each part: "1 of 8: Power Supply. Compatible."
- Enter — Inspect the focused part.
- Escape — Go back.

### Part Inspection

- Announced: "Inspecting: [name]. [Enter to install / Not compatible.] Escape to go back."
- Enter — Install and return to the tutorial.

---

## Virtual PC — In-Game Operating System

### Desktop

- Down Arrow or Up Arrow — Navigate program icons.
- Enter — Launch the focused program.
- Program name announced on launch.
- Desktop announced on boot with program count.
- F11 — List all currently open windows.

### Window Focus

- Window name announced when brought to front via the taskbar.
- No double-announcement with launch.

### BIOS

- Tab — Cycle tabs. Tab name and all its settings announced.
- Down Arrow or Up Arrow — Navigate settings within the tab.
- Left Arrow or Right Arrow — Adjust the focused setting's value.
- Enter — Activate a setting.
- Confirm prompt: Enter for Yes, Escape for No.

### Hardware Info App

- All component rows read automatically on open.
- H — Re-read the full summary.

### HW Monitor (Live Sensors)

- Sensor count announced when the app opens.
- Numpad 7 — Read all live sensor values (CPU core temps, GPU temp, current wattage, max wattage).

### GPU Tuner (Overclocking)

- GPU name and all three current parameter values announced on open.
- Down Arrow or Up Arrow — Cycle between Core Clock, Memory Clock, and Voltage.
- Left Arrow or Right Arrow — Adjust the focused parameter.
- Enter — Apply settings.
- R — Reset to defaults.
- F1 — Re-read the current parameter.

### 3DMark Benchmark

- "3DMark started." announced when the benchmark begins.
- Score, CPU score, GPU score, and component names announced when results appear.
- B — Re-read last result.

### OCCT Stress Test

- Start and stop announced.
- All sensor readings announced when the test finishes.
- B — Re-read last result. OCCT takes priority over 3DMark.

### PC Stats (Build Summary)

- All stats and component names read on open.
- Enter — Dismiss.
- P — Re-read.

### Will It Run

- Program list on open: Down Arrow or Up Arrow to browse, Enter to check a program.
- Results: overall pass or fail, then each category (CPU, GPU, RAM, VRAM, Storage) with needed value, actual value, and pass or fail.
- Back navigates to the program list.

### Virus Scan App

- State changes announced automatically: In Progress, Clean, Dirty.
- Enter — Start scan or dismiss results.
- F1 — Re-read current state.

### PC Bay (Auction House)

- Down Arrow or Up Arrow — Navigate Buy or Sell list.
- Home or End — First or last.
- Enter — View item detail (Buy) or collect/remove auction (Sell).
- Purchase result announced.

### Email App (on virtual PC)

- Down Arrow or Up Arrow — Navigate inbox.
- Home or End — First or last.
- Enter — Read email: sender, subject, date, and body.
- E — Re-read current email.

### Add and Remove Programs

- Down Arrow or Up Arrow — Navigate program list.
- Home or End — First or last.
- Enter — Install or uninstall.
- Numpad 5 — Re-read current item.
- Install progress and restart prompts announced.

### Music Player

- Down Arrow or Up Arrow — Navigate track list.
- Home or End — First or last.
- Enter — Play the selected track.
- Track name announced on play. Play and pause state announced.

### Lighting App (RGB)

- Down Arrow or Up Arrow — Navigate LED groups.
- Home or End — First or last.
- Space — Toggle selection on the focused group.
- State announced per group. Enter on Apply: "Lighting applied."

### Market App

- All category names and current day percentage values read on open.

### Reviews App

- Company name, star rating, and top three reviews read on open.
- Rating changes announced when reviews update.

### Rank App

- Down Arrow or Up Arrow — Navigate the ranking list.
- Home or End — First or last.
- Entry announced: rank, name, and score.

### Notes App

- Note count announced when the app opens.
- Switch to Compact View to navigate notes:
  - Left Arrow — Previous note.
  - Right Arrow — Next note.
  - Each note: position, title, and body text.
  - F1 — Re-read the current note.

### Wallpaper App

- Tab switch between Client, Custom, and Workshop tabs: each switch announced with tab name.
- "Wallpaper applied." announced when a wallpaper is applied.

### Rename Company

- Current company name announced when the dialog opens.
- New name announced when confirmed.
- "Cancelled." announced on back.

---

## Esports DLC — Mobile Phone Messenger

The messenger app shows conversations with story characters. Messages are announced automatically when you open a conversation.

- When you open a conversation: the contact name is announced, followed by the last five messages.
- If a new message arrives while reading a conversation: announced immediately.
- Press Back to return to the conversation list: count of threads and unread messages announced.

---

## IT Support DLC — TikkIT Kanban Board

The TikkIT app is a Kanban-style job board with five columns: Backlog, In Progress, Blocked, Completed, and Nightshift.

### Board Navigation

- Left Arrow or Right Arrow — Switch between columns. Column name and card count announced.
- Down Arrow or Up Arrow — Navigate cards within the current column.
- Home or End — First or last card in the column.
- Enter — Open the focused card for full detail.
- F1 — Re-read the focused card.
- Announced on open: total job count with breakdown per column.

### Card Detail Popup

Announced automatically when a card is opened: staff name, role, subject, request description, objectives, payment, and budget.

- Enter — Positive action (Accept job, Go to site, Collect reward, etc.).
- N — Negative action (Decline, Discard to nightshift, etc.).
- Escape — Close the popup and return to the board.
- F1 — Re-read the card detail.

### New Job Notification

When a new IT ticket arrives: "New IT job: [subject] from [name]." announced automatically.

---

## IT Support DLC — Lift and Floors

When pressing a floor button in the office building:
- "Going to [floor name]." announced before the lift moves.
- "Arrived." announced when the lift reaches the destination and you exit.

---

## IT Support DLC — Tablet Shops (Bargain Basement, Office Shop, Mothership)

All DLC tablet shops share the same navigation:

- Item count announced on open.
- Down Arrow or Up Arrow — Navigate items.
- Home or End — First or last.
- Each item: name, price, stock count, and availability.
- F1 — Re-read the current item.

---

## Esports DLC — LikedIn Team Selection

When the team selection screen opens at the end of a league:
- Your current league likes and total likes announced.
- Number of available teams announced.
- Each team listed: name and likes required to join, followed by the team's pitch message.
- When a team is chosen: "Team chosen: [name]." announced.

---

## IT Support DLC — Status and Emails

- S key in IT Support mode: includes your current influencer level ("Level: N.") after the standard career status.
- IT Support job emails in the tablet inbox: full job details are announced, including the staff member's name, primary request, secondary information, deadline, and notes.

---

## Freebuild Mode

### Tool Upgrades Panel

- Item count announced on open.
- Down Arrow or Up Arrow — Navigate upgrades.
- Home or End — First or last.
- Space — Toggle the focused upgrade on or off.
- Announced per item: name and on or off state.

---

## Troubleshooting

**No sound from screen reader on startup:**
- Confirm Tolk.dll and nvdaControllerClient64.dll are in the game root folder (same folder as PCBS.exe, not in BepInEx).
- Confirm your screen reader (NVDA, JAWS, or Narrator) is running before launching the game.
- Check BepInEx\LogOutput.log. Look for errors on lines marked STEP A.

**Mod not loading:**
- Confirm PCBSAccess.dll is in BepInEx\plugins\.
- Confirm BepInEx is the 5.x x64 build.
- Check BepInEx\LogOutput.log for errors.

**Keys not responding:**
- Confirm F12 is not already bound to something in the game's own Options menu.
- Letter keys (S, T, J, W, I, F, N, H, B, P, E, G) are consumed by the mod globally.
- Navigation keys (arrows, Tab, Enter, Space) are context-sensitive and only active in the current screen.

**Workshop actions not working:**
- Use Space (hold) for install and remove. Enter is only for short-press actions (latches, toggles).
- Tab first to select a target, then hold Space.

---

## Building from Source

Requirements:
- .NET SDK 10 or later
- PC Building Simulator installed at: C:\Users\bilal\Downloads\PC Building Simulator

Build command (run from the game folder):

```
.\scripts\Build-Mod.ps1
```

This compiles the mod and copies PCBSAccess.dll to BepInEx\plugins\ automatically.

---

## Technical Details

- BepInEx 5.x x64 required. BepInEx 6 is not supported.
- Game engine: Unity 2018.4.16, Mono runtime, .NET 4.7.2.
- Mod ID: com.pcbsaccess.mod
- Version: 1.0.0
- Input system: Rewired (game native). The mod reads Unity Input directly for mod hotkeys and injects into the Rewired action state for Space-as-mouse-hold simulation.
- Screen reader: Tolk bridge. Reads to the first available screen reader in order: NVDA, JAWS, Narrator.
- Localization: English and French. Language set automatically from the game's I2 Localization setting.
