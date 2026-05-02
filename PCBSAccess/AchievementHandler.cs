using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces when an achievement is newly unlocked.
    ///
    /// Patches Achievements.SetProgress (private static) using Prefix+Postfix
    /// to detect the first-time completed transition.
    /// </summary>
    public static class AchievementHandler
    {
        private static string FormatId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            string spaced = id.Replace("_", " ").ToLower();
            if (spaced.Length == 0) return spaced;
            return char.ToUpper(spaced[0]) + spaced.Substring(1);
        }

        [HarmonyPatch(typeof(Achievements), "SetProgress")]
        static class Achievements_SetProgress_Patch
        {
            static void Prefix(AchievementDesc ach, ref bool __state)
            {
                __state = Achievements.IsCompleted(ach.m_id);
            }

            static void Postfix(AchievementDesc ach, bool __state)
            {
                if (__state) return;
                if (!Achievements.IsCompleted(ach.m_id)) return;
                try
                {
                    string name = FormatId(ach.m_id);
                    ScreenReader.Say(Loc.Get("achievement_unlocked", name));
                    DebugLogger.LogState($"AchievementHandler: unlocked '{ach.m_id}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"AchievementHandler.Postfix: {ex.Message}");
                }
            }
        }
    }
}
