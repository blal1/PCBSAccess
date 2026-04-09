using System.Collections.Generic;
using ChatPoolSystem;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for chat window accessibility.
    /// Auto-activates when ChatGuild window is focused.
    /// Ctrl+Up/Down for message history, Alt+Left/Right for channel tabs,
    /// Alt+U for user list, Alt+C for channel list.
    /// </summary>
    public class ChatHandler
    {
        #region Fields

        private static ChatHandler _instance;
        private ChatGuild _activeChat;
        private uDialog _activeDialog;
        private bool _isActive;
        private int _historyIndex = -1;
        private bool _isNicknamePanel;

        #endregion

        #region Static Entry

        /// <summary>Registers this handler instance.</summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>Called by patch when a message is received.</summary>
        public static void OnMessageReceived(PlayerUtilsChat.ChatMessage message)
        {
            if (_instance == null) return;
            if (!ModConfig.AnnounceChatMessages) return;

            string nick = StripRichText(message.nickName ?? "");
            string msg = StripRichText(message.message ?? "");
            ScreenReader.Say(Loc.Get("chat_message", nick, msg));
        }

        /// <summary>Called by patch when a private message is received.</summary>
        public static void OnPrivateMessageReceived(PlayerUtilsChat.ChatMessage message, string otherNickname)
        {
            if (_instance == null) return;
            if (!ModConfig.AnnounceChatMessages) return;

            string nick = StripRichText(otherNickname ?? message.nickName ?? "");
            string msg = StripRichText(message.message ?? "");
            ScreenReader.Say(Loc.Get("chat_private_message", nick, msg));
        }

        /// <summary>Called by patch when channel tab is switched.</summary>
        public static void OnChannelSwitched(ChatGuild.NetworkChannelInfo channel)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance._historyIndex = -1;
            string name = channel?.channelName ?? "unknown";
            ScreenReader.Say(Loc.Get("chat_channel_switched", name));
        }

        /// <summary>Called by patch when nickname registration succeeds.</summary>
        public static void OnNicknameRegistered()
        {
            if (_instance == null) return;
            _instance._isNicknamePanel = false;
            ScreenReader.Say(Loc.Get("chat_nickname_registered"));
        }

        #endregion

        #region Public Properties

        /// <summary>Whether chat handler is currently active.</summary>
        public bool IsActive => _isActive;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update().
        /// Returns true if input was consumed.
        /// </summary>
        public bool Update()
        {
            CheckFocusedWindow();

            if (!_isActive) return false;

            if (_activeChat == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            return HandleInput();
        }

        /// <summary>Returns help text for F1.</summary>
        public string GetHelpText()
        {
            if (_isNicknamePanel)
            {
                return Loc.Get("chat_nickname");
            }
            return Loc.Get("chat_help");
        }

        #endregion

        #region Focus Detection

        private void CheckFocusedWindow()
        {
            var taskbar = Object.FindObjectOfType<uDialog_TaskBar>();
            if (taskbar == null)
            {
                if (_isActive) Deactivate();
                return;
            }

            uDialog currentTask = taskbar.CurrentTask;
            if (currentTask == null)
            {
                if (_isActive) Deactivate();
                return;
            }

            if (currentTask == _activeDialog && _isActive) return;

            ChatGuild chat = currentTask.GetComponentInChildren<ChatGuild>();
            if (chat != null)
            {
                if (!_isActive || chat != _activeChat)
                {
                    Activate(chat, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(ChatGuild chat, uDialog dialog)
        {
            _activeChat = chat;
            _activeDialog = dialog;
            _isActive = true;
            _historyIndex = -1;

            _isNicknamePanel = chat.panelNickName != null && chat.panelNickName.gameObject.activeInHierarchy;

            if (_isNicknamePanel)
            {
                ScreenReader.Say(Loc.Get("chat_nickname"));
            }
            else
            {
                var currentChannel = ReflectionHelper.GetPrivateField<ChatGuild.NetworkChannelInfo>(chat, "currentChannel");
                string channelName = currentChannel?.channelName ?? "general";
                ScreenReader.Say(Loc.Get("chat_focused", channelName));
            }

            DebugLogger.LogState($"ChatHandler: activated, nickname={_isNicknamePanel}");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeChat = null;
            _activeDialog = null;
            _historyIndex = -1;
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            if (_isNicknamePanel) return false;

            // Ctrl+Up = scroll history up (older)
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
                return true;
            }

            // Ctrl+Down = scroll history down (newer)
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
                return true;
            }

            // Alt+Left = previous channel tab
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SwitchChannel(-1);
                return true;
            }

            // Alt+Right = next channel tab
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.RightArrow))
            {
                SwitchChannel(1);
                return true;
            }

            // Alt+U = user list
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.U))
            {
                AnnounceUserList();
                return true;
            }

            // Alt+C = channel list
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.C))
            {
                _activeChat.OnShowChannels();
                return true;
            }

            // Space = repeat channel info (only if input field not focused)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_activeChat.inputField != null && _activeChat.inputField.isFocused)
                {
                    return false;
                }
                var currentChannel = ReflectionHelper.GetPrivateField<ChatGuild.NetworkChannelInfo>(
                    _activeChat, "currentChannel");
                string channelName = currentChannel?.channelName ?? "general";
                ScreenReader.Say(Loc.Get("chat_focused", channelName));
                return true;
            }

            return false;
        }

        #endregion

        #region Message History

        private void NavigateHistory(int direction)
        {
            var currentChannel = ReflectionHelper.GetPrivateField<ChatGuild.NetworkChannelInfo>(
                _activeChat, "currentChannel");
            if (currentChannel == null) return;

            GameObject panelChat = currentChannel.GetPanelChat();
            if (panelChat == null) return;

            ChatListAdapter adapter = panelChat.GetComponentInChildren<ChatListAdapter>();
            if (adapter == null) return;

            int itemCount = adapter.GetItemsCount();
            if (itemCount == 0)
            {
                ScreenReader.Say(Loc.Get("chat_no_messages"));
                return;
            }

            if (_historyIndex < 0)
            {
                _historyIndex = itemCount - 1;
            }
            else
            {
                _historyIndex += direction;
            }

            if (_historyIndex < 0) _historyIndex = 0;
            if (_historyIndex >= itemCount) _historyIndex = itemCount - 1;

            var data = adapter.Data;
            if (data != null && _historyIndex < data.Count)
            {
                string nick = StripRichText(data[_historyIndex].nickname ?? "");
                string msg = StripRichText(data[_historyIndex].message ?? "");
                if (string.IsNullOrEmpty(nick))
                {
                    ScreenReader.Say(msg);
                }
                else
                {
                    ScreenReader.Say(Loc.Get("chat_history_item", nick, msg));
                }
            }
        }

        #endregion

        #region Channel Switching

        private void SwitchChannel(int direction)
        {
            var activeChannels = ReflectionHelper.GetPrivateField<List<ChatGuild.NetworkChannelInfo>>(
                _activeChat, "activeChannels");
            var currentChannel = ReflectionHelper.GetPrivateField<ChatGuild.NetworkChannelInfo>(
                _activeChat, "currentChannel");

            if (activeChannels == null || activeChannels.Count == 0)
            {
                ScreenReader.Say(Loc.Get("chat_no_channels"));
                return;
            }

            int currentIdx = activeChannels.IndexOf(currentChannel);
            int newIdx = currentIdx + direction;

            if (newIdx < 0) newIdx = activeChannels.Count - 1;
            if (newIdx >= activeChannels.Count) newIdx = 0;

            _activeChat.OnSelectTab(activeChannels[newIdx]);
            _historyIndex = -1;
        }

        #endregion

        #region User List

        private void AnnounceUserList()
        {
            var currentChannel = ReflectionHelper.GetPrivateField<ChatGuild.NetworkChannelInfo>(
                _activeChat, "currentChannel");
            if (currentChannel == null) return;

            GameObject panelUsers = currentChannel.GetPanelUsers();
            if (panelUsers == null)
            {
                ScreenReader.Say(Loc.Get("chat_no_users"));
                return;
            }

            UserChatList[] users = panelUsers.GetComponentsInChildren<UserChatList>(true);
            if (users.Length == 0)
            {
                ScreenReader.Say(Loc.Get("chat_no_users"));
                return;
            }

            string channelName = currentChannel.channelName ?? "channel";
            List<string> names = new List<string>();
            foreach (var user in users)
            {
                var userChat = user.GetUserChat();
                if (userChat != null)
                {
                    names.Add(userChat.nickName);
                }
            }

            string nameList = string.Join(", ", names);
            ScreenReader.Say(Loc.Get("chat_users", users.Length, channelName, nameList));
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Strips Unity rich text tags from a string for clean screen reader output.
        /// </summary>
        private static string StripRichText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", "");
        }

        #endregion
    }
}
