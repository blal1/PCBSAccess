using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces slider, toggle, and dropdown values in the Options menu.
    /// Adds Tab/Shift+Tab navigation; Left/Right arrows adjust the focused control.
    ///
    /// Open/close detection must be done by polling activeSelf because OnEnable cannot be
    /// patched (the method contains LINQ which causes an IL compile error in Harmony).
    /// PollState() is called from Main.Update(); key handling is in Context.HandleInput().
    /// </summary>
    public static class OptionsMenuHandler
    {
        #region State

        private static bool _active;
        private static OptionsMenu _menu;

        /// <summary>Controls in Tab order (Slider, Toggle, Dropdown, Button only).</summary>
        private static Selectable[] _controls;
        private static int _focused = -1;

        /// <summary>Maps Selectable instance ID → Loc label key.</summary>
        private static readonly Dictionary<int, string> _labels = new Dictionary<int, string>();

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "OptionsMenu";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Navigate(shift ? -1 : 1);
                    return true;
                }

                // Focused control interaction
                if (_controls == null || _focused < 0 || _focused >= _controls.Length)
                    return false;

                Selectable sel = _controls[_focused];

                if (sel is Slider slider)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))  { AdjustSlider(slider, -1); return true; }
                    if (Input.GetKeyDown(KeyCode.RightArrow)) { AdjustSlider(slider,  1); return true; }
                }
                else if (sel is Toggle toggle)
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        toggle.isOn = !toggle.isOn;
                        return true;
                    }
                }
                else if (sel is Dropdown dropdown)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        dropdown.value = Mathf.Max(0, dropdown.value - 1);
                        return true;
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        dropdown.value = Mathf.Min(dropdown.options.Count - 1, dropdown.value + 1);
                        return true;
                    }
                }
                else if (sel is Button btn)
                {
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        ButtonHelper.Click(btn);
                        return true;
                    }
                }

                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_options")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>True while the options menu is visible.</summary>
        public static bool IsActive => _active;

        /// <summary>Resets on scene change. Called from Main.OnSceneLoaded.</summary>
        public static void Reset()
        {
            _active   = false;
            _menu     = null;
            _controls = null;
            _focused  = -1;
            InputRouter.Pop(_ctx);
        }

        /// <summary>Called from MainMenu_Start_Patch to initialize the menu reference
        /// since OptionsMenu.Start() never fires (object starts inactive).</summary>
        public static void InitMenu(OptionsMenu menu)
        {
            _menu = menu;
            DebugLogger.LogState("OptionsMenuHandler: menu reference initialized from MainMenu");
        }

        /// <summary>
        /// Polls for open/close. Called from Main.Update each frame (replaces the old Update()).
        /// No key handling here — that is in Context.HandleInput().
        /// </summary>
        public static void PollState()
        {
            // OnEnable patch cannot be applied (IL Compile Error due to LINQ in target method).
            // Lazy-find the menu if Start() wasn't re-triggered (e.g. DontDestroyOnLoad MonoBehaviour).
            if (_menu == null)
                _menu = UnityEngine.Object.FindObjectOfType<OptionsMenu>();
            if (_menu == null) return;

            bool isNowOpen = _menu.gameObject.activeSelf;
            if (isNowOpen && !_active)
            {
                _active   = true;
                _focused  = -1;
                _controls = null;
                InputRouter.Push(_ctx);
                ScreenReader.Say(Loc.Get("opts_opened"));
                DebugLogger.LogState("OptionsMenuHandler: active (polled)");
            }
            else if (!isNowOpen && _active)
            {
                _active = false;
                InputRouter.Pop(_ctx);
                DebugLogger.LogState("OptionsMenuHandler: deactivated (polled)");
            }
        }

        #endregion

        #region Navigation

        private static void Navigate(int direction)
        {
            RefreshControls();

            if (_controls == null || _controls.Length == 0)
            {
                ScreenReader.Say(Loc.Get("opts_no_controls"));
                return;
            }

            _focused = ((_focused + direction) % _controls.Length + _controls.Length) % _controls.Length;
            AnnounceControl(_controls[_focused]);
            DebugLogger.LogState($"OptionsMenuHandler: focused index {_focused} ({_controls[_focused].GetType().Name})");
        }

        private static void RefreshControls()
        {
            if (_menu == null) { _controls = null; return; }

            var found = _menu.GetComponentsInChildren<Selectable>(false);
            var list = new List<Selectable>();

            foreach (Selectable s in found)
            {
                if (!s.gameObject.activeInHierarchy) continue;
                if (!s.IsInteractable()) continue;
                if (!(s is Slider) && !(s is Toggle) && !(s is Dropdown) && !(s is Button)) continue;
                list.Add(s);
            }

            // Sort top-to-bottom by screen position
            list.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
            _controls = list.ToArray();

            if (_controls.Length == 0) { _focused = -1; return; }
            if (_focused >= _controls.Length) _focused = _controls.Length - 1;
        }

        #endregion

        #region Announce helpers

        private static void AnnounceControl(Selectable sel)
        {
            string label = GetLabel(sel);

            if (sel is Slider slider)
            {
                ScreenReader.Say(FormatSlider(label, slider));
            }
            else if (sel is Toggle toggle)
            {
                ScreenReader.Say(FormatToggle(label, toggle.isOn));
            }
            else if (sel is Dropdown dropdown)
            {
                string option = dropdown.options.Count > 0
                    ? dropdown.options[dropdown.value].text
                    : string.Empty;
                ScreenReader.Say($"{label}: {option}");
            }
            else if (sel is Button)
            {
                ScreenReader.Say(label);
            }
        }

        private static void AnnounceSlider(string labelKey, Slider slider)
        {
            if (!_active) return;
            ScreenReader.Say(FormatSlider(Loc.Get(labelKey), slider));
        }

        private static void AnnounceToggle(string labelKey, bool isOn)
        {
            if (!_active) return;
            ScreenReader.Say(FormatToggle(Loc.Get(labelKey), isOn));
        }

        private static void AnnounceDropdown(string labelKey, Dropdown dropdown)
        {
            if (!_active) return;
            string option = dropdown.options.Count > 0
                ? dropdown.options[dropdown.value].text
                : string.Empty;
            ScreenReader.Say($"{Loc.Get(labelKey)}: {option}");
        }

        private static string FormatSlider(string label, Slider slider)
        {
            float range = slider.maxValue - slider.minValue;
            int pct = range > 0
                ? Mathf.RoundToInt((slider.value - slider.minValue) / range * 100f)
                : 0;
            return $"{label}: {pct}%";
        }

        private static string FormatToggle(string label, bool isOn)
        {
            return $"{label}: {Loc.Get(isOn ? "opt_on" : "opt_off")}";
        }

        private static string GetLabel(Selectable sel)
        {
            int id = sel.GetInstanceID();
            if (_labels.TryGetValue(id, out string key))
                return Loc.Get(key);
            return sel.name;
        }

        #endregion

        #region Slider step

        private static void AdjustSlider(Slider slider, int direction)
        {
            float step = (slider.maxValue - slider.minValue) / 20f;
            slider.value = Mathf.Clamp(slider.value + direction * step, slider.minValue, slider.maxValue);
            // announcement fires via onValueChanged listener
        }

        #endregion

        #region Label map + subscriptions (built once in Start patch)

        private static void BuildLabelMap(OptionsMenu m)
        {
            _labels.Clear();
            MapSel(m.lookSens,    "opt_look_sens");
            MapSel(m.cursorSens,  "opt_cursor_sens");
            MapSel(m.zoomSens,    "opt_zoom_sens");
            MapSel(m.musicVolume, "opt_music_vol");
            MapSel(m.soundVolume, "opt_sound_vol");
            MapSel(m.fullscreen,  "opt_fullscreen");
            MapSel(m.vsync,       "opt_vsync");
            MapSel(m.invertY,     "opt_invert_y");
            MapSel(m.tooltips,    "opt_tooltips");
            MapSel(m.pushScroll,  "opt_push_scroll");
            MapSel(m.enableChroma,"opt_chroma");
            MapSel(m.enableAura,  "opt_aura");
            MapSel(m.hideTabletWhenMinimised, "opt_hide_tablet");
            if (m.m_language != null) MapSel(m.m_language, "opt_language");
            MapSel(m.quality,          "opt_quality");
            MapSel(m.screenResolution, "opt_resolution");
            MapSel(m.apply, "opt_apply");
            MapSel(m.back,  "opt_back");
        }

        private static void MapSel(Selectable s, string key)
        {
            if (s != null) _labels[s.GetInstanceID()] = key;
        }

        private static void SubscribeAll(OptionsMenu m)
        {
            SubSlider(m.lookSens,    "opt_look_sens");
            SubSlider(m.cursorSens,  "opt_cursor_sens");
            SubSlider(m.zoomSens,    "opt_zoom_sens");
            SubSlider(m.musicVolume, "opt_music_vol");
            SubSlider(m.soundVolume, "opt_sound_vol");

            SubToggle(m.fullscreen,  "opt_fullscreen");
            SubToggle(m.vsync,       "opt_vsync");
            SubToggle(m.invertY,     "opt_invert_y");
            SubToggle(m.tooltips,    "opt_tooltips");
            SubToggle(m.pushScroll,  "opt_push_scroll");
            SubToggle(m.enableChroma,"opt_chroma");
            SubToggle(m.enableAura,  "opt_aura");
            SubToggle(m.hideTabletWhenMinimised, "opt_hide_tablet");

            if (m.m_language != null) SubDropdown(m.m_language, "opt_language");
            SubDropdown(m.quality,          "opt_quality");
            SubDropdown(m.screenResolution, "opt_resolution");
        }

        private static void SubSlider(Slider s, string key)
        {
            if (s == null) return;
            s.onValueChanged.AddListener(_ => AnnounceSlider(key, s));
        }

        private static void SubToggle(Toggle t, string key)
        {
            if (t == null) return;
            t.onValueChanged.AddListener(val => AnnounceToggle(key, val));
        }

        private static void SubDropdown(Dropdown d, string key)
        {
            if (d == null) return;
            d.onValueChanged.AddListener(_ => AnnounceDropdown(key, d));
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(OptionsMenu), "Start")]
        static class OptionsMenu_Start_Patch
        {
            static void Postfix(OptionsMenu __instance)
            {
                try
                {
                    _menu = __instance;
                    BuildLabelMap(__instance);
                    SubscribeAll(__instance);
                    DebugLogger.LogState("OptionsMenuHandler: subscribed to all controls");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OptionsMenu_Start_Patch: {ex.Message}");
                }
            }
        }

        // OnEnable patch removed — Harmony cannot patch it (IL Compile Error due to LINQ in method body).
        // Open/close detection is handled by polling _menu.gameObject.activeSelf in PollState().

        [HarmonyPatch(typeof(OptionsMenu), "OnDisable")]
        static class OptionsMenu_OnDisable_Patch
        {
            static void Postfix()
            {
                _active = false;
                InputRouter.Pop(_ctx);
                DebugLogger.LogState("OptionsMenuHandler: deactivated (patch)");
            }
        }

        #endregion
    }
}
