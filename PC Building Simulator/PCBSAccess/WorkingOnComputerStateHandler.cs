using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces when the player enters or exits the "working on computer" state (building mode).
    /// </summary>
    public static class WorkingOnComputerStateHandler
    {
        // WorkingOnComputerState.OnStateEnter() has complex IL that HarmonyX cannot compile.
        // Patch the base class OnStateEnter() instead — it fires first, has simpler IL.
        [HarmonyPatch(typeof(WorkingOnComputerStateBase), "OnStateEnter")]
        static class WorkingOnComputerState_OnEnter_Patch
        {
            static void Postfix(WorkingOnComputerStateBase __instance)
            {
                try
                {
                    Case cas = typeof(WorkingOnComputerStateBase)
                        .GetField("m_case", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue(__instance) as Case;
                    if (cas == null) return; // HBPC or no-case context — skip
                    string pcName = null;
                    try { pcName = cas.m_computer?.GetUIName(); } catch { }
                    if (string.IsNullOrEmpty(pcName)) pcName = Loc.Get("working_unknown_pc");
                    ScreenReader.Say(Loc.Get("working_on_pc", pcName));
                    DebugLogger.LogState($"WorkingOnComputerState: entered, pc='{pcName}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkingOnComputerState_OnStateEnter_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(WorkingOnComputerState), "OnStateExit")]
        static class WorkingOnComputerState_OnExit_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("working_done"));
                    DebugLogger.LogState("WorkingOnComputerState: exited");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkingOnComputerState_OnStateExit_Patch: {ex.Message}");
                }
            }
        }
    }
}
