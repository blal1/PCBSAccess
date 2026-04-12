# Chapter 3: Designing the Nervous System

In accessibility modding, the "Nervous System" is the pathway from an event to the screen reader. This chapter focuses on how to make your mod "speak" effectively using the `ScreenReader` wrapper.

## 1. The ScreenReader Utility
Your mod should never call `Tolk` directly. Instead, we use a `ScreenReader.cs` wrapper. This ensures that:
1.  Errors are handled gracefully.
2.  Announcements can be easily silenced.
3.  The mod only tries to speak if a screen reader is actually running.

### Key Methods
*   **`ScreenReader.Initialize()`**: Connects to Tolk at startup.
*   **`ScreenReader.Say(text, interrupt)`**: The primary method for speaking.
    *   `interrupt = true`: Stops previous speech immediately. Use for critical updates (e.g., "Health Low").
    *   `interrupt = false`: Queues the speech to be read after the current announcement finishes. Use for hints (e.g., "Press F1 for help").
*   **`ScreenReader.Stop()`**: Immediately silences the screen reader. Use this when a menu is closed.

## 2. Best Practices for Speech
Screen reader users process information linearly. If your mod is too wordy, it becomes a burden.

*   **Be Concise:** "3 of 10: Health Potion" is better than "You are currently selecting the third item out of ten in your inventory, which is a Health Potion."
*   **Use the Position Pattern:** Always provide the index and total for lists (e.g., "Item 5 of 20").
*   **Silence is Golden:** Don't announce information that hasn't changed.

### The Queueing Philosophy
If you have multiple pieces of information to share, use `SayQueued`:

```csharp
// Interrupt the current speech with the main info
ScreenReader.Say("Inventory Opened");

// Queue the secondary info to play after "Inventory Opened" finishes
ScreenReader.Say("15 items total. Press Arrow Keys to navigate.", false);
```

## 3. Avoiding "The Flood"
A common mistake is announcing too many events simultaneously. If three enemies die at once, don't announce each one. Instead, summarize: "3 enemies defeated."

### Checkpoint: Does it Work?
Test your `ScreenReader.cs` by adding a line in your `Main.OnInitialize()`:
```csharp
ScreenReader.Say("Accessibility mod loaded and ready.");
```
If you hear this, your bridge is solid!

---
*Next: [Chapter 4: The Brain (State Management)](chapter-4.md)*
