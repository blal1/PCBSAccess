using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces when a job objective transitions from unsatisfied → satisfied.
    ///
    /// Patches Objective.UpdateSatisifiedBy with Prefix (capture old state) +
    /// Postfix (detect false→true transition). Announces objective description.
    ///
    /// Hidden objectives are skipped — only visible objectives announced.
    /// </summary>
    public static class ObjectiveHandler
    {
        #region Patches

        [HarmonyPatch(typeof(Objective), "UpdateSatisifiedBy")]
        static class Objective_UpdateSatisifiedBy_Patch
        {
            /// <summary>Capture satisfaction state before update.</summary>
            static void Prefix(Objective __instance, out bool __state)
            {
                __state = __instance.IsSatisified();
            }

            /// <summary>Detect false→true transition and announce.</summary>
            static void Postfix(Objective __instance, Job job, bool __result, bool __state)
            {
                try
                {
                    // Only fire on the transition: was not satisfied, now is
                    if (__state || !__result) return;

                    // Skip hidden objectives (player shouldn't know about these)
                    if (__instance.IsHidden()) return;

                    string desc = __instance.GetDescription(job);
                    if (string.IsNullOrEmpty(desc)) return;

                    ScreenReader.Say(Loc.Get("objective_complete", desc));
                    DebugLogger.LogState($"ObjectiveHandler: '{desc}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Objective_UpdateSatisifiedBy_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
