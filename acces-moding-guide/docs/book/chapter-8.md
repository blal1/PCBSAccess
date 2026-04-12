# Chapter 8: Mastering Menus

Menus are the most common accessibility barrier. In this chapter, we establish the patterns for standardized navigation, ensuring that a blind player can navigate any screen with ease.

## 1. Standardized Navigation Patterns
Consistency is your best friend. Every screen should follow the same rules:

*   **Pfeiltasten (Arrow Keys):** Use these for navigation within a list or grid.
*   **Position Announcement:** Always use the "X of Y" pattern (e.g., "3 of 12: Health Potion").
*   **State Changes:** If selecting an item changes something else on the screen (e.g., updating a description), announce the main info first, then the change.

## 2. Grid Coordinates
For inventories or maps that use a grid (rows and columns), announce the coordinate as well as the name:
`"Row 1, Column 4: Rusty Sword"`

## 3. Contextual Key-Hints
When a new panel opens, announce the most important keys immediately.
`"Inventory Opened. Arrow Keys to navigate, Space to use, F2 for description."`
**Hint:** Use `ScreenReader.SayQueued` for these hints to avoid interrupting the main title.

## 4. The Accessibility Checklist
Before declaring a menu "accessible," verify these points:
*   [ ] Can I reach **every** button with the keyboard?
*   [ ] Does the mod announce when the menu **opens** and **closes**?
*   [ ] Does it announce the **current selection** as I move?
*   [ ] Are the **Escape** and **Enter** keys handled correctly (via `AccessStateManager`)?
*   [ ] Can I get a **Status Summary** at any time with a dedicated key (like F-keys)?

### Implementation Pattern: The Handler
Each menu should have a dedicated `Handler` class.
```csharp
public class InventoryHandler : IHandler {
    private int _selectedIndex = 0;
    
    public void Navigate(int direction) {
        _selectedIndex += direction;
        // ... logic to wrap around ...
        AnnounceSelection();
    }
    
    private void AnnounceSelection() {
        var item = GetItem(_selectedIndex);
        ScreenReader.Say($"{_selectedIndex + 1} of {total}: {item.Name}");
    }
}
```

By following these patterns, you ensure that once a player learns how to navigate one menu in your mod, they have learned how to navigate them all.

---
*Next: [Chapter 9: Living in the World](chapter-9.md)*
