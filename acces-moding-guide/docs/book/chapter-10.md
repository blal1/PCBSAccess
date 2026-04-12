# Chapter 10: Advanced Interception

## Introduction: The Scalpel of Modding

Up to this point in our journey, we have treated Harmony as a blunt instrument. We have used `Postfix` patches to listen for when a menu opens or closes, and we have used `Prefix` patches to read simple parameters. For many accessibility mods, this is sufficient. 

However, as you delve into more complex games, you will encounter scenarios where the blunt instrument fails. You will find that the game developers wrote a single, massive, 2,000-line `Update()` method that handles combat, inventory, and dialogue all at once. You cannot simply `Postfix` this method, because it runs 60 times a second, and you only care about a specific event buried deep inside it.

This is where you must upgrade from a hammer to a scalpel. This chapter covers the advanced capabilities of the Harmony library. We will explore how to manipulate method arguments, how to bypass the C# compiler's access restrictions dynamically, and finally, how to use `Transpilers` to surgically alter the game's Intermediate Language (IL) byte-code on the fly.

---

## 1. Mastering the Prefix: Modification and Cancellation

A `Prefix` patch runs immediately before the original game method executes. We previously noted that returning `false` from a Prefix will cancel the original method entirely. But the Prefix has another, arguably more powerful capability: it can **modify** the arguments being passed to the original method.

### Intercepting and Altering State
Imagine a game where the player takes damage from a trap. The method signature might look like this:
`public void TakeDamage(float amount, DamageType type)`

If you want to create an "Accessibility Assist" mode that reduces trap damage for blind players (perhaps because the traps rely on purely visual cues that cannot be telegraphed via audio), you can modify the `amount` variable before the game even processes it.

To do this in Harmony, you must pass the argument by `ref`.

```csharp
[HarmonyPatch(typeof(PlayerStats), "TakeDamage")]
public class ReduceTrapDamagePatch
{
    // Note the 'ref' keyword on the amount parameter
    public static void Prefix(ref float amount, DamageType type)
    {
        if (type == DamageType.VisualTrap)
        {
            // Reduce damage by 50%
            amount = amount * 0.5f;
            DebugLogger.Log($"Reduced visual trap damage to {amount}");
            ScreenReader.Say("Trap triggered. Damage reduced.", true);
        }
    }
}
```

Because the argument is passed by reference (`ref`), changes you make in the Prefix are passed along to the original game method. The game engine genuinely believes the trap only dealt half damage. This is a profound level of control over the game's logic.

---

## 2. Dynamic Access: The Power of AccessTools

Throughout this book, we have emphasized the importance of accessing `private` fields. We built a `ReflectionHelper` to handle this efficiently. However, Harmony provides its own utility class called `AccessTools` which is incredibly robust and handles edge cases that standard reflection might miss.

`AccessTools` is not just for fields. It can find private methods, hidden properties, and even nested classes that the decompiler struggles to display properly.

### Invoking Private Methods
Sometimes you don't want to patch a method; you want to *use* it. If a developer wrote a highly useful private method like `void RecalculateInventoryWeight()`, and you need to trigger that calculation after your mod automatically sorts the inventory, you must invoke it via reflection.

```csharp
// Find the private method using AccessTools
MethodInfo recalcMethod = AccessTools.Method(typeof(InventoryManager), "RecalculateInventoryWeight");

if (recalcMethod != null)
{
    // Invoke the method on the Singleton instance
    recalcMethod.Invoke(InventoryManager.Instance, null);
}
```

### The Delegate Shortcut (Advanced Performance)
Using `MethodInfo.Invoke` is slow. If you need to call a private method repeatedly (e.g., inside an Update loop), you should use `AccessTools` to create a **Delegate**. A delegate acts like a direct, strongly-typed pointer to the method, and calling it is almost as fast as a normal method call.

```csharp
// Define a delegate that matches the private method's signature
delegate void RecalcWeightDelegate(InventoryManager instance);

// Create the fast delegate
RecalcWeightDelegate fastRecalc = AccessTools.MethodDelegate<RecalcWeightDelegate>(
    AccessTools.Method(typeof(InventoryManager), "RecalculateInventoryWeight")
);

// Call it blazingly fast
fastRecalc(InventoryManager.Instance);
```

---

## 3. Manual Patching: When Attributes Fail

Until now, we have used Attributes (e.g., `[HarmonyPatch(typeof(MyClass), "MyMethod")]`) to tell Harmony what to patch. This is clean and easy to read. But it has a fatal flaw: the `typeof()` operator requires you to know the exact class name when you compile your mod.

What if the class name is dynamic? What if you are modding a game that uses a complex framework where the UI classes are generated at runtime, or you need to patch a generic method like `GetComponent<T>()`?

In these scenarios, you must use **Manual Patching**. Instead of relying on attributes, you write code that tells Harmony exactly what to do during your mod's initialization phase.

