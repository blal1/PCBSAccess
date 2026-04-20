namespace PCBSAccess
{
    /// <summary>
    /// Categorized debug logger. All output is gated behind Main.DebugMode (F12 toggle).
    /// Zero overhead when debug mode is off.
    /// </summary>
    public static class DebugLogger
    {
        /// <summary>Log what the screen reader announces.</summary>
        public static void LogScreenReader(string text)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[SR] {text}");
        }

        /// <summary>Log a key press and what action it triggered.</summary>
        public static void LogInput(string key, string action = null)
        {
            if (!Main.DebugMode) return;
            string msg = action != null ? $"{key} -> {action}" : key;
            Main.Log.LogInfo($"[INPUT] {msg}");
        }

        /// <summary>Log a screen or menu state change.</summary>
        public static void LogState(string description)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[STATE] {description}");
        }

        /// <summary>Log a value read from the game.</summary>
        public static void LogGame(string name, object value)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[GAME] {name} = {value}");
        }

        /// <summary>Log a handler decision or action.</summary>
        public static void LogHandler(string handler, string message)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[HANDLER] [{handler}] {message}");
        }

        /// <summary>Log a warning (null check failures, unexpected state). Always logs, not gated by DebugMode.</summary>
        public static void LogWarning(string message)
        {
            Main.Log?.LogWarning($"[WARN] {message}");
        }
    }
}
