using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces which OS window is brought to the front.
    /// Fires when the player clicks the taskbar or an OS icon to switch apps.
    ///
    /// Deduplication: skips re-announcing the same window within 1 second
    /// to prevent double-announce with OSHandler's "Launching" message.
    /// </summary>
    public static class WindowFocusHandler
    {
        private static string _lastWindowId = null;
        private static float  _lastAnnounceTime = -10f;
        private const float   c_cooldown = 1.0f;

        public static void Reset()
        {
            _lastWindowId      = null;
            _lastAnnounceTime  = -10f;
        }

        [HarmonyPatch(typeof(OS), "BringToFront")]
        static class OS_BringToFront_Patch
        {
            static void Postfix(WindowFrame wf)
            {
                try
                {
                    if (wf == null) return;

                    OSProgramDesc desc = wf.GetProgramDesc();
                    if (desc == null) return;

                    string id   = desc.m_id;
                    string name = desc.m_uiName;
                    if (string.IsNullOrEmpty(name)) return;

                    // Same window within cooldown — skip (launch message already said the name)
                    float now = Time.realtimeSinceStartup;
                    if (id == _lastWindowId && now - _lastAnnounceTime < c_cooldown) return;

                    _lastWindowId     = id;
                    _lastAnnounceTime = now;

                    ScreenReader.Say(Loc.Get("window_focused", name));
                    DebugLogger.LogState($"WindowFocusHandler: '{name}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WindowFocusHandler.BringToFront: {ex.Message}");
                }
            }
        }
    }
}
