# PC Building Simulator: Screens Reference

This document enumerates the primary screens and UI contexts in the game, along with their corresponding handlers in the `PCBSAccess` mod.

| Screen / Context | Game Class / State | Mod Handler |
| :--- | :--- | :--- |
| **Main Menu** | `MainMenu` | `MainMenuHandler` |
| **Workshop (Free Roam)** | `WalkingState` | `WorkshopWayfindingHandler`, `WorldInteractionHandler` |
| **Build Mode (Workbench)** | `WorkingOnComputerState` | `BuildModeHandler` |
| **Inventory** | `InventoryPanelSwitcher` | `InventoryHandler` |
| **Computer OS** | `OS` / Desktop Apps | `ComputerOSHandler` |
| **Shop** | `ShopApp` | `ShopHandler` |
| **Jobs / Email** | `EmailApp` | `CareerJobHandler` |
| **Options Menu** | `OptionsMenu` | `OptionsMenuHandler` |
| **Save/Load Menu** | `SaveLoadMenu` | `SaveLoadMenuHandler` |
| **Message Box** | `MessageBox` | `MessageBoxHandler` |
| **Phone / Tablet** | `Phone` / `Tablet` | `PhoneHandler`, `DLC2Handler` |
| **Tutorials** | `TutorialUI`, `HowToBuildAPC` | `HowToBuildAPCHandler` |
| **Benchmarks** | `3DMark` | `BenchmarkHandler` |

## Screen Descriptions

### Main Menu
The title screen where players start new games, load existing ones, or access options.

### Workshop
The 3D environment where the player moves between workbenches, the shop computer, and the exit.

### Build Mode
The zoomed-in view of a PC case on a workbench. This is where physical assembly, disassembly, cabling, and piping occur.

### Computer OS
The virtual operating system running on PCs in the game. Players use this to run benchmarks, check emails, buy parts, and configure RGB lighting.

### Inventory
A panel that shows parts owned by the player, separated by category (CPU, GPU, Motherboard, etc.).

### Shop
An app within the Computer OS used to purchase new PC parts.

### Jobs / Email
An app within the Computer OS where players accept jobs from customers and receive feedback.
