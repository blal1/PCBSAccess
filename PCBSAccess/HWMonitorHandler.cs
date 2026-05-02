using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Reads live hardware sensor data from the HWMonitor app (real-time CPU/GPU temps, wattage).
    /// Distinct from HWInfoApp (which shows static part summary).
    ///
    /// On open (HWMonitor.Start): announces sensor count.
    /// Numpad7 hotkey: reads all current sensor name + value pairs.
    /// </summary>
    public static class HWMonitorHandler
    {
        private static readonly List<HWInfoRow> _rows = new List<HWInfoRow>();
        private static string _lastSummary;

        public static void Reset()
        {
            _rows.Clear();
            _lastSummary = null;
        }

        [HarmonyPatch(typeof(HWMonitor), "Start")]
        static class HWMonitor_Start_Patch
        {
            static void Postfix(HWMonitor __instance)
            {
                try
                {
                    _rows.Clear();
                    _lastSummary = null;

                    HWInfoRow[] found = __instance.GetComponentsInChildren<HWInfoRow>(includeInactive: false);
                    foreach (HWInfoRow r in found)
                        _rows.Add(r);

                    string open = Loc.Get("hwmonitor_open", _rows.Count);
                    ScreenReader.Say(open);
                    DebugLogger.LogState($"HWMonitorHandler: {_rows.Count} sensors");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"HWMonitorHandler.Start: {ex.Message}");
                }
            }
        }

        /// <summary>Called from Main hotkey (Numpad7).</summary>
        public static void AnnounceSensors()
        {
            try
            {
                if (_rows.Count == 0)
                {
                    ScreenReader.Say(Loc.Get("hwmonitor_none"));
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(Loc.Get("hwmonitor_reading"));
                foreach (HWInfoRow row in _rows)
                {
                    if (row == null) continue;
                    string key = row.m_key?.text ?? string.Empty;
                    string val = row.m_value?.text ?? string.Empty;
                    if (string.IsNullOrEmpty(key)) continue;
                    sb.Append(" ").Append(key).Append(": ").Append(val).Append(".");
                }
                _lastSummary = sb.ToString();
                ScreenReader.Say(_lastSummary);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"HWMonitorHandler.AnnounceSensors: {ex.Message}");
            }
        }

        public static void AnnounceLastSensors()
        {
            if (!string.IsNullOrEmpty(_lastSummary))
                ScreenReader.Say(Loc.Get("hwmonitor_reread", _lastSummary));
            else
                ScreenReader.Say(Loc.Get("hwmonitor_none"));
        }
    }
}
