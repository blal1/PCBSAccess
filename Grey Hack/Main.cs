using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace GreyHackAccess
{
    /// <summary>
    /// Main mod entry point. Coordinates all handlers and processes global hotkeys.
    /// </summary>
    [BepInPlugin("com.blal1.GreyHackAccess", "GreyHackAccess", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        #region Fields

        private bool _gameReady = false;
        private BiosMenuHandler _biosMenuHandler;
        private BootUpHandler _bootUpHandler;
        private TerminalHandler _terminalHandler;
        private WindowFocusHandler _windowFocusHandler;
        private TaskSwitcherHandler _taskSwitcherHandler;
        private DesktopIconHandler _desktopIconHandler;
        private StartMenuHandler _startMenuHandler;
        private FileExplorerHandler _fileExplorerHandler;
        private WelcomeHandler _welcomeHandler;
        private DialogHandler _dialogHandler;
        private NotificationHandler _notificationHandler;
        private IntroHandler _introHandler;
        private TextInputHandler _textInputHandler;
        private ContextMenuHandler _contextMenuHandler;
        private NotepadHandler _notepadHandler;
        private MailHandler _mailHandler;
        private ChatHandler _chatHandler;
        private Harmony _harmony;

        /// <summary>
        /// Debug mode - when true, logs all screenreader output and detailed game state.
        /// Toggle with F12.
        /// </summary>
        public static bool DebugMode = false;

        /// <summary>
        /// BepInEx logger instance, accessible from other classes via Main.Log.
        /// </summary>
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        #endregion

        #region Lifecycle

        void Awake()
        {
            Log = Logger;
            ScreenReader.Initialize();
            Loc.Initialize();
            ModConfig.Initialize(Config);
            InitializeHandlers();

            SceneManager.sceneLoaded += OnSceneLoaded;

            StartCoroutine(AnnounceStartupDelayed());
        }

        private void InitializeHandlers()
        {
            _harmony = new Harmony("com.blal1.GreyHackAccess");
            _harmony.PatchAll();

            _biosMenuHandler = new BiosMenuHandler();
            _biosMenuHandler.Register();

            _bootUpHandler = new BootUpHandler();
            _bootUpHandler.Register();

            _terminalHandler = new TerminalHandler();
            _terminalHandler.Register();

            _windowFocusHandler = new WindowFocusHandler();
            _windowFocusHandler.Register();

            _taskSwitcherHandler = new TaskSwitcherHandler();
            _taskSwitcherHandler.Register();

            _desktopIconHandler = new DesktopIconHandler();
            _desktopIconHandler.Register();

            _startMenuHandler = new StartMenuHandler();
            _startMenuHandler.Register();

            _fileExplorerHandler = new FileExplorerHandler();
            _fileExplorerHandler.Register();

            _welcomeHandler = new WelcomeHandler();
            _welcomeHandler.Register();

            _dialogHandler = new DialogHandler();
            _dialogHandler.Register();

            _introHandler = new IntroHandler();
            _introHandler.Register();

            _notificationHandler = new NotificationHandler();
            _notificationHandler.Register();

            _textInputHandler = new TextInputHandler();
            _textInputHandler.Register();

            _contextMenuHandler = new ContextMenuHandler();
            _contextMenuHandler.Register();

            _notepadHandler = new NotepadHandler();
            _notepadHandler.Register();

            _mailHandler = new MailHandler();
            _mailHandler.Register();

            _chatHandler = new ChatHandler();
            _chatHandler.Register();
        }

        private IEnumerator AnnounceStartupDelayed()
        {
            yield return new WaitForSeconds(1f);
            ScreenReader.Say(Loc.Get("mod_loaded"));
        }

        void Update()
        {
            if (!CheckGameReady()) return;

            // Settings menu takes priority
            if (ModConfig.IsMenuOpen)
            {
                ModConfig.Update();
                return;
            }

            if (ProcessHotkeys()) return;

            UpdateHandlers();
        }

        private bool CheckGameReady()
        {
            return _gameReady;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo($"Scene loaded: {scene.name}");
            DebugLogger.LogState($"Scene changed to: {scene.name}");
            _gameReady = true;

            if (scene.name.Equals("Game"))
            {
                // Intro is done, deactivate IntroHandler
                _introHandler.Deactivate();
                IntroHandler.OnGameSceneLoading();
            }
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ScreenReader.Shutdown();
        }

        #endregion

        #region Hotkeys

        /// <summary>
        /// Processes global hotkeys. Returns true if a key was handled.
        /// </summary>
        private bool ProcessHotkeys()
        {
            // F12 = Toggle debug mode
            if (Input.GetKeyDown(KeyCode.F12))
            {
                DebugMode = !DebugMode;
                var status = DebugMode ? "enabled" : "disabled";
                Logger.LogInfo($"Debug mode {status}");
                ScreenReader.Say($"Debug mode {status}");
                return true;
            }

            // Ctrl+F11 = Mod settings
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F11))
            {
                ModConfig.ToggleMenu();
                return true;
            }

            // F1 = Help
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLogger.LogInput("F1", "Help");
                AnnounceHelp();
                return true;
            }

            // Alt+N = Repeat last notification
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.N))
            {
                DebugLogger.LogInput("Alt+N", "Repeat notification");
                _notificationHandler.RepeatLastNotification();
                return true;
            }

            // Shift+F10 = Context menu (for non-file-explorer contexts)
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F10))
            {
                // File explorer handles its own Shift+F10
                if (!_fileExplorerHandler.IsActive)
                {
                    DebugLogger.LogInput("Shift+F10", "Context menu");
                    _contextMenuHandler.TriggerContextMenu();
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Handler Updates

        private void UpdateHandlers()
        {
            // Context menus consume input when active (highest priority)
            if (_contextMenuHandler.Update()) return;

            // Error/question dialogs consume input when active
            if (_dialogHandler.Update()) return;

            // Welcome dialog consumes input when active
            if (_welcomeHandler.Update()) return;

            // Start menu consumes input when active
            if (_startMenuHandler.Update()) return;

            // Desktop icon navigation consumes input when active
            if (_desktopIconHandler.Update()) return;

            // File explorer consumes input when active
            if (_fileExplorerHandler.Update()) return;

            // Notepad consumes input when active
            if (_notepadHandler.Update()) return;

            // Mail consumes input when active
            if (_mailHandler.Update()) return;

            // Chat consumes input when active
            if (_chatHandler.Update()) return;

            _biosMenuHandler.Update();
            _bootUpHandler.Update();
            _terminalHandler.Update();
            _windowFocusHandler.Update();
            _textInputHandler.Update();
        }

        #endregion

        #region Help

        private void AnnounceHelp()
        {
            if (_contextMenuHandler.IsActive)
            {
                ScreenReader.Say(_contextMenuHandler.GetHelpText());
                return;
            }

            if (_dialogHandler.IsActive)
            {
                ScreenReader.Say(_dialogHandler.GetHelpText());
                return;
            }

            if (_welcomeHandler.IsActive)
            {
                ScreenReader.Say(_welcomeHandler.GetHelpText());
                return;
            }

            if (_startMenuHandler.IsActive)
            {
                ScreenReader.Say(_startMenuHandler.GetHelpText());
                return;
            }

            if (_biosMenuHandler.IsActive)
            {
                ScreenReader.Say(_biosMenuHandler.GetHelpText());
                return;
            }

            if (_bootUpHandler.IsActive)
            {
                ScreenReader.Say(_bootUpHandler.GetHelpText());
                return;
            }

            if (_desktopIconHandler.IsActive)
            {
                ScreenReader.Say(_desktopIconHandler.GetHelpText());
                return;
            }

            if (_fileExplorerHandler.IsActive)
            {
                ScreenReader.Say(_fileExplorerHandler.GetHelpText());
                return;
            }

            if (_notepadHandler.IsActive)
            {
                ScreenReader.Say(_notepadHandler.GetHelpText());
                return;
            }

            if (_mailHandler.IsActive)
            {
                ScreenReader.Say(_mailHandler.GetHelpText());
                return;
            }

            if (_chatHandler.IsActive)
            {
                ScreenReader.Say(_chatHandler.GetHelpText());
                return;
            }

            if (_terminalHandler.IsActive)
            {
                ScreenReader.Say(_terminalHandler.GetHelpText());
                return;
            }

            if (_windowFocusHandler.IsActive)
            {
                ScreenReader.Say(_windowFocusHandler.GetHelpText());
                return;
            }

            ScreenReader.Say(Loc.Get("help_text"));
        }

        #endregion
    }
}
