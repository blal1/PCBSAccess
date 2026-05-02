using System;
using System.Collections.Generic;
using System.Reflection;
using PCBS;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the workshop selection carousel and enables keyboard navigation.
    ///
    /// Up/Down/Home/End: navigate workshops.
    /// Enter (first press): select workshop — shows preview, announces name + confirm hint.
    /// Enter (second press): confirm selection and load the workshop.
    /// Escape: cancel confirmation or close the menu.
    /// Locked workshops are announced as locked; confirming them opens the DLC store prompt.
    /// </summary>
    public static class WorkshopSelectionHandler
    {
        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static List<WorkshopSelectionButton> _buttons = new List<WorkshopSelectionButton>();
        private static int _focusIndex = -1;
        private static bool _confirming;             // true after a workshop is selected, waiting for Enter to confirm
        private static WorkshopSelectionMenu _menu;  // reference saved in Init patch

        // Cached reflection — set once in Init patch
        private static MethodInfo _onWorkshopButtonClickedMethod;

        /// <summary>Announcement text queued in the Init patch, spoken once the menu becomes visible.</summary>
        private static string _pendingAnnouncement;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "WorkshopSelection";
            public bool IsActive => _active && IsOpen();

            public bool HandleInput()
            {
                if (_confirming)
                {
                    if (Input.GetKeyDown(KeyCode.Return))  { ConfirmWorkshop(); return true; }
                    if (Input.GetKeyDown(KeyCode.Escape))  { CancelConfirm();   return true; }
                    return false;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);                    return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);                   return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);                  return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_buttons.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Return))     { SelectFocused();                return true; }
                if (Input.GetKeyDown(KeyCode.Escape))     { GoBack();                       return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_workshopsel")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>True while the workshop selection overlay is open. Used by Main for F1 context.</summary>
        public static bool IsActive => _active;

        /// <summary>
        /// Polls for open/close (edge detection). Called from Main.Update as PollState().
        /// No key reading — that happens in Context.HandleInput().
        /// </summary>
        public static void PollState()
        {
            bool isOpen = IsOpen();
            if (isOpen && !_wasOpen && !_active)
            {
                // Menu just became visible — push context and announce
                _active = true;
                InputRouter.Push(_ctx);
                if (_pendingAnnouncement != null)
                {
                    ScreenReader.Say(_pendingAnnouncement);
                    ScreenReader.Say(Loc.Get("daysummary_nav_hint"), interrupt: false);
                    _pendingAnnouncement = null;
                }
                DebugLogger.LogState($"WorkshopSelectionHandler: menu became visible, {_buttons.Count} workshops");
            }
            else if (!isOpen && _wasOpen) OnClose();
            _wasOpen = isOpen;
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _active              = false;
            _wasOpen             = false;
            _confirming          = false;
            _focusIndex          = -1;
            _pendingAnnouncement = null;
            _buttons.Clear();
            _menu       = null;
            _onWorkshopButtonClickedMethod = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Open / Close

        private static bool IsOpen()
        {
            try
            {
                return _menu != null && _menu.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void OnClose()
        {
            _active     = false;
            _confirming = false;
            _focusIndex = -1;
            _buttons.Clear();
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int direction)
        {
            if (_buttons.Count == 0) return;
            int next = Mathf.Clamp(_focusIndex + direction, 0, _buttons.Count - 1);
            if (next == _focusIndex)
            {
                ScreenReader.Say(direction > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceButton(_buttons[_focusIndex], _focusIndex + 1, _buttons.Count);
        }

        private static void NavigateTo(int index)
        {
            if (_buttons.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _buttons.Count - 1);
            AnnounceButton(_buttons[_focusIndex], _focusIndex + 1, _buttons.Count);
        }

        #endregion

        #region Selection & Confirmation

        /// <summary>
        /// First Enter press: calls OnWorkshopButtonClicked via Reflection, which shows the preview
        /// panel. Avoids invoking Button.onClick directly (which would trigger OnPointerExit
        /// with a null delegate and throw NullReferenceException).
        /// </summary>
        private static void SelectFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _buttons.Count) return;
            WorkshopSelectionButton wsb = _buttons[_focusIndex];
            if (wsb == null) return;

            try
            {
                if (_menu == null || (object)_onWorkshopButtonClickedMethod == null)
                {
                    DebugLogger.LogWarning("WorkshopSelectionHandler.SelectFocused: menu or method ref lost.");
                    return;
                }

                // Call private OnWorkshopButtonClicked — sets m_selectedWorkshopData and shows preview
                _onWorkshopButtonClickedMethod.Invoke(_menu, new object[] { wsb });

                _confirming = true;
                string name   = FormatSceneName(wsb.WorkshopData.sceneName);
                string locked = wsb.IsLocked
                    ? $" {Loc.Get("shop_locked")}. Press Enter to open DLC store, Escape to cancel."
                    : " Press Enter to confirm, Escape to cancel.";
                ScreenReader.Say($"{name} selected.{locked}");
                DebugLogger.LogState($"WorkshopSelectionHandler: selected '{name}' locked={wsb.IsLocked}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"WorkshopSelectionHandler.SelectFocused: {ex.Message}");
            }
        }

        /// <summary>Second Enter press: confirm and load the workshop (or open DLC store if locked).</summary>
        private static void ConfirmWorkshop()
        {
            if (_menu == null) return;
            try
            {
                _menu.OnConfirmWorkshop();
                _confirming = false;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"WorkshopSelectionHandler.ConfirmWorkshop: {ex.Message}");
            }
        }

        /// <summary>Escape while confirming: cancel back to the workshop list.</summary>
        private static void CancelConfirm()
        {
            if (_menu == null) return;
            try
            {
                _menu.OnBack();
                _confirming = false;
                // Re-announce the focused workshop so user knows where they are
                if (_focusIndex >= 0 && _focusIndex < _buttons.Count)
                    AnnounceButton(_buttons[_focusIndex], _focusIndex + 1, _buttons.Count);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"WorkshopSelectionHandler.CancelConfirm: {ex.Message}");
            }
        }

        /// <summary>Escape while in selection mode: close the workshop menu.</summary>
        private static void GoBack()
        {
            if (_menu == null) return;
            try { _menu.OnBack(); }
            catch (Exception ex) { DebugLogger.LogWarning($"WorkshopSelectionHandler.GoBack: {ex.Message}"); }
        }

        #endregion

        #region Announce

        private static void AnnounceButton(WorkshopSelectionButton btn, int pos, int total)
        {
            if (btn == null) return;
            try
            {
                string name   = FormatSceneName(btn.WorkshopData.sceneName);
                string locked = btn.IsLocked ? $" {Loc.Get("shop_locked")}." : string.Empty;
                ScreenReader.Say($"{Loc.Get("workshop_sel_label")}, {pos} {Loc.Get("nav_of")} {total}: {name}.{locked}");
                DebugLogger.LogState($"WorkshopSelectionHandler: {pos}/{total} '{name}' locked={btn.IsLocked}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"WorkshopSelectionHandler.AnnounceButton: {ex.Message}");
            }
        }

        /// <summary>Converts a sceneName like "Workshop_MyGarage" to "My Garage".</summary>
        private static string FormatSceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return Loc.Get("ingamemenu_unknown");
            const string prefix = "Workshop_";
            string name = sceneName.StartsWith(prefix)
                ? sceneName.Substring(prefix.Length)
                : sceneName;
            return name.Replace('_', ' ').Trim();
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires after WorkshopSelectionMenu.Init sets up all pages and buttons.
        /// Saves menu reference, caches reflection, collects buttons, auto-focuses first.
        /// </summary>
        [HarmonyPatch(typeof(WorkshopSelectionMenu), "Init")]
        static class WorkshopSelectionMenu_Init_Patch
        {
            static void Postfix(WorkshopSelectionMenu __instance)
            {
                try
                {
                    _menu       = __instance;
                    _confirming = false;
                    _focusIndex = -1;
                    _buttons.Clear();

                    // Cache the private method once
                    _onWorkshopButtonClickedMethod = typeof(WorkshopSelectionMenu).GetMethod(
                        "OnWorkshopButtonClicked",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if ((object)_onWorkshopButtonClickedMethod == null)
                        DebugLogger.LogWarning("WorkshopSelectionHandler: OnWorkshopButtonClicked not found via Reflection.");

                    WorkshopSelectionButton[] all =
                        __instance.GetComponentsInChildren<WorkshopSelectionButton>(includeInactive: true);

                    foreach (WorkshopSelectionButton btn in all)
                        _buttons.Add(btn);

                    int count = _buttons.Count;
                    if (count == 0)
                    {
                        _pendingAnnouncement = Loc.Get("workshop_sel_empty");
                        DebugLogger.LogState("WorkshopSelectionHandler: init (no workshops), waiting for visibility");
                        return;
                    }

                    // Auto-focus the first workshop so user immediately hears its name
                    _focusIndex = 0;
                    string firstName = FormatSceneName(_buttons[0].WorkshopData.sceneName);
                    string firstLock = _buttons[0].IsLocked ? $" {Loc.Get("shop_locked")}." : string.Empty;

                    // Store announcement — will be spoken by PollState() once IsOpen() becomes true
                    _pendingAnnouncement =
                        $"{Loc.Get("workshop_sel_open", count)} " +
                        $"{Loc.Get("workshop_sel_label")}, 1 {Loc.Get("nav_of")} {count}: {firstName}.{firstLock}";

                    DebugLogger.LogState($"WorkshopSelectionHandler: init, {count} workshops, focused=0 '{firstName}', waiting for visibility");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkshopSelectionMenu_Init_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
