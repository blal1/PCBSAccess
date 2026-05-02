using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the PC Stats (build summary) panel.
    ///
    /// Patches PCStats.Init(ComputerSave) postfix:
    ///   Announces title, status, benchmark score, invoice/resale value.
    ///   Queues all part entries (name + value).
    ///
    /// F10 (wired in Main): re-reads the last summary.
    /// Enter: OK button to dismiss.
    /// </summary>
    public static class PCStatsHandler
    {
        #region State

        private static string _lastSummary;
        private static bool   _wasOpen;

        #endregion

        #region Public API

        public static void AnnounceLastSummary()
        {
            if (!string.IsNullOrEmpty(_lastSummary))
                ScreenReader.Say(Loc.Get("pcstats_reread", _lastSummary));
            else
                ScreenReader.Say(Loc.Get("pcstats_none"));
        }

        public static void Reset()
        {
            _lastSummary = null;
            _wasOpen = false;
        }

        public static void Update()
        {
            if (WorkshopUI.m_pcStats == null) return;

            bool open = WorkshopUI.m_pcStats.gameObject.activeSelf;

            // Closed transition
            if (_wasOpen && !open)
            {
                DebugLogger.LogState("PCStatsHandler: panel closed");
            }
            _wasOpen = open;

            if (!open) return;

            // Enter = OK
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                WorkshopUI.m_pcStats.OnOK();
            }
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires after PCStats.Init — all Text fields are populated.
        /// Announces title, status, benchmark, value, then all parts.
        /// </summary>
        [HarmonyPatch(typeof(PCStats), "Init")]
        static class PCStats_Init_Patch
        {
            static void Postfix(PCStats __instance)
            {
                try
                {
                    string title     = __instance.m_title     != null ? __instance.m_title.text     : string.Empty;
                    string status    = __instance.m_status    != null ? __instance.m_status.text    : string.Empty;
                    string benchmark = __instance.m_benchmark != null ? __instance.m_benchmark.text : string.Empty;
                    string value     = __instance.m_value     != null ? __instance.m_value.text     : string.Empty;

                    _lastSummary = $"{title}. {status}. {benchmark}. {value}.";

                    ScreenReader.Say(title + ".");
                    ScreenReader.Say(status    + ".", interrupt: false);
                    ScreenReader.Say(benchmark + ".", interrupt: false);
                    ScreenReader.Say(value     + ".", interrupt: false);
                    ScreenReader.Say(Loc.Get("pcstats_parts_hint"), interrupt: false);

                    // Queue all parts
                    PCStatEntry[] parts = __instance.m_components
                        .GetComponentsInChildren<PCStatEntry>(includeInactive: false);
                    foreach (PCStatEntry entry in parts)
                    {
                        string name = entry.m_name  != null ? entry.m_name.text  : string.Empty;
                        string val  = entry.m_value != null ? entry.m_value.text : string.Empty;
                        if (string.IsNullOrEmpty(name)) continue;

                        string line = string.IsNullOrEmpty(val) || val == "-"
                            ? name
                            : $"{name}: {val}";
                        ScreenReader.Say(line + ".", interrupt: false);
                    }

                    ScreenReader.Say(Loc.Get("pcstats_dismiss_hint"), interrupt: false);
                    DebugLogger.LogState($"PCStatsHandler: '{title}', {parts.Length} parts");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"PCStats_Init_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
