using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces OCCT stress test events and results.
    ///
    /// OnOn patch:     "Stress test started."
    /// OnOff patch:    "Stress test stopped." / "Cancelled."
    /// OnFinish patch: "OCCT complete." + sensor summary (CPU temp, GPU temp, wattage).
    /// F9 (wired in Main, context-aware): re-reads last sensor summary.
    /// </summary>
    public static class OCCTHandler
    {
        #region State

        internal static string _lastSensors;
        private static OCCTApp _lastApp;
        private static string  _lastStatus;
        private static float   _throttleAnnounceTime = -99f;

        #endregion

        #region Public API

        public static void AnnounceLastSensors()
        {
            if (!string.IsNullOrEmpty(_lastSensors))
                ScreenReader.Say(Loc.Get("occt_reread", _lastSensors));
            else
                ScreenReader.Say(Loc.Get("occt_none"));
        }

        public static void Reset()
        {
            _lastSensors = null;
            _lastApp = null;
            _lastStatus = null;
            _throttleAnnounceTime = -99f;
        }

        /// <summary>Polls m_status for CPU throttle text. Called from Main.Update.</summary>
        public static void Update()
        {
            if (_lastApp == null) return;
            try
            {
                string status = _lastApp.m_status?.text;
                if (status == _lastStatus) return;
                _lastStatus = status;

                // Announce if status contains throttle keyword and cooldown elapsed
                if (!string.IsNullOrEmpty(status) &&
                    status.IndexOf("Throttl", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                    UnityEngine.Time.time - _throttleAnnounceTime > 5f)
                {
                    _throttleAnnounceTime = UnityEngine.Time.time;
                    ScreenReader.Say(Loc.Get("occt_throttle"));
                    DebugLogger.LogState("OCCTHandler: CPU throttle detected");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"OCCTHandler.Update: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        private static string ReadSensors(OCCTApp app)
        {
            try
            {
                var rows = app.m_sensorValues.GetComponentsInChildren<OCCTSensorRow>(includeInactive: false);
                if (rows == null || rows.Length == 0) return string.Empty;

                var sb = new System.Text.StringBuilder();
                foreach (var row in rows)
                {
                    string name = row.m_name  != null ? row.m_name.text  : string.Empty;
                    string val  = row.m_value != null ? row.m_value.text : string.Empty;
                    string max  = row.m_max   != null ? row.m_max.text   : string.Empty;
                    if (string.IsNullOrEmpty(name)) continue;

                    sb.Append(name).Append(": ").Append(val);
                    if (!string.IsNullOrEmpty(max)) sb.Append(" (max ").Append(max).Append(')');
                    sb.Append(". ");
                }
                return sb.ToString().Trim();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"OCCTHandler.ReadSensors: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(OCCTApp), "OnOn")]
        static class OCCTApp_OnOn_Patch
        {
            static void Postfix(OCCTApp __instance)
            {
                try
                {
                    _lastApp = __instance;
                    ScreenReader.Say(Loc.Get("occt_started"));
                    DebugLogger.LogState("OCCTHandler: test started");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OCCTApp_OnOn_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(OCCTApp), "OnOff")]
        static class OCCTApp_OnOff_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("occt_stopped"));
                    DebugLogger.LogState("OCCTHandler: test stopped");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OCCTApp_OnOff_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Fires when automatic test cycle completes.
        /// Reads sensor rows for final summary.
        /// </summary>
        [HarmonyPatch(typeof(OCCTApp), "OnFinish")]
        static class OCCTApp_OnFinish_Patch
        {
            static void Postfix(OCCTApp __instance)
            {
                try
                {
                    _lastApp = __instance;
                    string sensors = ReadSensors(__instance);
                    _lastSensors = sensors;

                    ScreenReader.Say(Loc.Get("occt_complete"));
                    if (!string.IsNullOrEmpty(sensors))
                        ScreenReader.Say(sensors, interrupt: false);

                    DebugLogger.LogState("OCCTHandler: test complete");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OCCTApp_OnFinish_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
