using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the key-binding / controls menu.
    ///
    /// On open: announces "Controls. N bindings. Up Down to navigate."
    /// Up/Down/Home/End: navigate KeyBinding rows.
    /// Each row: "[action name]. [keyboard key]."
    /// Enter: activate the Keyboard redefine button for the focused row.
    /// When redefine prompt shows: reads "Press new key for [action]."
    /// After a key is assigned: re-reads the updated binding.
    /// </summary>
    public static class KeyBindingMenuHandler
    {
        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static readonly List<KeyBinding> _rows = new List<KeyBinding>();
        private static int _focusIndex = -1;
        private static KeyBindingMenu _menu;

        private static readonly FieldInfo _fControl = typeof(KeyBinding)
            .GetField("m_control", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "KeyBindings";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);                  return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);                 return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);               return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_rows.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Return))     { ActivateFocused();            return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("keybind_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patches

        [HarmonyPatch(typeof(KeyBindingMenu), "Awake")]
        static class KeyBindingMenu_Awake_Patch
        {
            static void Postfix(KeyBindingMenu __instance)
            {
                try
                {
                    _menu = __instance;
                    BuildList(__instance);
                    _focusIndex = -1;
                    _active     = true;
                    _wasOpen    = true;
                    InputRouter.Push(_ctx);
                    ScreenReader.Say(Loc.Get("keybind_open", _rows.Count));
                    ScreenReader.Say(Loc.Get("keybind_hint"), interrupt: false);
                    DebugLogger.LogState($"KeyBindingMenuHandler: {_rows.Count} bindings");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"KeyBindingMenuHandler.Awake: {ex.Message}");
                }
            }
        }

        // Fires when the "Press new key" prompt appears.
        [HarmonyPatch(typeof(KeyBindingMenu), "Redefine")]
        static class KeyBindingMenu_Redefine_Patch
        {
            static void Postfix(KeyBindingMenu __instance)
            {
                try
                {
                    string prompt = __instance.m_redefineText?.text ?? string.Empty;
                    if (!string.IsNullOrEmpty(prompt))
                        ScreenReader.Say(prompt);
                    DebugLogger.LogState($"KeyBindingMenuHandler: redefine '{prompt}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"KeyBindingMenuHandler.Redefine: {ex.Message}");
                }
            }
        }

        // Fires when input mapping finishes (success or cancel).
        [HarmonyPatch(typeof(KeyBindingMenu), "OnStopped")]
        static class KeyBindingMenu_OnStopped_Patch
        {
            static void Postfix()
            {
                try
                {
                    // Re-read the focused row to confirm the new binding.
                    if (_focusIndex >= 0 && _focusIndex < _rows.Count)
                        AnnounceRow(_focusIndex);
                    else
                        ScreenReader.Say(Loc.Get("keybind_mapping_done"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"KeyBindingMenuHandler.OnStopped: {ex.Message}");
                }
            }
        }

        #endregion

        #region Poll / close

        public static void PollState()
        {
            if (!_wasOpen) return;
            bool open = IsOpen();
            if (!open && _wasOpen) OnClose();
            _wasOpen = open;
        }

        private static bool IsOpen()
        {
            try
            {
                return _menu != null && _menu.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void OnClose()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _rows.Clear();
            _menu       = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _rows.Clear();
            _menu       = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

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
            AnnounceRow(_focusIndex);
        }

        private static void NavigateTo(int index)
        {
            if (_rows.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _rows.Count - 1);
            AnnounceRow(_focusIndex);
        }

        private static void ActivateFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _rows.Count) return;
            KeyBinding row = _rows[_focusIndex];
            if (row == null) return;
            // Prefer keyboard button if interactable, otherwise joystick.
            if (row.m_keyboard != null && row.m_keyboard.IsInteractable())
                row.m_keyboard.onClick.Invoke();
            else if (row.m_joystick != null && row.m_joystick.IsInteractable())
                row.m_joystick.onClick.Invoke();
            else
                ScreenReader.Say(Loc.Get("keybind_not_rebindable"));
        }

        #endregion

        #region Announce

        private static void AnnounceRow(int index)
        {
            if (index < 0 || index >= _rows.Count) return;
            KeyBinding row = _rows[index];
            if (row == null) return;

            string actionName = row.m_text?.text ?? string.Empty;
            string keyName    = GetCurrentMapping(row);

            string text = string.IsNullOrEmpty(keyName)
                ? Loc.Get("keybind_item_unbound", index + 1, _rows.Count, actionName)
                : Loc.Get("keybind_item",         index + 1, _rows.Count, actionName, keyName);

            ScreenReader.Say(text);
            DebugLogger.LogState($"KeyBindingMenuHandler: [{index + 1}] '{actionName}' = '{keyName}'");
        }

        private static string GetCurrentMapping(KeyBinding row)
        {
            try
            {
                InputBase ctrl = _fControl?.GetValue(row) as InputBase;
                if (ctrl == null) return string.Empty;
                return ctrl.GetCurrentMapping();
            }
            catch { return string.Empty; }
        }

        #endregion

        #region Build list

        private static void BuildList(KeyBindingMenu menu)
        {
            _rows.Clear();
            try
            {
                KeyBinding[] found = menu.m_bindings.content
                    .GetComponentsInChildren<KeyBinding>(includeInactive: false);
                foreach (KeyBinding kb in found)
                    _rows.Add(kb);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"KeyBindingMenuHandler.BuildList: {ex.Message}");
            }
        }

        #endregion
    }
}
