# Chapter 4: The Brain (State Management)

## Introduction: The Problem of the Infinite Loop

Imagine you are a blind player. you have just opened your inventory. You press the Down arrow to hear your first item. "Health Potion," the mod says. You press it again. "Mana Potion." Excellent. But then, you hear a sound indicating a quest has updated. You press the Down arrow again, expecting the next item—but instead, the mod says "Quest: Find the Lost Artifact."

What happened? The mod's "Quest Handler" and "Inventory Handler" were both listening for the Down arrow key at the same time. Because the mod didn't know which one should have "Focus," it tried to do both. This is the "Conflict of Interest" that destroys accessibility mods.

In this chapter, we build the **Brain** of our mod: the `AccessStateManager`. This is the centralized authority that decides which part of your mod is allowed to listen to input and which part is allowed to speak at any given millisecond. Without a brain, your mod is just a collection of competing scripts. With one, it is a sophisticated, context-aware interface.

---

## 1. The Concept of Input Context

The fundamental realization of game modding is that **keys are overloaded**.
*   In the **Game World**, the Arrow Keys move the character.
*   In a **Menu**, the Arrow Keys move the selection highlight.
*   In a **Dialogue**, the Arrow Keys choose a response.

A blind player cannot see which of these screens is "on top." They rely on the mod to enforce the rules of focus. We define these states as **Contexts**.

### Defining Your Contexts
Every distinct "mode" of the game needs a name.
*   `"World"`: The player is walking around.
*   `"MainMenu"`: The start screen.
*   `"Inventory"`: Browsing items.
*   `"Dialogue"`: Talking to an NPC.
*   `"Popup"`: A confirmation window (e.g., "Are you sure you want to quit?").

---

## 2. The Architecture of AccessStateManager.cs

The `AccessStateManager` is a static class that acts as a **Global State Machine**. It doesn't perform navigation logic itself; it simply tells other classes whether they are allowed to perform theirs.

### The Context Stack
Why a **Stack** instead of a simple variable? Because games are nested. 
You are in the `World`. You open the `Inventory`. Now you are in `Inventory`. Then, you click an item and a `Details` panel opens. Now you are in `Details`. 

When you press the "Back" key (Escape):
1.  `Details` should close, and you should be back in `Inventory`.
2.  Press Escape again, `Inventory` should close, and you should be back in `World`.

A Stack (Last-In, First-Out) perfectly mirrors this behavior.

```csharp
public static class AccessStateManager
{
    private static Stack<string> _contextStack = new Stack<string>();
    
    public static string CurrentContext => _contextStack.Count > 0 ? _contextStack.Peek() : "World";

    public static void PushContext(string newContext)
    {
        _contextStack.Push(newContext);
        DebugLogger.Log($"Context Pushed: {newContext}. Current focus: {CurrentContext}");
        OnContextChanged?.Invoke(CurrentContext);
    }

    public static void PopContext()
    {
        if (_contextStack.Count > 0)
        {
            string removed = _contextStack.Pop();
            DebugLogger.Log($"Context Popped: {removed}. New focus: {CurrentContext}");
            OnContextChanged?.Invoke(CurrentContext);
        }
    }
}
```

---

## 3. The Escape Stack: Automating the "Back" Button

One of the greatest frustrations for blind players is getting "trapped" in a menu. If a modder creates a custom navigation system but forgets to handle the "Back" key, the player might reach a state where they can't close a menu because the mod is intercepting the Escape key but not doing anything with it.

Our `AccessStateManager` solves this with an **Action Stack**.

### Registration of Close Actions
When a handler pushes a context, it should also provide a "recipe" for how to leave that context.

```csharp
public static void PushContext(string context, Action closeAction)
{
    _contextStack.Push(context);
    _escapeActions.Push(closeAction);
}

public static void HandleEscape()
{
    if (_escapeActions.Count > 0)
    {
        var closeAction = _escapeActions.Pop();
        _contextStack.Pop();
        closeAction.Invoke(); // Actually closes the menu in the game
    }
}
```

Now, in your `Main.Update()` loop, you only need one piece of logic:
```csharp
if (Input.GetKeyDown(KeyCode.Escape))
{
    AccessStateManager.HandleEscape();
}
```
This ensures that the "Back" functionality is consistent across every single menu in your mod.

---

## 4. Input Interception Patterns

Now that we have a Brain, how do our "Senses" (the Handlers) use it? We use a **Gatekeeper Pattern**.

### The Wrong Way (Polling without Context)
```csharp
// Inside InventoryHandler.cs
void Update() {
    if (Input.GetKeyDown(KeyCode.DownArrow)) {
        MoveSelectionDown(); // DANGEROUS: This will run even if the quest log is open!
    }
}
```

