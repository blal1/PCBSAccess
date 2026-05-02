using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PCBSAccess
{
    /// <summary>
    /// PCBSAccess — accessibility mod for PC Building Simulator.
    /// Entry point: initializes framework, applies Harmony patches, polls hotkeys.
    /// Keep this class small — all feature logic goes in Handler classes.
    /// </summary>
    // Negative execution order ensures Main.Update() runs before game MonoBehaviours (order 0).
    // Required so PollState() positions the HBPC cursor BEFORE GameController raycasts.
    [UnityEngine.DefaultExecutionOrder(-100)]
    [BepInPlugin("com.pcbsaccess.mod", "PCBSAccess", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        #region Fields

        /// <summary>Global instance for running Coroutines from static handlers.</summary>
        public static Main Instance { get; private set; }

        /// <summary>True when game singletons are ready for access.</summary>
        private bool _gameReady = false;

        /// <summary>When true, DebugLogger writes to the BepInEx log. Toggle with F12.</summary>
        public static bool DebugMode = false;

        /// <summary>BepInEx logger — accessible from all framework classes via Main.Log.</summary>
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        // Phase 3 handlers (static — no instance needed)

        #endregion

        private static void Step(string s) { if (Log != null) Log.LogInfo($"[PCBSAccess] STEP {s}"); }

        #region Lifecycle

        void Awake()
        {
            Instance = this;
            Log = Logger;
            try { Step("A"); ScreenReader.Initialize(); } catch (Exception ex) { Log.LogError($"[PCBSAccess] STEP A: {ex.GetType().Name}: {ex.Message}"); return; }
            try { Step("B"); Loc.Initialize(); } catch (Exception ex) { Log.LogError($"[PCBSAccess] STEP B: {ex.GetType().Name}: {ex.Message}"); return; }
            try { Step("C"); var harmony = new Harmony("com.pcbsaccess.mod");
            Step("D"); ApplyPatches(harmony); } catch (Exception ex) { Log.LogError($"[PCBSAccess] STEP C/D: {ex.GetType().Name}: {ex.Message}"); return; }
            try { Step("E"); CareerEventHandler.Subscribe(); } catch (Exception ex) { Log.LogError($"[PCBSAccess] STEP E (non-fatal): {ex.GetType().Name}: {ex.Message}"); }
            try { Step("F"); SceneManager.sceneLoaded += OnSceneLoaded; } catch (Exception ex) { Log.LogError($"[PCBSAccess] STEP F: {ex.GetType().Name}: {ex.Message}"); return; }
            try { Step("G"); StartCoroutine(StartupCoroutine()); } catch (Exception ex) { Log.LogError($"[PCBSAccess] STEP G: {ex.GetType().Name}: {ex.Message}"); return; }
            Step("H"); Log.LogInfo("[PCBSAccess] Awake complete");
        }

        /// <summary>Announces startup, logs the current scene, and activates the right handler
        /// if we started inside the main menu (logo splash may have already completed).</summary>
        private IEnumerator StartupCoroutine()
        {
            yield return new WaitForSeconds(1f);
            ScreenReader.Say(Loc.Get("mod_loaded"));

            string scene = SceneManager.GetActiveScene().name;
            Log.LogInfo($"[PCBSAccess] Scene at startup +1s: {scene}");

            if (scene == "Menu_V2")
            {
                var mm = UnityEngine.Object.FindObjectOfType<MainMenu>();
                if (mm != null) MainMenuHandler.OnMainMenuStart(mm);
            }
        }

        void Update()
        {
            if (ProcessHotkeys()) return;

            // Navigation keys are routed exclusively to the topmost active IInputContext.
            InputRouter.Update();

            // Handlers below only do state-polling (open/close detection) — no key reading.
            OptionsMenuHandler.PollState();
            DaySummaryHandler.PollState();
            InGameMenuHandler.PollState();
            MessageBoxHandler.PollState();
            WorkshopSelectionHandler.PollState();
            WorkshopNavigatorHandler.PollState();

            // Pure event-driven handler that still needs a per-frame poll for delivery checks.
            DeliveryHandler.Update();
            ManifestHandler.PollState();
            KeyBindingMenuHandler.PollState();
            WhatsNewPopUpHandler.PollState();
            VirusScanAppHandler.PollState();
            FreebuildOptionsHandler.PollState();
            SearchFiltersPanelHandler.PollState();
            TabletShopHandler.PollState();

            // Deferred list-refresh handlers: these retain Update() for one-frame-deferred rebuild,
            // but key handling has moved to their respective IInputContext implementations.
            MusicPlayerAppHandler.Update();
            LightingAppHandler.Update();
            OCCTHandler.Update();
        }

        void OnDestroy()
        {
            CareerEventHandler.Unsubscribe();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ScreenReader.Shutdown();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogInfo($"[PCBSAccess] OnSceneLoaded: {scene.name}");
            _gameReady = false;

            // Clear the input stack first so no stale context leaks across scenes.
            InputRouter.Reset();

            MainMenuHandler.Reset();
            OptionsMenuHandler.Reset();
            CareerJobHandler.Reset();
            InventoryHandler.Reset();
            SaveLoadMenuHandler.Reset();
            JobStatusHandler.Reset();
            WorkshopBuildHandler.Reset();
            ShopHandler.Reset();
            InGameMenuHandler.Reset();
            MessageBoxHandler.Reset();
            HowToBuildAPCHandler.Reset();
            OSHandler.Reset();
            DeliveryHandler.Reset();
            DaySummaryHandler.Reset();
            JobResultHandler.Reset();
            WorkshopSelectionHandler.Reset();
            WorkshopNavigatorHandler.Reset();
            PCBayHandler.Reset();
            HWInfoHandler.Reset();
            EmailAppHandler.Reset();
            ThreedMarkHandler.Reset();
            OCCTHandler.Reset();
            PCStatsHandler.Reset();
            AddProgramHandler.Reset();
            ReviewAppHandler.Reset();
            RankAppHandler.Reset();
            VirusScanAppHandler.Reset();
            MarketAppHandler.Reset();
            BiosHandler.Reset();
            MusicPlayerAppHandler.Reset();
            LightingAppHandler.Reset();
            CalendarHandler.Reset();
            WindowFocusHandler.Reset();
            GPUTunerHandler.Reset();
            HWMonitorHandler.Reset();
            ManifestHandler.Reset();
            KeyBindingMenuHandler.Reset();
            WhatsNewPopUpHandler.Reset();
            NotesAppHandler.Reset();
            FreebuildOptionsHandler.Reset();
            SearchFiltersPanelHandler.Reset();
            TikkITHandler.Reset();
            TabletShopHandler.Reset();
            WalkingStateHandler.Reset();

            if (scene.name == "Menu_V2")
            {
                StartCoroutine(ActivateMainMenuAfterOneFrame());
            }
            else if (scene.name.StartsWith("Workshop_"))
            {
                StartCoroutine(AnnounceSceneLoaded("scene_workshop_loaded"));
            }
        }

        /// <summary>Waits one frame so MainMenu MonoBehaviour has finished Awake/Start
        /// before we try to scan its buttons.</summary>
        private IEnumerator ActivateMainMenuAfterOneFrame()
        {
            yield return null;
            var mm = UnityEngine.Object.FindObjectOfType<MainMenu>();
            if (mm != null) MainMenuHandler.OnMainMenuStart(mm);
        }

        /// <summary>Waits 2 s for the scene to fully initialize, then announces the loc string.</summary>
        private IEnumerator AnnounceSceneLoaded(string locKey)
        {
            yield return new WaitForSeconds(2f);
            ScreenReader.Say(Loc.Get(locKey));
        }

        #endregion

        #region Harmony Patching

        /// <summary>
        /// Applies Harmony patches one by one with individual try-catch.
        /// Prevents a single failing patch (e.g. PCBSInput not yet ready) from
        /// aborting all subsequent patches and crashing Awake().
        /// No LINQ — avoids System.Core resolution issues in Unity Mono.
        /// </summary>
        private static void ApplyPatches(Harmony harmony)
        {
            // Collect types safely — handle partial load gracefully
            Type[] types = GetAssemblyTypes();

            Log.LogInfo($"[PCBSAccess] Scanning {types.Length} types for patches...");
            int ok = 0, fail = 0;
            foreach (var type in types)
            {
                if ((object)type == null) continue;
                try
                {
                    if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0) continue;
                    harmony.CreateClassProcessor(type).Patch();
                    Log.LogInfo($"[PCBSAccess] Patch OK: {type.Name}");
                    ok++;
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"[PCBSAccess] Patch skip: {type.Name} — {ex.Message}");
                    fail++;
                }
            }
            Log.LogInfo($"[PCBSAccess] Patches: {ok} OK, {fail} skipped.");
        }

        private static Type[] GetAssemblyTypes()
        {
            try
            {
                return typeof(Main).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.LogWarning("[PCBSAccess] Partial type load — some patches may be skipped.");
                // Manual filter without LINQ
                int count = 0;
                for (int i = 0; i < e.Types.Length; i++)
                    if ((object)e.Types[i] != null) count++;
                var result = new Type[count];
                int idx = 0;
                for (int i = 0; i < e.Types.Length; i++)
                    if ((object)e.Types[i] != null) result[idx++] = e.Types[i];
                return result;
            }
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
            catch (Exception ex)
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

            if (Input.GetKeyDown(KeyCode.F11))
            {
                DebugLogger.LogInput("F11", "OSOpenWindows");
                string windows = OSHandler.GetOpenWindowNames();
                if (!string.IsNullOrEmpty(windows))
                    ScreenReader.Say(Loc.Get("os_open_windows", windows));
                else
                    ScreenReader.Say(Loc.Get("os_no_windows"));
                return true;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLogger.LogInput("F1", "ContextHelp");
                if (!InputRouter.AnnounceTopContextHelp())
                    ScreenReader.Say(Loc.Get("help_workshop")); // fallback for workshop/walking
                return true;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                DebugLogger.LogInput("S", "CareerStatus");
                AnnounceCareerStatus();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                DebugLogger.LogInput("T", "RepeatTask");
                HowToBuildAPCHandler.AnnounceCurrentTask();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                DebugLogger.LogInput("J", "RepeatJob");
                CareerJobHandler.AnnounceCurrentJob();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                DebugLogger.LogInput("W", "RepeatTooltip");
                WorkshopBuildHandler.AnnounceLastTooltip();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                DebugLogger.LogInput("I", "RepeatInventoryItem");
                InventoryHandler.AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                DebugLogger.LogInput("F", "RepeatSaveSlot");
                SaveLoadMenuHandler.AnnounceCurrentSlot();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                DebugLogger.LogInput("N", "NextObjective");
                JobStatusHandler.AnnounceNextObjective();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                DebugLogger.LogInput("H", "RepeatHWInfo");
                HWInfoHandler.AnnounceLastSummary();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                DebugLogger.LogInput("B", "RepeatBenchmarkOrOCCT");
                if (!string.IsNullOrEmpty(OCCTHandler._lastSensors))
                    OCCTHandler.AnnounceLastSensors();
                else
                    ThreedMarkHandler.AnnounceLastResult();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                DebugLogger.LogInput("P", "RepeatPCStats");
                PCStatsHandler.AnnounceLastSummary();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                DebugLogger.LogInput("E", "RepeatEmail");
                EmailAppHandler.AnnounceCurrentEmail();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                DebugLogger.LogInput("G", "RepeatShopItem");
                ShopHandler.AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                DebugLogger.LogInput("Numpad7", "HWMonitorSensors");
                HWMonitorHandler.AnnounceSensors();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.F5))
            {
                DebugLogger.LogInput(Input.GetKeyDown(KeyCode.F5) ? "F5" : "Numpad2", "GoToCustomerPC");
                WalkingStateHandler.GoToCustomerPC();
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

                string text = $"{Loc.Get("cash")}: {career.GetCash().ToCash()}. " +
                              $"{Loc.Get("kudos")}: {career.GetKudos()}. " +
                              $"{Loc.Get("rating")}: {career.GetStarRating():F1} {Loc.Get("stars")}.";

                ScreenReader.Say(text);

                // In IT Support DLC mode, also announce influencer level.
                if (GameController.IsFuturLabDLC() || GameController.IsITSupportDLC())
                {
                    try
                    {
                        int level = LevelProgression.GetZeroIndexedLevel(career.GetKudos()) + 1;
                        ScreenReader.Say(Loc.Get("status_it_level", level), interrupt: false);
                    }
                    catch { /* LevelProgression may not be available outside DLC scenes */ }
                }

                // Announce today's date and the next 3 upcoming calendar events.
                AnnounceUpcomingEvents(career);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"AnnounceCareerStatus failed: {ex.Message}");
            }
        }

        private void AnnounceUpcomingEvents(CareerStatus career)
        {
            try
            {
                Calendar cal = career.GetCalendar();
                if (cal == null) return;

                int today = cal.GetToday();
                string dateStr = cal.GetDateString(today, "d");
                ScreenReader.Say(Loc.Get("status_today", dateStr), interrupt: false);

                // Look ahead up to 7 days for events.
                int found = 0;
                for (int d = today; d <= today + 7 && found < 3; d++)
                {
                    foreach (CalendarEvent ev in cal.GetEventsOnDay(d))
                    {
                        if (!ev.IsVisible()) continue;
                        string desc = ev.GetDescription();
                        if (string.IsNullOrEmpty(desc)) continue;

                        string when = d == today
                            ? Loc.Get("status_event_today", desc)
                            : Loc.Get("status_event_in", cal.GetDateString(d, "d"), desc);

                        ScreenReader.Say(when, interrupt: false);
                        found++;
                        if (found >= 3) break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"AnnounceUpcomingEvents failed: {ex.Message}");
            }
        }

        #endregion
    }
}
