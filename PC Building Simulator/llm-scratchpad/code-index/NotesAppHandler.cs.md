# Code Index for NotesAppHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces the Notes app when it opens: note count.
- Line 8: ///
- Line 9: /// Patches NotesApp.Start Postfix to read note count from NotesAppCloud.
- Line 10: /// </summary>
- Line 11: public static class NotesAppHandler
- Line 14: static class NotesApp_Start_Patch
- Line 16: static void Postfix(NotesApp __instance)
