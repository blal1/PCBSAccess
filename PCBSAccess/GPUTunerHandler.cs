using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the in-game GPU overclocking tool (GPUTuner).
    ///
    /// On open: announces GPU name + current Core Clock, Mem Clock, Voltage.
    /// Up/Down: cycle through the three parameters.
    /// Left/Right: decrease / increase the focused parameter by one step.
    /// Enter: apply settings (calls OnApply).
    /// R key: reset to defaults (calls OnReset).
    /// F1: re-read current parameter value.
    /// </summary>
    public static class GPUTunerHandler
    {
        #region State

        private static GPUTuner      _instance;
        private static GPUTunerParam _clockParam;
        private static GPUTunerParam _memParam;
        private static GPUTunerParam _voltsParam;
        private static GPUTunerParam[] _params; // ordered: Clock, Mem, Voltage
        private static int           _focusIndex = 0;
        private static bool          _active;

        private static readonly FieldInfo _fClockParam = typeof(GPUTuner)
            .GetField("m_clockParam", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fMemParam = typeof(GPUTuner)
            .GetField("m_memParam", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fVoltsParam = typeof(GPUTuner)
            .GetField("m_voltsParam", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "GPUTuner";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);    return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);   return true; }
                if (Input.GetKeyDown(KeyCode.LeftArrow))  { Adjust(-1);     return true; }
                if (Input.GetKeyDown(KeyCode.RightArrow)) { Adjust(1);      return true; }
                if (Input.GetKeyDown(KeyCode.Return))     { Apply();        return true; }
                if (Input.GetKeyDown(KeyCode.R))          { ResetToDefaults(); return true; }
                if (Input.GetKeyDown(KeyCode.F1))         { ReReadParam();  return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("gpututner_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patches

        [HarmonyPatch(typeof(GPUTuner), "Start")]
        static class GPUTuner_Start_Patch
        {
            static void Postfix(GPUTuner __instance)
            {
                try
                {
                    _instance   = __instance;
                    _clockParam = _fClockParam?.GetValue(__instance) as GPUTunerParam;
                    _memParam   = _fMemParam?.GetValue(__instance)   as GPUTunerParam;
                    _voltsParam = _fVoltsParam?.GetValue(__instance) as GPUTunerParam;
                    _params     = new[] { _clockParam, _memParam, _voltsParam };
                    _focusIndex = 0;
                    _active     = true;
                    InputRouter.Push(_ctx);
                    AnnounceOpen(__instance);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"GPUTunerHandler.Start: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(GPUTuner), "OnApply")]
        static class GPUTuner_OnApply_Patch
        {
            static void Postfix(GPUTuner __instance)
            {
                try
                {
                    AnnounceValues(Loc.Get("gpututner_applied"));
                    DebugLogger.LogState("GPUTunerHandler: applied");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"GPUTunerHandler.OnApply: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(GPUTuner), "OnReset")]
        static class GPUTuner_OnReset_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("gpututner_reset"));
                    DebugLogger.LogState("GPUTunerHandler: reset");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"GPUTunerHandler.OnReset: {ex.Message}");
                }
            }
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            _active     = false;
            _instance   = null;
            _clockParam = null;
            _memParam   = null;
            _voltsParam = null;
            _params     = null;
            _focusIndex = 0;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_params == null) return;
            _focusIndex = (_focusIndex + dir + _params.Length) % _params.Length;
            ReReadParam();
        }

        private static void Adjust(int dir)
        {
            if (_params == null || _focusIndex >= _params.Length) return;
            GPUTunerParam p = _params[_focusIndex];
            if (p == null) return;
            p.Change(dir);
            ReReadParam();
        }

        private static void Apply()
        {
            if (_instance == null) return;
            _instance.OnApply();
        }

        private static void ResetToDefaults()
        {
            if (_instance == null) return;
            _instance.OnReset();
        }

        private static void ReReadParam()
        {
            if (_params == null || _focusIndex >= _params.Length) return;
            GPUTunerParam p = _params[_focusIndex];
            if (p == null) return;
            string name  = p.m_name?.text ?? string.Empty;
            string value = GetSliderValueText(p);
            ScreenReader.Say($"{_focusIndex + 1} {Loc.Get("nav_of")} {_params.Length}: {name}. {value}.");
        }

        #endregion

        #region Announce

        private static void AnnounceOpen(GPUTuner tuner)
        {
            try
            {
                string gpuName = GetGPUName(tuner);
                ScreenReader.Say(Loc.Get("gpututner_open", gpuName));
                AnnounceValues(null);
                ScreenReader.Say(Loc.Get("gpututner_hint"), interrupt: false);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"GPUTunerHandler.AnnounceOpen: {ex.Message}");
            }
        }

        private static void AnnounceValues(string prefix)
        {
            if (_params == null) return;
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(prefix)) sb.Append(prefix).Append(". ");
            for (int i = 0; i < _params.Length; i++)
            {
                GPUTunerParam p = _params[i];
                if (p == null) continue;
                if (i > 0) sb.Append(". ");
                sb.Append(p.m_name?.text ?? string.Empty);
                sb.Append(": ");
                sb.Append(GetSliderValueText(p));
            }
            ScreenReader.Say(sb.ToString());
        }

        private static string GetSliderValueText(GPUTunerParam p)
        {
            if (p?.m_slider == null) return string.Empty;
            // Use the game's own formatted text from the absolute-value display
            Text absText = GetAbsValueText(p);
            if (absText != null && !string.IsNullOrEmpty(absText.text))
                return StripRichText(absText.text);
            return ((int)p.m_slider.value).ToString();
        }

        // m_absValue is private on GPUTunerParam — but it's the same as the
        // GPUTuner's m_clockSpeedText / m_memSpeedText / m_voltageText.
        // Map by index: 0=clock, 1=mem, 2=volts.
        private static readonly FieldInfo _fClockSpeedText = typeof(GPUTuner)
            .GetField("m_clockSpeedText", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo _fMemSpeedText = typeof(GPUTuner)
            .GetField("m_memSpeedText", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo _fVoltageText = typeof(GPUTuner)
            .GetField("m_voltageText", BindingFlags.Public | BindingFlags.Instance);

        private static Text GetAbsValueText(GPUTunerParam p)
        {
            if (_instance == null) return null;
            if (p == _clockParam) return _fClockSpeedText?.GetValue(_instance) as Text;
            if (p == _memParam)   return _fMemSpeedText?.GetValue(_instance)   as Text;
            if (p == _voltsParam) return _fVoltageText?.GetValue(_instance)    as Text;
            return null;
        }

        private static string GetGPUName(GPUTuner tuner)
        {
            try
            {
                VirtualComputer vc = tuner.GetComponentInParent<VirtualComputer>();
                return vc?.GetComputer()?.GetGPU()?.GetPart()?.m_uiName ?? Loc.Get("gpututner_unknown_gpu");
            }
            catch { return Loc.Get("gpututner_unknown_gpu"); }
        }

        private static string StripRichText(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return System.Text.RegularExpressions.Regex.Replace(s, "<[^>]+>", string.Empty).Trim();
        }

        #endregion
    }
}
