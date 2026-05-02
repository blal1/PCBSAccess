using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Part Rankings app.
    /// Up/Down navigate the result list; Numpad6 re-reads current entry.
    /// Patches RankApp.RefreshList (list built) and SelectResult (item focused by game).
    /// </summary>
    public static class RankAppHandler
    {
        #region State

        private static RankApp _app;
        private static List<RankAppRow> _rows = new List<RankAppRow>();
        private static int _focusIndex = -1;

        private static FieldInfo _resultRowsField;
        private static MethodInfo _selectResultMethod;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "RankApp";
            public bool IsActive => _app != null;

            public bool HandleInput()
            {
                if (_rows.Count == 0) return false;

                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);              return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);             return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);            return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_rows.Count - 1); return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_rank")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        public static void Reset()
        {
            _app        = null;
            _rows.Clear();
            _focusIndex = -1;
            InputRouter.Pop(_ctx);
        }

        /// <summary>Re-reads current entry. Called from Main on Numpad6.</summary>
        public static void AnnounceCurrentEntry()
        {
            if (_focusIndex >= 0 && _focusIndex < _rows.Count)
                AnnounceRow(_rows[_focusIndex], _focusIndex + 1, _rows.Count);
            else
                ScreenReader.Say(Loc.Get("rank_no_item"));
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
            InvokeSelectResult(_focusIndex);
            AnnounceRow(_rows[_focusIndex], _focusIndex + 1, _rows.Count);
        }

        private static void NavigateTo(int index)
        {
            if (_rows.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _rows.Count - 1);
            InvokeSelectResult(_focusIndex);
            AnnounceRow(_rows[_focusIndex], _focusIndex + 1, _rows.Count);
        }

        private static void InvokeSelectResult(int index)
        {
            try
            {
                if (_app == null) return;
                if (_selectResultMethod == null)
                    _selectResultMethod = typeof(RankApp).GetMethod("SelectResult",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                _selectResultMethod?.Invoke(_app, new object[] { index });
            }
            catch (Exception ex) { DebugLogger.LogWarning($"RankAppHandler.InvokeSelectResult: {ex.Message}"); }
        }

        private static void AnnounceRow(RankAppRow row, int pos, int total)
        {
            if (row == null) return;
            string rank  = row.m_rank?.text  ?? "?";
            string name  = row.m_name?.text  ?? "?";
            string score = row.m_score?.text ?? "?";
            ScreenReader.Say($"{Loc.Get("rank_entry_label")}, {pos} {Loc.Get("nav_of")} {total}: {rank}. {name}. {Loc.Get("rank_score_label")}: {score}.");
            DebugLogger.LogState($"RankAppHandler: {pos}/{total} rank={rank} name='{name}' score={score}");
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(RankApp), "RefreshList")]
        static class RankApp_RefreshList_Patch
        {
            static void Postfix(RankApp __instance)
            {
                try
                {
                    _app        = __instance;
                    _focusIndex = -1;
                    _rows.Clear();

                    if (_resultRowsField == null)
                        _resultRowsField = typeof(RankApp).GetField("m_resultRows",
                            BindingFlags.NonPublic | BindingFlags.Instance);

                    var resultRows = _resultRowsField?.GetValue(__instance) as List<RankAppRow>;
                    if (resultRows != null) _rows.AddRange(resultRows);

                    if (_rows.Count == 0)
                    {
                        ScreenReader.Say(Loc.Get("rank_empty"));
                        return;
                    }

                    InputRouter.Push(_ctx);
                    _focusIndex = 0;
                    ScreenReader.Say(Loc.Get("rank_open", _rows.Count));
                    ScreenReader.Say($"{Loc.Get("rank_entry_label")}, 1 {Loc.Get("nav_of")} {_rows.Count}: " +
                                     $"{_rows[0].m_rank?.text}. {_rows[0].m_name?.text}. " +
                                     $"{Loc.Get("rank_score_label")}: {_rows[0].m_score?.text}.", interrupt: false);

                    DebugLogger.LogState($"RankAppHandler: {_rows.Count} entries");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"RankApp_RefreshList_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
