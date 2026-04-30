# Code Index for WillItRunHandler.cs

- Line 7: /// <summary>
- Line 8: /// Announces results from the "Will It Run" tablet app.
- Line 9: ///
- Line 10: /// Patches WillItRunApp.ShowIndividualResult — announces each category
- Line 11: /// (CPU, GPU, RAM, VRAM, Storage) with needed/got values and pass/fail.
- Line 12: /// </summary>
- Line 13: public static class WillItRunHandler
- Line 18: static class WillItRunApp_ShowIndividualResult_Patch
- Line 20: static void Postfix(WillItRunApp __instance, ProgramRequirementsDesc desc)
