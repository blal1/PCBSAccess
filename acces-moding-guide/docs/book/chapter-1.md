# Chapter 1: The Modding Ecosystem

## Introduction: The Philosophy of Accessibility Modding

Before we write our first line of code or decompile our first DLL, we must fundamentally understand the environment we are working in. Accessibility modding is not merely about writing scripts; it is the delicate art of injecting new behavior into existing, compiled, and often obfuscated software without breaking its original functionality. For blind players, accessibility modding is the difference between a game being an unplayable black box and an immersive, engaging world.

The goal of this book is to teach you how to build professional-grade accessibility mods for Unity games. We operate under a strict philosophy: **Playability, Not Simplification**. Our objective is to translate visual data into auditory data, allowing a blind player to make the same tactical, strategic, and navigational decisions as a sighted player. We do not build auto-battlers, we do not build bots, and we do not implement cheats unless absolutely necessary to bypass an inherently visual-only puzzle. We build bridges.

To build these bridges, we rely on a powerful stack of community-driven tools. These tools form the modding ecosystem. Understanding how these tools interact is the most important step in becoming an accessibility modder.

---

## 1. The Unity Engine: The Target

Unity is one of the most popular game engines in the world, powering thousands of indie and AAA titles alike. For a modder, Unity is an excellent target because of how it is built.

### The Architecture of a Unity Game
At its core, Unity uses C# as its scripting language. When a game developer writes a script for a character controller or an inventory system, they write it in C#. When the game is "built" (compiled) for release, this C# code is not turned directly into machine code (ones and zeros). Instead, it is compiled into an "Intermediate Language" (IL) and packaged into `.dll` (Dynamic Link Library) files.

The most important file you will interact with is usually named `Assembly-CSharp.dll`. This file contains almost all of the unique, game-specific logic written by the developers. Because it is in Intermediate Language, we can use tools called "decompilers" to reverse-engineer the DLL back into readable C# code. This means that even without the original source code, we can see exactly how the game works.

### Mono vs. IL2CPP
Unity games are built using one of two scripting backends: Mono or IL2CPP. Understanding the difference is crucial for choosing the right tools.

**Mono:**
In a Mono build, the game's C# code is compiled directly into the standard .NET DLLs mentioned above. If you look inside the game's installation folder and see a directory named `[GameName]_Data\Managed`, and inside that folder you see `Assembly-CSharp.dll`, you are looking at a Mono game. Mono games are the easiest to mod because the code is highly readable when decompiled, and injecting new code is straightforward.

**IL2CPP (Intermediate Language To C++):**
To improve performance and security, Unity introduced IL2CPP. In this backend, the game's C# code is first compiled into IL, but then Unity converts that IL into C++ code, which is then compiled into a native binary. If you look in a game folder and see a folder named `il2cpp_data` and no `Managed` folder, the game is IL2CPP.

Modding IL2CPP games used to be incredibly difficult, but modern mod loaders have largely solved this problem by dynamically generating "dummy" DLLs (proxies) that mirror the original C# structures. This allows us to write C# mods that interact with the C++ game engine as if it were a standard Mono game. However, you will notice that decompiling an IL2CPP game yields less readable code (often missing method bodies), meaning you have to rely more on reading method signatures and understanding the game's architecture through intuition and experimentation.

### GameObjects and Components
Unity uses an Entity-Component-System (ECS) architecture. Everything in a Unity scene is a `GameObject`. A `GameObject` on its own does nothing; it is simply an empty container with a position in the world (a Transform).

To make a `GameObject` do something, developers attach `Components` to it. 
*   If a `GameObject` needs to be drawn on screen, it gets a `MeshRenderer` component.
*   If it needs physical gravity, it gets a `Rigidbody` component.
*   If it represents the player's health, it might get a custom `PlayerHealth` component.
*   If it is a piece of text on the screen, it gets a `Text` or `TextMeshProUGUI` component.

