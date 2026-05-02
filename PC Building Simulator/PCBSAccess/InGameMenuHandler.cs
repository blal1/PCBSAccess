using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the in-game pause menu and navigates its buttons.
    ///
    /// Up/Down: navigate visible buttons (top to bottom).
    /// Home: jump to first button.
    /// End:  jump to last button.
    /// Enter: activate the focused button.
    /// Announces menu name and button list on open.
    ///
    /// Open/close detection via polling activeSelf (PollState).
    /// Key handling via IInputContext (Context.HandleInput).
    /// </summary>
    public static class InGameMenuHandler
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
            public string ContextName => "InGameMenu";
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
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_ingamemenu")); }
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
            bool isOpen = IsMenuOpen();
            if (isOpen && !_wasOpen)  OnOpen();
            if (!isOpen && _wasOpen)  OnClose();
            _wasOpen = isOpen;
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _buttons.Clear();
            _focusIndex = -1;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Open / Close

        private static bool IsMenuOpen()
        {
            try
            {
                return CommonUI.inGameMenu != null && CommonUI.inGameMenu.gameObject.activeSelf;
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

                if (_buttons.Count == 0)
                {
                    ScreenReader.Say(Loc.Get("ingamemenu_open_empty"));
                    return;
                }

                ScreenReader.Say(Loc.Get("ingamemenu_open"));
                ScreenReader.Say(Loc.Get("ingamemenu_nav_hint"), interrupt: false);

                DebugLogger.LogState($"InGameMenuHandler: open, {_buttons.Count} buttons");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InGameMenuHandler.OnOpen: {ex.Message}");
            }
        }

        private static void OnClose()
        {
            _active     = false;
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
            ScreenReader.Say($"{Loc.Get("ingamemenu_label")}, {pos} {Loc.Get("nav_of")} {total}: {name}");
            DebugLogger.LogState($"InGameMenuHandler: {pos}/{total} '{name}'");
        }

        #endregion

        #region Button collection

        private static List<Button> CollectButtons()
        {
            var result = new List<Button>();
            try
            {
                InGameMenu menu = CommonUI.inGameMenu;
                if (menu == null) return result;

                Button[] allButtons = menu.GetComponentsInChildren<Button>(includeInactive: false);
                foreach (Button btn in allButtons)
                {
                    if (btn.gameObject.activeSelf && btn.IsInteractable())
                        result.Add(btn);
                }

                result.Sort((a, b) =>
                    b.transform.position.y.CompareTo(a.transform.position.y));
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InGameMenuHandler.CollectButtons: {ex.Message}");
            }
            return result;
        }

        #endregion
    }
}
