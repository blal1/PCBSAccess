using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for context menu keyboard accessibility.
    /// Detects when context menus open (via Harmony patches), provides
    /// Up/Down navigation, Enter to select, Escape to close.
    /// Also triggers context menus via Shift+F10 in terminal/desktop contexts.
    /// </summary>
    public class ContextMenuHandler
    {
        #region Types

        private struct MenuItem
        {
            public GameObject Object;
            public string Label;
            public bool Interactable;
        }

        #endregion

        #region Fields

        private static ContextMenuHandler _instance;

        private bool _active;
        private ContextualMenu _trackedMenu;
        private List<MenuItem> _items = new();
        private int _currentIndex;

        #endregion

        #region Static Entry

        /// <summary>
        /// Called by Harmony patch when any context menu opens.
        /// </summary>
        public static void OnContextMenuOpened(ContextualMenu menu)
        {
            if (_instance == null) return;
            _instance.ActivateMenu(menu);
        }

        /// <summary>
        /// Called by Harmony patch when all context menus are cleared.
        /// </summary>
        public static void OnContextMenusCleared()
        {
            if (_instance == null) return;
            _instance.Deactivate(true);
        }

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether a context menu is currently active and consuming input.
        /// </summary>
        public bool IsActive => _active;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update(). Returns true if input was consumed.
        /// </summary>
        public bool Update()
        {
            if (!_active) return false;

            // Check if menu was destroyed or cleared
            if (_trackedMenu == null || _items.Count == 0)
            {
                Deactivate(false);
                return false;
            }

            // Verify first item still exists (menu may have been cleared)
            if (_items[0].Object == null)
            {
                Deactivate(false);
                return false;
            }

            // Down = next item
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Navigate(1);
                return true;
            }

            // Up = previous item
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Navigate(-1);
                return true;
            }

            // Enter = select current item
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SelectCurrentItem();
                return true;
            }

            // Escape = close menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
                return true;
            }

            // Space = repeat current item
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceCurrentItem();
                return true;
            }

            // Consume other keys to prevent pass-through
            return false;
        }

        /// <summary>
        /// Attempts to open a context menu for the current context.
        /// Called when Shift+F10 is pressed outside file explorer.
        /// </summary>
        public void TriggerContextMenu()
        {
            // Try terminal context first
            if (TryOpenTerminalContextMenu()) return;

            // No context available
            ScreenReader.Say(Loc.Get("context_no_context"));
        }

        /// <summary>
        /// Returns help text for F1 during context menu navigation.
        /// </summary>
        public string GetHelpText()
        {
            return Loc.Get("context_help");
        }

        #endregion

        #region Activation

        private void ActivateMenu(ContextualMenu menu)
        {
            _trackedMenu = menu;
            _items.Clear();
            _currentIndex = 0;

            // Collect menu items from the menu's children
            foreach (Transform child in menu.transform)
            {
                if (child == null || !child.gameObject.activeInHierarchy) continue;

                var option = child.GetComponent<OpcionContextual>();
                if (option == null) continue;

                var button = child.GetComponent<Button>();
                var tmp = child.GetComponentInChildren<TMP_Text>();
                string label = tmp != null ? tmp.text : child.gameObject.name;
                bool interactable = button != null && button.interactable;

                _items.Add(new MenuItem
                {
                    Object = child.gameObject,
                    Label = label,
                    Interactable = interactable
                });
            }

            if (_items.Count == 0)
            {
                _trackedMenu = null;
                return;
            }

            _active = true;

            // Announce menu opened
            ScreenReader.Say(Loc.Get("context_opened", _items.Count));
            AnnounceCurrentItem();

            DebugLogger.LogState($"ContextMenuHandler: activated with {_items.Count} items");
        }

        private void Deactivate(bool announce)
        {
            if (!_active) return;

            _active = false;
            _trackedMenu = null;
            _items.Clear();
            _currentIndex = 0;

            if (announce)
            {
                ScreenReader.Say(Loc.Get("context_closed"));
            }

            DebugLogger.LogState("ContextMenuHandler: deactivated");
        }

        #endregion

        #region Navigation

        private void Navigate(int direction)
        {
            if (_items.Count == 0) return;

            _currentIndex += direction;
            if (_currentIndex >= _items.Count) _currentIndex = 0;
            if (_currentIndex < 0) _currentIndex = _items.Count - 1;

            AnnounceCurrentItem();
        }

        private void SelectCurrentItem()
        {
            if (_currentIndex < 0 || _currentIndex >= _items.Count) return;

            var item = _items[_currentIndex];
            if (!item.Interactable)
            {
                ScreenReader.Say(Loc.Get("context_item_disabled", item.Label, _currentIndex + 1, _items.Count));
                return;
            }

            // Click the option
            var option = item.Object.GetComponent<OpcionContextual>();
            if (option != null)
            {
                DebugLogger.LogState($"ContextMenuHandler: selecting '{item.Label}'");
                option.OptionSelected();
            }
        }

        private void CloseMenu()
        {
            if (ControlContextualMenus.Singleton != null)
            {
                ControlContextualMenus.Singleton.ClearMenus();
            }
            // Deactivate will be called by the ClearMenus patch
        }

        private void AnnounceCurrentItem()
        {
            if (_currentIndex < 0 || _currentIndex >= _items.Count) return;

            var item = _items[_currentIndex];
            if (item.Interactable)
            {
                ScreenReader.Say(Loc.Get("context_item", item.Label, _currentIndex + 1, _items.Count));
            }
            else
            {
                ScreenReader.Say(Loc.Get("context_item_disabled", item.Label, _currentIndex + 1, _items.Count));
            }
        }

        #endregion

        #region Context Menu Triggers

        private bool TryOpenTerminalContextMenu()
        {
            if (ContextualMenuTerminal.Singleton == null) return false;

            // Find a focused terminal
            var terminals = Object.FindObjectsOfType<ContextualTerminal>();
            foreach (var ct in terminals)
            {
                if (ct.selectableTerminal == null) continue;

                // Check if the terminal's selectable is currently selected
                var eventSystem = UnityEngine.EventSystems.EventSystem.current;
                if (eventSystem != null && eventSystem.currentSelectedGameObject == ct.selectableTerminal.gameObject)
                {
                    ControlContextualMenus.Singleton?.ClearMenus();
                    ContextualMenuTerminal.Singleton.OpenMenu(ct.options, ct.terminalListAdapter, ct.selectableTerminal);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
