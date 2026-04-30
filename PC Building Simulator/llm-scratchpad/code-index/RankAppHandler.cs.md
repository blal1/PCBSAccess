# Code Index for RankAppHandler.cs

- Line 9: /// <summary>
- Line 10: /// Announces the Part Rankings app.
- Line 11: /// Up/Down navigate the result list; Numpad6 re-reads current entry.
- Line 12: /// Patches RankApp.RefreshList (list built) and SelectResult (item focused by game).
- Line 13: /// </summary>
- Line 14: public static class RankAppHandler
- Line 29: private sealed class Context : IInputContext, IHelpContext
- Line 34: public bool HandleInput()
- Line 44: public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_rank")); }
- Line 53: public static void Reset()
- Line 61: /// <summary>Re-reads current entry. Called from Main on Numpad6.</summary>
- Line 62: public static void AnnounceCurrentEntry()
- Line 74: private static void Navigate(int dir)
- Line 88: private static void NavigateTo(int index)
- Line 96: private static void InvokeSelectResult(int index)
- Line 109: private static void AnnounceRow(RankAppRow row, int pos, int total)
- Line 124: static class RankApp_RefreshList_Patch
- Line 126: static void Postfix(RankApp __instance)
