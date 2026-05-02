using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces tutorial messages and checklist tasks during How to Build a PC mode.
    /// Patches: TutorialUI.Init, HBPCCheckList.SetTitleAndClear, HBPCCheckListItem.SetState.
    ///
    /// When the TutorialUI popup is visible, Enter/Space dismisses it by clicking its OK button.
    /// F2 (wired in Main) re-reads the last announced task.
    /// </summary>
    public static class HowToBuildAPCHandler
    {
        #region State

        private static string _lastTask = null;
        private static bool _popupActive = false;   // true while TutorialUI popup is visible
        private static TutorialUI _cachedUI = null;
        private static float _activatedTime = -1f;  // kept for reference but no longer used for guard
        private static bool _inputReady = false;    // true once all keys released after popup appears

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "HowToBuildAPC-Popup";
            public bool IsActive => _popupActive && IsTutorialVisible();

            public bool HandleInput()
            {
                // Wait until Enter and Space are both released before accepting any press.
                // This is immune to timing issues — no matter how long scene init takes,
                // we only accept input AFTER the user has lifted all keys.
                if (!_inputReady)
                {
                    if (!Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.Space))
                        _inputReady = true;
                    return false;
                }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    DismissPopup();
                    return true;
                }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_workshop")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Resets on scene change. Called from Main.OnSceneLoaded.</summary>
        public static void Reset()
        {
            _lastTask      = null;
            _popupActive   = false;
            _cachedUI      = null;
            _activatedTime = -1f;
            _inputReady    = false;
            InputRouter.Pop(_ctx);
        }

        /// <summary>Stores the most recent checklist task text for F2 re-read.</summary>
        internal static void SetLastTask(string text)
        {
            _lastTask = text;
        }

        /// <summary>Re-announces the current checklist task. Called from Main on F2.</summary>
        public static void AnnounceCurrentTask()
        {
            if (_lastTask != null)
                ScreenReader.Say(Loc.Get("hbpc_reread", _lastTask));
            else
                ScreenReader.Say(Loc.Get("hbpc_no_task"));
        }

        #endregion

        #region Popup dismissal

        private static bool IsTutorialVisible()
        {
            try
            {
                if (_cachedUI != null)
                    return _cachedUI.gameObject.activeSelf;
                return CommonUI.tutorialUI != null && CommonUI.tutorialUI.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void DismissPopup()
        {
            try
            {
                TutorialUI ui = _cachedUI ?? CommonUI.tutorialUI;
                if (ui == null) return;

                DebugLogger.LogState("HowToBuildAPCHandler: dismissing popup via TutorialUI.Dismiss()");
                _popupActive = false;
                InputRouter.Pop(_ctx);
                // Dismiss() calls GoToPreviousState() — identical to what Escape does in-game.
                ui.Dismiss();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"HowToBuildAPCHandler.DismissPopup: {ex.Message}");
            }
        }

        #endregion

        #region Patches

        /// <summary>
        /// Announces tutorial popup title and body when the tutorial UI is shown.
        /// Fires on every TutorialUI.Init call (career tutorials and HBPC step messages).
        /// </summary>
        [HarmonyPatch(typeof(TutorialUI), "Init")]
        static class TutorialUI_Init_Patch
        {
            static void Postfix(TutorialUI __instance)
            {
                try
                {
                    _cachedUI      = __instance;
                    _popupActive   = true;
                    _activatedTime = Time.realtimeSinceStartup;
                    _inputReady    = false;
                    InputRouter.Push(_ctx);

                    string title = __instance.m_title.text;
                    string body  = __instance.m_body.text;
                    DebugLogger.LogState($"TutorialUI.Init: title='{title}'");
                    ScreenReader.Say(title + ". " + body);

                    // Auto-dismiss after 3 s — user is blind, popup has no visual value.
                    // Enter/Space also works immediately (handled via IInputContext).
                    __instance.StartCoroutine(AutoDismissAfterDelay(__instance, 3f));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TutorialUI_Init_Patch: {ex.Message}");
                }
            }
        }

        private static System.Collections.IEnumerator AutoDismissAfterDelay(TutorialUI ui, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (_popupActive && ui != null && ui.gameObject.activeSelf)
            {
                DebugLogger.LogState("HowToBuildAPCHandler: auto-dismissing popup");
                DismissPopup();
            }
        }

        /// <summary>
        /// Announces a new HBPC checklist section (e.g. "Install RAM") when the title is set.
        /// </summary>
        [HarmonyPatch(typeof(HBPCCheckList), "SetTitleAndClear")]
        static class HBPCCheckList_SetTitleAndClear_Patch
        {
            static void Postfix(string title)
            {
                try
                {
                    DebugLogger.LogState($"HBPC section: '{title}'");
                    ScreenReader.Say(title);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"HBPCCheckList_SetTitleAndClear_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Announces a checklist item when it becomes the current active task.
        /// Ignores INCOMPLETE (fires on spawn) and COMPLETE states.
        /// </summary>
        [HarmonyPatch(typeof(HBPCCheckListItem), "SetState")]
        static class HBPCCheckListItem_SetState_Patch
        {
            static void Postfix(HBPCCheckListItem __instance, HBPCCheckListItem.State state)
            {
                if (state != HBPCCheckListItem.State.CURRENT) return;

                try
                {
                    string task = __instance.m_step?.text;
                    if (string.IsNullOrEmpty(task)) return;

                    DebugLogger.LogState($"HBPC task current: '{task}'");
                    SetLastTask(task);
                    ScreenReader.Say(task);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"HBPCCheckListItem_SetState_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
