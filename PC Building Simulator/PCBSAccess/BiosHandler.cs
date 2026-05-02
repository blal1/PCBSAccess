using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the BIOS screen and enables keyboard navigation.
    ///
    /// Tab: cycles BIOS tabs.
    /// Up/Down: navigate settings in the current tab.
    /// Left/Right: invoke the minus/plus buttons on the focused setting.
    /// Enter/Keypad: invoke the click button on the focused setting.
    /// If a prompt dialog is visible, Enter = yes, Escape = no.
    /// </summary>
    public static class BiosHandler
    {
        #region State

        private static Bios _bios;
        private static List<BiosSetting> _settings = new List<BiosSetting>();
        private static int _focusIndex = -1;
        private static int _currentTabButtonIndex;
        private static int _tabCount;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "Bios";
            public bool IsActive => _bios != null;

            public bool HandleInput()
            {
                if (_bios == null) return false;

                // If a prompt dialog is visible it takes absolute priority
                if (_bios.m_prompt != null && _bios.m_prompt.activeSelf)
                {
                    if (Input.GetKeyDown(KeyCode.Return))  { _bios.OnPromptYes(); return true; }
                    if (Input.GetKeyDown(KeyCode.Escape))  { _bios.OnPromptNo();  return true; }
                    return false;
                }

                if (Input.GetKeyDown(KeyCode.Tab))        { CycleTab();     return true; }

                if (_settings.Count == 0) return false;

                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);    return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);   return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);  return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_settings.Count - 1); return true; }

                if (_focusIndex < 0 || _focusIndex >= _settings.Count) return false;
                BiosSetting setting = _settings[_focusIndex];

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    ButtonHelper.Click(setting.m_minus);
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ButtonHelper.Click(setting.m_plus);
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    ButtonHelper.Click(setting.m_click);
                    return true;
                }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_bios")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _bios      = null;
            _settings.Clear();
            _focusIndex = -1;
            _currentTabButtonIndex = 0;
            _tabCount  = 0;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_settings.Count == 0) return;
            int next = Mathf.Clamp(_focusIndex + dir, 0, _settings.Count - 1);
            if (next == _focusIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceSetting(_settings[_focusIndex]);
        }

        private static void NavigateTo(int index)
        {
            if (_settings.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _settings.Count - 1);
            AnnounceSetting(_settings[_focusIndex]);
        }

        private static void AnnounceSetting(BiosSetting setting)
        {
            if (setting == null) return;
            try
            {
                string name  = setting.m_name?.text ?? string.Empty;
                string value = setting.m_value != null     ? setting.m_value.text
                             : setting.m_fixedValue != null ? setting.m_fixedValue.text
                             : string.Empty;
                ScreenReader.Say($"{name}: {value}");
                DebugLogger.LogState($"BiosHandler: setting '{name}' = '{value}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"BiosHandler.AnnounceSetting: {ex.Message}");
            }
        }

        private static void CycleTab()
        {
            if (_bios == null || _tabCount == 0) return;
            try
            {
                _currentTabButtonIndex = (_currentTabButtonIndex + 1) % _tabCount;
                Button[] tabBtns = _bios.GetComponentsInChildren<Button>(includeInactive: false);
                List<Button> tabButtons = new List<Button>();
                foreach (Button b in tabBtns)
                    if (b.name.StartsWith("Tab")) tabButtons.Add(b);

                if (_currentTabButtonIndex < tabButtons.Count)
                    ButtonHelper.Click(tabButtons[_currentTabButtonIndex]);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"BiosHandler.CycleTab: {ex.Message}");
            }
        }

        #endregion

        #region Patches

        /// <summary>Fires when the BIOS opens. Saves reference, counts tabs, announces tab names.</summary>
        [HarmonyPatch(typeof(Bios), "OnEnable")]
        static class Bios_OnEnable_Patch
        {
            static void Postfix(Bios __instance)
            {
                try
                {
                    _bios = __instance;
                    _focusIndex = -1;
                    _settings.Clear();
                    _currentTabButtonIndex = 0;
                    InputRouter.Push(_ctx);

                    // Count and announce tabs
                    Button[] allBtns = __instance.GetComponentsInChildren<Button>(includeInactive: false);
                    var tabBtns = new List<Button>();
                    foreach (Button b in allBtns)
                        if (b.name.StartsWith("Tab")) tabBtns.Add(b);
                    _tabCount = tabBtns.Count;

                    var sb = new StringBuilder();
                    sb.Append(Loc.Get("bios_opened"));
                    sb.Append(' ');
                    for (int i = 0; i < tabBtns.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(ButtonHelper.GetLabel(tabBtns[i]));
                    }
                    sb.Append('.');
                    ScreenReader.Say(sb.ToString());
                    DebugLogger.LogState($"BiosHandler: opened, {_tabCount} tabs");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Bios_OnEnable_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the user selects a tab. Collects settings and announces first.</summary>
        [HarmonyPatch(typeof(Bios), "OnTab")]
        static class Bios_OnTab_Patch
        {
            static void Postfix(Bios __instance)
            {
                try
                {
                    _settings.Clear();
                    _focusIndex = -1;

                    // Identify active tab: the non-interactable button
                    Button[] tabs = __instance.GetComponentsInChildren<Button>(includeInactive: false);
                    string activeTabName = string.Empty;
                    foreach (Button btn in tabs)
                    {
                        if (btn.name.StartsWith("Tab") && !btn.IsInteractable())
                        {
                            activeTabName = ButtonHelper.GetLabel(btn);
                            break;
                        }
                    }

                    // Collect BiosSetting children of the active content panel
                    BiosSetting[] all = __instance.GetComponentsInChildren<BiosSetting>(includeInactive: false);
                    _settings.AddRange(all);

                    string tabLine = $"{Loc.Get("bios_tab")}: {activeTabName}";
                    ScreenReader.Say(tabLine);

                    if (_settings.Count > 0)
                    {
                        _focusIndex = 0;
                        AnnounceSetting(_settings[0]);
                        ScreenReader.Say(Loc.Get("bios_nav_hint"), interrupt: false);
                    }
                    DebugLogger.LogState($"BiosHandler: tab '{activeTabName}', {_settings.Count} settings");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Bios_OnTab_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when a confirm/reset prompt appears.</summary>
        [HarmonyPatch(typeof(Bios), "ShowPrompt")]
        static class Bios_ShowPrompt_Patch
        {
            static void Postfix(string message)
            {
                try
                {
                    ScreenReader.Say($"{message} {Loc.Get("bios_prompt_hint")}");
                    DebugLogger.LogState($"BiosHandler: prompt '{message}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Bios_ShowPrompt_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(Bios), "OnPromptYes")]
        static class Bios_OnPromptYes_Patch
        {
            static void Postfix()
            {
                try { ScreenReader.Say(Loc.Get("bios_confirmed")); }
                catch (Exception ex) { DebugLogger.LogWarning($"Bios_OnPromptYes_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(Bios), "OnPromptNo")]
        static class Bios_OnPromptNo_Patch
        {
            static void Postfix()
            {
                try { ScreenReader.Say(Loc.Get("bios_cancelled")); }
                catch (Exception ex) { DebugLogger.LogWarning($"Bios_OnPromptNo_Patch: {ex.Message}"); }
            }
        }

        #endregion
    }
}