As an accessibility modder, your primary job will be writing code that searches for specific `GameObjects`, extracts the relevant `Components` (like reading the text off a UI element), and translates that data into speech.

---

## 2. Mod Loaders: BepInEx and MelonLoader

A "Mod Loader" is a specialized program that acts as a middleman between the operating system and the game. When you launch a game normally, the operating system loads the game's executable file into memory and starts running it. When a mod loader is installed, it hijacks this startup process. It loads itself into memory first, prepares an environment for your mod, loads your mod's DLL, and then allows the game to continue starting up.

There are two primary mod loaders used in the Unity modding community: BepInEx and MelonLoader.

### BepInEx
BepInEx is the industry standard for many Unity mods. It has been around for a long time, is incredibly stable, and has a massive community. 

*   **Architecture:** BepInEx uses a system of "Preloaders" and "Plugins". Your accessibility mod will be a Plugin. BepInEx provides a base class called `BaseUnityPlugin` which inherits from Unity's `MonoBehaviour`. This means your mod can hook directly into Unity's lifecycle events like `Awake()`, `Start()`, and `Update()`.
*   **Strengths:** BepInEx is incredibly lightweight and has fantastic support for Mono games. It also has a mature configuration system, allowing players to easily edit settings files to change how your mod behaves.
*   **Weaknesses:** While BepInEx 6 supports IL2CPP, its IL2CPP support is sometimes considered less user-friendly out-of-the-box compared to MelonLoader.

### MelonLoader
MelonLoader is a slightly newer alternative that gained massive popularity due to its robust, built-in support for IL2CPP games.

*   **Architecture:** MelonLoader uses a different initialization flow. Instead of inheriting from `MonoBehaviour`, your mod inherits from `MelonMod`. MelonLoader provides its own lifecycle callbacks, such as `OnInitializeMelon()` and `OnUpdate()`. 
*   **Strengths:** MelonLoader's "Il2CppAssemblyUnhollower" (now usually Cpp2IL) automatically generates the proxy DLLs you need to mod IL2CPP games the very first time the game is launched. It makes modding an IL2CPP game feel almost identical to modding a Mono game.
*   **Weaknesses:** MelonLoader can sometimes be heavier, and its aggressive hooking can trigger anti-cheat software in multiplayer games more easily than BepInEx.

### Which should you choose?
In this book, we will provide templates for both. However, the general rule of thumb in the accessibility modding community is:
1.  Check what the game's existing modding community uses. If a game has 100 mods on NexusMods using BepInEx, use BepInEx. You don't want to force players to install two different mod loaders.
2.  If there is no existing community and the game is Mono, use BepInEx.
3.  If there is no existing community and the game is IL2CPP, use MelonLoader.

Ultimately, 95% of your code (the accessibility logic, the screen reader wrapper, the state management) will be identical regardless of which loader you use. Only the entry point (the "Main" class) changes.

---

## 3. Harmony: The Master Interceptor

If the Mod Loader is the vehicle that gets your code into the game, **Harmony** is the surgical tool you use to attach your code to the game's nervous system.

Harmony is a library included by default with both BepInEx and MelonLoader. It allows you to "patch" game methods at runtime. Without Harmony, your mod would only be able to poll the game state every frame (e.g., constantly asking "Is the menu open? Is the menu open?"). With Harmony, you can intercept the exact moment the game executes a specific piece of code.

### How Harmony Works
When you use Harmony to patch a method, Harmony finds where that method lives in the computer's RAM. It overwrites the very first few bytes of that method with a "Jump" instruction. When the game tries to execute its own method, the CPU hits the Jump instruction and is redirected to your mod's code instead.

There are three primary types of patches you will use:

### 1. The Postfix Patch
A Postfix patch runs *after* the original game method has finished executing. This is the most common patch in accessibility modding. 
*   **Use Case:** The game calls `InventoryUI.Show()`. You want to announce "Inventory Opened" to the screen reader. You place a Postfix patch on `Show()`. The game draws the UI, and then your patch runs and triggers the speech.
*   **Reading Results:** A Postfix patch can also read the `return` value of the original method, allowing you to see what decision the game just made.

