# Chapter 4: The Brain (State Management)

The "Brain" of your accessibility mod is the system that tracks exactly where the player is in the game. This is critical because the same keys (like the Arrow Keys) might do different things in a menu than they do in the game world.

## 1. Context-Aware Input
Without state management, your mod might try to navigate an inventory that isn't even open. We solve this using the `AccessStateManager`.

### The Core Concept
The `AccessStateManager` maintains a "Stack" of active contexts.
1.  When you open a menu, you **Push** its name onto the stack.
2.  When you close it, you **Pop** it off.

## 2. Using the AccessStateManager
Our framework provides a centralized manager that your handler classes can use to register interest in input.

### Identifying the Context
```csharp
// In your Update() loop:
if (AccessStateManager.CurrentContext == "Inventory") {
    // Only process Arrow Keys if the inventory is active
    HandleInventoryNavigation();
}
```

## 3. The "Escape" Stack
A vital feature for screen reader users is the "Back" or "Escape" functionality. When multiple menus are open (e.g., a confirmation popup on top of an inventory), the `AccessStateManager` ensures that pressing `Escape` closes the topmost menu first.

### Implementing the Stack
Our `AccessStateManager.cs.template` includes an `EscapeStack`. When a handler opens a UI, it should register itself:
```csharp
AccessStateManager.PushContext("SubMenu", () => CloseSubMenu());
```
When `Escape` is pressed, the manager automatically executes the last registered "Close" action.

## 4. Event-Driven State Changes
Instead of polling every frame to see if a menu is open, the best practice is to fire events. The `AccessStateManager` can trigger an `OnContextChanged` event that all your handlers listen to. This allows them to:
*   Reset their selection index.
*   Silence any previous announcements.
*   Prepare the first announcement for the new context.

### Summary
The "Brain" prevents your mod from talking over itself or performing the wrong actions. By using a centralized state manager, your mod becomes a cohesive system rather than a collection of conflicting scripts.

---
*Next: [Chapter 5: The Senses (Extraction & Reflection)](chapter-5.md)*
