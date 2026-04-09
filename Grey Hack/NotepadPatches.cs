using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for notepad accessibility.
    /// Announces file loaded events.
    /// </summary>

    /// <summary>
    /// Patches Notepad.ResumeConnectionWindow(string) to announce file loaded.
    /// </summary>
    [HarmonyPatch(typeof(Notepad), "ResumeConnectionWindow", new[] { typeof(string) })]
    public class NotepadResumeConnectionPatch
    {
        static void Postfix(Notepad __instance, string info)
        {
            DebugLogger.LogState($"Notepad.ResumeConnectionWindow: info length={info?.Length ?? 0}");
            NotepadHandler.OnFileLoaded(__instance);
        }
    }
}
