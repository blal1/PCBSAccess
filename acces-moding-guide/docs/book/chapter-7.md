# Chapter 7: Identifying Game Logic

## Introduction: The Illusion of the Interface

In the previous chapter, we learned how to navigate decompiled code and find the classes that make up the game. However, finding a class is only the first step. To make a game accessible, you must understand the difference between the **Interface** (what the game looks like) and the **Logic** (what the game actually is).

A common trap for novice modders is trying to read the game's state by looking at the UI. For example, if a player wants to know their health, a novice modder might try to find the `GameObject` named "HealthBarText", extract the string "45/100", and announce it. 

This approach is fragile, slow, and often wrong. The UI is merely a projection, a shadow on the wall. The UI only updates when the game tells it to, and sometimes, for visual reasons (like smooth animations or damage numbers ticking down slowly), the UI does not accurately reflect the immediate reality. 

Your goal as an accessibility architect is to bypass the shadows and find the **Source of Truth**. You must find the raw data variables and the core methods that modify them.

---

## 1. Finding the Source of Truth

The Source of Truth is the absolute, definitive location in the computer's memory where a piece of information is stored. 

### The Trail of Crumbs
How do you find it? You work backward from the interface. 

1.  **Find the UI Element:** Using the techniques from Chapter 6, you find the class responsible for the health bar, perhaps named `UI_HealthDisplay`.
2.  **Find the Update Method:** Look inside `UI_HealthDisplay` for a method like `UpdateUI()` or `Refresh()`.
3.  **Identify the Data Source:** Look at the code inside `UpdateUI()`. 
    ```csharp
    public void UpdateUI() {
        float current = PlayerManager.Instance.ActivePlayer.CombatStats.CurrentHealth;
        this.healthText.text = current.ToString();
    }
    ```
4.  **The Revelation:** You have found it. The Source of Truth is not `this.healthText`. The Source of Truth is `PlayerManager.Instance.ActivePlayer.CombatStats.CurrentHealth`.

Whenever your mod needs to announce the player's health, it must read that exact variable. If the UI glitches, if the health bar is hidden behind a menu, or if the game's language changes, your mod will still read the correct numerical value because you are looking at the raw data.

### The Danger of Visual State
Consider an inventory system. A sighted player knows an item is "equipped" because it has a glowing yellow border around it. 

If you look at the `UI_InventorySlot` class, you might see:
```csharp
public void SetEquippedState(bool state) {
    if (state) {
        this.borderImage.color = Color.yellow;
    } else {
        this.borderImage.color = Color.white;
    }
}
```
A novice modder might write a script that says: "If the border is yellow, announce 'Equipped'." 
**Never do this.** Colors are visual states. If the developer changes the equipped color to blue in a patch, your mod breaks. 

Instead, trace the logic backward. Why did `SetEquippedState(true)` get called? Because somewhere, an `InventoryManager` executed code like this:
```csharp
if (item.IsEquipped) {
    slot.SetEquippedState(true);
}
```
The Source of Truth is `item.IsEquipped`. You read the boolean variable, never the color.

---

## 2. Identifying Hookable Methods (Harmony Targets)

Finding the data is only half the equation. You also need to know *when* the data changes. You do not want to constantly poll the health variable 60 times a second to see if it changed. You want the game to tell you when it changes.

To do this, we use Harmony patches. But you cannot patch just any method. You must choose your targets wisely.

### The Ideal Patch Target
An ideal Harmony target is a method that represents a distinct, undeniable change in the game's state. It should be:
1.  **Infrequent:** It should only run when an action happens (e.g., `TakeDamage()`), not every frame (e.g., `Update()`).
2.  **Authoritative:** It should be the central point through which all logic flows. If there are five different ways to take damage, you don't want to patch all five. You want to find the one underlying method they all call, like `ApplyHealthChange(int amount)`.
3.  **Data-Rich:** It should have parameters or return values that contain the information you need.

### Target Example 1: Menus and UI
When making menus accessible, you need to know when they open and close.
Look for methods named:
*   `Show()`, `Hide()`
*   `Open()`, `Close()`
*   `SetVisible(bool isVisible)`
*   `Awake()`, `OnDestroy()` (If the UI object is instantiated and destroyed rather than hidden).

**The Patch:** Place a `Postfix` on `Open()`. Inside the Postfix, push the menu's context to your `AccessStateManager` and announce the menu's title. Place a `Postfix` on `Close()` to pop the context.

### Target Example 2: Combat and Status
When handling combat, you want immediate feedback.
Look for methods named:
*   `TakeDamage(DamageInfo info)`
*   `Heal(int amount)`
*   `Die()`, `Kill()`
*   `OnLevelUp()`

