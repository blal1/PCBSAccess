# PC Building Simulator — Tutorial Overview

**Note:** Actual tutorial string text lives in Unity binary AssetBundles and cannot be read from code files.
This document is derived from `GameTutorial` enum keys, class names, and code structure in Assembly-CSharp-firstpass.

---

## How the Tutorial System Works

- Tutorial popups use `TutorialUI` — two public `Text` fields: `m_title` and `m_body`.
- Text is fetched at runtime from I2 Localization AssetBundles via `.Localized()` extension method.
- `Init(string id)` sets `m_title.text = (id + "_TITLE").Localized()` and `m_body.text = id.Localized()`.
- Triggered by `GameController.Get().ShowTutorial(GameTutorial step)`.
- Dismissed by `Dismiss()` → calls `GameController.Get().GoToPreviousState()`.
- Tutorial state checked with `CareerStatus.Get().WantsTutorial(GameTutorial step)`.

---

## Two Tutorial Modes

### 1. Career Tutorial (in-game popups)

Triggered automatically as player progresses through career. Each step is a popup with title + body text.

### 2. How to Build a PC (HowToBuildAPC mode)

Separate scene — `GameMode.HOW_TO_BUILD_A_PC`. Launched from main menu ("How to Build a PC" button).
- Guided step-by-step physical PC assembly in 3D.
- Step types: MESSAGE, UNINSTALL, INSTALL, TURN_ON, THERMAL_PASTE.
- Teaches actual component placement: CPU, RAM, GPU, power connectors, etc.
- Uses `HBPCState` game state — player follows instructions to physically build a PC.
- Triggers `Achievements.OnHBPC()` on completion.

---

## Career Tutorial Steps (GameTutorial enum)

Steps are shown roughly in the order a new player encounters them.

### Introduction

| Key | What it covers |
|---|---|
| FREEBUILD_INTRO | Introduction shown when starting Free Build mode |
| INTRO | Introduction shown when starting a new Career |
| ACCEPT_JOB | How to accept your first job from the job board |

### First Job — Repair / Diagnostic

| Key | What it covers |
|---|---|
| PC_ARRIVED | Customer's PC has arrived at your shop |
| GOT_IT | Confirmation/acknowledgement step (no separate title) |
| PLUG_IN | How to plug in the PC to diagnose it |
| INSERT_USB | How to insert a USB diagnostic drive |
| POWER_ON | How to power on the PC |
| INSTALL_VIRUS_SCAN | How to run a virus scan from the USB |
| COLLECT_REWARD | How to collect payment after completing a job |
| JOB_COMPLETE | Job completion summary |

### Purchasing Parts

| Key | What it covers |
|---|---|
| BUY_COMPONENTS | How to use the in-game shop to order parts |
| TIME_DELIVERY | Delivery timing — parts arrive next day |
| END_DAY | How to end the working day |
| PARTS_ARRIVED | Notification that ordered parts have arrived |

### Building a PC from Scratch

| Key | What it covers |
|---|---|
| BUILD_ACCEPT | How to accept a PC build job |
| BUILD_ASSIGN | How to assign parts to a build job |
| PLACE_PC | How to place a PC on the work bench |
| COLLECT_DELIVERY | How to collect a delivered PC |

### Cleaning

| Key | What it covers |
|---|---|
| CLEAN | How to clean a dusty PC (compressed air, etc.) |

### Diagnostics

| Key | What it covers |
|---|---|
| ACCEPT_WILL_IT_RUN | How to accept a "Will It Run?" job (compatibility check) |
| DIAGNOSTIC_ACCEPT | How to accept a full diagnostic job |
| DIAGNOSTIC_STATE | How to run diagnostics on customer PCs |
| RETURN_PC | How to return a repaired PC to the customer |
| REPLACE_PC_PARTS | How to replace faulty components |

### Overclocking

| Key | What it covers |
|---|---|
| CPU_OVERCLOCK_JOB_START | Notification that an overclocking job is starting |
| GPU_OVERCLOCK_JOB_START | Notification that a GPU overclocking job is starting |
| CPU_OVERCLOCKING | How to overclock a CPU (BIOS settings) |
| GPU_OVERCLOCKING | How to overclock a GPU |

### Custom Water Cooling (CWC)

| Key | What it covers |
|---|---|
| CWC_UNLOCK | Water cooling is now unlocked |
| CWC_EDIT_MODE | How to enter water cooling edit mode |
| CWC_INVENTORY | How to access water cooling components in inventory |
| CWC_RIGID_TUBING | How to use rigid (hard) tubing for water cooling loops |

### Shop / Business Management

| Key | What it covers |
|---|---|
| REVIEWS | How customer reviews work and affect your shop |
| HIDDEN_TASKS | Hidden/secret tasks that can earn bonus rewards |
| END_DAY_CHECK | End-of-day summary / business check |
| MARKET_APP | How to use the Market app (stock prices, buying components) |

### PC Bay (Display Window)

| Key | What it covers |
|---|---|
| PC_BAY_UNLOCK | PC display bay is now unlocked |
| PC_BAY_OPEN | How to open and use the PC display bay |

### Auction House

| Key | What it covers |
|---|---|
| FIRST_AUCTION | How the auction house works — buy/sell second-hand parts |
| FIRST_AUCTION_COMPLETE | First auction completed |

### Peripherals (Monitors, Keyboards, etc.)

| Key | What it covers |
|---|---|
| PERIPHERALS_UNLOCK | Peripherals are now available to buy and sell |
| PERIPHERALS_INVENTORY | How to access peripherals in inventory |

### Decorations

| Key | What it covers |
|---|---|
| DECORATIONS_POPUP | How to decorate your shop with furniture and items |

---

## Game Flow Summary (for accessibility mod context)

1. **Main Menu** — New Career, Continue, Free Build, How to Build a PC
2. **Career start** — Small shop, first job arrives by email
3. **Early jobs** — Repair and diagnostic: plug in PC, run USB scan, return to customer
4. **Shop expansion** — Buy parts from web shop, wait for delivery, build PCs to order
5. **Skill unlocks** — Overclocking, water cooling, auction house, PC display bay
6. **Business growth** — Reviews, star rating, shop decorations, peripherals
7. **End of day** — Required to process deliveries, receive payment, advance time

---

## Key Classes for Tutorial Accessibility Feature

| Class | Purpose | Access |
|---|---|---|
| `TutorialUI` | Shows tutorial popup (title + body) | `CommonUI.tutorialUI` |
| `GameTutorial` | Enum of all tutorial steps | `PCBS.GameTutorial` |
| `Tutorial` | Static helper | `Tutorial.ShowVerboseControls()` |
| `CareerStatus` | Checks tutorial state | `CareerStatus.Get().WantsTutorial(step)` |
| `GameController` | Triggers tutorial | `.ShowTutorial(GameTutorial step)` |
| `HowToBuildAPC` | Separate PC build tutorial | Scene: HowToBuildAPC_V2 |

### Harmony patch to intercept tutorial popups
```csharp
[HarmonyPatch(typeof(TutorialUI), "Init")]
class TutorialInitPatch
{
    static void Postfix(TutorialUI __instance, string id)
    {
        // __instance.m_title.text and __instance.m_body.text are now populated
        string title = __instance.m_title.text;
        string body = __instance.m_body.text;
        ScreenReader.Speak($"{title}. {body}");
    }
}
```

This patch fires AFTER Init() sets the text, so the real localized strings are already in the Text fields.
No need to look up AssetBundles — just read what the game already loaded.
