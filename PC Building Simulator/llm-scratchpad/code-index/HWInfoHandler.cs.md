# Code Index for HWInfoHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces hardware info when the HWInfo tablet app opens.
- Line 8: ///
- Line 9: /// Patches HWInfoApp.Start postfix — reads all summary rows (component name + properties)
- Line 10: /// and announces them in sequence.
- Line 11: /// F8 (wired in Main): re-reads the last hardware summary.
- Line 12: /// </summary>
- Line 13: public static class HWInfoHandler
- Line 23: /// <summary>Re-reads the last hardware summary. Called from Main on F8.</summary>
- Line 24: public static void AnnounceLastSummary()
- Line 32: /// <summary>Resets on scene change.</summary>
- Line 33: public static void Reset()
- Line 42: /// <summary>
- Line 43: /// Fires after HWInfo app initialises. Reads all component summary rows.
- Line 44: /// Uses the row prefab structure: parent row = component type/name, child rows = properties.
- Line 45: /// </summary>
- Line 47: static class HWInfoApp_Start_Patch
- Line 49: static void Postfix(HWInfoApp __instance)
