# Code Index for AchievementHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces when an achievement is newly unlocked.
- Line 8: ///
- Line 9: /// Patches Achievements.SetProgress (private static) using Prefix+Postfix
- Line 10: /// to detect the first-time completed transition.
- Line 11: /// </summary>
- Line 12: public static class AchievementHandler
- Line 14: private static string FormatId(string id)
- Line 23: static class Achievements_SetProgress_Patch
- Line 25: static void Prefix(AchievementDesc ach, ref bool __state)
- Line 30: static void Postfix(AchievementDesc ach, bool __state)
