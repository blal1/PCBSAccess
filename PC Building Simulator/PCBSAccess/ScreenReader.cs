using System;
using System.Runtime.InteropServices;

namespace PCBSAccess
{
    /// <summary>
    /// Wrapper for Tolk screen reader bridge library.
    /// Requires Tolk.dll and nvdaControllerClient64.dll in the game folder.
    /// </summary>
    public static class ScreenReader
    {
        #region Native Imports

        [DllImport("Tolk.dll")]
        private static extern void Tolk_Load();

        [DllImport("Tolk.dll")]
        private static extern void Tolk_Unload();

        [DllImport("Tolk.dll")]
        private static extern bool Tolk_IsLoaded();

        [DllImport("Tolk.dll")]
        private static extern bool Tolk_HasSpeech();

        [DllImport("Tolk.dll", CharSet = CharSet.Unicode)]
        private static extern bool Tolk_Output(string text, bool interrupt);

        [DllImport("Tolk.dll")]
        private static extern bool Tolk_Silence();

        [DllImport("Tolk.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr Tolk_DetectScreenReader();

        #endregion

        #region Fields

        private static bool _available = false;
        private static bool _initialized = false;

        #endregion

        #region Public API

        /// <summary>Initializes Tolk. Call once at mod startup from Main.Awake().</summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                Tolk_Load();
                _available = Tolk_IsLoaded() && Tolk_HasSpeech();

                if (_available)
                {
                    IntPtr srPtr = Tolk_DetectScreenReader();
                    string srName = srPtr != IntPtr.Zero
                        ? Marshal.PtrToStringUni(srPtr)
                        : "Unknown";
                    Main.Log.LogInfo($"[ScreenReader] Detected: {srName}");
                }
                else
                {
                    Main.Log.LogWarning("[ScreenReader] No screen reader detected or Tolk not available.");
                }
            }
            catch (DllNotFoundException)
            {
                Main.Log.LogError("[ScreenReader] Tolk.dll not found. Place Tolk.dll and nvdaControllerClient64.dll in the game folder.");
                _available = false;
            }
            catch (Exception ex)
            {
                Main.Log.LogError($"[ScreenReader] Init failed: {ex.Message}");
                _available = false;
            }

            _initialized = true;
        }

        /// <summary>
        /// Speaks text via the active screen reader.
        /// When Main.DebugMode is true, also logs the announcement.
        /// </summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="interrupt">If true (default), cuts off current speech before speaking.</param>
        public static void Say(string text, bool interrupt = true)
        {
            if (string.IsNullOrEmpty(text)) return;

            DebugLogger.LogScreenReader(text);

            if (!_available) return;

            try
            {
                Tolk_Output(text, interrupt);
            }
            catch (Exception ex)
            {
                Main.Log.LogWarning($"[ScreenReader] Say failed: {ex.Message}");
            }
        }

        /// <summary>Queues text after current speech finishes (interrupt = false).</summary>
        public static void SayQueued(string text) => Say(text, false);

        /// <summary>Stops current speech immediately.</summary>
        public static void Stop()
        {
            if (!_available) return;
            try { Tolk_Silence(); }
            catch (Exception ex) { Main.Log?.LogWarning($"[ScreenReader] Stop failed: {ex.Message}"); }
        }

        /// <summary>Shuts down Tolk. Call from Main.OnDestroy().</summary>
        public static void Shutdown()
        {
            if (!_initialized) return;
            try { Tolk_Unload(); }
            catch (Exception ex) { Main.Log?.LogWarning($"[ScreenReader] Shutdown failed: {ex.Message}"); }
            _initialized = false;
            _available = false;
        }

        /// <summary>True if a screen reader was detected and Tolk initialized successfully.</summary>
        public static bool IsAvailable => _available;

        #endregion
    }
}
