# Code Index for ThreedMarkHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces 3DMark benchmark results.
- Line 8: ///
- Line 9: /// OnRun patch: "Benchmark started."
- Line 10: /// CalcResults patch: reads CPU/GPU/overall scores + hardware names when results appear.
- Line 11: /// F9 (wired in Main, context-aware): re-reads last score summary.
- Line 12: /// </summary>
- Line 13: public static class ThreedMarkHandler
- Line 23: public static void AnnounceLastResult()
- Line 31: public static void Reset()
- Line 41: static class ThreedMarkApp_OnRun_Patch
- Line 43: static void Postfix()
- Line 57: /// <summary>
- Line 58: /// Fires after CalcResults() — all score Text fields are populated at this point.
- Line 59: /// CalcResults() is called before SetPage(m_resultsPage), so values are ready.
- Line 60: /// </summary>
- Line 62: static class ThreedMarkApp_CalcResults_Patch
- Line 64: static void Postfix(ThreedMarkApp __instance)
