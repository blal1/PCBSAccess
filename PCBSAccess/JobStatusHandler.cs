using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces job status panel when toggled on and allows cycling through objectives.
    ///
    /// On panel show: announces job title and objective completion count.
    /// F7 (wired in Main): cycles through objectives one by one.
    /// </summary>
    public static class JobStatusHandler
    {
        #region State

        private static bool _panelVisible;
        private static JobStatusObjective[] _objectives;
        private static int _objectiveIndex = -1;
        private static string _jobSummary;

        #endregion

        #region Public API

        /// <summary>Cycles to the next objective. Called from Main on F7.</summary>
        public static void AnnounceNextObjective()
        {
            if (!_panelVisible)
            {
                ScreenReader.Say(Loc.Get("jobstatus_closed"));
                return;
            }

            if (_objectives == null || _objectives.Length == 0)
            {
                ScreenReader.Say(Loc.Get("jobstatus_no_objectives"));
                return;
            }

            _objectiveIndex = (_objectiveIndex + 1) % _objectives.Length;
            AnnounceObjective(_objectives[_objectiveIndex]);
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _panelVisible = false;
            _objectives = null;
            _objectiveIndex = -1;
            _jobSummary = null;
        }

        #endregion

        #region Announce helpers

        private static void AnnounceObjective(JobStatusObjective obj)
        {
            if (obj == null) return;
            try
            {
                string text = obj.m_text.text;
                string status;
                if (obj.m_met.activeSelf)
                    status = Loc.Get("jobstatus_met");
                else if (obj.m_optional)
                    status = Loc.Get("jobstatus_optional");
                else
                    status = Loc.Get("jobstatus_not_met");

                int total = _objectives != null ? _objectives.Length : 0;
                int idx = _objectiveIndex + 1;
                ScreenReader.Say($"{idx} {Loc.Get("jobstatus_of")} {total}. {text}. {status}.");
                DebugLogger.LogState($"JobStatusHandler: objective {idx}/{total} '{text}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"JobStatusHandler.AnnounceObjective: {ex.Message}");
            }
        }

        private static IEnumerator AnnounceAfterFrame(JobStatus jobStatus)
        {
            yield return null;
            try
            {
                if (jobStatus == null) yield break;

                // Read job header
                string header = jobStatus.m_jobHeader?.m_title?.text ?? string.Empty;

                // Read objectives
                _objectives = jobStatus.m_objectiveContainer?.m_contents
                    ?.GetComponentsInChildren<JobStatusObjective>(includeInactive: false);
                _objectiveIndex = -1;

                int total = _objectives != null ? _objectives.Length : 0;
                int met = 0;
                if (_objectives != null)
                {
                    foreach (var obj in _objectives)
                    {
                        if (obj.m_met.activeSelf) met++;
                    }
                }

                string summaryText = total == 0
                    ? $"{Loc.Get("jobstatus_open")} {header}. {Loc.Get("jobstatus_no_objectives")}"
                    : $"{Loc.Get("jobstatus_open")} {header}. {met} {Loc.Get("jobstatus_of")} {total} {Loc.Get("jobstatus_complete")}. {Loc.Get("jobstatus_press_f7")}.";

                _jobSummary = summaryText;
                ScreenReader.Say(summaryText);
                DebugLogger.LogState($"JobStatusHandler: open, {met}/{total} objectives met");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"JobStatusHandler.AnnounceAfterFrame: {ex.Message}");
            }
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires when any AlphaTransition panel shows or hides.
        /// Checks if the instance is a JobStatus and announces accordingly.
        /// </summary>
        [HarmonyPatch(typeof(AlphaTransition), "SetVisible")]
        static class AlphaTransition_SetVisible_Patch
        {
            static void Postfix(AlphaTransition __instance, MonoBehaviour focus)
            {
                try
                {
                    JobStatus jobStatus = __instance as JobStatus;
                    if (jobStatus == null) return;

                    if (focus != null)
                    {
                        // Panel becoming visible
                        _panelVisible = true;
                        jobStatus.StartCoroutine(AnnounceAfterFrame(jobStatus));
                    }
                    else
                    {
                        // Panel hiding
                        _panelVisible = false;
                        _objectives = null;
                        _objectiveIndex = -1;
                        ScreenReader.Say(Loc.Get("jobstatus_closed"));
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"AlphaTransition_SetVisible_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
