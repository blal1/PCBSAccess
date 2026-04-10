using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for router firewall rule configuration.
    /// Browse mode: Up/Down to navigate, Enter to edit.
    /// Edit mode: Tab to cycle fields, Left/Right for Allow/Deny, Alt+A for Any toggle.
    /// </summary>
    public class RouterFirewallSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private List<ItemHwFirewall> _items = new();
        private bool _editMode;
        private int _editFieldIndex;
        private ItemHwFirewall _editingItem;

        private static readonly string[] _fieldNames = { "Action", "Port", "Source address", "Destination address" };

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
                ScreenReader.Say(Loc.Get("router_fw_empty"));
            else
                ScreenReader.Say(Loc.Get("router_firewall", _items.Count));
        }

        public string GetPanelName() => "Firewall";

        public string GetHelpText() => Loc.Get("router_fw_help");

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
                var configUI = _browser.GetComponentInChildren<HwFirewallConfigUI>();
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
                    var toggle = typeof(ItemHwFirewall).GetField("selection", BindingFlags.NonPublic | BindingFlags.Instance);
                    var sel = toggle?.GetValue(_items[_currentIndex]) as UnityEngine.UI.Toggle;
                    if (sel != null) sel.isOn = true;

                    var configUI = _browser.GetComponentInChildren<HwFirewallConfigUI>();
                    configUI?.RemoveSelected();
                    ScreenReader.Say(Loc.Get("router_fw_deleted", 1));
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
                _editFieldIndex = (_editFieldIndex + 1) % 4;
                AnnounceEditField();
                FocusEditField();
                return true;
            }

            if (_editFieldIndex == 0 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
            {
                if (_editingItem.dropdownAction != null)
                {
                    int newVal = _editingItem.dropdownAction.value == 0 ? 1 : 0;
                    _editingItem.dropdownAction.value = newVal;
                    string action = _editingItem.dropdownAction.options[newVal].text;
                    ScreenReader.Say(Loc.Get("router_fw_action_toggle", action));
                }
                return true;
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.A))
            {
                ToggleAny();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _editingItem.SaveChanges();
                ScreenReader.Say(Loc.Get("router_fw_saved"));
                _editMode = false;
                RefreshItems();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _editMode = false;
                ScreenReader.Say(Loc.Get("router_fw_cancelled"));
                return true;
            }

            return false;
        }

        private void EnterEditMode(ItemHwFirewall item)
        {
            _editMode = true;
            _editingItem = item;
            _editFieldIndex = 0;
            ScreenReader.Say(Loc.Get("router_fw_editing"));
            AnnounceEditField();
        }

        private void ToggleAny()
        {
            switch (_editFieldIndex)
            {
                case 1:
                    if (_editingItem.togglePort != null)
                    {
                        _editingItem.togglePort.isOn = !_editingItem.togglePort.isOn;
                        string state = _editingItem.togglePort.isOn
                            ? Loc.Get("router_fw_any_on", "Port")
                            : Loc.Get("router_fw_any_off", "Port", _editingItem.port?.text ?? "");
                        ScreenReader.Say(state);
                    }
                    break;
                case 2:
                    if (_editingItem.toggleSource != null)
                    {
                        _editingItem.toggleSource.isOn = !_editingItem.toggleSource.isOn;
                        string state = _editingItem.toggleSource.isOn
                            ? Loc.Get("router_fw_any_on", "Source")
                            : Loc.Get("router_fw_any_off", "Source", _editingItem.sourceAddress?.text ?? "");
                        ScreenReader.Say(state);
                    }
                    break;
                case 3:
                    if (_editingItem.toggleDest != null)
                    {
                        _editingItem.toggleDest.isOn = !_editingItem.toggleDest.isOn;
                        string state = _editingItem.toggleDest.isOn
                            ? Loc.Get("router_fw_any_on", "Destination")
                            : Loc.Get("router_fw_any_off", "Destination", _editingItem.destAddress?.text ?? "");
                        ScreenReader.Say(state);
                    }
                    break;
            }
        }

        private void AnnounceEditField()
        {
            if (_editingItem == null) return;
            string value = "";
            switch (_editFieldIndex)
            {
                case 0:
                    value = _editingItem.dropdownAction?.options[_editingItem.dropdownAction.value].text ?? "";
                    break;
                case 1:
                    value = _editingItem.togglePort?.isOn == true ? "Any" : (_editingItem.port?.text ?? "");
                    break;
                case 2:
                    value = _editingItem.toggleSource?.isOn == true ? "Any" : (_editingItem.sourceAddress?.text ?? "");
                    break;
                case 3:
                    value = _editingItem.toggleDest?.isOn == true ? "Any" : (_editingItem.destAddress?.text ?? "");
                    break;
            }
            ScreenReader.Say(Loc.Get("router_fw_field", _fieldNames[_editFieldIndex], value));
        }

        private void FocusEditField()
        {
            if (_editingItem == null) return;
            switch (_editFieldIndex)
            {
                case 1:
                    if (_editingItem.port != null && !_editingItem.togglePort.isOn)
                    { _editingItem.port.Select(); _editingItem.port.ActivateInputField(); }
                    break;
                case 2:
                    if (_editingItem.sourceAddress != null && !_editingItem.toggleSource.isOn)
                    { _editingItem.sourceAddress.Select(); _editingItem.sourceAddress.ActivateInputField(); }
                    break;
                case 3:
                    if (_editingItem.destAddress != null && !_editingItem.toggleDest.isOn)
                    { _editingItem.destAddress.Select(); _editingItem.destAddress.ActivateInputField(); }
                    break;
            }
        }

        private void RefreshItems()
        {
            _items.Clear();
            var configUI = _browser?.GetComponentInChildren<HwFirewallConfigUI>();
            if (configUI == null) return;

            var itemsField = typeof(HwFirewallConfigUI).GetField("itemsRouter", BindingFlags.NonPublic | BindingFlags.Instance);
            var items = itemsField?.GetValue(configUI) as List<ItemHwFirewall>;
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
            string action = item.dropdownAction?.options[item.dropdownAction.value].text ?? "?";
            string port = item.togglePort?.isOn == true ? "Any" : (item.port?.text ?? "?");
            string source = item.toggleSource?.isOn == true ? "Any" : (item.sourceAddress?.text ?? "?");
            string dest = item.toggleDest?.isOn == true ? "Any" : (item.destAddress?.text ?? "?");

            ScreenReader.Say(Loc.Get("router_fw_rule", _currentIndex + 1, _items.Count, action, port, source, dest));
        }
    }
}
