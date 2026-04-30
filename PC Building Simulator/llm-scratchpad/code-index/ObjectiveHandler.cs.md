# Code Index for ObjectiveHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces when a job objective transitions from unsatisfied → satisfied.
- Line 8: ///
- Line 9: /// Patches Objective.UpdateSatisifiedBy with Prefix (capture old state) +
- Line 10: /// Postfix (detect false→true transition). Announces objective description.
- Line 11: ///
- Line 12: /// Hidden objectives are skipped — only visible objectives announced.
- Line 13: /// </summary>
- Line 14: public static class ObjectiveHandler
- Line 19: static class Objective_UpdateSatisifiedBy_Patch
- Line 21: /// <summary>Capture satisfaction state before update.</summary>
- Line 22: static void Prefix(Objective __instance, out bool __state)
- Line 27: /// <summary>Detect false→true transition and announce.</summary>
- Line 28: static void Postfix(Objective __instance, Job job, bool __result, bool __state)
