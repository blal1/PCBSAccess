# Code Index for EmailAppHandler.cs

- Line 7: /// <summary>
- Line 8: /// Announces the tablet EmailApp — email rows and content pane.
- Line 9: ///
- Line 10: /// Patches EmailApp.OnClickRow postfix: announces from, subject, date, body.
- Line 11: /// Up/Down/Home/End navigation through inbox rows when the app is active.
- Line 12: /// </summary>
- Line 13: public static class EmailAppHandler
- Line 25: private sealed class Context : IInputContext, IHelpContext
- Line 30: public bool HandleInput()
- Line 50: public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_email")); }
- Line 59: /// <summary>Re-reads the currently selected email. Called from Main on E.</summary>
- Line 60: public static void AnnounceCurrentEmail()
- Line 70: /// <summary>Resets on scene change.</summary>
- Line 71: public static void Reset()
- Line 83: private static void SelectRow(int index)
- Line 93: /// <summary>Fires when any email row is selected. Announces email content.</summary>
- Line 95: static class EmailApp_OnClickRow_Patch
- Line 97: static void Postfix(EmailApp __instance, EmailRowBase row)
