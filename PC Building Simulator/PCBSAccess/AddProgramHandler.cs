using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the AddProgram tablet app (software install/remove).
    ///
    /// SetMode patch: captures instance, announces mode + program list.
    /// UpdateProgramList patch: refreshes item list after any change.
    /// ShowProgressDialog patch: announces install/remove start.
    /// OnRestartYes/No patches: announces restart outcome.
    ///
    /// Up/Down/Home/End: navigate program list.
    /// Enter: invoke the selected program's click action (install/remove).
    /// Numpad5 (wired in Main): re-reads current program.
    /// </summary>
    public static class AddProgramHandler
    {
        #region State

        private static AddProgramApp _app;
        private static ProgramIcon[] _items;
        private static int _index = -1;
        private static bool _addMode;

        // Reflection: ProgramIcon.m_onClick is a private Action field
        private static FieldInfo _onClickField;
        private static FieldInfo OnClickField =>
            _onClickField ?? (_onClickField = typeof(ProgramIcon)
                .GetField("m_onClick", BindingFlags.NonPublic | BindingFlags.Instance));

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "AddProgram";
            public bool IsActive => _app != null;

            public bool HandleInput()
            {
                if (_items == null || _items.Length == 0) return false;

                // Guard: only handle input when install/restart popups are not active
                if (_app != null && (_app.m_installPopup.activeSelf || _app.m_restartPopup.activeSelf))
                    return false;

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (_index > 0) AnnounceItem(--_index);
                    else ScreenReader.Say(Loc.Get("nav_first_item"));
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (_index < _items.Length - 1) AnnounceItem(++_index);
                    else ScreenReader.Say(Loc.Get("nav_last_item"));
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.Home))   { _index = 0; AnnounceItem(_index); return true; }
                if (Input.GetKeyDown(KeyCode.End))    { _index = _items.Length - 1; AnnounceItem(_index); return true; }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    TriggerSelected();
                    return true;
                }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_addprogram")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        public static void AnnounceCurrentItem()
        {
            if (_items != null && _index >= 0 && _index < _items.Length)
                AnnounceItem(_index);
            else
                ScreenReader.Say(Loc.Get("addprog_no_item"));
        }

        public static void Reset()
        {
            _app   = null;
            _items = null;
            _index = -1;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Helpers

        private static void AnnounceItem(int index)
        {
            try
            {
                string name = _items[index].m_text != null ? _items[index].m_text.text : "?";
                string pos  = $"{index + 1} {Loc.Get("nav_of")} {_items.Length}";
                ScreenReader.Say($"{pos}. {name}.");
                DebugLogger.LogState($"AddProgramHandler [{index}]: {name}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"AddProgramHandler.AnnounceItem: {ex.Message}");
            }
        }

        private static void TriggerSelected()
        {
            try
            {
                if (_index < 0 || _index >= _items.Length) return;
                var onClick = OnClickField?.GetValue(_items[_index]) as Action;
                onClick?.Invoke();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"AddProgramHandler.TriggerSelected: {ex.Message}");
            }
        }

        private static void RefreshList()
        {
            if (_app == null) return;
            _items = _app.m_programList.content
                .GetComponentsInChildren<ProgramIcon>(includeInactive: false);
            _index = _items.Length > 0 ? 0 : -1;
        }

        #endregion

        #region Patches

        /// <summary>SetMode fires when user switches Add/Remove tab.</summary>
        [HarmonyPatch(typeof(AddProgramApp), "SetMode")]
        static class AddProgramApp_SetMode_Patch
        {
            static void Postfix(AddProgramApp __instance, bool add)
            {
                try
                {
                    _app     = __instance;
                    _addMode = add;
                    RefreshList();
                    InputRouter.Push(_ctx);

                    string mode = add ? Loc.Get("addprog_add_mode") : Loc.Get("addprog_remove_mode");
                    if (_items == null || _items.Length == 0)
                    {
                        string msg = __instance.m_message != null ? __instance.m_message.text : string.Empty;
                        ScreenReader.Say($"{mode}. {msg}");
                    }
                    else
                    {
                        ScreenReader.Say(Loc.Get("addprog_open", mode, _items.Length));
                        AnnounceItem(0);
                    }
                    DebugLogger.LogState($"AddProgramHandler: mode={mode}, {_items?.Length ?? 0} items");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"AddProgramApp_SetMode_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>UpdateProgramList rebuilds the list — refresh our cached array.</summary>
        [HarmonyPatch(typeof(AddProgramApp), "UpdateProgramList")]
        static class AddProgramApp_UpdateProgramList_Patch
        {
            static void Postfix(AddProgramApp __instance)
            {
                try
                {
                    if (_app == null) _app = __instance;
                    RefreshList();
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"AddProgramApp_UpdateProgramList_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>ShowProgressDialog fires when install/remove begins.</summary>
        [HarmonyPatch(typeof(AddProgramApp), "ShowProgressDialog")]
        static class AddProgramApp_ShowProgressDialog_Patch
        {
            static void Postfix(AddProgramApp __instance, string action, OSProgramDesc desc)
            {
                try
                {
                    string name = desc?.m_uiName ?? string.Empty;
                    ScreenReader.Say(Loc.Get("addprog_progress", action, name));
                    DebugLogger.LogState($"AddProgramHandler: progress '{action}' '{name}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"AddProgramApp_ShowProgressDialog_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(AddProgramApp), "OnRestartYes")]
        static class AddProgramApp_OnRestartYes_Patch
        {
            static void Postfix()
            {
                try { ScreenReader.Say(Loc.Get("addprog_restarting")); }
                catch (Exception ex) { DebugLogger.LogWarning($"AddProgramApp_OnRestartYes_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(AddProgramApp), "OnRestartNo")]
        static class AddProgramApp_OnRestartNo_Patch
        {
            static void Postfix()
            {
                try { ScreenReader.Say(Loc.Get("addprog_restart_cancelled")); }
                catch (Exception ex) { DebugLogger.LogWarning($"AddProgramApp_OnRestartNo_Patch: {ex.Message}"); }
            }
        }

        #endregion
    }
}
