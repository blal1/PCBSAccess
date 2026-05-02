using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the Freebuild Options panel (tool upgrade toggles).
    ///
    /// On open: announces count + first item.
    /// Up/Down: navigate ToggleToolUpgrade rows.
    /// Space: toggle the focused upgrade on/off.
    /// F1: re-read focused upgrade.
    /// </summary>
    public static class FreebuildOptionsHandler
    {
        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static FreebuildOptions _panel;
        private static readonly List<ToggleToolUpgrade> _items = new List<ToggleToolUpgrade>();
        private static int _focusIndex = -1;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "FreebuildOptions";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);  return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1); return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);                    return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_items.Count - 1);     return true; }
                if (Input.GetKeyDown(KeyCode.Space))      { ToggleFocused();                  return true; }
                if (Input.GetKeyDown(KeyCode.F1))         { ReRead();                         return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("freebuild_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patches

        [HarmonyPatch(typeof(FreebuildOptions), "OnEnable")]
        static class FreebuildOptions_OnEnable_Patch
        {
            static void Postfix(FreebuildOptions __instance)
            {
                try
                {
                    _panel = __instance;
                    BuildList(__instance);
                    _focusIndex = _items.Count > 0 ? 0 : -1;
                    _active     = true;
                    _wasOpen    = true;
                    InputRouter.Push(_ctx);

                    ScreenReader.Say(Loc.Get("freebuild_open", _items.Count));
                    if (_items.Count > 0)
                    {
                        AnnounceItem(0);
                        ScreenReader.Say(Loc.Get("freebuild_hint"), interrupt: false);
                    }
                    DebugLogger.LogState($"FreebuildOptionsHandler: {_items.Count} upgrades");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"FreebuildOptions_OnEnable_Patch: {ex.Message}");
                }
            }
        }

        #endregion

        #region Poll / close

        public static void PollState()
        {
            if (!_wasOpen) return;
            bool open = _panel != null && _panel.gameObject.activeSelf;
            if (!open && _wasOpen) OnClose();
            _wasOpen = open;
        }

        private static void OnClose()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
            _panel      = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
            _panel      = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_items.Count == 0) return;
            int next = Mathf.Clamp(_focusIndex + dir, 0, _items.Count - 1);
            if (next == _focusIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceItem(_focusIndex);
        }

        private static void NavigateTo(int index)
        {
            if (_items.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _items.Count - 1);
            AnnounceItem(_focusIndex);
        }

        private static void ToggleFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _items.Count) return;
            ToggleToolUpgrade item = _items[_focusIndex];
            Toggle tog = item.GetComponent<Toggle>();
            if (tog == null) return;
            tog.isOn = !tog.isOn;
            AnnounceItem(_focusIndex);
        }

        private static void ReRead()
        {
            if (_focusIndex >= 0 && _focusIndex < _items.Count)
                AnnounceItem(_focusIndex);
        }

        #endregion

        #region Announce

        private static void AnnounceItem(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            ToggleToolUpgrade item = _items[index];
            string name  = item.m_name?.text ?? string.Empty;
            Toggle tog   = item.GetComponent<Toggle>();
            bool   isOn  = tog != null && tog.isOn;
            string state = isOn ? Loc.Get("opt_on") : Loc.Get("opt_off");
            ScreenReader.Say($"{index + 1} {Loc.Get("nav_of")} {_items.Count}: {name}. {state}.");
        }

        #endregion

        #region Build list

        private static void BuildList(FreebuildOptions panel)
        {
            _items.Clear();
            try
            {
                ToggleToolUpgrade[] found =
                    panel.m_list.content.GetComponentsInChildren<ToggleToolUpgrade>(includeInactive: false);
                foreach (ToggleToolUpgrade t in found)
                    _items.Add(t);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"FreebuildOptionsHandler.BuildList: {ex.Message}");
            }
        }

        #endregion
    }
}