### 2. The Prefix Patch
A Prefix patch runs *before* the original game method executes. 
*   **Use Case:** You want to read the parameters being passed to a function. For example, if the game calls `Player.TakeDamage(int amount)`, you can place a Prefix patch to read the `amount` variable and announce "Took 15 damage" before the damage is actually applied.
*   **Canceling Execution:** A Prefix patch can return `false`. If it does, Harmony skips the original game method entirely. This is incredibly powerful but dangerous. You can use this to prevent the game from processing certain inputs or playing certain visual effects, but if used incorrectly, you will break the game's internal logic.

### 3. The Transpiler Patch
A Transpiler is an advanced patch that does not run before or after a method; instead, it modifies the method's IL (Intermediate Language) instructions themselves. 
*   **Use Case:** Imagine a massive, 500-line method called `UpdateWorld()`, and right in the middle of it, the game updates the player's gold without calling any separate function. You can't use a Prefix or Postfix because you only care about that one specific line in the middle. A Transpiler allows you to scan the byte-code, find the exact instruction where gold is updated, and insert a custom call to your screen reader right there.
*   **Complexity:** Transpilers require an understanding of C# IL OpCodes (like `OpCodes.Ldarg_0` or `OpCodes.Call`). They are complex, fragile (easily broken by game updates), and should only be used as a last resort.

### The Power of AccessTools
Harmony also provides a utility class called `AccessTools`. Even if you aren't patching a method, `AccessTools` is invaluable because it allows you to access `private` and `internal` fields, properties, and methods that the game developers tried to hide. If the game stores the player's health in a private variable that you cannot normally read, `AccessTools` can fetch it for you.

---

## 4. Tolk: The Bridge to the Screen Reader

We have discussed how to get into the game and how to intercept its logic. Now, we must discuss how to get information *out* of the game and into the player's ears.

A screen reader (such as NVDA, JAWS, Narrator, or SuperNova) is a complex piece of software running on the operating system. Your Unity mod, running inside the game's process, cannot easily talk to these applications directly because each screen reader has its own unique API (Application Programming Interface).

Enter **Tolk**.

Tolk is a native C++ library designed specifically to solve this problem. It acts as an abstraction layer. Instead of writing code to talk to NVDA, and then writing different code to talk to JAWS, you write code that talks to Tolk. Tolk then interrogates the operating system, figures out which screen reader is currently running, and translates your message into the correct format for that specific screen reader.

### The Tolk Architecture
Because Tolk is a native C++ library, we cannot simply drag and drop it into our C# project like a normal dependency. We have to use a technique called P/Invoke (Platform Invocation Services) to call the C++ functions from our C# mod.

Furthermore, Tolk relies on specific client libraries to communicate with certain screen readers. The most notable is NVDA. For Tolk to send text to NVDA, it requires an additional DLL called the `nvdaControllerClient`. 

