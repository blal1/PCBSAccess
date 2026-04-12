# Kapitel 10: Fortgeschrittenes Abfangen

Harmony Patching ist die "Geheimzutat" des Accessibility-Moddings. Dieses Kapitel behandelt die fortgeschrittenen Techniken, die zum Abfangen komplexer Spiellogik erforderlich sind.

## 1. Die drei Arten von Patches
*   **Prefix:** Wird *vor* der Spielmethode ausgeführt. Verwenden Sie dies, um Parameter zu lesen oder um die ursprüngliche Methode **abzubrechen** (indem Sie `false` zurückgeben).
*   **Postfix:** Wird *nach* der Spielmethode ausgeführt. Verwenden Sie dies, um das **Ergebnis** einer Methode zu lesen oder um zu erkennen, dass eine Aktion abgeschlossen wurde.
*   **Transpiler:** Der mächtigste Patch. Er modifiziert den tatsächlichen "IL" (Byte-Code) der Methode. Verwenden Sie dies nur, wenn Sie Logik *innerhalb* einer sehr großen Methode ändern müssen.

## 2. Private Methoden abfangen
Harmony ist es egal, ob eine Methode `private` ist. Sie können sie trotzdem patchen, indem Sie `AccessTools` verwenden.
```csharp
[HarmonyPatch]
public class PrivateMethodPatch {
    static MethodBase TargetMethod() {
        return AccessTools.Method(typeof(PlayerClass), "InternalUpdateLogic");
    }
    // ... Postfix-Implementierung ...
}
```

## 3. Zugriff auf private Felder in einem Patch
Verwenden Sie das Präfix `___` (drei Unterstriche) in Ihren Patch-Parametern, um automatisch den Wert eines privaten Feldes aus der Instanz zu erhalten.
```csharp
[HarmonyPostfix]
public static void Postfix(PlayerClass __instance, int ___currentGold) {
    // ___currentGold enthält nun den Wert des privaten Feldes 'currentGold'
    ScreenReader.Say($"Gold geändert: {___currentGold}");
}
```

## 4. Die Strategie des "manuellen Patchens"
Manchmal können Sie keine Attribute zum Patchen verwenden, weil Sie den genauen Klassennamen erst kennen, wenn das Spiel läuft (häufig bei einigen komplexen Unity-Architekturen). Verwenden Sie in diesem Fall manuelles Patchen:
```csharp
var original = AccessTools.Method(typeof(GameClass), "MethodName");
var postfix = AccessTools.Method(typeof(MyPatch), "MyPostfix");
harmony.Patch(original, postfix: new HarmonyMethod(postfix));
```

### Profi-Tipp: Patchen von IL2CPP-Spielen
Wenn Sie ein IL2CPP-Spiel modden (über MelonLoader), sind die Methoden, die Sie patchen, eigentlich C++-Proxys. Harmony (über die Cpp2IL-Brücke) erledigt das meiste davon für Sie, aber stellen Sie sich auf leicht abweichende Methodensignaturen ein.

---
*Weiter: [Kapitel 11: Der polyglotte Mod](kapitel-11.md)*
