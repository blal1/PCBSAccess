using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FuturLab;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the Mobile Phone Messenger app (Esports DLC).
    ///
    /// When a conversation is opened: reads the contact name and announces
    /// the last few messages so the player knows what was said.
    ///
    /// New message notifications are already surfaced via the badge count
    /// on the phone notification widget — no extra polling needed here.
    /// </summary>
    public static class MessengerHandler
    {
        #region Reflection

        private static readonly FieldInfo _fSingleConversation =
            typeof(MobilePhoneAppMessenger).GetField("m_singleConversation",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fConversationList =
            typeof(MobilePhoneAppMessenger).GetField("m_conversationList",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fAllConversationItems =
            typeof(MobilePhoneAppMessengerSingleConversation).GetField("m_allConversationItems",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fMessageList =
            typeof(MobilePhoneAppMessengerSingleConversation).GetField("m_messageList",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fAllConversationListEntries =
            typeof(MobilePhoneAppMessengerConversationList).GetField("m_allConversationListEntries",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fParticipantsLabel =
            typeof(MobilePhoneAppMessengerConversationListEntry).GetField("m_participantsLabel",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fSubjectLabel =
            typeof(MobilePhoneAppMessengerConversationListEntry).GetField("m_subjectLabel",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fNumUnreadLabel =
            typeof(MobilePhoneAppMessengerConversationListEntry).GetField("m_numUnreadLabel",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fOtherPersonMessage =
            typeof(MobilePhoneAppMessengerSingleConversationItem).GetField("m_otherPersonMessage",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fOtherPersonName =
            typeof(MobilePhoneAppMessengerSingleConversationItem).GetField("m_otherPersonName",
                BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region Patches

        // Fires when the player opens a conversation from the list.
        [HarmonyPatch(typeof(MobilePhoneAppMessengerSingleConversation), "SetConversationID")]
        static class SetConversationID_Patch
        {
            static void Postfix(MobilePhoneAppMessengerSingleConversation __instance)
            {
                try
                {
                    var msgList = _fMessageList?.GetValue(__instance)
                        as MobilePhoneConversationManager.MessageList;

                    string threadTitle = msgList?.m_threadTitle ?? Loc.Get("messenger_unknown");

                    ScreenReader.Say(Loc.Get("messenger_opened", threadTitle));
                    AnnounceRecentMessages(__instance);

                    DebugLogger.LogState($"MessengerHandler: opened '{threadTitle}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MessengerHandler.SetConversationID: {ex.Message}");
                }
            }
        }

        // Fires when the player presses Back and returns to the conversation list.
        [HarmonyPatch(typeof(MobilePhoneAppMessenger), "OnBackButtonPressed")]
        static class OnBackButtonPressed_Patch
        {
            static void Postfix(MobilePhoneAppMessenger __instance)
            {
                try
                {
                    var convList = _fConversationList?.GetValue(__instance)
                        as MobilePhoneAppMessengerConversationList;
                    if (convList == null) return;

                    var entries = _fAllConversationListEntries?.GetValue(convList)
                        as List<MobilePhoneAppMessengerConversationListEntry>;
                    int count = entries?.Count ?? 0;

                    int unread = 0;
                    if (entries != null)
                    {
                        foreach (var entry in entries)
                        {
                            string unreadStr = (_fNumUnreadLabel?.GetValue(entry) as Text)?.text ?? "0";
                            if (int.TryParse(unreadStr, out int n) && n > 0) unread += n;
                        }
                    }

                    string msg = unread > 0
                        ? Loc.Get("messenger_list_unread", count, unread)
                        : Loc.Get("messenger_list", count);
                    ScreenReader.Say(msg);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MessengerHandler.OnBackButtonPressed: {ex.Message}");
                }
            }
        }

        // Fires when a new message arrives mid-conversation.
        [HarmonyPatch(typeof(MobilePhoneAppMessengerSingleConversation), "Tick")]
        static class SingleConversationTick_Patch
        {
            private static int _lastCount;

            static void Postfix(MobilePhoneAppMessengerSingleConversation __instance)
            {
                try
                {
                    if (!__instance.gameObject.activeSelf) return;

                    var items = _fAllConversationItems?.GetValue(__instance)
                        as List<MobilePhoneAppMessengerSingleConversationItem>;
                    if (items == null) return;

                    int count = items.Count;
                    if (count > _lastCount && _lastCount > 0)
                    {
                        // New message arrived — announce it.
                        var newest = items[count - 1];
                        string text = (_fOtherPersonMessage?.GetValue(newest) as Text)?.text
                                   ?? string.Empty;
                        string name = (_fOtherPersonName?.GetValue(newest)  as Text)?.text
                                   ?? string.Empty;
                        if (!string.IsNullOrEmpty(text))
                        {
                            string msg = string.IsNullOrEmpty(name)
                                ? Loc.Get("messenger_new_msg", text)
                                : Loc.Get("messenger_new_msg_named", name, text);
                            ScreenReader.Say(msg);
                        }
                    }
                    _lastCount = count;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MessengerHandler.Tick: {ex.Message}");
                }
            }
        }

        #endregion

        #region Helpers

        private static void AnnounceRecentMessages(MobilePhoneAppMessengerSingleConversation conv)
        {
            try
            {
                var items = _fAllConversationItems?.GetValue(conv)
                    as List<MobilePhoneAppMessengerSingleConversationItem>;
                if (items == null || items.Count == 0)
                {
                    ScreenReader.Say(Loc.Get("messenger_empty"), interrupt: false);
                    return;
                }

                // Read last 5 text messages, oldest first.
                int start = Math.Max(0, items.Count - 5);
                var sb = new StringBuilder();
                for (int i = start; i < items.Count; i++)
                {
                    var item = items[i];
                    string name = (_fOtherPersonName?.GetValue(item)    as Text)?.text ?? string.Empty;
                    string text = (_fOtherPersonMessage?.GetValue(item) as Text)?.text ?? string.Empty;
                    if (string.IsNullOrEmpty(text)) continue;

                    if (sb.Length > 0) sb.Append(" ");
                    if (!string.IsNullOrEmpty(name))
                        sb.Append($"{name}: {text}.");
                    else
                        sb.Append($"{text}.");
                }

                if (sb.Length > 0)
                    ScreenReader.Say(sb.ToString(), interrupt: false);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"MessengerHandler.AnnounceRecentMessages: {ex.Message}");
            }
        }

        #endregion
    }
}
