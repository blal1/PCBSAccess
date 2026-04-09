using BepInEx.Configuration;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Mod configuration using BepInEx ConfigFile.
    /// Settings saved to BepInEx/config/com.blal1.GreyHackAccess.cfg.
    /// Toggle settings menu with Ctrl+F11 in game.
    /// </summary>
    public static class ModConfig
    {
        #region Config Entries

        private static ConfigFile _config;
        private static ConfigEntry<int> _verbosity;
        private static ConfigEntry<bool> _announceEmptyStates;
        private static ConfigEntry<bool> _announceChatMessages;

        #endregion

        #region Public Accessors

        /// <summary>Announcement verbosity: 0=minimal, 1=normal, 2=verbose.</summary>
        public static int Verbosity => _verbosity.Value;

        /// <summary>Whether to announce empty states ("No items", "Empty").</summary>
        public static bool AnnounceEmptyStates => _announceEmptyStates.Value;

        /// <summary>Whether to auto-announce incoming chat messages.</summary>
        public static bool AnnounceChatMessages => _announceChatMessages.Value;

        #endregion

        #region Settings Menu State

        private static bool _menuOpen = false;
        private static int _currentSettingIndex = 0;

        private static readonly string[] _settingNames = new[]
        {
            "Verbosity",
            "Announce empty states",
            "Announce chat messages",
        };

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes mod preferences. Call once in Awake().
        /// </summary>
        public static void Initialize(ConfigFile config)
        {
            _config = config;

            _verbosity = config.Bind("Accessibility",
                "Verbosity", 1,
                new ConfigDescription(
                    "Announcement detail level: 0=minimal, 1=normal, 2=verbose",
                    new AcceptableValueRange<int>(0, 2)));

            _announceEmptyStates = config.Bind("Accessibility",
                "AnnounceEmptyStates", true,
                "Announce when lists or inventories are empty");

            _announceChatMessages = config.Bind("Accessibility",
                "AnnounceChatMessages", true,
                "Automatically announce new chat messages via screen reader");
        }

        #endregion

        #region Settings Menu

        /// <summary>
        /// Toggles the in-game settings menu.
        /// </summary>
        public static void ToggleMenu()
        {
            _menuOpen = !_menuOpen;

            if (_menuOpen)
            {
                _currentSettingIndex = 0;
                ScreenReader.Say("Mod settings opened");
                AnnounceCurrentSetting();
            }
            else
            {
                _config.Save();
                ScreenReader.Say("Mod settings closed and saved");
            }
        }

        /// <summary>
        /// Whether the settings menu is currently open.
        /// </summary>
        public static bool IsMenuOpen => _menuOpen;

        /// <summary>
        /// Processes input for the settings menu.
        /// </summary>
        public static void Update()
        {
            if (!_menuOpen) return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentSettingIndex--;
                if (_currentSettingIndex < 0)
                    _currentSettingIndex = _settingNames.Length - 1;
                AnnounceCurrentSetting();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentSettingIndex++;
                if (_currentSettingIndex >= _settingNames.Length)
                    _currentSettingIndex = 0;
                AnnounceCurrentSetting();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeCurrentSetting(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeCurrentSetting(1);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }

        private static void AnnounceCurrentSetting()
        {
            string name = _settingNames[_currentSettingIndex];
            string value = GetCurrentSettingValue();
            int pos = _currentSettingIndex + 1;
            int total = _settingNames.Length;

            ScreenReader.Say($"{pos} of {total}: {name}, {value}. Left right to change.");
        }

        private static string GetCurrentSettingValue()
        {
            switch (_currentSettingIndex)
            {
                case 0:
                    return _verbosity.Value switch
                    {
                        0 => "Minimal",
                        1 => "Normal",
                        2 => "Verbose",
                        _ => _verbosity.Value.ToString()
                    };
                case 1:
                    return _announceEmptyStates.Value ? "On" : "Off";
                case 2:
                    return _announceChatMessages.Value ? "On" : "Off";
                default:
                    return "Unknown";
            }
        }

        private static void ChangeCurrentSetting(int direction)
        {
            switch (_currentSettingIndex)
            {
                case 0:
                    int newVal = _verbosity.Value + direction;
                    if (newVal < 0) newVal = 2;
                    if (newVal > 2) newVal = 0;
                    _verbosity.Value = newVal;
                    break;
                case 1:
                    _announceEmptyStates.Value = !_announceEmptyStates.Value;
                    break;
                case 2:
                    _announceChatMessages.Value = !_announceChatMessages.Value;
                    break;
            }

            string name = _settingNames[_currentSettingIndex];
            string value = GetCurrentSettingValue();
            ScreenReader.Say($"{name}: {value}");
        }

        #endregion
    }
}
