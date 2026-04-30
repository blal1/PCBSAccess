# Code Index for ButtonHelper.cs

- Line 9: /// <summary>
- Line 10: /// Shared utilities for button label extraction and safe invocation.
- Line 11: /// Centralises two patterns that every navigating handler needs.
- Line 12: /// </summary>
- Line 13: public static class ButtonHelper
- Line 15: /// <summary>
- Line 16: /// Returns a human-readable label for a button.
- Line 17: /// Priority: Text child → TextMeshProUGUI child → GameObject name.
- Line 18: /// Never returns null or empty — icon-only buttons surface their object name.
- Line 19: /// </summary>
- Line 20: public static string GetLabel(Button btn)
- Line 33: public static void Click(Button btn)
