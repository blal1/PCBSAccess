using System.Collections.Generic;
using HarmonyLib;
using MailConfig;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for email client accessibility.
    /// Announces inbox load, email selection, deletion, and panel changes.
    /// </summary>

    /// <summary>
    /// Patches MailWindow.ResumeLogin to announce inbox loaded.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "ResumeLogin")]
    public class MailResumeLoginPatch
    {
        static void Postfix(MailWindow __instance, UserMail userMail)
        {
            DebugLogger.LogState($"MailWindow.ResumeLogin: {userMail?.emails?.Count ?? 0} emails");
            MailHandler.OnInboxLoaded(__instance);
        }
    }

    /// <summary>
    /// Patches MailWindow.OnClickMail(int, bool) to announce email opened.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "OnClickMail", new[] { typeof(int), typeof(bool) })]
    public class MailOnClickMailPatch
    {
        static void Postfix(MailWindow __instance, int indexMail)
        {
            DebugLogger.LogState($"MailWindow.OnClickMail: index={indexMail}");
            MailHandler.OnEmailSelected(__instance);
        }
    }

    /// <summary>
    /// Patches MailWindow.ResumeDeleteMail to announce email deleted.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "ResumeDeleteMail")]
    public class MailResumeDeletePatch
    {
        static void Postfix(MailWindow __instance)
        {
            DebugLogger.LogState("MailWindow.ResumeDeleteMail");
            MailHandler.OnEmailDeleted(__instance);
        }
    }

    /// <summary>
    /// Patches MailWindow.OnShowPanelRedactar to announce compose panel.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "OnShowPanelRedactar")]
    public class MailShowComposePatch
    {
        static void Postfix(MailWindow __instance)
        {
            DebugLogger.LogState("MailWindow.OnShowPanelRedactar");
            MailHandler.OnComposeOpened();
        }
    }

    /// <summary>
    /// Patches MailWindow.ShowPanelPrincipal to announce return to inbox.
    /// </summary>
    [HarmonyPatch(typeof(MailWindow), "ShowPanelPrincipal")]
    public class MailShowPrincipalPatch
    {
        static void Postfix(MailWindow __instance)
        {
            DebugLogger.LogState("MailWindow.ShowPanelPrincipal");
            MailHandler.OnReturnToInbox(__instance);
        }
    }
}
