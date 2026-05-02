using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Shared utilities for button label extraction and safe invocation.
    /// Centralises two patterns that every navigating handler needs.
    /// </summary>
    public static class ButtonHelper
    {
        /// <summary>
        /// Returns a human-readable label for a button.
        /// Priority: Text child → TextMeshProUGUI child → GameObject name.
        /// Never returns null or empty — icon-only buttons surface their object name.
        /// </summary>
        public static string GetLabel(Button btn)
        {
            if (btn == null) return string.Empty;

            Text txt = btn.GetComponentInChildren<Text>(includeInactive: false);
            if (txt != null && !string.IsNullOrEmpty(txt.text)) return txt.text;

            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(includeInactive: false);
            if (tmp != null && !string.IsNullOrEmpty(tmp.text)) return tmp.text;

            return btn.gameObject.name;
        }

        public static void Click(Button btn)
        {
            if (btn == null || !btn.IsInteractable()) return;
            try
            {
                if (EventSystem.current != null)
                {
                    var ped = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
                    ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.submitHandler);
                }
                else
                {
                    // Fallback if there's no EventSystem
                    btn.onClick.Invoke();
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ButtonHelper.Click '{btn.gameObject.name}': {ex.Message}");
            }
        }
    }
}
