using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the in-game virtual OS desktop icons and start menu items.
    ///
    /// Desktop: Up/Down/Home/End navigate icons; Enter launches the focused icon.
    /// Start menu: Up/Down navigate items; Enter launches; Escape closes.
    /// Windows key or game button toggles the start menu on top of the desktop context.
    ///
    /// Uses a single IInputContext for both modes, with _startMenuOpen as internal flag.
    /// </summary>
    public static class OSHandler
    {
        #region State

        private static OS _os;
        private static List<ProgramIcon> _icons = new List<ProgramIcon>();
        private static int _focusIndex = -1;
        private static bool _startMenuOpen;
        private static List<StartMenuItem> _startItems = new List<StartMenuItem>();
        private static int _startFocus = -1;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "OS";
            public bool IsActive => _os != null;

            public bool HandleInput()
            {
                if (_startMenuOpen)
                    return HandleStartMenu();
                return HandleDesktop();
            }

            private bool HandleStartMenu()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { NavStart(1);          return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { NavStart(-1);         return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavStartTo(0);        return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavStartTo(_startItems.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter)){ LaunchFromStart();    return true; }
                if (Input.GetKeyDown(KeyCode.Escape))     { ToggleStartMenu();    return true; }
                return false;
            }

            private bool HandleDesktop()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { NavIcons(1);          return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { NavIcons(-1);         return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavIconsTo(0);        return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavIconsTo(_icons.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.KeypadEnter)){ LaunchFocused();      return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_os")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _os           = null;
            _icons.Clear();
            _focusIndex   = -1;
            _startMenuOpen = false;
            _startItems.Clear();
            _startFocus   = -1;
            InputRouter.Pop(_ctx);
        }

        /// <summary>Returns a human-readable list of all open window titles — used by F11.</summary>
        public static string GetOpenWindowNames()
        {
            if (_os == null) return string.Empty;
            try
            {
                var frames = _os.GetComponentsInChildren<WindowFrame>(includeInactive: false);
                if (frames == null || frames.Length == 0) return string.Empty;
                var sb = new System.Text.StringBuilder();
                foreach (var wf in frames)
                {
                    string title = wf?.m_title?.text;
                    if (!string.IsNullOrEmpty(title)) { sb.Append(title); sb.Append(", "); }
                }
                if (sb.Length > 2) sb.Length -= 2; // trim trailing ", "
                return sb.ToString();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"OSHandler.GetOpenWindowNames: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Desktop icon navigation

        private static void RefreshIcons()
        {
            _icons.Clear();
            if (_os == null) return;
            ProgramIcon[] all = _os.GetComponentsInChildren<ProgramIcon>(includeInactive: false);
            foreach (ProgramIcon icon in all) _icons.Add(icon);
        }

        private static void NavIcons(int dir)
        {
            RefreshIcons();
            if (_icons.Count == 0) return;

            int next = Mathf.Clamp(_focusIndex + dir, 0, _icons.Count - 1);

            if (next == _focusIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceIcon(_icons[_focusIndex], _focusIndex + 1, _icons.Count);
        }

        private static void NavIconsTo(int index)
        {
            RefreshIcons();
            if (_icons.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _icons.Count - 1);
            AnnounceIcon(_icons[_focusIndex], _focusIndex + 1, _icons.Count);
        }

        private static void AnnounceIcon(ProgramIcon icon, int pos, int total)
        {
            try
            {
                Text label = icon.GetComponentInChildren<Text>();
                string name = label != null ? label.text : icon.gameObject.name;
                ScreenReader.Say($"{Loc.Get("os_desktop")}, {pos} {Loc.Get("nav_of")} {total}: {name}");
                DebugLogger.LogState($"OSHandler: icon {pos}/{total} '{name}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"OSHandler.AnnounceIcon: {ex.Message}");
            }
        }

        private static void LaunchFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _icons.Count) return;
            Button btn = _icons[_focusIndex].GetComponent<Button>();
            ButtonHelper.Click(btn);
        }

        #endregion

        #region Start menu

        private static void ToggleStartMenu()
        {
            if (_os == null) return;
            try { _os.OnOpenStartMenu(); }
            catch (Exception ex) { DebugLogger.LogWarning($"OSHandler.ToggleStartMenu: {ex.Message}"); }
        }

        private static void NavStart(int dir)
        {
            if (_startItems.Count == 0) return;
            int next = Mathf.Clamp(_startFocus + dir, 0, _startItems.Count - 1);
            if (next == _startFocus)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _startFocus = next;
            AnnounceStartItem(_startFocus);
        }

        private static void NavStartTo(int index)
        {
            if (_startItems.Count == 0) return;
            _startFocus = Mathf.Clamp(index, 0, _startItems.Count - 1);
            AnnounceStartItem(_startFocus);
        }

        private static void LaunchFromStart()
        {
            if (_startFocus < 0 || _startFocus >= _startItems.Count) return;
            try { _startItems[_startFocus].OnClick(); }
            catch (Exception ex) { DebugLogger.LogWarning($"OSHandler.LaunchFromStart: {ex.Message}"); }
        }

        private static void AnnounceStartItem(int idx)
        {
            if (idx < 0 || idx >= _startItems.Count) return;
            try
            {
                StartMenuItem item = _startItems[idx];
                string name = item.m_name?.text ?? item.gameObject.name;
                ScreenReader.Say($"{Loc.Get("os_startmenu")}, {idx + 1} {Loc.Get("nav_of")} {_startItems.Count}: {name}");
                DebugLogger.LogState($"OSHandler: start item {idx + 1}/{_startItems.Count} '{name}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"OSHandler.AnnounceStartItem: {ex.Message}");
            }
        }

        private static void RefreshStartItems()
        {
            _startItems.Clear();
            if (_os == null) return;
            try
            {
                // Find start menu through the task bar reference
                StartMenuItem[] all = _os.m_taskBar
                    ?.GetComponentInChildren<StartMenu>(includeInactive: true)
                    ?.GetComponentsInChildren<StartMenuItem>(includeInactive: false);
                if (all != null)
                    _startItems.AddRange(all);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"OSHandler.RefreshStartItems: {ex.Message}");
            }
        }

        #endregion

        #region Patches

        /// <summary>Fires when the OS boots up. Saves reference and announces desktop.</summary>
        [HarmonyPatch(typeof(OS), "OnStartup")]
        static class OS_OnStartup_Patch
        {
            static void Postfix(OS __instance)
            {
                try
                {
                    _os = __instance;
                    _focusIndex = -1;
                    _startMenuOpen = false;
                    RefreshIcons();
                    InputRouter.Push(_ctx);

                    ScreenReader.Say(Loc.Get("os_booted", _icons.Count));
                    DebugLogger.LogState($"OSHandler: desktop, {_icons.Count} icons");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OS_OnStartup_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when an OS program is launched.</summary>
        [HarmonyPatch(typeof(OS), "Launch")]
        static class OS_Launch_Patch
        {
            static void Postfix(OSProgramDesc desc)
            {
                try
                {
                    if (desc?.m_uiName != null)
                        ScreenReader.Say(Loc.Get("os_launching", desc.m_uiName));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OS_Launch_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when a window is closed.</summary>
        [HarmonyPatch(typeof(OS), "CloseWindow")]
        static class OS_CloseWindow_Patch
        {
            static void Postfix(WindowFrame wf)
            {
                try
                {
                    string title = wf?.m_title?.text ?? string.Empty;
                    ScreenReader.Say(Loc.Get("os_window_closed", title));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OS_CloseWindow_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the start menu is toggled. Refreshes items and announces.</summary>
        [HarmonyPatch(typeof(OS), "OnOpenStartMenu")]
        static class OS_OnOpenStartMenu_Patch
        {
            static void Postfix()
            {
                try
                {
                    _startMenuOpen = !_startMenuOpen;

                    if (_startMenuOpen)
                    {
                        RefreshStartItems();
                        _startFocus = 0;
                        ScreenReader.Say(Loc.Get("os_startmenu_open", _startItems.Count));
                        if (_startItems.Count > 0) AnnounceStartItem(0);
                    }
                    else
                    {
                        ScreenReader.Say(Loc.Get("os_startmenu_closed"));
                    }
                    DebugLogger.LogState($"OSHandler: start menu {(_startMenuOpen ? "open" : "closed")}, {_startItems.Count} items");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OS_OnOpenStartMenu_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the OS shuts down.</summary>
        [HarmonyPatch(typeof(OS), "OnShutdown")]
        static class OS_OnShutdown_Patch
        {
            static void Postfix()
            {
                try
                {
                    Reset();
                    ScreenReader.Say(Loc.Get("os_shutdown"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"OS_OnShutdown_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when a new email is received. Announces badge.</summary>
        [HarmonyPatch(typeof(OS), "OnNewEmail")]
        static class OS_OnNewEmail_Patch
        {
            static void Postfix()
            {
                try { ScreenReader.Say(Loc.Get("os_new_email")); }
                catch (Exception ex) { DebugLogger.LogWarning($"OS_OnNewEmail_Patch: {ex.Message}"); }
            }
        }

        // BringToFront is now handled by WindowFocusHandler with deduplication.

        #endregion
    }
}
