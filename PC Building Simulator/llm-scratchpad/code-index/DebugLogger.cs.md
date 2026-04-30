# Code Index for DebugLogger.cs

- Line 3: /// <summary>
- Line 4: /// Categorized debug logger. All output is gated behind Main.DebugMode (F12 toggle).
- Line 5: /// Zero overhead when debug mode is off.
- Line 6: /// </summary>
- Line 7: public static class DebugLogger
- Line 9: /// <summary>Log what the screen reader announces.</summary>
- Line 10: public static void LogScreenReader(string text)
- Line 16: /// <summary>Log a key press and what action it triggered.</summary>
- Line 17: public static void LogInput(string key, string action = null)
- Line 24: /// <summary>Log a screen or menu state change.</summary>
- Line 25: public static void LogState(string description)
- Line 31: /// <summary>Log a value read from the game.</summary>
- Line 32: public static void LogGame(string name, object value)
- Line 38: /// <summary>Log a handler decision or action.</summary>
- Line 39: public static void LogHandler(string handler, string message)
- Line 45: /// <summary>Log a warning (null check failures, unexpected state). Always logs, not gated by DebugMode.</summary>
- Line 46: public static void LogWarning(string message)
