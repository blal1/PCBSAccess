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
            _english["boot_specs"] = "CPU: {0}. RAM: {1} megabytes.";
            _english["boot_specs_unknown"] = "System specs not available.";
            _english["boot_specs_cpu_only"] = "CPU: {0}.";
            _english["boot_memory_ok"] = "Memory test OK.";
            _english["boot_os_loading"] = "Loading operating system.";
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
            _english["terminal_help"] = "Terminal. Type commands and press Enter. Up Down for command history. Shift Left Right jump words. Ctrl Up Down review output. Left Right letter by letter. Ctrl C cancel.";
            _english["terminal_autocomplete_none"] = "No match";
            _english["terminal_autocomplete_result"] = "Completed: {0}";
            _english["terminal_caret_end"] = "End of line";
            _english["terminal_caret_char"] = "{0}";
            _english["terminal_folder_changed"] = "{0}";
            _english["terminal_output_history_empty"] = "No output history";

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

            // Search
            _english["search_home"] = "Search. Type query in address bar, Enter to search.";
            _english["search_no_net"] = "No network connection.";
            _english["search_searching"] = "Searching...";
            _english["search_help"] = "Search page. Type in the address bar and press Enter to search. Results appear as web page links.";

            // Bank
            _english["bank_login"] = "Bank login. Tab between fields, Enter to submit.";
            _english["bank_register"] = "Bank registration. Tab between fields, Enter to submit.";
            _english["bank_logging_in"] = "Logging in...";
            _english["bank_creating"] = "Creating account...";
            _english["bank_registered"] = "Account created. Account number: {0}. {1}";
            _english["bank_account"] = "Bank account. Balance: {0}. {1} transactions.";
            _english["bank_transaction"] = "{0} of {1}: {2}";
            _english["bank_no_transactions"] = "No transactions.";
            _english["bank_balance"] = "Balance: {0}.";
            _english["bank_help_login"] = "Bank login. Tab between account and password fields. Enter to log in.";
            _english["bank_help_register"] = "Bank registration. Tab between fields. Enter to register.";
            _english["bank_help_account"] = "Bank account. Up Down to browse transactions. Space to repeat balance. Tab to transfer fields.";

            // Shop
            _english["shop_loaded"] = "Shop. {0} items. Up Down to browse, Enter to buy.";
            _english["shop_item"] = "{0} of {1}: {2}. {3}. ${4}";
            _english["shop_item_hw"] = "{0} of {1}: {2}. {3}. {4}. ${5}";
            _english["shop_empty"] = "Shop. No items available.";
            _english["shop_filter"] = "Filter: {0}";
            _english["shop_help"] = "Shop. Up Down to browse items. Enter to buy. Alt F to change filter. Space to repeat.";

            // Hack shop
            _english["hackshop_tools"] = "Hack shop tools. {0} items. Up Down to browse, Enter to buy.";
            _english["hackshop_exploits"] = "Hack shop exploits. Select library and version to search.";
            _english["hackshop_exploits_results"] = "Found {0} exploits. Up Down to browse.";
            _english["hackshop_item"] = "{0} of {1}: {2}. {3}. ${4}";
            _english["hackshop_exploit_item"] = "{0} of {1}: {2}. Service: {3}. ${4}. {5}";
            _english["hackshop_empty"] = "No items available.";
            _english["hackshop_filter"] = "Library: {0}";
            _english["hackshop_perm_filter"] = "Permission: {0}";
            _english["hackshop_help_tools"] = "Hack shop tools. Up Down to browse. Enter to buy. Alt F to change filter. Space to repeat.";
            _english["hackshop_help_exploits"] = "Hack shop exploits. Alt F to select library. Type version and Enter to search. Up Down to browse results. Enter to buy. Alt P for permission filter.";

            // Router - Port Forwarding
            _english["router_ports"] = "Port forwarding. {0} rules. Up Down to browse, Enter to edit, N to add new.";
            _english["router_port_rule"] = "Rule {0} of {1}: External {2} to {3}:{4}.{5}";
            _english["router_port_protected"] = " Protected.";
            _english["router_port_empty"] = "Port forwarding. No rules.";
            _english["router_port_editing"] = "Editing rule. Tab between fields, Enter to save, Escape to cancel.";
            _english["router_port_field"] = "{0}: {1}";
            _english["router_port_saved"] = "Rule saved.";
            _english["router_port_cancelled"] = "Edit cancelled.";
            _english["router_port_deleted"] = "Deleted {0} rules.";
            _english["router_port_protected_no_edit"] = "Protected rule. Cannot edit.";
            _english["router_port_help"] = "Port forwarding. Up Down to browse rules. Enter to edit. N to add. Delete to remove. Space to repeat.";

            // Router - Firewall
            _english["router_firewall"] = "Firewall rules. {0} rules. Up Down to browse, Enter to edit, N to add new.";
            _english["router_fw_rule"] = "Rule {0} of {1}: {2} port {3} from {4} to {5}.";
            _english["router_fw_empty"] = "Firewall. No rules.";
            _english["router_fw_editing"] = "Editing firewall rule. Tab between fields, Enter to save, Escape to cancel.";
            _english["router_fw_field"] = "{0}: {1}";
            _english["router_fw_action_toggle"] = "Action: {0}";
            _english["router_fw_any_on"] = "{0} set to Any.";
            _english["router_fw_any_off"] = "{0} restored to {1}.";
            _english["router_fw_saved"] = "Rule saved.";
            _english["router_fw_cancelled"] = "Edit cancelled.";
            _english["router_fw_deleted"] = "Deleted {0} rules.";
            _english["router_fw_help"] = "Firewall rules. Up Down to browse. Enter to edit. N to add. Delete to remove. In edit mode: Tab between fields, Left Right for Allow Deny, Alt A to toggle Any.";

            // Router help
            _english["router_help_panel"] = "Router help.";
            _english["router_help_text"] = "Router help. Alt R to read help content.";

            // Jobs
            _english["jobs_panel"] = "Jobs. {0} missions. Up Down to browse, Enter for details.";
            _english["jobs_police"] = "Police jobs. {0} missions. Up Down to browse, Enter for details.";
            _english["jobs_item"] = "{0} of {1}: {2}. {3}";
            _english["jobs_item_rep"] = "{0} of {1}: {2}. Reputation {3}. {4}";
            _english["jobs_empty"] = "No missions available.";
            _english["jobs_detail"] = "Mission: {0}. {1}";
            _english["jobs_back"] = "Back to job list.";
            _english["jobs_help"] = "Jobs. Up Down to browse missions. Enter to view details. Backspace to go back. Space to repeat.";

            // Mission Panel
            _english["mission_panel"] = "{0}. Type: {1}. Reward: {2}. Difficulty: {3}. {4}. Left Right to choose, Enter to accept, Escape to decline.";
            _english["mission_panel_nodesc"] = "{0}. Type: {1}. Reward: {2}. Difficulty: {3}. Left Right to choose, Enter to accept, Escape to decline.";
            _english["mission_accepted"] = "Mission accepted.";
            _english["mission_declined"] = "Mission declined.";
            _english["mission_nav_accept"] = "Accept";
            _english["mission_nav_decline"] = "Decline";
            _english["mission_diff_easy"] = "Easy";
            _english["mission_diff_medium"] = "Medium";
            _english["mission_diff_hard"] = "Hard";
            _english["mission_active"] = "Active mission. {0}: {1}. {2}";
            _english["mission_none"] = "No active mission.";
            _english["mission_help"] = "Mission contract. Left Right to navigate Accept Decline. Enter to confirm. Escape to decline. Space to repeat.";
            _english["mission_type_tutorial"] = "Tutorial";
            _english["mission_type_credentials"] = "Steal credentials";
            _english["mission_type_academic"] = "Academic record";
            _english["mission_type_police"] = "Police record";
            _english["mission_type_destroy"] = "Destroy computer";
            _english["mission_type_stealfile"] = "Steal file";
            _english["mission_type_deletefile"] = "Delete file";
            _english["mission_type_findhacker"] = "Find hacker";
            _english["mission_type_findevidence"] = "Find evidence";

            // Police
            _english["police_report"] = "Police report. Enter IP address and attach evidence file.";
            _english["police_submitted"] = "Report submitted.";
            _english["police_help"] = "Police report. Tab between IP field and attach button. Enter to submit. Space to repeat.";

            // CCTV
            _english["cctv_active"] = "CCTV camera. {0}W A S D to pan. Zoom in and out with plus and minus.";
            _english["cctv_title"] = "Camera: {0}. ";
            _english["cctv_password"] = "Enter camera password.";
            _english["cctv_help"] = "CCTV camera. W A S D to pan camera. This is a visual-only feed.";

            // ISP
            _english["isp_panel"] = "ISP configuration. Choose a package or manage your domain.";
            _english["isp_help"] = "ISP panel. Use Tab to navigate options. Enter to select.";

            // Currency
            _english["currency_panel"] = "Create cryptocurrency. Enter coin name, username, and password. Cost: $150.";
            _english["currency_help"] = "Cryptocurrency creation. Tab between fields. Enter to create. Cost is $150.";

            // CTF
            _english["ctf_panel"] = "CTF events. {0} missions. Up Down to browse, Enter for details.";
            _english["ctf_empty"] = "CTF events. No missions available.";
            _english["ctf_item"] = "{0} of {1}: {2}. By {3}. {4}";
            _english["ctf_detail"] = "CTF: {0}. {1}";
            _english["ctf_back"] = "Back to CTF list.";
            _english["ctf_help"] = "CTF events. Up Down to browse. Enter for details. Backspace to go back.";

            // Find Device
            _english["finddevice_panel"] = "Device manual finder. Type device model and search.";
            _english["finddevice_result"] = "Manual: {0}. Model: {1}.";
            _english["finddevice_help"] = "Device manual finder. Type a model name and press Enter to search. Alt R to read manual.";

            // Translation Window
            _english["trans_langname_mode"] = "Translation editor. {0} keys to translate. Type a language name and press Enter to start.";
            _english["trans_language_created"] = "Language {0} created. Translation editor ready.";
            _english["trans_editing_mode"] = "Translation editor. Keys {0}. Alt Left Right to navigate keys. Alt R to read English. Alt E to edit. Ctrl S to save.";
            _english["trans_key_announced"] = "Key {0}. {1}. {2}. {3}";
            _english["trans_status_translated"] = "translated";
            _english["trans_status_empty"] = "not translated";
            _english["trans_english_content"] = "English: {0}";
            _english["trans_english_empty"] = "English text is empty.";
            _english["trans_no_english"] = "No English text available.";
            _english["trans_translation_content"] = "Translation: {0}";
            _english["trans_translation_empty"] = "Translation is empty.";
            _english["trans_edit_field_empty"] = "Translation field. Empty. Type your translation.";
            _english["trans_edit_field_content"] = "Translation field. {0}";
            _english["trans_input_unfocused"] = "Editing finished. Text saved.";
            _english["trans_saved"] = "Translation saved.";
            _english["trans_first_key"] = "Already at first key.";
            _english["trans_last_key"] = "Already at last key.";
            _english["trans_undo"] = "Undo. {0}";
            _english["trans_redo"] = "Redo. {0}";
            _english["trans_no_undo"] = "Nothing to undo.";
            _english["trans_no_redo"] = "Nothing to redo.";
            _english["trans_preview_applied"] = "Translation preview applied to the game.";
            _english["trans_search_opened"] = "Key search. Type a keyword and press Tab to search. Up Down to browse results. Enter to select. Escape to close.";
            _english["trans_search_input_focused"] = "Search field. Type a keyword.";
            _english["trans_search_no_results"] = "No results found.";
            _english["trans_search_result"] = "{0} of {1}: {2}";
            _english["trans_search_wrap_first"] = "First result";
            _english["trans_search_wrap_last"] = "Last result";
            _english["trans_pagesearch_opened"] = "Go to key number. Type a number and press Enter.";
            _english["trans_upload_opened"] = "{0}. Tab between title, description, and changelog fields. Enter to publish. Escape to cancel.";
            _english["trans_upload_cancelled"] = "Upload cancelled.";
            _english["trans_upload_status"] = "Upload status: {0}";
            _english["trans_help_langname"] = "Translation editor. Type a language name in the field and press Enter to start translating. The editor will show all game text keys one by one.";
            _english["trans_help_editing"] = "Translation editor. Alt Left and Right to navigate keys. Alt R to read English text. Alt T to read your translation. Alt E to edit translation field. Escape to stop editing. Ctrl S to save. Ctrl Z to undo. Ctrl Y to redo. Alt F to search keys. Alt G to go to key number. Alt P to preview language in game. Alt W to publish. Alt C to copy file path. Ctrl Shift C to copy all keys to clipboard. Ctrl Shift V to paste translations from clipboard.";
            _english["trans_batch_copied"] = "Copied {0} keys to clipboard. Translate in a text editor then use Ctrl Shift V to paste back.";
            _english["trans_batch_pasted"] = "Applied {0} translations. {1} lines skipped.";
            _english["trans_batch_unavailable"] = "Translation data not available.";
            _english["trans_batch_no_session"] = "No translation session active.";
            _english["trans_batch_clipboard_empty"] = "Clipboard is empty.";
            _english["trans_help_search"] = "Key search. Type keyword and press Tab to search. Up Down to browse results. Enter to select a key. Escape to close search.";
            _english["trans_help_upload"] = "Workshop upload. Tab between fields. Enter to publish. Escape to cancel. Alt R to read upload status.";
            _english["trans_launch_opening"] = "Opening Translation Editor.";
            _english["trans_launch_unavailable"] = "Translation Editor not available. Launch a single player game first.";
            _english["trans_language_empty"] = "Language name is empty. Type a name then press Enter.";
            _english["specs_heading"] = "System specs:";
            _english["specs_cpu"] = "CPU {0}: {1}.";
            _english["specs_ram"] = "RAM: {0} modules, {1} megabytes total.";
            _english["specs_gpu"] = "GPU: {0}.";
            _english["specs_disk"] = "Disk: {0}, {1} megabytes.";
            _english["specs_motherboard"] = "Motherboard: {0}.";
            _english["specs_psu"] = "Power supply: {0}, {1} watts.";
            _english["specs_network_count"] = "Network devices: {0}.";
            _english["specs_unavailable"] = "System specs not available. Start a single player game first.";
            _english["specs_sysinfo_cpu"] = "CPU: {0}, {1}";
            _english["specs_sysinfo_ram"] = "RAM: {0} MB";
        }

        #endregion
    }
}
