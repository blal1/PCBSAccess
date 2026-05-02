using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces PC power-on and power-off events.
    ///
    /// Patches Case.PowerOn (returns bool — true = success) — announces on success.
    /// Patches Case.PowerOff — announces power off.
    /// </summary>
    public static class PCPowerHandler
    {
        #region Patches

        /// <summary>Fires after PowerOn. __result = true means PC successfully started.</summary>
        [HarmonyPatch(typeof(Case), "PowerOn")]
        static class Case_PowerOn_Patch
        {
            static void Postfix(bool __result)
            {
                try
                {
                    if (__result)
                    {
                        ScreenReader.Say(Loc.Get("pc_powered_on"));
                        DebugLogger.LogState("PCPowerHandler: PC powered on");
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Case_PowerOn_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the PC powers off.</summary>
        [HarmonyPatch(typeof(Case), "PowerOff")]
        static class Case_PowerOff_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("pc_powered_off"));
                    DebugLogger.LogState("PCPowerHandler: PC powered off");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Case_PowerOff_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
