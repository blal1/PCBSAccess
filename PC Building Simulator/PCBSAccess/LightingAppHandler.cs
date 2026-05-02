using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Lighting app (LED configuration).
    /// Start Postfix → announces LED group count.
    /// UpdateLightList Postfix → refreshes row list (re-announces if already initialized).
    /// OnApply Postfix → "Lighting changes applied."
    /// OnSelectionChanged Postfix → "Selected." / "Not selected."
    /// Navigation: Up/Down/Home/End to browse rows, Space to toggle selection.
    /// </summary>
    public static class LightingAppHandler
    {
        #region State

        private static LightingApp _app;
        private static List<LightingRow> _rows = new List<LightingRow>();
        private static int _focusIndex = -1;

        /// <summary>
        /// True after Start has completed — prevents UpdateLightList (called from within
        /// Start) from double-announcing on first open.
        /// </summary>
        private static bool _initialized = false;

        // Deferred refresh: read rows one frame after UpdateLightList so Unity can
        // process pending Destroy calls before querying GetComponentsInChildren.
        private static bool _needsRefresh = false;
        private static LightingApp _pendingRefreshApp = null;
        private static bool _pendingAnnounce = false;

        #endregion

        #region Public API

        public static void Reset()
        {
            _app = null;
            _rows.Clear();
            _focusIndex = -1;
            _initialized = false;
            _needsRefresh = false;
            _pendingRefreshApp = null;
            _pendingAnnounce = false;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "LightingApp";
            public bool IsActive => _app != null;

            public bool HandleInput()
            {
                if (_rows.Count == 0) return false;

                if (Input.GetKeyDown(KeyCode.DownArrow)) { Navigate(1);  return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))   { Navigate(-1); return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);               return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_rows.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Space))      { ToggleFocused();             return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_lighting")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API — deferred refresh

        /// <summary>
        /// Applies deferred row refresh. Called from Main.Update each frame.
        /// Key handling has moved to Context.HandleInput().
        /// </summary>
        public static void Update()
        {
            // Apply deferred row refresh (one frame after UpdateLightList)
            if (_needsRefresh && _pendingRefreshApp != null)
            {
                _needsRefresh = false;
                ApplyRefresh(_pendingRefreshApp, _pendingAnnounce);
                _pendingAnnounce = false;
            }
        }

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_rows.Count == 0) return;
            int next = Mathf.Clamp(_focusIndex + dir, 0, _rows.Count - 1);
            if (next == _focusIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceRow(_rows[_focusIndex]);
        }

        private static void NavigateTo(int index)
        {
            if (_rows.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _rows.Count - 1);
            AnnounceRow(_rows[_focusIndex]);
        }

        private static void AnnounceRow(LightingRow row)
        {
            if (row == null) return;
            string name = row.m_name?.text ?? "?";
            string state = row.m_toggle.isOn
                ? Loc.Get("lighting_selected_state")
                : Loc.Get("lighting_deselected_state");
            ScreenReader.Say($"{name}. {state}.");
        }

        private static void ToggleFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _rows.Count) return;
            LightingRow row = _rows[_focusIndex];
            if (row == null) return;
            // Toggling invokes onValueChanged → fires OnSelectionChanged → patch announces state
            row.m_toggle.isOn = !row.m_toggle.isOn;
        }

        #endregion

        #region Helpers

        private static void ApplyRefresh(LightingApp app, bool announce)
        {
            _app = app;
            _rows.Clear();
            if (app.m_lightList?.content != null)
            {
                LightingRow[] arr =
                    app.m_lightList.content.GetComponentsInChildren<LightingRow>(false);
                if (arr != null)
                    foreach (LightingRow r in arr)
                        if (r != null) _rows.Add(r);
            }
            if (_focusIndex >= _rows.Count) _focusIndex = _rows.Count > 0 ? 0 : -1;
            if (announce)
                ScreenReader.Say(Loc.Get("lighting_open", _rows.Count));
            if (_rows.Count > 0)
                InputRouter.Push(_ctx);
            else
                InputRouter.Pop(_ctx);
            DebugLogger.LogState($"LightingAppHandler: {_rows.Count} LED rows after refresh, announce={announce}");
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(LightingApp), "Start")]
        static class LightingApp_Start_Patch
        {
            static void Postfix(LightingApp __instance)
            {
                try
                {
                    _initialized = true;
                    // UpdateLightList was already called from within Start — rows are refreshed.
                    // Re-read now (after full Start completes) and announce.
                    _pendingRefreshApp = __instance;
                    _pendingAnnounce = true;
                    _needsRefresh = true;
                    DebugLogger.LogState("LightingAppHandler: Start complete, deferred announce scheduled");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"LightingApp_Start_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(LightingApp), "UpdateLightList")]
        static class LightingApp_UpdateLightList_Patch
        {
            static void Postfix(LightingApp __instance)
            {
                try
                {
                    if (!_initialized)
                    {
                        // Called from within Start — just cache; Start_Patch will announce
                        _app = __instance;
                        return;
                    }
                    // Config change triggered UpdateLightList after app was fully open
                    _pendingRefreshApp = __instance;
                    _pendingAnnounce = true;
                    _needsRefresh = true;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"LightingApp_UpdateLightList_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(LightingApp), "OnApply")]
        static class LightingApp_OnApply_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("lighting_applied"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"LightingApp_OnApply_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(LightingApp), "OnSelectionChanged")]
        static class LightingApp_OnSelectionChanged_Patch
        {
            static void Postfix(bool add)
            {
                try
                {
                    ScreenReader.Say(add
                        ? Loc.Get("lighting_selected_state")
                        : Loc.Get("lighting_deselected_state"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"LightingApp_OnSelectionChanged_Patch: {ex.Message}");
                }
            }
        }

        #endregion

        #endregion
    }
}
