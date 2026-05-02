using System;
using System.Reflection;
using FuturLab;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the tablet EmailApp — email rows and content pane.
    ///
    /// Patches EmailApp.OnClickRow postfix: announces from, subject, date, body.
    /// Up/Down/Home/End navigation through inbox rows when the app is active.
    /// </summary>
    public static class EmailAppHandler
    {
        #region State

        private static EmailApp      _app;
        private static EmailRowBase[] _rows;
        private static int            _index = -1;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "EmailApp";
            // Active only while the virtual-PC EmailApp's game object is alive and visible.
            public bool IsActive => _app != null && _app.gameObject.activeInHierarchy;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.Escape)) { CloseWindow(); return true; }

                if (_rows == null || _rows.Length == 0) return false;

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (_index > 0) SelectRow(_index - 1);
                    else ScreenReader.Say(Loc.Get("nav_first_item"));
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (_index < _rows.Length - 1) SelectRow(_index + 1);
                    else ScreenReader.Say(Loc.Get("nav_last_item"));
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.Home)) { SelectRow(0);                return true; }
                if (Input.GetKeyDown(KeyCode.End))  { SelectRow(_rows.Length - 1); return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_email")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Re-reads the currently selected email without re-triggering OnClickRow.</summary>
        public static void AnnounceCurrentEmail()
        {
            if (_rows == null || _index < 0 || _index >= _rows.Length)
            {
                ScreenReader.Say(Loc.Get("email_no_selection"));
                return;
            }
            // Read directly — do NOT call SelectRow/OnClick which would re-trigger both patches.
            AnnounceEmailRow(_rows[_index], _index);
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _app   = null;
            _rows  = null;
            _index = -1;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Helpers

        private static void CloseWindow()
        {
            if (_app == null) return;
            try
            {
                WindowFrame wf = _app.GetComponentInParent<WindowFrame>();
                if (wf != null)
                    wf.OnClose(); // OS_CloseWindow_Patch fires and announces the title
                else
                    DebugLogger.LogWarning("EmailAppHandler.CloseWindow: no WindowFrame found");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"EmailAppHandler.CloseWindow: {ex.Message}");
            }
        }

        private static void SelectRow(int index)
        {
            _index = index;
            _rows[_index].OnClick();   // triggers OnClickRow → fires our patch
        }

        /// <summary>Announce an email row directly without triggering OnClickRow patches.</summary>
        private static void AnnounceEmailRow(EmailRowBase row, int index)
        {
            if (row == null) return;
            try
            {
                IEmail email = row.GetEmail();
                if (email == null) return;

                string from    = email.GetFrom();
                string subject = email.GetSubject();
                string date    = row.m_date != null ? row.m_date.text : string.Empty;
                string pos     = $"{index + 1} {Loc.Get("nav_of")} {(_rows?.Length ?? 1)}";

                ScreenReader.Say($"{pos}. {from}. {subject}.");
                if (!string.IsNullOrEmpty(date)) ScreenReader.Say(date, interrupt: false);

                ITSupportEmailRowJob itRow = row as ITSupportEmailRowJob;
                if (itRow != null)
                    AnnounceITSupportRow(itRow);
                else
                {
                    string body = email.GetBody();
                    if (!string.IsNullOrEmpty(body)) ScreenReader.Say(body, interrupt: false);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"EmailAppHandler.AnnounceEmailRow: {ex.Message}");
            }
        }

        #endregion

        #region Patches

        // Reflection for IT Support DLC email row fields.
        private static readonly FieldInfo _fITStaffName   = typeof(ITSupportEmailRowJob).GetField("m_targetStaffName",  BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fITPrimaryText = typeof(ITSupportEmailRowJob).GetField("m_primaryText",      BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fITSecondText  = typeof(ITSupportEmailRowJob).GetField("m_secondaryText",    BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fITDeadline    = typeof(ITSupportEmailRowJob).GetField("m_deadlineText",     BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fITNote        = typeof(ITSupportEmailRowJob).GetField("m_noteText",         BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Fires when a virtual-PC email row is selected.
        /// Skips career-inbox EmailApp instances (handled by CareerJobHandler).
        /// </summary>
        [HarmonyPatch(typeof(EmailApp), "OnClickRow")]
        static class EmailApp_OnClickRow_Patch
        {
            static void Postfix(EmailApp __instance, EmailRowBase row)
            {
                try
                {
                    // Career inbox emails are NOT inside a VirtualComputer — skip them here.
                    if (__instance.GetComponentInParent<VirtualComputer>() == null) return;

                    // Capture and refresh row list whenever selection fires
                    _app  = __instance;
                    _rows = __instance.m_inbox.content
                        .GetComponentsInChildren<EmailRowBase>(includeInactive: false);

                    // Find selected index
                    for (int i = 0; i < _rows.Length; i++)
                    {
                        if (_rows[i] == row) { _index = i; break; }
                    }

                    InputRouter.Push(_ctx);

                    IEmail email = row.GetEmail();
                    if (email == null) return;

                    string from    = email.GetFrom();
                    string subject = email.GetSubject();
                    string date    = row.m_date != null ? row.m_date.text : string.Empty;

                    string pos = $"{_index + 1} {Loc.Get("nav_of")} {_rows.Length}";
                    ScreenReader.Say($"{pos}. {from}. {subject}.");
                    if (!string.IsNullOrEmpty(date)) ScreenReader.Say(date, interrupt: false);

                    // IT Support DLC job rows use specialised fields — GetBody() returns placeholder.
                    ITSupportEmailRowJob itRow = row as ITSupportEmailRowJob;
                    if (itRow != null)
                    {
                        AnnounceITSupportRow(itRow);
                    }
                    else
                    {
                        string body = email.GetBody();
                        if (!string.IsNullOrEmpty(body)) ScreenReader.Say(body, interrupt: false);
                    }

                    DebugLogger.LogState($"EmailAppHandler: [{_index}] '{subject}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"EmailApp_OnClickRow_Patch: {ex.Message}");
                }
            }
        }

        private static void AnnounceITSupportRow(ITSupportEmailRowJob row)
        {
            try
            {
                string staff    = (_fITStaffName  ?.GetValue(row) as Text)?.text ?? string.Empty;
                string primary  = (_fITPrimaryText?.GetValue(row) as Text)?.text ?? string.Empty;
                string secondary= (_fITSecondText ?.GetValue(row) as Text)?.text ?? string.Empty;
                string deadline = (_fITDeadline   ?.GetValue(row) as Text)?.text ?? string.Empty;
                string note     = (_fITNote       ?.GetValue(row) as Text)?.text ?? string.Empty;

                if (!string.IsNullOrEmpty(staff))    ScreenReader.Say(staff,    interrupt: false);
                if (!string.IsNullOrEmpty(primary))  ScreenReader.Say(primary,  interrupt: false);
                if (!string.IsNullOrEmpty(secondary) && secondary != primary)
                                                     ScreenReader.Say(secondary, interrupt: false);
                if (!string.IsNullOrEmpty(deadline)) ScreenReader.Say(deadline, interrupt: false);
                if (!string.IsNullOrEmpty(note))     ScreenReader.Say(note,     interrupt: false);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"EmailAppHandler.AnnounceITSupportRow: {ex.Message}");
            }
        }

        #endregion
    }
}
