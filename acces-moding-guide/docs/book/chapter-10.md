# Chapter 10: Advanced Interception

Harmony Patching is the "Secret Sauce" of accessibility modding. This chapter covers the advanced techniques needed to intercept complex game logic.

## 1. The Three Types of Patches
*   **Prefix:** Runs *before* the game method. Use this to read parameters or to **cancel** the original method (by returning `false`).
*   **Postfix:** Runs *after* the game method. Use this to read the **result** of a method or to detect that an action has completed.
*   **Transpiler:** The most powerful. It modifies the actual "IL" (byte-code) of the method. Use this only when you need to change logic *inside* a very large method.

## 2. Intercepting Private Methods
Harmony doesn't care if a method is `private`. You can still patch it by using `AccessTools`.
```csharp
[HarmonyPatch]
public class PrivateMethodPatch {
    static MethodBase TargetMethod() {
        return AccessTools.Method(typeof(PlayerClass), "InternalUpdateLogic");
    }
    // ... Postfix implementation ...
}
```

## 3. Accessing Private Fields in a Patch
Use the `___` (three underscores) prefix in your patch parameters to automatically receive the value of a private field from the instance.
```csharp
[HarmonyPostfix]
public static void Postfix(PlayerClass __instance, int ___currentGold) {
    // ___currentGold now contains the value of the private 'currentGold' field
    ScreenReader.Say($"Gold changed: {___currentGold}");
}
```

## 4. The "Manual Patch" Strategy
Sometimes, you cannot use attributes for patching because you don't know the exact class name until the game is running (common in some complex Unity architectures). In this case, use manual patching:
```csharp
var original = AccessTools.Method(typeof(GameClass), "MethodName");
var postfix = AccessTools.Method(typeof(MyPatch), "MyPostfix");
harmony.Patch(original, postfix: new HarmonyMethod(postfix));
```

### Pro Tip: Patching IL2CPP Games
When modding an IL2CPP game (via MelonLoader), the methods you patch are actually C++ proxies. Harmony (via the Cpp2IL bridge) handles most of this for you, but be prepared for slightly different method signatures.

---
*Next: [Chapter 11: The Polyglot Mod](chapter-11.md)*
