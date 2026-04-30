# Code Index for PCPowerHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces PC power-on and power-off events.
- Line 8: ///
- Line 9: /// Patches Case.PowerOn (returns bool — true = success) — announces on success.
- Line 10: /// Patches Case.PowerOff — announces power off.
- Line 11: /// </summary>
- Line 12: public static class PCPowerHandler
- Line 16: /// <summary>Fires after PowerOn. __result = true means PC successfully started.</summary>
- Line 18: static class Case_PowerOn_Patch
- Line 20: static void Postfix(bool __result)
- Line 37: /// <summary>Fires when the PC powers off.</summary>
- Line 39: static class Case_PowerOff_Patch
- Line 41: static void Postfix()
