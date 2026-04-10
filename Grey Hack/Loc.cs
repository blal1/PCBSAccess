using System.Collections.Generic;

namespace GreyHackAccess
{
    /// <summary>
    /// Localization for the accessibility mod.
    /// All screenreader strings go through Loc.Get().
    /// </summary>
    public static class Loc
    {
        #region Fields

        private static bool _initialized = false;
        private static readonly Dictionary<string, string> _english = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes localization. Call once at mod startup.
        /// </summary>
        public static void Initialize()
        {
            InitializeStrings();
            _initialized = true;
        }

        /// <summary>
        /// Gets a localized string.
        /// </summary>
        public static string Get(string key)
        {
            if (!_initialized) Initialize();

            if (_english.TryGetValue(key, out string value))
                return value;

            return key;
        }

        /// <summary>
        /// Gets a localized string with placeholders {0}, {1}, etc.
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// All translations defined here.
        /// </summary>
        private static void InitializeStrings()
        {
            // General
            _english["mod_loaded"] = "GreyHackAccess loaded. F1 for help.";
            _english["help_text"] = "Key bindings: F1 Help. F12 Toggle debug mode. Ctrl F11 Mod settings.";
            _english["unknown"] = "Unknown";

            // Placeholders
            _english["item_count"] = "{0} items";

            // Intro Cutscene
            _english["intro_started"] = "Grey Hack. Intro playing.";
            _english["intro_skip_available"] = "Press Escape to skip intro.";
            _english["intro_skipping"] = "Skipping intro. Loading main menu.";
            _english["intro_loading_menu"] = "Loading main menu.";

            // BiosMenu (Main Menu)
            _english["bios_menu_opened"] = "Main Menu. {0} options. Up Down to navigate, Enter to select.";
            _english["bios_position"] = "{2}, {0} of {1}";
            _english["bios_toggle"] = "{2}, checkbox, {3}, {0} of {1}";
            _english["bios_checked"] = "checked";
            _english["bios_unchecked"] = "unchecked";
            _english["bios_toggle_changed"] = "{0}: {1}";
            _english["bios_panel_selected"] = "{0}";
            _english["bios_game_mode"] = "Choose game mode. {0} of {1}: {2}";
            _english["bios_play_options"] = "Game options. {0} of {1}: {2}";
            _english["bios_start_button"] = "Start Game";
            _english["bios_connecting"] = "Connecting to server";
            _english["bios_connection_error"] = "Connection failed. {0}. Press Backspace to go back.";
            _english["bios_loading_sp"] = "Starting single player. Loading.";
            _english["bios_back_to"] = "Back to {0}";
            _english["bios_main_menu"] = "main menu";
            _english["bios_game_modes"] = "game mode selection";
            _english["bios_game_options"] = "game options";
            _english["bios_help"] = "Up Down navigate. Enter select. Backspace go back.";
            _english["bios_wrap_first"] = "First item";
            _english["bios_wrap_last"] = "Last item";

            // BiosMenu - Language Panel
            _english["bios_language_opened"] = "Language selection. {0} languages. Up Down to navigate, Enter to select.";
            _english["bios_language_selected"] = "Language set to {0}";
            _english["bios_language_help"] = "Language panel. Up Down to navigate languages. Enter to select. Backspace to go back.";

            // BiosMenu - Audio Panel
            _english["bios_audio_opened"] = "Audio settings. {0} options. Up Down to navigate, Left Right to adjust sliders, Enter to toggle.";
            _english["bios_audio_music"] = "Music Volume";
            _english["bios_audio_sfx"] = "SFX Volume";
            _english["bios_audio_bg"] = "Background Audio";
            _english["bios_audio_menu_music"] = "Main Menu Music";
            _english["bios_audio_slider_item"] = "{2}, slider, {3} percent, {0} of {1}";
            _english["bios_audio_slider_value"] = "{0}: {1} percent";
            _english["bios_audio_help"] = "Audio panel. Up Down to navigate. Left Right to adjust volume. Enter to toggle. Backspace to go back.";

            // BiosMenu - Graphics Panel
            _english["bios_graphics_opened"] = "Graphics settings. {0} options. Up Down to navigate, Left Right to adjust, Enter to activate.";
            _english["bios_graphics_dropdown"] = "{2}, dropdown, {3}, {0} of {1}";
            _english["bios_graphics_dropdown_value"] = "{0}: {1}";
            _english["bios_graphics_slider"] = "{2}, slider, {3}, {0} of {1}";
            _english["bios_graphics_slider_value"] = "{0}: {1}";
            _english["bios_graphics_help"] = "Graphics panel. Up Down to navigate. Left Right to adjust dropdowns and sliders. Enter to toggle or press button. Backspace to go back.";

            // BiosMenu - Credits Panel
            _english["bios_credits_content"] = "Credits. {0}";
            _english["bios_credits_empty"] = "Credits.";
            _english["bios_credits_help"] = "Credits panel. Backspace to go back.";

            // Boot Sequence
            _english["boot_starting"] = "Booting up. Press Delete to cancel and return to menu.";
            _english["boot_starting_first"] = "Booting up. First time setup will begin after boot.";
            _english["boot_starting_safe"] = "Booting into safe mode.";
            _english["boot_desktop_ready"] = "Desktop loaded.";
            _english["boot_help_bios"] = "Computer is booting. Press Delete to cancel and return to menu.";
            _english["boot_help_wait"] = "Please wait, loading.";
            _english["boot_help_install"] = "First time setup. Tab between fields. Enter to submit.";
            _english["boot_help_safe"] = "Safe mode terminal. Type reboot to restart, or exit to return to menu.";

            // First Install
            _english["boot_install_form"] = "First time setup. Enter your username, computer name, and password. Tab between fields.";
            _english["boot_field_empty"] = "{0}, empty";
            _english["boot_field_value"] = "{0}, {1}";
            _english["boot_field_password"] = "{0}, filled";
            _english["boot_field_username"] = "Username";
            _english["boot_field_pcname"] = "Computer name";
            _english["boot_field_password_label"] = "Password";
            _english["boot_field_confirm"] = "Confirm password";
            _english["boot_installing"] = "Installing. Please wait.";
            _english["boot_install_done"] = "Installation complete.";
            _english["boot_error"] = "Error: {0}";

            // Game Over
            _english["boot_game_over"] = "Game over.";
            _english["boot_game_over_msg"] = "Game over. {0}";

            // Terminal
            _english["terminal_focus"] = "Terminal. {0} at {1}";
            _english["terminal_focus_remote"] = "Terminal. {0} at {1}, remote: {2}";
            _english["terminal_history"] = "{0}";
            _english["terminal_output_long"] = "{0} lines. {1}. Last: {2}";
            _english["terminal_help"] = "Terminal. Type commands and press Enter. Up Down for command history. Ctrl C cancel command.";

            // Window Focus
            _english["window_opened"] = "{0}, opened";
            _english["window_closed"] = "{0}, closed";
            _english["window_focused"] = "{0}";
            _english["window_minimized"] = "{0}, minimized";
            _english["window_help"] = "Ctrl Tab to switch windows. Alt D to browse desktop icons.";
            _english["window_help_context"] = "In {0}. Ctrl Tab to switch windows. Alt D to browse desktop icons.";

            // Task Switcher
            _english["switcher_opened"] = "Task switcher. {0} windows. {1}";
            _english["switcher_item"] = "{3}, {1} of {2}";
            _english["switcher_activated"] = "Switched to {0}";
            _english["switcher_cancelled"] = "Task switcher cancelled";

            // Desktop Icons
            _english["desktop_nav_entered"] = "Desktop icons. {0} items. Up Down to navigate, Enter to open, Escape to exit.";
            _english["desktop_nav_exited"] = "Desktop icons closed";
            _english["desktop_no_icons"] = "No desktop icons";
            _english["desktop_icon_item"] = "{2}, {3}, {0} of {1}";
            _english["desktop_icon_opening"] = "Opening {0}";
            _english["desktop_icon_help"] = "Desktop icons. Up Down to navigate, Enter to open, Escape to exit, Space to repeat.";
            _english["desktop_wrap_first"] = "First item";
            _english["desktop_wrap_last"] = "Last item";
            _english["desktop_type_folder"] = "folder";
            _english["desktop_type_file"] = "file";
            _english["desktop_type_program"] = "program";

            // Start Menu
            _english["startmenu_opened"] = "Start menu. {0} items. Up Down to navigate, Enter to select, Escape to close.";
            _english["startmenu_closed"] = "Start menu closed";
            _english["startmenu_item"] = "{3}{2}, {0} of {1}";
            _english["startmenu_has_submenu"] = ", submenu, press Enter or Right arrow";
            _english["startmenu_activating"] = "Opening {0}";
            _english["startmenu_programs_opened"] = "Programs. {0} items. Up Down to navigate, Enter to launch, Left arrow to go back.";
            _english["startmenu_programs_closed"] = "Back to start menu.";
            _english["startmenu_program_item"] = "{2}, {0} of {1}";
            _english["startmenu_launching"] = "Launching {0}";
            _english["startmenu_no_programs"] = "No programs available";
            _english["startmenu_wrap_first"] = "First item";
            _english["startmenu_wrap_last"] = "Last item";
            _english["startmenu_help"] = "Start menu. Up Down to navigate, Enter to select, Right arrow to open Programs submenu, Escape to close.";
            _english["startmenu_help_submenu"] = "Programs submenu. Up Down to navigate, Enter to launch, Left arrow or Backspace to go back.";

            // Dialogs (Error, Question, Kernel Panic, Game Over)
            _english["dialog_announced"] = "{0}. {1} buttons. {2}";
            _english["dialog_announced_no_buttons"] = "{0}";
            _english["dialog_button"] = "{0}, {1} of {2}";
            _english["dialog_selecting"] = "{0}";
            _english["dialog_btn_ok"] = "OK";
            _english["dialog_btn_restart"] = "Restart";
            _english["dialog_kernel_panic"] = "Kernel panic. System crash.";
            _english["dialog_help"] = "Dialog. Up Down or Left Right to navigate buttons, Enter to select, Escape to dismiss, Space to repeat.";

            // Game Over detail view
            _english["dialog_gameover_details_shown"] = "Details shown. {0} traces. Up Down to navigate.";
            _english["dialog_gameover_details_hidden"] = "Details hidden.";
            _english["dialog_gameover_trace"] = "Trace {0} of {1}: {2} {3}, action {4}, from {5}";
            _english["dialog_gameover_copied"] = "Trace log copied to clipboard.";
            _english["dialog_gameover_help"] = "Game over. Left Right for buttons, Enter to select. In detail view: Up Down for traces.";
            _english["dialog_gameover_router"] = "Router";
            _english["dialog_gameover_computer"] = "Computer";

            // Welcome Dialog
            _english["welcome_panel"] = "Tutorial setup. {0} options. Up Down to navigate, Enter to select.";
            _english["welcome_button"] = "{0}, {1} of {2}";
            _english["welcome_selecting"] = "Selecting {0}";
            _english["welcome_wrap_first"] = "First item";
            _english["welcome_wrap_last"] = "Last item";
            _english["welcome_help"] = "Tutorial setup dialog. Up Down to navigate options, Enter to select, Space to repeat.";

            // File Explorer
            _english["explorer_focused"] = "File explorer. {0}. {1} items.";
            _english["explorer_file_item"] = "{2}, {3}, {0} of {1}";
            _english["explorer_empty"] = "Folder is empty";
            _english["explorer_opening_folder"] = "Opening folder {0}";
            _english["explorer_opening_file"] = "Opening {0}";
            _english["explorer_going_up"] = "Going up";
            _english["explorer_going_back"] = "Going back";
            _english["explorer_going_forward"] = "Going forward";
            _english["explorer_going_home"] = "Going home";
            _english["explorer_folder_changed"] = "{0}. {1} items.";
            _english["explorer_no_selection"] = "No file selected";
            _english["explorer_renaming"] = "Renaming {0}. Type new name and press Enter.";
            _english["explorer_deleting"] = "Deleting {0}";
            _english["explorer_context_menu"] = "Context menu: {0}";
            _english["explorer_wrap_first"] = "First item";
            _english["explorer_wrap_last"] = "Last item";
            _english["explorer_help"] = "File explorer. Up Down to navigate files, Enter to open, Backspace to go up, Alt Left Right for history, Alt Home for home, Shift F10 for context menu, F2 to rename, Delete to delete.";
            _english["explorer_type_folder"] = "folder";
            _english["explorer_type_text"] = "text file";
            _english["explorer_type_source"] = "source code";
            _english["explorer_type_log"] = "log file";
            _english["explorer_type_image"] = "image";
            _english["explorer_type_pdf"] = "PDF";
            _english["explorer_type_program"] = "program";
            _english["explorer_type_binary"] = "binary file";
            _english["explorer_type_file"] = "file";

            // Text Input Feedback
            _english["input_backspace"] = "backspace";
            _english["input_delete"] = "delete";
            _english["input_deleted_chars"] = "{0} characters deleted";
            _english["input_space"] = "space";
            _english["input_dot"] = "dot";
            _english["input_comma"] = "comma";
            _english["input_slash"] = "slash";
            _english["input_backslash"] = "backslash";
            _english["input_dash"] = "dash";
            _english["input_underscore"] = "underscore";
            _english["input_at"] = "at";
            _english["input_colon"] = "colon";
            _english["input_semicolon"] = "semicolon";
            _english["input_exclamation"] = "exclamation";
            _english["input_question"] = "question";
            _english["input_hash"] = "hash";
            _english["input_dollar"] = "dollar";
            _english["input_equals"] = "equals";
            _english["input_plus"] = "plus";
            _english["input_star"] = "star";
            _english["input_pipe"] = "pipe";
            _english["input_greater"] = "greater than";
            _english["input_less"] = "less than";
            _english["input_left_paren"] = "left paren";
            _english["input_right_paren"] = "right paren";
            _english["input_left_bracket"] = "left bracket";
            _english["input_right_bracket"] = "right bracket";
            _english["input_left_brace"] = "left brace";
            _english["input_right_brace"] = "right brace";
            _english["input_tilde"] = "tilde";
            _english["input_caret"] = "caret";
            _english["input_ampersand"] = "ampersand";
            _english["input_percent"] = "percent";
            _english["input_quote"] = "quote";
            _english["input_apostrophe"] = "apostrophe";
            _english["input_backtick"] = "backtick";
            _english["input_tab"] = "tab";

            // Notifications
            _english["notif_announced"] = "{0}: {1}";
            _english["notif_announced_info"] = "{0}";
            _english["notif_repeat"] = "Last notification: {0}";
            _english["notif_none"] = "No notifications yet";
            _english["notif_type_info"] = "Info";
            _english["notif_type_warning"] = "Warning";
            _english["notif_type_mail"] = "Mail";
            _english["notif_type_notepad"] = "Notepad";
            _english["notif_type_codeeditor"] = "Code editor";
            _english["notif_type_logwindow"] = "Log";
            _english["notif_type_configlan"] = "Network config";
            _english["notif_type_map"] = "Map";
            _english["notif_type_connection"] = "Connection";
            _english["notif_type_exploitreport"] = "Exploit report";
            _english["notif_type_clipboard"] = "Clipboard";

            // Context Menus
            _english["context_opened"] = "{0} options. Up Down to navigate, Enter to select, Escape to close.";
            _english["context_item"] = "{0}, {1} of {2}";
            _english["context_item_disabled"] = "{0}, disabled, {1} of {2}";
            _english["context_closed"] = "Context menu closed";
            _english["context_no_context"] = "No context menu available here";
            _english["context_help"] = "Context menu. Up Down to navigate, Enter to select, Escape to close, Space to repeat.";

            // Notepad
            _english["notepad_focused"] = "Notepad. {0}";
            _english["notepad_focused_new"] = "Notepad. New file.";
            _english["notepad_file_loaded"] = "File loaded. {0}";
            _english["notepad_read_content"] = "{0}";
            _english["notepad_empty"] = "File is empty";
            _english["notepad_help"] = "Notepad. Alt R to read content. Ctrl S to save, Ctrl O to open file. F1 for help.";

            // Mail
            _english["mail_login"] = "Mail login. Enter username and password.";
            _english["mail_inbox"] = "Inbox. {0} emails.";
            _english["mail_outbox"] = "Outbox. {0} emails.";
            _english["mail_no_emails"] = "No emails.";
            _english["mail_item"] = "{0} of {1}. From {2}. {3}. {4}";
            _english["mail_item_unread"] = "unread";
            _english["mail_item_read"] = "read";
            _english["mail_reading"] = "From {0}. Subject: {1}.";
            _english["mail_read_body"] = "{0}";
            _english["mail_empty_body"] = "Message is empty.";
            _english["mail_compose"] = "Compose email. Tab to move between fields.";
            _english["mail_reply"] = "Reply. Type your message.";
            _english["mail_deleted"] = "Email deleted. {0} emails remaining.";
            _english["mail_back_to_inbox"] = "Back to inbox.";
            _english["mail_switched_inbox"] = "Inbox.";
            _english["mail_switched_outbox"] = "Outbox.";
            _english["mail_focused"] = "Mail. {0}";
            _english["mail_help_login"] = "Mail login. Tab between username and password fields, Enter to log in. F1 for help.";
            _english["mail_help_inbox"] = "Mail inbox. Up Down to navigate emails, Enter to read, N to compose, Tab to switch inbox outbox, Delete to delete. F1 for help.";
            _english["mail_help_read"] = "Reading email. Alt R to read body, R to reply, Backspace to go back, Delete to delete. F1 for help.";
            _english["mail_help_compose"] = "Compose email. Tab between fields, Ctrl Enter to send, Escape to cancel. F1 for help.";

            // Chat
            _english["chat_focused"] = "Chat. Channel: {0}.";
            _english["chat_nickname"] = "Chat. Enter a nickname to register.";
            _english["chat_nickname_registered"] = "Nickname registered. You can now chat.";
            _english["chat_channel_switched"] = "Channel: {0}.";
            _english["chat_message"] = "{0}: {1}";
            _english["chat_private_message"] = "Private from {0}: {1}";
            _english["chat_users"] = "{0} users in {1}: {2}";
            _english["chat_no_users"] = "No users in channel.";
            _english["chat_no_messages"] = "No messages.";
            _english["chat_no_channels"] = "No channels open.";
            _english["chat_history_item"] = "{0}: {1}";
            _english["chat_help"] = "Chat. Ctrl Up Down to read message history. Alt Left Right to switch channels. Alt U for user list. Alt C for channel list. F1 for help.";

            // Tutorial
            _english["tutorial_focused"] = "Tutorial. Page {0} of {1}.";
            _english["tutorial_refocused"] = "Tutorial. Page {0} of {1}.";
            _english["tutorial_action_required"] = "Action required: {0}";
            _english["tutorial_next_available"] = "Press Enter to continue.";
            _english["tutorial_wrong_action"] = "{0}";
            _english["tutorial_complete"] = "Tutorial complete.";
            _english["tutorial_help"] = "Tutorial. Enter to go to next page. Escape to skip tutorial. Space to re-read page. F1 for help.";
            _english["tutorial_pending_open_terminal"] = "Open the terminal.";
            _english["tutorial_pending_launch_pwd"] = "Type pwd in the terminal.";
            _english["tutorial_pending_launch_ls"] = "Type ls in the terminal.";
            _english["tutorial_pending_launch_cd"] = "Type cd in the terminal.";
            _english["tutorial_pending_launch_mkdir"] = "Type mkdir in the terminal.";
            _english["tutorial_pending_explorer_root"] = "Navigate to the root folder.";
            _english["tutorial_pending_explorer_bin"] = "Navigate to the bin folder.";
            _english["tutorial_pending_explorer_usrbin"] = "Navigate to usr bin.";
            _english["tutorial_pending_mail"] = "Check your mail.";
            _english["tutorial_pending_remote_conn"] = "Connect to a remote computer.";
            _english["tutorial_pending_trace_system"] = "Open the system log on the remote computer.";
            _english["tutorial_pending_remote_explorer"] = "Open the file explorer on the remote computer.";
            _english["tutorial_pending_create_mail_button"] = "Create a mail account.";
            _english["tutorial_pending_select_login_issues"] = "Select login issues.";

            // Browser
            _english["browser_focused"] = "Browser. {0}";
            _english["browser_loading"] = "Loading {0}";
            _english["browser_back"] = "Back.";
            _english["browser_forward"] = "Forward.";
            _english["browser_home"] = "Home.";
            _english["browser_no_history"] = "No more history.";
            _english["browser_help"] = "Browser. Alt Left and Right for history. Alt Home for search. Alt R to read content. Up Down to navigate. Enter to select. F1 for help.";
            _english["browser_prebuy"] = "Purchase: {0}. Price: ${1}. {2}";
            _english["browser_prebuy_coupons"] = "{0} coupons.";
            _english["browser_prebuy_version"] = "Version: {0}.";
            _english["browser_prebuy_source_on"] = "Source code included.";
            _english["browser_prebuy_source_off"] = "Source code not included.";
            _english["browser_prebuy_help"] = "Purchase dialog. Up Down to change version. Tab to toggle source code. Enter to buy. Escape to cancel.";

            // Web page
            _english["web_page"] = "Web page. {0} links.";
            _english["web_page_no_links"] = "Web page. No interactive links.";
            _english["web_link"] = "{0} of {1}: {2}";
            _english["web_error_not_found"] = "Page not found.";
            _english["web_error_url_not_found"] = "URL not found.";
            _english["web_error_no_net"] = "No network access.";
            _english["web_searching"] = "Searching...";
            _english["web_help"] = "Web page. Up Down to navigate links. Enter to activate link. Alt R to read page. Space to repeat.";
        }

        #endregion
    }
}
