using System.Text.RegularExpressions;
using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for tutorial window accessibility.
    /// Intercepts page builds and wrong action messages.
    /// </summary>

    /// <summary>
    /// Patches AdvancedTutorial.ConstruyePagina to announce new page content.
    /// ConstruyePagina is private, so we use HarmonyPatch with method name string.
    /// </summary>
    [HarmonyPatch(typeof(AdvancedTutorial), "ConstruyePagina")]
    public class TutorialPageBuiltPatch
    {
        static void Postfix(AdvancedTutorial __instance)
        {
            DebugLogger.LogState("AdvancedTutorial.ConstruyePagina postfix fired");
            TutorialHandler.OnPageBuilt(__instance);
        }
    }

    /// <summary>
    /// Patches AdvancedTutorial.ShowWrongAction to announce error feedback.
    /// ShowWrongAction is private, takes a string parameter.
    /// </summary>
    [HarmonyPatch(typeof(AdvancedTutorial), "ShowWrongAction")]
    public class TutorialWrongActionPatch
    {
        static void Postfix(string text)
        {
            string clean = Regex.Replace(text ?? "", "<.*?>", "");
            DebugLogger.LogState($"AdvancedTutorial.ShowWrongAction: '{clean}'");
            TutorialHandler.OnWrongAction(text);
        }
    }
}
