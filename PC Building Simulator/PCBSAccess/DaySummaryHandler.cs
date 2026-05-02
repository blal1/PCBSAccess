using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the day summary screen (shown each morning and after advancing the day).
    ///
    /// On open: announces current date + cash + kudos.
    /// Up/Down: navigate visible buttons.
    /// Home/End: jump to first/last button.
    /// Enter: activate the focused button.
    ///
    /// Open/close detection via polling activeSelf (PollState).
    /// Key handling via IInputContext (Context.HandleInput).
    /// </summary>
    public static class DaySummaryHandler
    {
        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static List<Button> _buttons = new List<Button>();
        private static int _focusIndex = -1;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "DaySummary";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);                          return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);                         return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);                        return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_buttons.Count - 1);       return true; }
                if (Input.GetKeyDown(KeyCode.Return))     { ActivateFocused();                    return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_daysummary")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>
        /// Polls for open/close. Called from Main.Update each frame (replaces former Update()).
        /// No key handling here — that is in Context.HandleInput().
        /// </summary>
        public static void PollState()
        {
            bool isOpen = IsOpen();
            if (isOpen && !_wasOpen)  OnOpen();
            if (!isOpen && _wasOpen)  OnClose();
            _wasOpen = isOpen;
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _buttons.Clear();
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Open / Close

        private static bool IsOpen()
        {
            try
            {
                return WorkshopUI.m_daySummary != null
                    && WorkshopUI.m_daySummary.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void OnOpen()
        {
            try
            {
                _active     = true;
                _focusIndex = -1;
                _buttons    = CollectButtons();
                InputRouter.Push(_ctx);

                string date  = GetDateString();
                string cash  = CareerStatus.Get()?.GetCash().ToCash() ?? string.Empty;
                int    kudos = CareerStatus.Get()?.GetKudos() ?? 0;

                string intro = Loc.Get("daysummary_open", date, cash, kudos);
                ScreenReader.Say(intro);

                if (_buttons.Count > 0)
                    ScreenReader.Say(Loc.Get("daysummary_nav_hint"), interrupt: false);

                DebugLogger.LogState($"DaySummaryHandler: open, {_buttons.Count} buttons, date={date}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"DaySummaryHandler.OnOpen: {ex.Message}");
            }
        }

        private static void OnClose()
        {
            _active     = false;
            _focusIndex = -1;
            _buttons.Clear();
            InputRouter.Pop(_ctx);
        }

        private static string GetDateString()
        {
            try
            {
                var cal = CareerStatus.Get()?.GetCalendar();
                if (cal == null) return string.Empty;
                return cal.GetDateString(cal.GetToday(), "d");
            }
            catch { return string.Empty; }
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

        private static void ActivateFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _buttons.Count) return;
            ButtonHelper.Click(_buttons[_focusIndex]);
        }

        #endregion

        #region Announce

        private static void AnnounceButton(Button btn, int pos, int total)
        {
            if (btn == null) return;
            string name = ButtonHelper.GetLabel(btn);
            ScreenReader.Say($"{Loc.Get("daysummary_label")}, {pos} {Loc.Get("nav_of")} {total}: {name}");
            DebugLogger.LogState($"DaySummaryHandler: button {pos}/{total} '{name}'");
        }

        #endregion

        #region Button collection

        private static List<Button> CollectButtons()
        {
            var result = new List<Button>();
            try
            {
                DaySummary ds = WorkshopUI.m_daySummary;
                if (ds == null) return result;

                Button[] all = ds.GetComponentsInChildren<Button>(includeInactive: false);
                foreach (Button btn in all)
                {
                    if (btn.gameObject.activeSelf && btn.IsInteractable())
                        result.Add(btn);
                }

                result.Sort((a, b) =>
                    b.transform.position.y.CompareTo(a.transform.position.y));
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"DaySummaryHandler.CollectButtons: {ex.Message}");
            }
            return result;
        }

        #endregion
    }
}
