using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for router port forwarding configuration.
    /// Browse mode: Up/Down to navigate, Enter to edit.
    /// Edit mode: Tab to cycle fields, Enter to save, Escape to cancel.
    /// </summary>
    public class RouterPortSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private List<ItemPortForward> _items = new();
        private bool _editMode;
        private int _editFieldIndex;
        private ItemPortForward _editingItem;

        private static readonly string[] _fieldNames = { "External port", "Internal port", "LAN IP" };

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            _editMode = false;
            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _editMode = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;
            return _editMode ? HandleEditInput() : HandleBrowseInput();
        }

        public void AnnounceState()
        {
            RefreshItems();
            if (_items.Count == 0)
                ScreenReader.Say(Loc.Get("router_port_empty"));
            else
                ScreenReader.Say(Loc.Get("router_ports", _items.Count));
        }

        public string GetPanelName() => "Port Forwarding";

        public string GetHelpText() => Loc.Get("router_port_help");

        /// <summary>Called when router config is loaded.</summary>
        public void OnRouterConfigLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            _currentIndex = 0;
            _editMode = false;
            RefreshItems();
            AnnounceState();
        }

        private bool HandleBrowseInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentRule();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentRule();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                    EnterEditMode(_items[_currentIndex]);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                var configUI = _browser.GetComponentInChildren<RouterConfigUI>();
                if (configUI != null)
                {
                    configUI.AddEntry();
                    RefreshItems();
                    if (_items.Count > 0)
                    {
                        _currentIndex = _items.Count - 1;
                        EnterEditMode(_items[_currentIndex]);
                    }
                }
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    var toggle = typeof(ItemPortForward).GetField("selection", BindingFlags.NonPublic | BindingFlags.Instance);
                    var sel = toggle?.GetValue(_items[_currentIndex]) as UnityEngine.UI.Toggle;
                    if (sel != null) sel.isOn = true;

                    var configUI = _browser.GetComponentInChildren<RouterConfigUI>();
                    configUI?.RemoveSelected();
                    ScreenReader.Say(Loc.Get("router_port_deleted", 1));
                    RefreshItems();
                    _currentIndex = Mathf.Min(_currentIndex, _items.Count - 1);
                }
                return true;
            }

            return false;
        }

        private bool HandleEditInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _editFieldIndex = (_editFieldIndex + 1) % 3;
                AnnounceEditField();
                FocusEditField();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _editingItem.SaveChanges();
                ScreenReader.Say(Loc.Get("router_port_saved"));
                _editMode = false;
                RefreshItems();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _editMode = false;
                ScreenReader.Say(Loc.Get("router_port_cancelled"));
                return true;
            }

            return false;
        }

        private void EnterEditMode(ItemPortForward item)
        {
            var protectedField = typeof(ItemPortForward).GetField("protectedIcon", BindingFlags.Public | BindingFlags.Instance);
            var icon = protectedField?.GetValue(item) as GameObject;
            if (icon != null && icon.activeInHierarchy)
            {
                ScreenReader.Say(Loc.Get("router_port_protected_no_edit"));
                return;
            }

            _editMode = true;
            _editingItem = item;
            _editFieldIndex = 0;
            ScreenReader.Say(Loc.Get("router_port_editing"));
            AnnounceEditField();
            FocusEditField();
        }

        private void AnnounceEditField()
        {
            if (_editingItem == null) return;
            string value = "";
            switch (_editFieldIndex)
            {
                case 0: value = _editingItem.externalPort?.text ?? ""; break;
                case 1: value = _editingItem.internalPort?.text ?? ""; break;
                case 2: value = _editingItem.lanIpAddress?.text ?? ""; break;
            }
            ScreenReader.Say(Loc.Get("router_port_field", _fieldNames[_editFieldIndex], value));
        }

        private void FocusEditField()
        {
            if (_editingItem == null) return;
            TMP_InputField field = null;
            switch (_editFieldIndex)
            {
                case 0: field = _editingItem.externalPort; break;
                case 1: field = _editingItem.internalPort; break;
                case 2: field = _editingItem.lanIpAddress; break;
            }
            if (field != null)
            {
                field.Select();
                field.ActivateInputField();
            }
        }

        private void RefreshItems()
        {
            _items.Clear();
            var configUI = _browser?.GetComponentInChildren<RouterConfigUI>();
            if (configUI == null) return;

            var itemsField = typeof(RouterConfigUI).GetField("itemsRouter", BindingFlags.NonPublic | BindingFlags.Instance);
            var items = itemsField?.GetValue(configUI) as List<ItemPortForward>;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }
        }

        private void AnnounceCurrentRule()
        {
            if (_currentIndex >= _items.Count) return;
            var item = _items[_currentIndex];
            string ext = item.externalPort?.text ?? "?";
            string intern = item.internalPort?.text ?? "?";
            string ip = item.lanIpAddress?.text ?? "?";

            string protectedStr = "";
            var protectedIcon = typeof(ItemPortForward).GetField("protectedIcon", BindingFlags.Public | BindingFlags.Instance);
            var icon = protectedIcon?.GetValue(item) as GameObject;
            if (icon != null && icon.activeInHierarchy)
                protectedStr = Loc.Get("router_port_protected");

            ScreenReader.Say(Loc.Get("router_port_rule", _currentIndex + 1, _items.Count, ext, ip, intern, protectedStr));
        }
    }
}
