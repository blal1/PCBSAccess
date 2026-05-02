using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the job result screen when a job is submitted.
    ///
    /// Fires on JobResult.Init(Job) postfix.
    /// Announces: job title, success/fail, labour, total payout, star rating, review.
    /// Enter activates the OK button to dismiss.
    /// </summary>
    public static class JobResultHandler
    {
        #region State

        private static bool _active;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "JobResult";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.Return)) { ActivateOK(); return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_jobresult")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _active = false;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Open / Close

        private static bool IsOpen()
        {
            try
            {
                return WorkshopUI.m_jobResult != null
                    && WorkshopUI.m_jobResult.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void ActivateOK()
        {
            _active = false;
            InputRouter.Pop(_ctx);
            try { WorkshopUI.m_jobResult?.OnOK(); }
            catch (Exception ex) { DebugLogger.LogWarning($"JobResultHandler.ActivateOK: {ex.Message}"); }
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(JobResult), "Init")]
        static class JobResult_Init_Patch
        {
            static void Postfix(JobResult __instance, Job job)
            {
                try
                {
                    _active = true;
                    InputRouter.Push(_ctx);

                    bool success = job.IsSuccess();
                    string from    = job.GetFrom();
                    string subject = job.GetSubject();
                    float stars    = job.GetStars();

                    string labour   = __instance.m_labour.text;
                    string total    = __instance.m_total.text;
                    string result   = success ? Loc.Get("jobresult_success") : Loc.Get("jobresult_failed");

                    ScreenReader.Say($"{from}. {subject}. {result}.");
                    ScreenReader.Say(Loc.Get("jobresult_payout", labour, total), interrupt: false);
                    ScreenReader.Say(Loc.Get("jobresult_stars", stars.ToString("F1")), interrupt: false);

                    if (__instance.m_reviewPanel != null
                        && __instance.m_reviewPanel.activeSelf
                        && __instance.m_review != null
                        && !string.IsNullOrWhiteSpace(__instance.m_review.text))
                    {
                        ScreenReader.Say(__instance.m_review.text, interrupt: false);
                    }

                    ScreenReader.Say(Loc.Get("jobresult_dismiss_hint"), interrupt: false);

                    DebugLogger.LogState($"JobResultHandler: '{subject}', success={success}, stars={stars:F1}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"JobResult_Init_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
