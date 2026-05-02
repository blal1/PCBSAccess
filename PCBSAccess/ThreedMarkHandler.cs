using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces 3DMark benchmark results.
    ///
    /// OnRun patch: "Benchmark started."
    /// CalcResults patch: reads CPU/GPU/overall scores + hardware names when results appear.
    /// F9 (wired in Main, context-aware): re-reads last score summary.
    /// </summary>
    public static class ThreedMarkHandler
    {
        #region State

        private static string _lastResult;

        #endregion

        #region Public API

        public static void AnnounceLastResult()
        {
            if (!string.IsNullOrEmpty(_lastResult))
                ScreenReader.Say(Loc.Get("threedmark_reread", _lastResult));
            else
                ScreenReader.Say(Loc.Get("threedmark_none"));
        }

        public static void Reset()
        {
            _lastResult = null;
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(ThreedMarkApp), "OnRun")]
        static class ThreedMarkApp_OnRun_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("threedmark_started"));
                    DebugLogger.LogState("ThreedMarkHandler: benchmark started");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ThreedMarkApp_OnRun_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Fires after CalcResults() — all score Text fields are populated at this point.
        /// CalcResults() is called before SetPage(m_resultsPage), so values are ready.
        /// </summary>
        [HarmonyPatch(typeof(ThreedMarkApp), "CalcResults")]
        static class ThreedMarkApp_CalcResults_Patch
        {
            static void Postfix(ThreedMarkApp __instance)
            {
                try
                {
                    string overall = __instance.m_score != null && __instance.m_score.Length > 0
                        ? __instance.m_score[0].text
                        : "?";
                    string cpu  = __instance.m_cpuScore != null ? __instance.m_cpuScore.text : "?";
                    string gpu  = __instance.m_gpuScore != null ? __instance.m_gpuScore.text : "?";
                    string cpuName = __instance.m_cpu  != null ? __instance.m_cpu.text  : string.Empty;
                    string gpuName = __instance.m_gpu  != null ? __instance.m_gpu.text  : string.Empty;

                    _lastResult = Loc.Get("threedmark_result_summary", overall, cpu, gpu);

                    ScreenReader.Say(Loc.Get("threedmark_complete"));
                    ScreenReader.Say(Loc.Get("threedmark_score_overall", overall), interrupt: false);
                    ScreenReader.Say(Loc.Get("threedmark_score_cpu", cpu),         interrupt: false);
                    ScreenReader.Say(Loc.Get("threedmark_score_gpu", gpu),         interrupt: false);
                    if (!string.IsNullOrEmpty(cpuName))
                        ScreenReader.Say(Loc.Get("threedmark_cpu_name", cpuName),  interrupt: false);
                    if (!string.IsNullOrEmpty(gpuName))
                        ScreenReader.Say(Loc.Get("threedmark_gpu_name", gpuName),  interrupt: false);

                    DebugLogger.LogState($"ThreedMarkHandler: score={overall}, cpu={cpu}, gpu={gpu}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ThreedMarkApp_CalcResults_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
