# Code Index for VirusScanAppHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces virus scan state changes (Standard → Scanning → Dirty/Clean).
- Line 8: /// Patches the private SetVisuals method to read title + message after each state change.
- Line 9: /// Deduplicates — same title not announced twice in a row.
- Line 10: /// </summary>
- Line 11: public static class VirusScanAppHandler
- Line 15: public static void Reset()
- Line 20: /// <summary>
- Line 21: /// Patches the private SetVisuals(State) method.
- Line 22: /// Private enum param not referenced in our signature — Harmony matches by name.
- Line 23: /// </summary>
- Line 25: static class VirusScanApp_SetVisuals_Patch
- Line 27: static void Postfix(VirusScanApp __instance)