```csharp
public override void OnInitializeMelon() // Or Awake() in BepInEx
{
    var harmony = new HarmonyLib.Harmony("com.myname.myaccessibilitymod");

    // 1. Find the original method dynamically
    Type targetType = AccessTools.TypeByName("SomeObscureNamespace.HiddenClass");
    MethodInfo originalMethod = AccessTools.Method(targetType, "TheMethodIWant");

    // 2. Find your custom patch method
    MethodInfo myPostfix = AccessTools.Method(typeof(MyPatchClass), nameof(MyPatchClass.MyCustomPostfix));

    // 3. Instruct Harmony to perform the surgery
    harmony.Patch(originalMethod, postfix: new HarmonyMethod(myPostfix));
    
    DebugLogger.Log("Manual patch applied successfully.");
}

// The patch class does NOT have Harmony attributes
public class MyPatchClass
{
    public static void MyCustomPostfix()
    {
        ScreenReader.Say("The hidden method was called!");
    }
}
```

Manual patching is slightly more verbose, but it offers absolute flexibility. It is the only way to patch generics and dynamically loaded assemblies.

---

## 4. The Transpiler: Rewriting the Byte-Code

We now arrive at the most difficult, dangerous, and powerful tool in the modder's arsenal: The Transpiler.

A Transpiler is a Harmony patch that does not run before or after a method. Instead, Harmony passes the Transpiler the actual compiled IL (Intermediate Language) instructions of the original method as a list. Your Transpiler is allowed to insert, delete, or modify instructions in this list, and then hand it back to Harmony. Harmony then compiles this mutated list into executable machine code.

You are literally rewriting the game's code on the fly.

### When to use a Transpiler
You should only use a Transpiler when:
1.  A method is massive, and you only want to intercept a single, specific event buried inside it.
2.  The method lacks any useful Prefix or Postfix hooks.
3.  You need to alter the logic in a way that a Prefix cancellation cannot achieve (e.g., changing an `if` statement's condition in the middle of a loop).

### Understanding IL Code
To write a Transpiler, you must understand IL. C# code is compiled into instructions called `OpCodes`. 
For example, the C# code `int a = 5 + 3;` might become:
1.  `Ldc_I4_5` (Load the constant integer 5 onto the stack)
2.  `Ldc_I4_3` (Load the constant integer 3 onto the stack)
3.  `Add` (Pop the two numbers, add them, and push the result)
4.  `Stloc_0` (Store the result in local variable 0)

### Anatomy of a Transpiler
A Transpiler receives an `IEnumerable<CodeInstruction>` and must return an `IEnumerable<CodeInstruction>`.

Imagine a game method that looks like this:
```csharp
public void UpdatePlayer()
{
    MovePlayer();
    if (health < 10) {
        PlayLowHealthWarningSound(); // We want to announce this!
    }
    DrawUI();
}
```

We want to insert our `ScreenReader.Say()` call right after the low health sound plays.

```csharp
[HarmonyPatch(typeof(PlayerController), "UpdatePlayer")]
public static class LowHealthTranspiler
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        
        // We need to find the method call to PlayLowHealthWarningSound
        MethodInfo targetMethod = AccessTools.Method(typeof(PlayerController), "PlayLowHealthWarningSound");
        
        // We need the MethodInfo for our own injection
        MethodInfo mySayMethod = AccessTools.Method(typeof(ScreenReader), "Say");

        for (int i = 0; i < codes.Count; i++)
        {
            // If the instruction is calling our target method...
            if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == targetMethod)
            {
                // We found the injection point!
                // We want to insert our code IMMEDIATELY AFTER this instruction (i + 1)
                
                // 1. Load the string "Health Critical!" onto the IL stack
                var loadStringInst = new CodeInstruction(OpCodes.Ldstr, "Health Critical!");
                
                // 2. Load the boolean 'true' (for the interrupt parameter) onto the stack
                var loadBoolInst = new CodeInstruction(OpCodes.Ldc_I4_1);
                
                // 3. Call the ScreenReader.Say method
                var callInst = new CodeInstruction(OpCodes.Call, mySayMethod);
                
                // Insert them in reverse order at i+1
                codes.Insert(i + 1, callInst);
                codes.Insert(i + 1, loadBoolInst);
                codes.Insert(i + 1, loadStringInst);
                
                DebugLogger.Log("Transpiler successfully injected screen reader call.");
                break; // Stop searching once we've injected
            }
        }

        // Return the modified code list to Harmony
        return codes.AsEnumerable();
    }
}
```

### The Fragility of Transpilers
Transpilers are incredibly fragile. In the example above, we relied on finding the `PlayLowHealthWarningSound` method call. If the game developer releases a patch and renames that method to `PlayWarning()`, our Transpiler will fail to find it, the injection will not happen, and the mod will compile perfectly but fail silently at runtime.

Therefore, Transpilers should only be used by advanced modders, and they must be accompanied by extensive error logging to ensure you know exactly when an update has broken your IL pattern matching.

---

## Conclusion: The Ultimate Authority

With mastery over Prefixes, Postfixes, `AccessTools`, manual patching, and Transpilers, you have achieved ultimate authority over the game's execution environment. There is no variable you cannot read, no method you cannot intercept, and no logic you cannot alter.

You have the tools to force any game, no matter how stubbornly visual or how poorly coded, to yield its data to your accessibility framework.

However, all of this technical power is meaningless if the player cannot understand the output. If your mod intercepts a complex state change but announces it in broken English, or worse, in a language the player does not speak, the accessibility fails at the final hurdle. 

In the next chapter, we will address the final frontier of accessibility modding: **The Polyglot Mod**. We will learn how to design a robust localization framework that ensures your mod can speak to players around the world in their native tongue, seamlessly syncing with the game's own internal language settings.

---
*Character Count Check: ~11,300 characters.*
