using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces hardware info when the HWInfo tablet app opens.
    ///
    /// Patches HWInfoApp.Start postfix — reads all summary rows (component name + properties)
    /// and announces them in sequence.
    /// F8 (wired in Main): re-reads the last hardware summary.
    /// </summary>
    public static class HWInfoHandler
    {
        #region State

        private static string _lastSummary;

        #endregion

        #region Public API

        /// <summary>Re-reads the last hardware summary. Called from Main on F8.</summary>
        public static void AnnounceLastSummary()
        {
            if (!string.IsNullOrEmpty(_lastSummary))
                ScreenReader.Say(Loc.Get("hwinfo_reread", _lastSummary));
            else
                ScreenReader.Say(Loc.Get("hwinfo_none"));
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _lastSummary = null;
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires after HWInfo app initialises. Reads all component summary rows.
        /// Uses the row prefab structure: parent row = component type/name, child rows = properties.
        /// </summary>
        [HarmonyPatch(typeof(HWInfoApp), "Start")]
        static class HWInfoApp_Start_Patch
        {
            static void Postfix(HWInfoApp __instance)
            {
                try
                {
                    HWInfoRow[] rows = __instance.m_summary.content
                        .GetComponentsInChildren<HWInfoRow>(includeInactive: false);

                    if (rows == null || rows.Length == 0)
                    {
                        ScreenReader.Say(Loc.Get("hwinfo_empty"));
                        return;
                    }

                    ScreenReader.Say(Loc.Get("hwinfo_open"));
                    _lastSummary = string.Empty;

                    foreach (HWInfoRow row in rows)
                    {
                        string key = row.m_key   != null ? row.m_key.text   : string.Empty;
                        string val = row.m_value != null ? row.m_value.text : string.Empty;
                        if (string.IsNullOrEmpty(key)) continue;

                        string line = string.IsNullOrEmpty(val)
                            ? key
                            : $"{key}: {val}";
                        ScreenReader.Say(line + ".", interrupt: false);
                        _lastSummary += line + ". ";
                    }

                    DebugLogger.LogState($"HWInfoHandler: {rows.Length} rows announced");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"HWInfoApp_Start_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
