# Code Index for JobResultHandler.cs

- Line 7: /// <summary>
- Line 8: /// Announces the job result screen when a job is submitted.
- Line 9: ///
- Line 10: /// Fires on JobResult.Init(Job) postfix.
- Line 11: /// Announces: job title, success/fail, labour, total payout, star rating, review.
- Line 12: /// Enter activates the OK button to dismiss.
- Line 13: /// </summary>
- Line 14: public static class JobResultHandler
- Line 24: private sealed class Context : IInputContext, IHelpContext
- Line 29: public bool HandleInput()
- Line 34: public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_jobresult")); }
- Line 43: /// <summary>Resets on scene change.</summary>
- Line 44: public static void Reset()
- Line 54: private static bool IsOpen()
- Line 64: private static void ActivateOK()
- Line 77: static class JobResult_Init_Patch
- Line 79: static void Postfix(JobResult __instance, Job job)