**The Patch:** Place a `Prefix` on `TakeDamage`. Read the `amount` parameter and announce "Took X damage!". Or, place a `Postfix` on `TakeDamage`, read the *new* health from the Source of Truth, and announce "Health is now Y". (Or better yet, announce both: "Took X damage, Y health remaining.")

### Target Example 3: Inventory and Items
When navigating inventories, you need to know when the selection changes.
Look for methods named:
*   `SelectItem(Item item)`
*   `HoverSlot(UI_Slot slot)`
*   `UpdateSelectionIndex(int index)`

**The Patch:** Place a `Postfix` on `SelectItem`. Read the `item` parameter. Use your localization system to translate its name, use reflection to read its stats, and announce the full block of text to the screen reader.

---

## 3. The Art of the Postfix vs. Prefix

Choosing between a Prefix (before the original code) and a Postfix (after the original code) requires careful thought.

### Why Postfix is Usually Better
In 90% of accessibility scenarios, you want a **Postfix**. 
If you announce "Inventory Opened" in a Prefix, you are speaking *before* the game has actually drawn the inventory. If the game's original `Open()` method crashes or aborts halfway through, your mod lied to the player. 

By using a Postfix, you guarantee that the game has successfully completed its action before you announce the result. Furthermore, in a Postfix, the game's state (the Source of Truth variables) has already been updated to reflect the new reality.

### When to Use a Prefix
You use a Prefix when you need information that is going to be destroyed or altered by the original method.

Imagine a method `SellItem(Item i)`. The original game code removes the item from the inventory and deletes the object. If you use a Postfix, and you try to read `item.Name`, the object might already be destroyed, causing a Null Reference Exception that crashes your mod.

In this case, you use a Prefix. You read the item's name and price *before* the game deletes it, store those strings in local variables, and then announce "Sold [Name] for [Price] gold."

### Advanced: Prefix Canceling
As mentioned in Chapter 1, returning `false` in a Prefix stops the original game method from running. This is extremely dangerous but sometimes necessary.

Imagine a game where pressing `Escape` hard-closes the game without asking for confirmation. This is terrible for blind players who might press it accidentally. You could write a Prefix patch on the `HandleEscapeInput()` method. Your Prefix intercepts the keypress, opens a custom confirmation dialog via your mod, and returns `false` to prevent the game from quitting. You have effectively hijacked the game's control flow. Use this power sparingly.

---

## 4. Dealing with Asynchronous Logic (Coroutines)

Unity makes heavy use of **Coroutines**. A Coroutine is a method that can pause its execution, wait for a few frames or seconds, and then resume. They are often used for animations, network requests, or delayed events.

In decompiled code, Coroutines look like absolute gibberish. A method named `ShowMenuAnimation()` will actually return an `IEnumerator`, and the decompiler will generate a hidden, nested class (like `<ShowMenuAnimation>d__12`) to handle the state machine.

### The Coroutine Trap
You **cannot** easily patch a Coroutine with Harmony. If you put a Postfix on a method that returns an `IEnumerator`, your Postfix will run immediately when the Coroutine *starts*, not when it *finishes*. The game says "Start the 3-second opening animation," your Postfix runs instantly and announces "Menu Opened," but the menu won't actually be interactive for another 3 seconds. The player presses arrow keys, nothing happens, and they assume the game is broken.

### The Solution: Find the Callback
How do you solve this? You must find what happens *after* the Coroutine finishes.

Look at the code inside the Coroutine (you usually have to dig into the generated `<>d__` class in dnSpy). Look at the very end of the method. Does it fire an event? Does it call a method like `OnAnimationComplete()`? Does it set a boolean `isInteractive = true`?

If it calls `OnAnimationComplete()`, that is your Harmony target. You patch that method instead. If it sets a boolean, you might have to abandon Harmony for this specific interaction and use a Polling check in your `Update()` loop, waiting for that boolean to become true before announcing the menu.

---

## Conclusion: The Architecture of Action

You now know how to look past the visual illusions of the user interface and find the bedrock data structures that define the game. You know how to identify the critical junctures where that data changes, and you know how to deploy Harmony patches to intercept the flow of time.

You have moved from being an observer to being an active participant in the game's execution cycle. You are no longer just reading the Matrix; you are altering its code.

In the next chapter, we will take these intercepted events and turn them into standardized, accessible experiences. We will dive deep into **Mastering Menus**, establishing the rigid mathematical patterns required to make grid inventories, scrolling lists, and complex UI trees navigable by a player who can only perceive one element at a time.

---
*Character Count Check: ~10,700 characters.*
