using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for chat accessibility.
    /// Announces new messages, channel switches, and nickname registration.
    /// </summary>

    /// <summary>
    /// Patches ChatGuild.RecibeMensaje to announce new messages.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "RecibeMensaje")]
    public class ChatRecibeMensajePatch
    {
        static void Postfix(ChatGuild __instance, PlayerUtilsChat.ChatMessage message, bool __result)
        {
            if (__result)
            {
                DebugLogger.LogState($"ChatGuild.RecibeMensaje: {message.nickName}: {message.message?.Substring(0, System.Math.Min(50, message.message?.Length ?? 0))}");
                ChatHandler.OnMessageReceived(message);
            }
        }
    }

    /// <summary>
    /// Patches ChatGuild.RecibeMensajePrivado to announce private messages.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "RecibeMensajePrivado")]
    public class ChatRecibeMensajePrivadoPatch
    {
        static void Postfix(ChatGuild __instance, PlayerUtilsChat.ChatMessage message, string otherNickname, bool __result)
        {
            if (__result)
            {
                DebugLogger.LogState($"ChatGuild.RecibeMensajePrivado: from {otherNickname}");
                ChatHandler.OnPrivateMessageReceived(message, otherNickname);
            }
        }
    }

    /// <summary>
    /// Patches ChatGuild.OnSelectTab to announce channel switch.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "OnSelectTab")]
    public class ChatOnSelectTabPatch
    {
        static void Postfix(ChatGuild __instance, ChatGuild.NetworkChannelInfo channel)
        {
            DebugLogger.LogState($"ChatGuild.OnSelectTab: {channel?.channelName}");
            ChatHandler.OnChannelSwitched(channel);
        }
    }

    /// <summary>
    /// Patches ChatGuild.ResumeConnectionWindow(bool) to announce nickname registered.
    /// </summary>
    [HarmonyPatch(typeof(ChatGuild), "ResumeConnectionWindow", new[] { typeof(bool) })]
    public class ChatResumeConnectionPatch
    {
        static void Postfix(bool success)
        {
            if (success)
            {
                DebugLogger.LogState("ChatGuild: nickname registered");
                ChatHandler.OnNicknameRegistered();
            }
        }
    }
}