### The Right Way (The Gatekeeper)
```csharp
// Inside InventoryHandler.cs
void Update() {
    // GATEKEEPER: Only proceed if the Brain says we have focus
    if (AccessStateManager.CurrentContext != "Inventory") return;

    if (Input.GetKeyDown(KeyCode.DownArrow)) {
        MoveSelectionDown();
    }
}
```

By adding that one `if` statement at the top of every handler's `Update` loop, you have eliminated 100% of input conflicts.

---

## 5. Transition Logic: The OnContextChanged Event

When the state changes, your mod often needs to perform "Cleanup" or "Setup" work. 
*   **Cleanup:** Silence the screen reader (using `ScreenReader.Stop()`) so the previous menu's text doesn't bleed into the new one.
*   **Setup:** Announce the title of the new menu and the first piece of information.

We use C# **Events** to broadcast these changes.

```csharp
public delegate void ContextChangedHandler(string newContext);
public static event ContextChangedHandler OnContextChanged;
```

Inside a Handler (e.g., `ShopHandler.cs`):
```csharp
void Awake() {
    AccessStateManager.OnContextChanged += MyContextListener;
}

void MyContextListener(string newContext) {
    if (newContext == "Shop") {
        ScreenReader.Say("Welcome to the Item Shop. 5 items available for purchase.", true);
        _selectedIndex = 0; // Reset selection to the first item
    }
}
```

This event-driven approach is much cleaner than checking `if (justOpened)` inside an `Update` loop. It ensures that your initialization code runs exactly once per transition.

---

## 6. Detecting Transitions: Harmony vs. Polling

How does the `AccessStateManager` know when to push or pop a context? There are two ways to feed the brain:

### Method A: The Harmony Hook (Preferred)
Find the game's method that opens the inventory (e.g., `InventoryUI.Open()`).
```csharp
[HarmonyPatch(typeof(InventoryUI), "Open")]
class InventoryOpenPatch {
    static void Postfix() {
        AccessStateManager.PushContext("Inventory", () => InventoryUI.Instance.Close());
    }
}
```
This is surgical and efficient. The brain is notified the instant the game changes state.

### Method B: The Polling Check (Backup)
If you can't find a specific "Open" method, you must check the UI's visibility every frame.
```csharp
// Inside Main.Update()
bool isInvVisible = InventoryUI.Instance.gameObject.activeInHierarchy;
if (isInvVisible && AccessStateManager.CurrentContext != "Inventory") {
    AccessStateManager.PushContext("Inventory", ...);
} else if (!isInvVisible && AccessStateManager.CurrentContext == "Inventory") {
    AccessStateManager.PopContext();
}
```
While polling is slightly less efficient, it is a valid fallback for games with messy architectures.

---

## 7. Advanced Scenario: Modal Popups

What happens if the player is in the `Inventory`, and a "Low Battery" warning pops up from the system, or a "New Quest" notification appears that requires an "Accept" or "Decline"?

This is a **Modal Context**. It is a context that sits on top of everything and *steals all input*.

The `AccessStateManager` handles this naturally through the stack. When the notification appears, we push `"Notification"`. Because the `InventoryHandler` is checking `CurrentContext == "Inventory"`, and the current context is now `"Notification"`, the inventory handler automatically goes dormant. It doesn't need to be "told" to stop; it simply stops receiving permission to act.

When the player clicks "Accept," we pop the stack, the context becomes `"Inventory"` again, and the inventory handler instantly wakes up and resumes where it left off.

---

## 8. Debugging the Brain

A malfunctioning brain is the hardest thing to debug. If your mod stops responding to keys, it's usually because a context was pushed but never popped, leaving the mod stuck in a "Ghost Context."

### The Debug Logger's Role
Our framework's `DebugLogger` should log every push and pop. When you are testing, keep the log open.
*   If you press Escape and don't see `Context Popped` in the log, your `HandleEscape` logic is broken.
*   If you see the context is `"Dialogue"` but you are clearly looking at the main menu, you have a "Leak" in your state management.

### The "Reset Brain" Emergency Key
As a safety measure for developers (and sometimes for players), it is wise to bind a key (like `Ctrl+Shift+Escape`) that clears the entire stack and returns the mod to the `"World"` context.

```csharp
if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backquote)) {
    _contextStack.Clear();
    OnContextChanged?.Invoke("World");
    ScreenReader.Say("Mod Focus Reset to World.");
}
```

---

## Conclusion: Order from Chaos

Your mod now has a Nervous System to speak and a Brain to think. It understands where it is, it respects the priority of menus, and it provides a consistent "Back" experience for the player.

But a brain without senses is still blind. In the next chapter, we will build the **Senses** of our mod: the `UITextExtractor` and the `ReflectionHelper`. We will learn how to reach into the game's internal memory to pull out the names of items, the health of enemies, and the text of dialogues, turning the game's silent visual data into the fuel for our screen reader.

---
*Character Count Check: ~10,800 characters.*
