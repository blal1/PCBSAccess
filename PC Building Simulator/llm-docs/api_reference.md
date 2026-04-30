# PC Building Simulator: API Reference

## 1. Core Singletons
The game uses several singleton classes for central access to game state and data.

- **GameController**:
  - Access: `GameController.instance` / `GameController.Get()`
  - Description: Main game state machine and mode controller.
- **CareerStatus**:
  - Access: `CareerStatus.Get()`
  - Description: Player progression data (cash, kudos, star rating).
- **CommonUI**:
  - Access: `CommonUI.s_instance`
  - Description: Central access to global UI panels (Save Menu, Options, Message Box).
- **WorkshopController**:
  - Access: `WorkshopController.Get()`
  - Description: Manages the workshop environment and slots.
- **PartsDatabase**:
  - Access: `PartsDatabase.s_instance`
  - Description: Definition of all PC parts in the game.
- **InputModule**:
  - Access: `InputModule.s_instance`
  - Description: Custom UI input handling.

---

## 2. Input System (Rewired)
The game uses **Rewired**. Standard `Input.GetKeyDown` will not work for game-rebindable actions.

- **Usage Pattern**:
  ```csharp
  if (PCBSInput.m_action.GetDown()) {
      // Perform interaction
  }
  ```
- **Key Actions in `PCBSInput`**:
  - `m_moveX`, `m_moveY`: WASD movement.
  - `m_lookX`, `m_lookY`: Mouse look.
  - `m_action`: Interact (E).
  - `m_secondaryAction`: Secondary (Q).
  - `m_inventory`: Open inventory (Tab/I).
  - `m_menuAction`: Confirm in menus.
  - `m_menuBack`: Cancel/Back in menus.

---

## 3. UI Components
The game uses a mix of standard Unity UI and TextMeshPro.

- **Unity UI Text**: Used in most legacy UI panels.
- **TextMeshPro (TMPro)**: Used in newer panels and the InputModule.
- **Accessing Private UI Fields**: Many UI references are marked as `[SerializeField] private`. Accessing them requires C# Reflection.
  ```csharp
  // Example using ReflectionHelper pattern
  var field = typeof(MainMenu).GetField("m_someText", BindingFlags.NonPublic | BindingFlags.Instance);
  var textComponent = (Text)field.GetValue(mainMenuInstance);
  ```

---

## 4. Game States
The game logic is driven by a state machine in `GameController`.

- **GameState.WalkingState**: Free movement in the workshop.
- **GameState.WorkingOnComputerState**: Interacting with a PC on a workbench.
- **GameState.UI**: Global UI screens (Main Menu, Options).

---

## 5. Localization
The game uses **I2 Localization**.
- **Current Language**: `I2.Loc.LocalizationManager.CurrentLanguage` (e.g., "English", "French").
- **Language Code**: `I2.Loc.LocalizationManager.CurrentLanguageCode` (e.g., "en-US").
