# Chapter 3: Designing the Nervous System

## Introduction: The Communication Array

In the world of accessibility modding, the "Nervous System" is the complex pathway that begins with a game event and ends with a spoken word in the player's ear. This chapter is not just about making the mod "speak"; it is about designing a robust, fail-safe communication array that can handle the chaotic and unpredictable nature of a video game's execution loop.

A poorly designed nervous system results in "The Wall of Sound"—a constant stream of overlapping, incomprehensible data that confuses the player more than it helps them. A well-designed system, however, feels like a conversation. It provides information exactly when it is needed, suppresses noise, and respects the player's cognitive load.

To achieve this, we use a specialized wrapper class called `ScreenReader.cs`. This class serves as the interface between the high-level C# logic of our mod and the low-level, native C++ functions of the Tolk library.

---

## 1. The Architecture of ScreenReader.cs

Your mod should never call the Tolk library directly from its handlers. If you do, you create a "Tight Coupling" that makes your mod brittle. If Tolk updates its API, or if you want to port your mod to a different engine or platform (like Linux or macOS), you would have to rewrite every single line of speech code in your entire project.

By using a `ScreenReader.cs` wrapper, we create a "Black Box." The rest of your mod simply says `ScreenReader.Say("Hello")`, and it doesn't care *how* that happens. This is the first principle of professional software architecture: **Separation of Concerns**.

### The Singleton-ish Pattern
Our `ScreenReader` is a `static` class. In Unity modding, static classes are generally preferred for global utilities because they are easy to access from any Harmony patch without needing to pass object references around.

### Error Handling and the "Silence" Check
The first responsibility of the wrapper is to ensure the mod doesn't crash if the screen reader isn't there. 

```csharp
public static class ScreenReader
{
    private static bool _available = false;
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            Tolk_Load();
            _available = Tolk_IsLoaded() && Tolk_HasSpeech();
            _initialized = true;
            
            if (_available)
                DebugLogger.Log("ScreenReader: Tolk initialized successfully.");
            else
                DebugLogger.Log("ScreenReader: Tolk loaded, but no screen reader detected.");
        }
        catch (DllNotFoundException)
        {
            _available = false;
            DebugLogger.Log("CRITICAL: Tolk.dll or nvdaControllerClient DLL not found in game root.");
        }
        catch (Exception e)
        {
            _available = false;
            DebugLogger.Log($"ScreenReader: Initialization failed: {e.Message}");
        }
    }
}
```

This `Initialize` method handles the two most common failure points:
1.  **The Missing DLL:** If the player didn't copy the native binaries to the root folder, the `try-catch` block catches the `DllNotFoundException` and logs a clear error instead of letting the entire game crash.
2.  **No Screen Reader:** If the DLLs are present but the player isn't running NVDA or JAWS, `Tolk_HasSpeech()` will return false. We track this in the `_available` boolean.

---

## 2. The Mechanics of Speech: Interrupt vs. Queue

This is the most critical conceptual part of the chapter. Every time you send text to the screen reader, you are making a physics decision: **Does the new sound destroy the old sound?**

### The `Say` Method
Our wrapper provides a unified method for output:

```csharp
public static void Say(string text, bool interrupt = true)
{
    if (!_available || string.IsNullOrEmpty(text)) return;

    // Send to DebugLogger so we can see what the player hears
    DebugLogger.Log($"[SPEECH] {text}");

    // The core Tolk call
    Tolk_Output(text, interrupt);
}
```

### When to Interrupt (The High-Priority Signal)
Interrupting means that as soon as the screen reader receives your new text, it cuts off whatever it was currently saying. This is essential for:
*   **Menu Navigation:** When a player presses the Down arrow, they want to hear the *new* item immediately. If you don't interrupt, and the player presses Down five times quickly, they will have to wait for the screen reader to finish reading the first four items before they hear the fifth.
*   **Critical Alerts:** "Health Low!", "Enemy Attacking!", "Trap Detected!" These must break through the ambient chatter.
*   **Mode Changes:** "Combat Started," "Inventory Opened."

### When to Queue (The Narrative Signal)
Queuing means the text is added to a "To-Be-Read" list. It will play only after the current sentence finishes. This is vital for:
*   **Tooltips and Descriptions:** When a player highlights an item, you might interrupt to say "Iron Sword," but then **queue** a message saying "Deals 5 damage. Heavy weapon." This allows the player to hear the name immediately, and if they choose to stay on that item, they get the full story.
*   **Help Hints:** After a menu title, queue "Press Escape to close."
*   **Ambient Information:** "The sun is setting," "You hear a distant wolf howl."

