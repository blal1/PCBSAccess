using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces job details when navigating the email inbox and announces
    /// cash/kudos changes as they happen during career play.
    ///
    /// Up/Down arrows: navigate inbox rows.
    /// Enter: trigger the positive action button (Accept / Collect).
    /// F3 (wired in Main): re-read the last announced job.
    /// </summary>
    public static class CareerJobHandler
    {
        #region State

        private static EmailApp _emailApp;
        private static string _lastJobSummary;

        // Cached reflection fields — resolved once, then reused
        private static FieldInfo _rowsField;
        private static FieldInfo _selectedIndexField;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "CareerJobInbox";

            public bool IsActive
            {
                get
                {
                    try { return _emailApp != null && _emailApp.gameObject.activeInHierarchy; }
                    catch { return false; }
                }
            }

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow)) { NavigateRows(1);          return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))   { NavigateRows(-1);         return true; }
                if (Input.GetKeyDown(KeyCode.Home))      { NavigateToRow(0);         return true; }
                if (Input.GetKeyDown(KeyCode.End))       { NavigateToLastRow();      return true; }
                if (Input.GetKeyDown(KeyCode.Return))    { ClickPositive();          return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_careerjob")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Resets on scene change. Called from Main.OnSceneLoaded.</summary>
        public static void Reset()
        {
            _emailApp = null;
            _lastJobSummary = null;
            InputRouter.Pop(_ctx);
        }

        /// <summary>Re-reads the last announced job. Called from Main on F3.</summary>
        public static void AnnounceCurrentJob()
        {
            if (!string.IsNullOrEmpty(_lastJobSummary))
                ScreenReader.Say(Loc.Get("job_reread", _lastJobSummary));
            else
                ScreenReader.Say(Loc.Get("job_none"));
        }

        #endregion

        #region Navigation

        private static void NavigateRows(int direction)
        {
            if (_emailApp == null) return;

            try
            {
                List<EmailRowBase> rows = GetRows(_emailApp);
                if (rows == null || rows.Count == 0) return;

                int current = GetSelectedIndex(_emailApp);
                int next = Mathf.Clamp(current + direction, 0, rows.Count - 1);

                if (next != current)
                    _emailApp.OnClickRow(rows[next]);
                else
                    ScreenReader.Say(direction > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CareerJobHandler.NavigateRows: {ex.Message}");
            }
        }

        private static void NavigateToRow(int index)
        {
            if (_emailApp == null) return;
            try
            {
                List<EmailRowBase> rows = GetRows(_emailApp);
                if (rows == null || rows.Count == 0) return;
                _emailApp.OnClickRow(rows[Mathf.Clamp(index, 0, rows.Count - 1)]);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CareerJobHandler.NavigateToRow: {ex.Message}");
            }
        }

        private static void NavigateToLastRow()
        {
            if (_emailApp == null) return;
            try
            {
                List<EmailRowBase> rows = GetRows(_emailApp);
                if (rows != null && rows.Count > 0)
                    _emailApp.OnClickRow(rows[rows.Count - 1]);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CareerJobHandler.NavigateToLastRow: {ex.Message}");
            }
        }

        private static void ClickPositive()
        {
            var btn = _emailApp?.m_pane?.m_positiveButton;
            if (btn != null && btn.gameObject.activeSelf)
                ButtonHelper.Click(btn);
        }

        #endregion

        #region Announce helpers

        private static void AnnounceRow(EmailRowBase row)
        {
            if (row == null) return;

            try
            {
                IEmail email = row.GetEmail();
                if (email == null) return;

                string from    = email.GetFrom();
                string subject = email.GetSubject();
                string action  = email.GetPositiveAction();

                string text;
                if (email is Job job)
                {
                    string labour = FormatCash(job.GetLabour());
                    int    kudos  = job.GetKudos();
                    string actionPart = string.IsNullOrEmpty(action) ? string.Empty : $" {action}.";
                    text = $"{from}. {subject}. {Loc.Get("job_labour")}: {labour}. {Loc.Get("job_kudos")}: {kudos}.{actionPart}";
                }
                else
                {
                    text = $"{from}. {subject}.";
                }

                _lastJobSummary = text;
                ScreenReader.Say(text);
                DebugLogger.LogState($"CareerJobHandler: '{subject}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CareerJobHandler.AnnounceRow: {ex.Message}");
            }
        }

        private static string FormatCash(int amount)
            => "$" + Math.Abs(amount).ToString("N0");

        #endregion

        #region Reflection helpers

        private static List<EmailRowBase> GetRows(EmailApp app)
        {
            if ((object)_rowsField == null)
                _rowsField = typeof(EmailApp).GetField("m_rows",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            return _rowsField?.GetValue(app) as List<EmailRowBase>;
        }

        private static int GetSelectedIndex(EmailApp app)
        {
            if ((object)_selectedIndexField == null)
                _selectedIndexField = typeof(EmailApp).GetField("m_selectedRowIndex",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            return (object)_selectedIndexField != null ? (int)_selectedIndexField.GetValue(app) : 0;
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires when a career inbox email row is selected.
        /// Skips virtual-PC EmailApp instances (those are handled by EmailAppHandler).
        /// </summary>
        [HarmonyPatch(typeof(EmailApp), "OnClickRow")]
        static class EmailApp_OnClickRow_Patch
        {
            static void Postfix(EmailApp __instance, EmailRowBase row)
            {
                try
                {
                    // Virtual-PC emails live inside a VirtualComputer — skip them here.
                    if (__instance.GetComponentInParent<VirtualComputer>() != null) return;

                    _emailApp = __instance;
                    InputRouter.Push(_ctx);
                    AnnounceRow(row);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"EmailApp_OnClickRow_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Announces cash received (job collection, auction sale, etc.).</summary>
        [HarmonyPatch(typeof(CareerStatus), "AddCash")]
        static class CareerStatus_AddCash_Patch
        {
            static void Postfix(int amount, CareerStatus __instance)
            {
                try
                {
                    if (amount <= 0) return;
                    int total = __instance.GetCash();
                    ScreenReader.Say(Loc.Get("career_cash_gained", FormatCash(amount), FormatCash(total)));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"CareerStatus_AddCash_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Announces cash spent (parts, rent, utilities, etc.) when the spend succeeds.</summary>
        [HarmonyPatch(typeof(CareerStatus), "SpendCash")]
        static class CareerStatus_SpendCash_Patch
        {
            static void Postfix(bool __result, int cash, CareerStatus __instance)
            {
                try
                {
                    if (!__result || cash <= 0) return;
                    int total = __instance.GetCash();
                    ScreenReader.Say(Loc.Get("career_cash_spent", FormatCash(cash), FormatCash(total)));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"CareerStatus_SpendCash_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Announces kudos earned (job completion).</summary>
        [HarmonyPatch(typeof(CareerStatus), "AddKudos")]
        static class CareerStatus_AddKudos_Patch
        {
            static void Postfix(int kudos, CareerStatus __instance)
            {
                try
                {
                    if (kudos <= 0) return;
                    int total = __instance.GetKudos();
                    ScreenReader.Say(Loc.Get("career_kudos", kudos, total));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"CareerStatus_AddKudos_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
