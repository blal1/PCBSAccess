using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces message box dialogs when they appear and provides keyboard shortcuts.
    ///
    /// Enter:  confirms (Yes / OK).
    /// Escape: cancels  (No)  — only in two-button mode.
    /// Announces title + body + hint on show.
    ///
    /// Open/close detection via polling activeSelf (PollState).
    /// Key handling via IInputContext (Context.HandleInput).
    /// </summary>
    public static class MessageBoxHandler
    {
        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static bool _twoButtons;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "MessageBox";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    Confirm();
                    return true;
                }
                if (_twoButtons && Input.GetKeyDown(KeyCode.Escape))
                {
                    Cancel();
                    return true;
                }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_messagebox")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>
        /// Polls for close. Called from Main.Update each frame (replaces former Update()).
        /// No key handling here — that is in Context.HandleInput().
        /// </summary>
        public static void PollState()
        {
            bool isOpen = IsOpen();
            if (!isOpen && _wasOpen) OnClose();
            _wasOpen = isOpen;
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _twoButtons = false;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Open / Close

        private static bool IsOpen()
        {
            try
            {
                return CommonUI.messageBox != null && CommonUI.messageBox.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void OnClose()
        {
            _active     = false;
            _twoButtons = false;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Activate

        private static void Confirm()
        {
            _active = false;        // prevent double-fire before activeSelf resets
            InputRouter.Pop(_ctx);
            try { CommonUI.messageBox?.OnYes(); }
            catch (Exception ex) { DebugLogger.LogWarning($"MessageBoxHandler.Confirm: {ex.Message}"); }
        }

        private static void Cancel()
        {
            _active = false;        // prevent double-fire before activeSelf resets
            InputRouter.Pop(_ctx);
            try { CommonUI.messageBox?.OnNo(); }
            catch (Exception ex) { DebugLogger.LogWarning($"MessageBoxHandler.Cancel: {ex.Message}"); }
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires after MessageBox.Show sets title/body.
        /// Show is static — no __instance parameter needed.
        /// Parameter names must match the original method's parameter names for Harmony injection.
        /// </summary>
        [HarmonyPatch(typeof(MessageBox), "Show")]
        static class MessageBox_Show_Patch
        {
            static void Postfix(string title, string body, bool twoButtons)
            {
                try
                {
                    _active     = true;
                    _twoButtons = twoButtons;
                    InputRouter.Push(_ctx);

                    string hint = twoButtons
                        ? Loc.Get("msgbox_hint_yesno")
                        : Loc.Get("msgbox_hint_ok");

                    ScreenReader.Say($"{title}. {body} {hint}");
                    DebugLogger.LogState($"MessageBoxHandler: '{title}', twoButtons={twoButtons}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MessageBox_Show_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
