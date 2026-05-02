using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces workshop part tooltips when hovering slots during assembly/disassembly.
    /// Skips re-announcement if the context (part) hasn't changed.
    ///
    /// F4 / Numpad0 (wired in Main): re-read the last tooltip.
    /// </summary>
    public static class WorkshopBuildHandler
    {
        #region State

        private static string _lastAnnouncement;
        private static MonoBehaviour _lastContext;

        #endregion

        #region Public API

        /// <summary>Re-reads the last tooltip. Called from Main on F4 / Numpad0.</summary>
        public static void AnnounceLastTooltip()
        {
            if (!string.IsNullOrEmpty(_lastAnnouncement))
                ScreenReader.Say(Loc.Get("tooltip_reread", _lastAnnouncement));
            else
                ScreenReader.Say(Loc.Get("tooltip_none"));
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _lastAnnouncement = null;
            _lastContext = null;
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires when any tooltip is shown.
        /// Announces action + part name + function (if changed context).
        /// </summary>
        [HarmonyPatch(typeof(ToolTips), "Set",
            new[] { typeof(string), typeof(MonoBehaviour), typeof(string), typeof(string) })]
        static class ToolTips_Set_Patch
        {
            static void Postfix(string text, MonoBehaviour context, string subject, string moreInfo)
            {
                try
                {
                    // Only announce when context changes — skip same-part re-hover
                    if (context == _lastContext) return;
                    _lastContext = context;

                    string announcement = BuildAnnouncement(text, subject, moreInfo);
                    if (string.IsNullOrEmpty(announcement)) return;

                    _lastAnnouncement = announcement;
                    ScreenReader.Say(announcement);
                    DebugLogger.LogState($"WorkshopBuildHandler: '{announcement}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ToolTips_Set_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the tooltip is cleared. Resets context tracking.</summary>
        [HarmonyPatch(typeof(ToolTips), "Clear", new System.Type[0])]
        static class ToolTips_Clear_Patch
        {
            static void Postfix()
            {
                try
                {
                    _lastContext = null;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ToolTips_Clear_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Fires when the building mode changes (assembly, disassembly, cabling, piping).
        /// Announces the new mode name so the user always knows which mode is active.
        /// </summary>
        [HarmonyPatch(typeof(WorkingOnPC), "SetMode")]
        static class WorkingOnPC_SetMode_Patch
        {
            static void Postfix(WorkingOnComputerState.Mode mode)
            {
                try
                {
                    string key = ModeToLocKey(mode);
                    if (key != null)
                    {
                        ScreenReader.Say(Loc.Get(key));
                        DebugLogger.LogState($"WorkshopBuildHandler: mode={mode}");
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkingOnPC_SetMode_Patch: {ex.Message}");
                }
            }

            private static string ModeToLocKey(WorkingOnComputerState.Mode mode)
            {
                switch (mode)
                {
                    case WorkingOnComputerState.Mode.ASSEMBLY:          return "mode_assembly";
                    case WorkingOnComputerState.Mode.DISASSEMBLY:       return "mode_disassembly";
                    case WorkingOnComputerState.Mode.CABLING:           return "mode_cabling";
                    case WorkingOnComputerState.Mode.PIPING:            return "mode_piping";
                    case WorkingOnComputerState.Mode.COMBO_ASSEMBLY:    return "mode_combo_assembly";
                    case WorkingOnComputerState.Mode.COMBO_DISASSEMBLY: return "mode_combo_disassembly";
                    default:                                             return null;
                }
            }
        }

        #endregion

        #region Helpers

        private static string BuildAnnouncement(string action, string subject, string moreInfo)
        {
            if (string.IsNullOrEmpty(action) && string.IsNullOrEmpty(subject))
                return null;

            string result = string.IsNullOrEmpty(action) ? string.Empty : action.Trim();

            if (!string.IsNullOrEmpty(subject))
                result += (result.Length > 0 ? ". " : string.Empty) + subject;

            if (!string.IsNullOrEmpty(moreInfo))
                result += ". " + moreInfo;

            return result;
        }

        #endregion
    }
}
