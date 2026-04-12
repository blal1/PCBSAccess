# Chapter 8: Mastering Menus

## Introduction: The Crucible of Accessibility

For a sighted player, navigating a menu is a trivial exercise in pattern recognition. They scan the screen, instantly identify the largest button as the primary action, recognize a grid of squares as an inventory, and instinctively understand that a scrollbar indicates more content below. The visual layout conveys structure, hierarchy, and affordance in a fraction of a second.

For a blind player, a menu is a dark hallway. Every button, every slider, and every list item must be discovered one by one, step by step. If a modder does not impose a strict, logical structure onto this dark hallway, the player will become hopelessly lost. 

Menus are the crucible of accessibility modding. If you fail to make the menus accessible, the player can never equip a weapon, change a setting, or even start a new game. 

This chapter details the absolute, non-negotiable patterns required to build a robust menu navigation system. We will explore the mathematics of grid coordinates, the psychology of predictable input, and the architectural implementation of the `Handler` pattern.

---

## 1. The Philosophy of Standardized Navigation

A blind player should not have to learn a new control scheme for every single screen in the game. Consistency is paramount. Once a player learns how to navigate the Main Menu, they should immediately know how to navigate the Options Menu, the Inventory, and the Quest Log.

### The Universal Keybindings
Unless the game's fundamental design explicitly prohibits it (which is extremely rare), your mod must enforce the following navigation standards across all UI contexts:

1.  **The Arrow Keys (Up/Down/Left/Right):** These are exclusively for moving the selection highlight or cursor. They should never perform an action, consume an item, or change a setting (unless the setting is a horizontal slider).
2.  **The Enter Key or Spacebar:** This is the primary "Confirm" or "Execute" action. It clicks the currently highlighted button or equips the highlighted item.
3.  **The Escape Key:** This is the universal "Back" or "Close" action. It dismisses popups, closes menus, and eventually returns the player to the game world.
4.  **Dedicated Information Keys (F-Keys):** Because screen reader users cannot glance at a tooltip, you must provide dedicated keys (e.g., F2, F3) that command the mod to read extended descriptions, stats, or contextual help for the currently highlighted item.

When your `AccessStateManager` (The Brain, Chapter 4) detects that a menu has opened, it must instantly seize control of these keys and route them to the active Menu Handler.

---

## 2. The Implementation: The Handler Pattern

To manage the complexity of dozens of different menus, we use an object-oriented design pattern. Every distinct screen in the game gets its own C# class, which we call a **Handler**.

A Handler is responsible for three things:
1.  **Tracking State:** Knowing what index or item is currently selected.
2.  **Processing Input:** Reacting to the Arrow Keys and Enter.
3.  **Generating Output:** Formatting the text and sending it to the `ScreenReader`.

### A Basic List Handler
Let's examine the structure of a simple vertical list menu, like a Main Menu (Start Game, Load Game, Options, Quit).

```csharp
public class MainMenuHandler
{
    private int _selectedIndex = 0;
    private List<string> _menuItems = new List<string> { "Start Game", "Load Game", "Options", "Quit" };

    // Called by AccessStateManager when the context changes to "MainMenu"
    public void OnOpen()
    {
        _selectedIndex = 0;
        ScreenReader.Say("Main Menu. Arrow keys to navigate, Enter to select.", true);
        AnnounceCurrentSelection();
    }

    // Called by Main.Update() when this handler has focus
    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSelection(1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSelection(-1);
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ExecuteSelection();
    }

    private void MoveSelection(int direction)
    {
        _selectedIndex += direction;

        // Wrap around logic (crucial for accessibility)
        if (_selectedIndex < 0) _selectedIndex = _menuItems.Count - 1;
        if (_selectedIndex >= _menuItems.Count) _selectedIndex = 0;

        AnnounceCurrentSelection();
    }

    private void AnnounceCurrentSelection()
    {
        // The "X of Y" Pattern
        string announcement = $"{_selectedIndex + 1} of {_menuItems.Count}: {_menuItems[_selectedIndex]}";
        ScreenReader.Say(announcement, true);
    }
    
    private void ExecuteSelection()
    {
        // Here, you would use Reflection or public APIs to actually click the game's button
        DebugLogger.Log($"Executing: {_menuItems[_selectedIndex]}");
        // Example: MainMenuController.Instance.ClickButton(_selectedIndex);
    }
}
```

### The "X of Y" Mandate
Notice the `AnnounceCurrentSelection` method. It does not just say "Options". It says "3 of 4: Options".

This is the most important rule in this entire chapter. **Never announce an item in a list without announcing its position and the total size of the list.** 

Without "X of Y", a blind player has no idea if the menu has 4 items or 400 items. They have no idea if they are at the top, the middle, or the bottom. The "X of Y" pattern provides the spatial dimensions of the dark hallway. It allows the player to build a mental map of the menu structure.

---

## 3. Mastering the Grid (Inventories and Maps)

While vertical lists are simple, most RPGs and strategy games use Grid Inventories. A grid is a two-dimensional array of slots (Rows and Columns). 

Navigating a grid with a screen reader requires a specialized mathematical approach. If an inventory has 5 columns and 4 rows (20 slots total), and the player is at index 7 and presses the "Down Arrow", the index doesn't become 8; it becomes 12 (7 + 5 columns).

