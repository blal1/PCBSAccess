using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the "What's New" popup when it appears.
    /// Reads title and body text via reflection and announces them.
    /// Enter dismisses the popup.
    /// </summary>
    public static class WhatsNewPopUpHandler
    {
        private static bool _active;
        private static bool _wasOpen;
        private static WhatsNewPopUp _instance;

        // PlatformSpecificUI is a nested struct — access fields by name.
        private static readonly FieldInfo _fActiveUI = typeof(WhatsNewPopUp)
            .GetField("m_activeUI", BindingFlags.NonPublic | BindingFlags.Instance);

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "WhatsNew";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
                {
                    _instance?.Close();
                    return true;
                }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("whatsnew_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        [HarmonyPatch(typeof(WhatsNewPopUp), "OnEnable")]
        static class WhatsNewPopUp_OnEnable_Patch
        {
            static void Postfix(WhatsNewPopUp __instance)
            {
                try
                {
                    _instance = __instance;
                    _active   = true;
                    _wasOpen  = true;
                    InputRouter.Push(_ctx);

                    string title = GetTitle(__instance);
                    string body  = GetBody(__instance);

                    string msg = string.IsNullOrEmpty(body)
                        ? title
                        : $"{title}. {body}";

                    ScreenReader.Say(msg);
                    ScreenReader.Say(Loc.Get("whatsnew_dismiss_hint"), interrupt: false);
                    DebugLogger.LogState("WhatsNewPopUpHandler: shown");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WhatsNewPopUpHandler.OnEnable: {ex.Message}");
                }
            }
        }

        public static void PollState()
        {
            if (!_wasOpen) return;
            bool open = _instance != null && _instance.gameObject.activeSelf;
            if (!open && _wasOpen) OnClose();
            _wasOpen = open;
        }

        private static void OnClose()
        {
            _active   = false;
            _wasOpen  = false;
            _instance = null;
            InputRouter.Pop(_ctx);
        }

        public static void Reset()
        {
            _active   = false;
            _wasOpen  = false;
            _instance = null;
            InputRouter.Pop(_ctx);
        }

        private static string GetTitle(WhatsNewPopUp popup)
        {
            try
            {
                object activeUI = _fActiveUI?.GetValue(popup);
                if (activeUI == null) return string.Empty;
                FieldInfo titleField = activeUI.GetType().GetField("title");
                Text titleText = titleField?.GetValue(activeUI) as Text;
                return titleText?.text ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private static string GetBody(WhatsNewPopUp popup)
        {
            try
            {
                object activeUI = _fActiveUI?.GetValue(popup);
                if (activeUI == null) return string.Empty;
                FieldInfo bodyField = activeUI.GetType().GetField("body");
                TMP_Text bodyText = bodyField?.GetValue(activeUI) as TMP_Text;
                if (bodyText == null) return string.Empty;
                // Strip rich text / hyperlink tags for clean reading.
                return System.Text.RegularExpressions.Regex
                    .Replace(bodyText.text, "<[^>]+>", string.Empty)
                    .Replace("\n\n", ". ")
                    .Replace("\n", " ")
                    .Trim();
            }
            catch { return string.Empty; }
        }
    }
}
