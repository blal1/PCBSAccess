# Chapter 7: Identifying Game Logic

Once you have identified the UI classes, you need to find the "Source of Truth"—the actual logic that tracks the game state. A `HealthBar` UI element is just a visual; the real health value is stored in a `PlayerStatus` or `Stats` class.

## 1. Finding the "Source of Truth"
Look for the code that *updates* the UI.

1.  Find the `Text` component for the health display in the code.
2.  See which variable is assigned to that text.
3.  Follow that variable back to its origin.

**Example:**
If `txt_Health.text = player.CurrentHP.ToString()` is in the UI code, then `player.CurrentHP` is your Source of Truth.

## 2. Hookable Methods (The Harmony Targets)
Harmony works best when you hook into methods that represent a clear change in state.

*   **Avoid Polling if possible:** Instead of checking `if (menu.isOpen)` every frame, find the method `menu.Show()` and put a **Postfix** patch on it.
*   **Action Methods:** Look for methods like `PickupItem()`, `TakeDamage()`, `LevelUp()`, `OnDialogueStart()`. These are perfect triggers for announcements.

## 3. Understanding Game Flow
Identify the "Manager" classes. Most Unity games have:
*   `GameManager`: The high-level state (In-Game, Paused, Game Over).
*   `UIManager`: Handles opening and closing all panels.
*   `InputManager`: Centralizes keyboard/mouse handling.

If you can find the `UIManager`, you can often hook into its central `OpenPanel(string name)` method to detect *any* menu opening in the entire game with a single patch.

## 4. The "Reflection" Shortcut
If you find the data you need but it's marked `private`, don't try to change the game's code. Use your `ReflectionHelper`.
```csharp
// We found the Player class, but currentGold is private.
// Use our helper to read it safely.
int gold = ReflectionHelper.GetField<int>(playerInstance, "currentGold");
```

### Summary: The Modder's Workflow
1.  **Detect** the event (via Harmony Patch).
2.  **Gather** the data (via Reflection or Public API).
3.  **Translate** to text (via Localization).
4.  **Announce** to player (via ScreenReader).

---
*Next: [Chapter 8: Mastering Menus](chapter-8.md)*