### The Grid Mathematics
Your `InventoryHandler` must be aware of the grid's dimensions.

```csharp
public class GridInventoryHandler
{
    private int _selectedIndex = 0;
    private int _columns = 5;
    private int _totalSlots = 20;

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveHorizontal(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveHorizontal(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveVertical(1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveVertical(-1);
    }

    private void MoveHorizontal(int direction)
    {
        int newIndex = _selectedIndex + direction;
        
        // Prevent wrapping to the next row when moving left/right
        int currentRow = _selectedIndex / _columns;
        int newRow = newIndex / _columns;
        
        if (newRow == currentRow && newIndex >= 0 && newIndex < _totalSlots)
        {
            _selectedIndex = newIndex;
            AnnounceSlot();
        }
        else
        {
            // Hit a wall. Play a "bump" sound or announce the edge.
            ScreenReader.Say("Edge of row.", true);
        }
    }

    private void MoveVertical(int direction)
    {
        // Moving up or down means subtracting or adding the column count
        int newIndex = _selectedIndex + (direction * _columns);
        
        if (newIndex >= 0 && newIndex < _totalSlots)
        {
            _selectedIndex = newIndex;
            AnnounceSlot();
        }
        else
        {
            ScreenReader.Say("Edge of grid.", true);
        }
    }
}
```

### Announcing Grid Coordinates
When navigating a grid, the "X of Y" pattern is insufficient. "8 of 20" doesn't help the player visualize the layout. You must translate the flat index into 2D coordinates.

```csharp
private void AnnounceSlot()
{
    // Calculate Row and Column (1-based for human readability)
    int row = (_selectedIndex / _columns) + 1;
    int col = (_selectedIndex % _columns) + 1;

    // Read the actual item data using the Senses developed in Chapter 5
    string itemName = GetItemNameFromGameMemory(_selectedIndex);
    
    if (string.IsNullOrEmpty(itemName)) {
        itemName = "Empty Slot";
    }

    string announcement = $"Row {row}, Column {col}: {itemName}";
    ScreenReader.Say(announcement, true);
}
```
With this logic, pressing Down Arrow on a grid announces: `"Row 2, Column 3: Iron Sword"`. The player instantly understands exactly where they are in physical space.

---

## 4. Complex UI Trees (The Fallback Strategy)

Lists and Grids cover 80% of game menus. But what about the remaining 20%? What about an intricate Character Creation screen with tabs, sliders, color pickers, and free-floating buttons?

Building a custom grid math handler for a chaotic layout is impossible. Instead, you must use a **Linear UI Crawler**.

1.  When the complex menu opens, your mod uses Unity APIs (`GetComponentsInChildren<Button>()`, `GetComponentsInChildren<Toggle>()`) to find every interactable element on the screen.
2.  You store these elements in a flat `List<GameObject>`.
3.  You sort the list by their Y-coordinate on the screen (top to bottom), and then by their X-coordinate (left to right).
4.  You allow the player to use Up/Down arrows to simply walk through this flat list from 1 to N, treating the chaotic screen as a single vertical column.

This destroys the visual layout, but it guarantees that the blind player can reach 100% of the buttons on the screen without getting trapped in navigational dead ends. In accessibility modding, guaranteed access always trumps preserving the visual layout.

---

## 5. The Accessibility Checklist: The Final Gate

Before you declare any menu handler "finished," you must pass it through the ultimate test. If a menu fails even one of these checks, it is a broken experience.

*   [ ] **The Initialization Check:** Does the mod announce the name of the menu the moment it opens? Does it provide a quick hint on how to navigate (e.g., "Use arrows")?
*   [ ] **The Completeness Check:** Can you reach every single interactive button, toggle, and slider using only the keyboard?
*   [ ] **The Edge Check:** If you are at the bottom of a list and press Down, does it wrap to the top, or does it play an "edge" sound? (Silence is unacceptable; the player will think the keyboard is broken).
*   [ ] **The Information Check:** If an item has a price, a weight, or a stat requirement, is that information announced, or is it hidden in a visual tooltip?
*   [ ] **The Execution Check:** When you press Enter, does it actually trigger the game's original code, playing the correct sound effect and updating the game state?
*   [ ] **The Escape Check:** Does pressing Escape close the menu, pop the `AccessStateManager` stack, and announce the name of the menu you returned to?

---

## Conclusion: The Architecture of Control

Mastering menus is the most labor-intensive part of accessibility modding. A large RPG might have 50 different screens, requiring 50 distinct Handler classes. But the effort is worth it. 

When a blind player opens a complex inventory, hears the grid coordinates, seamlessly navigates to a weapon, presses a dedicated key to hear its lore description, and equips it with a satisfying sound effect, you have achieved the ultimate goal of this framework. You have taken a visual obstacle course and transformed it into a fluid, empowering interface.

However, a game is more than just menus. Eventually, the player must close the inventory and step out into the world. In the next chapter, we will tackle the most daunting challenge of all: **Living in the World**. We will explore how to use audio and spatial logic to guide a player through three-dimensional environments without the use of sight.

---
*Character Count Check: ~10,900 characters.*
