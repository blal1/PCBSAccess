using BepInEx;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PCBSAccess
{
    /// <summary>
    /// PCBSAccess — accessibility mod for PC Building Simulator.
    /// Entry point: initializes framework, applies Harmony patches, polls hotkeys.
    /// Keep this class small — all feature logic goes in Handler classes.
    /// </summary>
    [BepInPlugin("com.pcbsaccess.mod", "PCBSAccess", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        #region Fields

        /// <summary>True when game singletons are ready for access.</summary>
        private bool _gameReady = false;

        /// <summary>When true, DebugLogger writes to the BepInEx log. Toggle with F12.</summary>
        public static bool DebugMode = false;

        /// <summary>BepInEx logger — accessible from all framework classes via Main.Log.</summary>
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        // Phase 3: handlers added here
        // private CareerStatusHandler _careerStatusHandler;

        #endregion

        #region Lifecycle

        void Awake()
        {
            Log = Logger;

            ScreenReader.Initialize();
            Loc.Initialize();

            var harmony = new Harmony("com.pcbsaccess.mod");
            harmony.PatchAll();

            SceneManager.sceneLoaded += OnSceneLoaded;

            StartCoroutine(AnnounceStartup());
        }

        private IEnumerator AnnounceStartup()
        {
            yield return new WaitForSeconds(1f);
            ScreenReader.Say(Loc.Get("mod_loaded"));
        }

        void Update()
        {
            if (!CheckGameReady()) return;

            if (ProcessHotkeys()) return;

            // Phase 3: handler updates
            // _careerStatusHandler?.Update();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ScreenReader.Shutdown();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DebugLogger.LogState($"Scene loaded: {scene.name}");
            _gameReady = false;
            AccessStateManager.ForceReset();
        }

        #endregion

        #region Game Ready Check

        private bool CheckGameReady()
        {
            if (_gameReady) return true;

            try
            {
                if (GameController.instance != null)
                {
                    _gameReady = true;
                    DebugLogger.LogState("Game ready.");
                }
            }
            catch (System.Exception ex)
            {
                // GameController not yet available — expected during scene load
                DebugLogger.LogGame("CheckGameReady", ex.Message);
            }

            return _gameReady;
        }

        #endregion

        #region Hotkeys

        /// <summary>Processes global hotkeys. Returns true if a key was consumed.</summary>
        private bool ProcessHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                DebugLogger.LogInput("F12", "ToggleDebug");
                DebugMode = !DebugMode;
                ScreenReader.Say(Loc.Get(DebugMode ? "debug_on" : "debug_off"));
                return true;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLogger.LogInput("F1", "CareerStatus");
                AnnounceCareerStatus();
                return true;
            }

            return false;
        }

        #endregion

        #region Career Status (F1)

        private void AnnounceCareerStatus()
        {
            try
            {
                var career = CareerStatus.Get();
                if (career == null)
                {
                    DebugLogger.LogWarning("AnnounceCareerStatus: CareerStatus.Get() returned null");
                    return;
                }

                string text = $"{Loc.Get("cash")}: {career.m_cash}. " +
                              $"{Loc.Get("kudos")}: {career.m_kudos}. " +
                              $"{Loc.Get("rating")}: {career.m_starRating} {Loc.Get("stars")}.";

                ScreenReader.Say(text);
            }
            catch (System.Exception ex)
            {
                DebugLogger.LogWarning($"AnnounceCareerStatus failed: {ex.Message}");
            }
        }

        #endregion
    }
}