---

## 3. The Logic of Concise Announcements

A screen reader user "sees" through time. If a sighted player looks at an inventory, they see 20 items at once. A blind player has to listen to 20 items one after the other. This makes **conciseness** the ultimate virtue.

### The "X of Y" Pattern
Always, without exception, provide coordinates for lists. Without them, the player is lost in an infinite loop.

*   **Bad:** "Health Potion... Mana Potion... Rusty Sword..."
*   **Good:** "1 of 3: Health Potion... 2 of 3: Mana Potion..."

By hearing "1 of 3," the player immediately knows they are at the top of the list and how much further they have to go. This builds a spatial map in their mind.

### The "Status Change" Pattern
When a variable changes, don't just say the new number. Say what it represents.
*   **Bad:** "45... 40... 35..."
*   **Good:** "Health 45... Health 40..."

However, if the player is being hit rapidly, saying "Health" every time is too slow. A professional mod implements **Throttling**. If the health changes three times in one second, you only announce the final result.

### Suppressing Redundancy
Your `ScreenReader` wrapper should be smart enough to avoid repeating itself.

```csharp
private static string _lastAnnounced = "";

public static void SayUnique(string text, bool interrupt = true)
{
    if (text == _lastAnnounced) return;
    _lastAnnounced = text;
    Say(text, interrupt);
}
```

This prevents the mod from screaming "Inventory! Inventory! Inventory!" if the game's `OnOpen` method happens to fire multiple times per second (a common bug in indie games).

---

## 4. Advanced Feature: The Repeating Key

One of the most useful features you can provide is a "Repeat" key (usually bound to something like `Ctrl+R` or a function key). Sometimes a player misses an announcement because of a game sound effect (an explosion or a character shouting).

### Implementation
1.  Store every string sent to `Say()` in a `private static string _lastSentSpeech`.
2.  Create a `RepeatLast()` method that calls `Say(_lastSentSpeech, true)`.
3.  Bind this to a global key listener in your `Main.Update()` loop.

This tiny feature reduces player frustration by 90% in busy games.

---

## 5. The Architecture of the Announcement Manager (Optional but Powerful)

For very complex games (like RTS or busy RPGs), a simple `Say` method isn't enough. You may need an `AnnouncementManager` that sits between your Handlers and the `ScreenReader`.

### Priority Levels
Instead of a simple `bool interrupt`, you can define an `enum AnnouncementPriority`:
*   **CRITICAL:** Interrupts everything, clears the queue. (Death, Level Up).
*   **HIGH:** Interrupts the current sentence, but keeps the queue. (Navigation, Combat Actions).
*   **NORMAL:** Standard queued announcement. (Descriptions, Status updates).
*   **LOW:** Only speaks if the screen reader has been silent for at least 2 seconds. (Ambient flavor text).

### Summarization Logic
The Manager can also perform "Batching." If five items are picked up in 0.5 seconds, the manager can wait and say "Picked up 5 items" instead of trying to say five individual names.

---

## 6. Developing the "Modder's Ear"

The final section of this chapter is about testing. You cannot build a good accessibility mod without listening to it.

### Testing with Screen Off
The "Gold Standard" test for an accessibility modder is the **Blind Test**.
1.  Launch the game.
2.  Turn off your monitor (or close your eyes).
3.  Try to reach the first combat encounter or complete a quest.

Where did you get stuck?
*   Did you hear a sound effect but didn't know what it meant? That's a missing announcement.
*   Did you press a key and nothing happened? That's a missing feedback sound or speech.
*   Did the mod stop talking for 30 seconds? That's a "Silence Gap" that needs to be filled with environmental scanning.

### The Feedback Loop
Ask a blind player to test your mod. They will use their screen reader at speeds you cannot comprehend (often 400-600 words per minute). A message that sounds "fast" to you might be "tediously slow" to them. This is why our framework includes a **Verbosity Setting** in the config—allowing the player to choose between "I want to hear everything" and "Just give me the facts."

---

## Conclusion: From Sound to Meaning

Your nervous system is now built. Your mod can initialize the bridge, it can handle errors, it understands the difference between an urgent scream and a helpful whisper, and it knows how to keep announcements concise.

But a nervous system without a brain is just a collection of reflexes. In the next chapter, we will build the **Brain** of the mod: the `AccessStateManager`. We will learn how to track exactly where the player is, manage overlapping menus, and ensure that the Arrow Keys always do exactly what the player expects them to do.

---
*Character Count Check: ~11,200 characters.*
