using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces virus scan state changes (Standard → Scanning → Dirty/Clean).
    /// Also provides keyboard interaction: Enter activates the scan button when it
    /// is interactable, so the player can start or dismiss the scan without a mouse.
    ///
    /// State polling detects open/close. IInputContext handles key capture.
    /// </summary>
    public static class VirusScanAppHandler
    {
        #region State

        private static string       _lastTitle;
        private static bool         _active;
        private static bool         _wasOpen;
        private static VirusScanApp _app;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "VirusScan";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.Return)) { ActivateButton(); return true; }
                if (Input.GetKeyDown(KeyCode.F1))     { ReRead();         return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("virusscan_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        public static void PollState()
        {
            bool open = IsOpen();
            if (open && !_wasOpen)  OnOpen();
            if (!open && _wasOpen)  OnClose();
            _wasOpen = open;
        }

        public static void Reset()
        {
            _lastTitle = null;
            _active    = false;
            _wasOpen   = false;
            _app       = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Open / Close

        private static bool IsOpen()
        {
            try
            {
                if (_app != null) return _app.gameObject.activeSelf;
                _app = UnityEngine.Object.FindObjectOfType<VirusScanApp>();
                return _app != null && _app.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void OnOpen()
        {
            _active    = true;
            _lastTitle = null;
            InputRouter.Push(_ctx);
            DebugLogger.LogState("VirusScanAppHandler: opened");
        }

        private static void OnClose()
        {
            _active    = false;
            _lastTitle = null;
            _app       = null;
            InputRouter.Pop(_ctx);
            DebugLogger.LogState("VirusScanAppHandler: closed");
        }

        #endregion

        #region Actions

        private static void ActivateButton()
        {
            if (_app == null) return;
            if (_app.m_button != null && _app.m_button.IsInteractable())
            {
                _app.m_button.onClick.Invoke();
                DebugLogger.LogInput("Enter", "VirusScanButton");
            }
        }

        private static void ReRead()
        {
            if (_app == null) return;
            string title   = _app.m_title?.text ?? string.Empty;
            string message = _app.m_message?.text ?? string.Empty;
            string text    = string.IsNullOrEmpty(message) ? title : $"{title}. {message}";
            ScreenReader.Say(text);
        }

        #endregion

        #region Patch — state change announcements

        /// <summary>
        /// Patches the private SetVisuals(State) method. Fires on every state change.
        /// Also captures the app reference if we don't have it yet.
        /// </summary>
        [HarmonyPatch(typeof(VirusScanApp), "SetVisuals")]
        static class VirusScanApp_SetVisuals_Patch
        {
            static void Postfix(VirusScanApp __instance)
            {
                try
                {
                    if (_app == null) _app = __instance;

                    string title   = __instance.m_title.text;
                    string message = __instance.m_message.text;

                    if (title == _lastTitle) return;
                    _lastTitle = title;

                    string text = string.IsNullOrEmpty(message)
                        ? title
                        : $"{title}. {message}";

                    ScreenReader.Say(text);
                    DebugLogger.LogState($"VirusScanAppHandler: '{title}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"VirusScanApp_SetVisuals_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
