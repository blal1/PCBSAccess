using System;
using System.Reflection;
using System.Text;
using FuturLab;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the LikedIn team-selection screen (Esports DLC).
    ///
    /// On open: reads current league likes, total likes, and the full list of
    /// available teams with their names, likes required, and team messages.
    ///
    /// This is the screen where the player picks a team to support at the end
    /// of an Esports league — a key story decision.
    /// </summary>
    public static class LikedInHandler
    {
        private static readonly FieldInfo _fLeagueLikes =
            typeof(MobilePhoneLikedIn).GetField("m_leagueLikes",
                BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo _fTotalLikes =
            typeof(MobilePhoneLikedIn).GetField("m_totalLikes",
                BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo _fTeamEntries =
            typeof(MobilePhoneLikedIn).GetField("m_teamEntries",
                BindingFlags.Public | BindingFlags.Instance);

        private static readonly FieldInfo _fTeamName =
            typeof(MobilePhoneLikedInTeamEntry).GetField("m_teamName",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fLikesRequired =
            typeof(MobilePhoneLikedInTeamEntry).GetField("m_likesRequired",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fTeamMessage =
            typeof(MobilePhoneLikedInTeamEntry).GetField("m_teamMessage",
                BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(MobilePhoneLikedIn), "OnStartShowing")]
        static class MobilePhoneLikedIn_OnStartShowing_Patch
        {
            static void Postfix(MobilePhoneLikedIn __instance)
            {
                try
                {
                    string leagueLikes = (__instance.m_leagueLikes?.text) ?? string.Empty;
                    string totalLikes  = (__instance.m_totalLikes?.text)  ?? string.Empty;

                    MobilePhoneLikedInTeamEntry[] entries =
                        __instance.m_teamEntries;

                    int active = 0;
                    if (entries != null)
                        foreach (var e in entries)
                            if (e.gameObject.activeSelf) active++;

                    ScreenReader.Say(Loc.Get("likedin_open", leagueLikes, totalLikes, active));

                    if (entries != null)
                    {
                        int idx = 0;
                        foreach (MobilePhoneLikedInTeamEntry entry in entries)
                        {
                            if (!entry.gameObject.activeSelf) continue;
                            idx++;

                            string name    = (_fTeamName    ?.GetValue(entry) as Text)?.text ?? string.Empty;
                            string likes   = (_fLikesRequired?.GetValue(entry) as Text)?.text ?? string.Empty;
                            string message = (_fTeamMessage  ?.GetValue(entry) as Text)?.text ?? string.Empty;

                            string line = Loc.Get("likedin_team", idx, active, name, likes);
                            ScreenReader.Say(line, interrupt: false);
                            if (!string.IsNullOrEmpty(message))
                                ScreenReader.Say(message, interrupt: false);
                        }
                    }

                    DebugLogger.LogState($"LikedInHandler: {active} teams shown");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"LikedInHandler.OnStartShowing: {ex.Message}");
                }
            }
        }

        // Fires when a team is chosen and the confirmation animation plays.
        [HarmonyPatch(typeof(MobilePhoneLikedIn), "TeamHasBeenChosen")]
        static class MobilePhoneLikedIn_TeamHasBeenChosen_Patch
        {
            static void Postfix(EsportsTeamDesc _team)
            {
                try
                {
                    if (_team == null) return;
                    ScreenReader.Say(Loc.Get("likedin_chosen", _team.GetLocalisedName()));
                    DebugLogger.LogState($"LikedInHandler: chose '{_team.GetLocalisedName()}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"LikedInHandler.TeamHasBeenChosen: {ex.Message}");
                }
            }
        }
    }
}