This leads to a strict requirement for your mod's installation:
Whenever a player installs your accessibility mod, they MUST place `Tolk.dll` and the appropriate `nvdaControllerClient` DLL (either 32-bit or 64-bit, matching the game's architecture) directly into the game's root folder, right next to the game's `.exe` file. If these files are placed in the `Mods` or `Plugins` folder, the game process will not be able to find them, and the mod will remain silent.

### Synchronous vs. Asynchronous Speech
When you tell a screen reader to speak, you must make a crucial decision: should this new message interrupt what the screen reader is currently saying, or should it wait its turn?

Tolk handles this via an `interrupt` boolean parameter.
*   **Interrupting (True):** The screen reader immediately stops its current sentence and begins reading the new text. This is essential for critical information. If a player opens a menu, they want to hear "Menu Opened" immediately. If they take damage, they need to know instantly.
*   **Queuing (False):** The text is added to a buffer and will be spoken as soon as the screen reader finishes its current tasks. This is vital for secondary information. For example, if you open an inventory, you might interrupt to say "Inventory", but then queue a message saying "Press F1 for controls". If you interrupted both times, the player would only hear "Press F1 for controls".

Mastering the balance between interrupting and queuing is what separates a frustrating accessibility mod from a smooth, professional one. We will explore this deeply when we build the `ScreenReader` wrapper class.

---

## 5. The Accessibility Modding Mindset

Having the right tools is only half the battle. The other half is approaching the game with the right mindset. Accessibility modding is an exercise in translation. You are translating visual space into linear, auditory time.

### Mental Models of Game State
Sighted players can process dozens of pieces of information simultaneously by glancing at the screen. They can see their health bar is half full, their ammo is low, an enemy is approaching from the left, and the minimap shows an objective to the north—all in a fraction of a second.

A blind player receiving this information through a screen reader must process it sequentially, one word at a time. Therefore, your job is not to dump all the data on the screen into the screen reader. Your job is to curate it.

You must build a "Mental Model" of the game state and determine what is relevant at any given microsecond.
*   If the player is in combat, they don't care that their ultimate ability will be off cooldown in 45 seconds; they care that the enemy is swinging a sword *right now*.
*   If the player is in an inventory menu, they don't care about the ambient world sounds; they care about the stats of the sword they currently have highlighted.

### The Limitations of Visual Rendering
Games are built to look good. This means developers often take shortcuts in the code that make sense visually but are nightmares for screen readers.
*   A developer might not use a real "List" component for an inventory. They might just draw 20 individual buttons on the screen and visually arrange them in a grid.
*   A developer might not use a proper text field for a stylized title. They might use an image of the text.
*   A developer might convey that an item is "equipped" not by changing a variable, but simply by tinting the item's background color green.

As an accessibility modder, you cannot trust the visual rendering. You must dig beneath the UI to find the underlying data structures. If an item is tinted green, you don't write code to check the color green; you decompile the game, find the method that colors it green, and see what variable it is checking (e.g., `item.isEquipped`). You then read that variable directly.

### The Case Study: A "Hello World" Mod
To visualize how this ecosystem comes together, imagine the simplest possible mod: announcing "Game Started" when the main menu loads.

1.  **The Engine:** The game is a Unity Mono game. We decompile `Assembly-CSharp.dll` and find a class named `MainMenuController` with a method `void Start()`.
2.  **The Loader:** We choose BepInEx. We create a C# class `Main` inheriting from `BaseUnityPlugin`.
3.  **The Bridge:** Inside our `Main` class, we write initialization code that hooks up to `Tolk.dll`, which the user has placed next to the game executable.
4.  **The Interceptor:** We use Harmony to create a `Postfix` patch targeting `MainMenuController.Start()`.
5.  **The Execution:** When the player launches the game, BepInEx injects our mod. Our mod initializes Tolk and applies the Harmony patch. The game engine eventually loads the main menu and executes `MainMenuController.Start()`. Harmony intercepts this, finishes the game's visual loading, and then executes our Postfix. Our Postfix sends the string "Game Started" to Tolk. Tolk translates this and sends it to NVDA. NVDA speaks the words to the player.

This entire chain happens in milliseconds, completely invisible to the game engine itself.

---

## Conclusion

The modding ecosystem is a delicate house of cards. Unity provides the canvas, BepInEx/MelonLoader provides the doorway, Harmony provides the scalpel, and Tolk provides the voice. 

If you understand how these four pillars interact, you are ready to begin building your laboratory. In the next chapter, we will walk through the exact steps required to install these tools on your system, configure your development environment, and prepare a blank canvas for your first accessibility mod. We will cover the installation of the .NET SDK, the configuration of your decompilers, and the creation of a robust scaffolding script that will automate the boilerplate code required to get a project off the ground.

Prepare your tools; the real work is about to begin.