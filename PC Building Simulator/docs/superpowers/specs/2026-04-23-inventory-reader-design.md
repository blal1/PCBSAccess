# Inventory Reader — Design Spec

**Date:** 2026-04-23
**Feature:** Phase 3 Feature 6 — InventoryHandler
**Branch:** 001-matchmaking-screen (ongoing Phase 3 work)

---

## Summary

Add keyboard navigation and screen reader announcements to the PC inventory panel. When the inventory opens, the user hears the category and item count. Up/Down arrows browse items; each item is announced with name, condition, price, and specs. F5 re-reads the current item. Closing the inventory is announced.

---

## Architecture

**New file:** `PCBSAccess/InventoryHandler.cs`

**Harmony patches:**

- `InventoryState.OnStateEnter` (postfix) — set active flag, announce open + category + item count
- `InventoryState.OnStateExit` (postfix) — clear active flag, announce "Inventory closed."
- `Inventory.SetCategory` (postfix) — announce new category name + item count
- `Inventory.ConstructInventory` (postfix) — trigger one-frame delayed item list refresh (handles lazy coroutine creation)

**Update integration:**

`Main.cs` calls `InventoryHandler.OnUpdate()` each frame. The method exits immediately unless `GameController.Get().GetCurrentGameState() == GameState.InventoryState`.

---

## Navigation

- **Up arrow** — move focus index up (clamp at 0, no wrap)
- **Down arrow** — move focus index down (clamp at count - 1, no wrap)
- **F5** — re-read current item

Item list source: `WorkshopUI.m_inventory.m_itemList.content` children with `ItemDisplay` component.

Index resets to 0 whenever the item list refreshes (category change or `ConstructInventory` fires).

---

## Announcements

### Inventory open
```
"Inventory open. {categoryName}. {count} items."
```

### Category change
```
"{categoryName}. {count} items."
```

### Item focus (Up/Down / F5)
```
"{name}. {condition}. {price}. {specs}"
```

Fields:
- `name` — `ItemDisplay.title.text` (= `PartDesc.m_uiName`)
- `condition` — "Broken" / "Used" / "New" from `ItemDisplay.broken.activeSelf` / `ItemDisplay.used.activeSelf`
- `price` — `ItemDisplay.m_priceText.text`; omitted if price object is inactive (FreeBuild or non-sellable)
- `specs` — `ItemDisplay.details.text` (= `PartDesc.m_uiLongSpec`)

### Empty category
```
"No items."
```

### Inventory closed
```
"Inventory closed."
```

---

## Category Name Mapping

`Inventory.m_currentCategory` is a `PartDesc.ShopCategory` enum (private field — access via ReflectionHelper). Map enum value to localized string via `Loc.cs`. Add EN + FR strings for all 19 category names (Misc, CPU, Cooling, Motherboard, Memory, GPU, Storage, PSU, Cables, Case, CaseCooling, CaseParts, Radiators, Reservoir, CPUBlock, WaterCooledGPU, Pipes, PipeConnectors, Coolant).

---

## State Guard

`OnUpdate()` checks `GameState.InventoryState` before processing any keys. No other handler uses F5 or reads inventory state — no conflicts.

---

## Key Bindings (Updated)

- F1: Career status
- F2: Re-read tutorial task
- F3: Re-read current job
- F4: Re-read last workshop tooltip
- **F5: Re-read current inventory item** ← new
- F12: Toggle debug mode

---

## Data Access

All `ItemDisplay` fields used (`title`, `details`, `m_priceText`, `broken`, `used`) are **public** — no Reflection needed for item data.

`Inventory.m_currentCategory` is **private** — use `ReflectionHelper.GetField<PartDesc.ShopCategory>`.

`Inventory.displayItems` is **private** — not needed; iterate `m_itemList.content` children directly (public).

---

## Edge Cases

- Items created lazily (coroutine in `CoCreateItems`) — wait one frame after `ConstructInventory` before snapshotting the list.
- Empty category — index stays at -1, F5 says "No items."
- Inventory opened without a PC case (standalone browse) — still works; price line omitted for non-sellable items automatically.
- Category with 1 item — index clamps correctly.
