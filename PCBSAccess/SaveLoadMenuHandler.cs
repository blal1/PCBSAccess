using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces save/load menu state and navigates save slots.
    ///
    /// Up/Down arrows: navigate save slots.
    /// F6 (wired in Main): re-read the focused save slot.
    /// Announces menu title and slot count on open.
    /// </summary>
    public static class SaveLoadMenuHandler
    {
        #region State

        private static bool _active;
        private static int _focusIndex = -1;
        private static string _lastSlotSummary;

        // Cached reflection
        private static FieldInfo _openForSaveField;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "SaveLoad";
            public bool IsActive => _active && IsMenuOpen();

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow)) { Navigate(1);  return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))   { Navigate(-1); return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_saveload")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Re-reads the focused save slot. Called from Main on F6.</summary>
        public static void AnnounceCurrentSlot()
        {
            if (!IsMenuOpen()) return;

            if (!string.IsNullOrEmpty(_lastSlotSummary))
                ScreenReader.Say(Loc.Get("save_reread", _lastSlotSummary));
            else
                ScreenReader.Say(Loc.Get("save_no_slot"));
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _active = false;
            _focusIndex = -1;
            _lastSlotSummary = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Helpers

        private static bool IsMenuOpen()
        {
            try
            {
                return CommonUI.saveMenu != null && CommonUI.saveMenu.gameObject.activeSelf;
            }
            catch
            {
                return false;
            }
        }

        private static SaveGameDisplay[] GetSlots()
        {
            try
            {
                var menu = CommonUI.saveMenu;
                if (menu == null) return null;
                return menu.m_scrollRect.content.GetComponentsInChildren<SaveGameDisplay>(includeInactive: false);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"SaveLoadMenuHandler.GetSlots: {ex.Message}");
                return null;
            }
        }

        private static bool IsOpenForSave(SaveLoadMenu menu)
        {
            try
            {
                if ((object)_openForSaveField == null)
                    _openForSaveField = typeof(SaveLoadMenu).GetField("m_openForSave",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                return (object)_openForSaveField != null && (bool)_openForSaveField.GetValue(menu);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Navigation

        private static void Navigate(int direction)
        {
            try
            {
                SaveGameDisplay[] slots = GetSlots();
                if (slots == null || slots.Length == 0)
                {
                    ScreenReader.Say(Loc.Get("save_no_slots"));
                    return;
                }

                int next = Mathf.Clamp(_focusIndex + direction, 0, slots.Length - 1);

                if (next == _focusIndex)
                {
                    AnnounceSlot(slots[next]);
                    return;
                }

                _focusIndex = next;
                AnnounceSlot(slots[_focusIndex]);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"SaveLoadMenuHandler.Navigate: {ex.Message}");
            }
        }

        private static void AnnounceSlot(SaveGameDisplay slot)
        {
            if (slot == null) return;
            try
            {
                string name = slot.saveName.text;
                string date = slot.saveDate.text;
                string text = $"{name}. {date}.";
                _lastSlotSummary = text;
                ScreenReader.Say(text);
                DebugLogger.LogState($"SaveLoadMenuHandler: '{name}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"SaveLoadMenuHandler.AnnounceSlot: {ex.Message}");
            }
        }

        #endregion

        #region Patches

        /// <summary>Fires when the save/load menu is opened. Announces title and slot count.</summary>
        [HarmonyPatch(typeof(SaveLoadMenu), "Open")]
        static class SaveLoadMenu_Open_Patch
        {
            static void Postfix(SaveLoadMenu __instance)
            {
                try
                {
                    _active = true;
                    _focusIndex = -1;
                    _lastSlotSummary = null;
                    InputRouter.Push(_ctx);

                    bool forSave = IsOpenForSave(__instance);
                    SaveGameDisplay[] slots = GetSlots();
                    int count = slots != null ? slots.Length : 0;

                    string title = forSave ? Loc.Get("save_menu_save") : Loc.Get("save_menu_load");
                    string text = count == 0
                        ? $"{title}. {Loc.Get("save_no_slots")}"
                        : $"{title}. {count} {Loc.Get("save_slots")}.";

                    ScreenReader.Say(text);
                    DebugLogger.LogState($"SaveLoadMenuHandler: {title}, {count} slots");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SaveLoadMenu_Open_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
