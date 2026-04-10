using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for hack shop tools and exploits panels.
    /// </summary>
    public class HackShopSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private bool _isExploits;
        private int _currentIndex;
        private List<Component> _items = new();

        private static FieldInfo _listaItemsField;
        private static FieldInfo _currentPanelField;

        static HackShopSubHandler()
        {
            _listaItemsField = typeof(HtmlBrowser).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
            _currentPanelField = typeof(HtmlBrowser).GetField("currentPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;

            BrowserPanel panel = BrowserPanel.HackShopTools;
            if (_currentPanelField != null)
            {
                try { panel = (BrowserPanel)(int)_currentPanelField.GetValue(browser); }
                catch { }
            }
            _isExploits = panel == BrowserPanel.HackShopExploits;

            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    var buyMethod = _items[_currentIndex].GetType().GetMethod("OnBuy");
                    buyMethod?.Invoke(_items[_currentIndex], null);
                }
                return true;
            }

            // Alt+F: cycle library/filter dropdown
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F))
            {
                if (_isExploits)
                {
                    var hackshop = _browser.GetComponentInChildren<BrowserHackshop>();
                    if (hackshop != null && hackshop.libraryId != null && hackshop.libraryId.options.Count > 0)
                    {
                        int next = (hackshop.libraryId.value + 1) % hackshop.libraryId.options.Count;
                        hackshop.libraryId.value = next;
                        ScreenReader.Say(Loc.Get("hackshop_filter", hackshop.libraryId.options[next].text));
                    }
                }
                else
                {
                    if (_browser.filterDropdown != null && _browser.filterDropdown.options.Count > 0)
                    {
                        int next = (_browser.filterDropdown.value + 1) % _browser.filterDropdown.options.Count;
                        _browser.filterDropdown.value = next;
                        ScreenReader.Say(Loc.Get("hackshop_filter", _browser.filterDropdown.options[next].text));
                    }
                }
                return true;
            }

            // Alt+P: cycle permission filter (exploits only)
            if (_isExploits && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.P))
            {
                if (_browser.filterDropdownPerm != null && _browser.filterDropdownPerm.options.Count > 0)
                {
                    int next = (_browser.filterDropdownPerm.value + 1) % _browser.filterDropdownPerm.options.Count;
                    _browser.filterDropdownPerm.value = next;
                    ScreenReader.Say(Loc.Get("hackshop_perm_filter", _browser.filterDropdownPerm.options[next].text));
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            if (_isExploits)
            {
                if (_items.Count > 0)
                    ScreenReader.Say(Loc.Get("hackshop_exploits_results", _items.Count));
                else
                    ScreenReader.Say(Loc.Get("hackshop_exploits"));
            }
            else
            {
                if (_items.Count > 0)
                    ScreenReader.Say(Loc.Get("hackshop_tools", _items.Count));
                else
                    ScreenReader.Say(Loc.Get("hackshop_empty"));
            }
        }

        public string GetPanelName() => _isExploits ? "Hack Shop Exploits" : "Hack Shop Tools";

        public string GetHelpText() => _isExploits ? Loc.Get("hackshop_help_exploits") : Loc.Get("hackshop_help_tools");

        /// <summary>Called when shop items are loaded from server.</summary>
        public void OnShopLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            _currentIndex = 0;
            RefreshItems();
            AnnounceState();
        }

        private void RefreshItems()
        {
            _items.Clear();
            var softItems = _listaItemsField?.GetValue(_browser) as List<ItemShop>;
            if (softItems != null)
            {
                foreach (var item in softItems)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }
        }

        private void AnnounceCurrentItem()
        {
            if (_currentIndex >= _items.Count) return;

            var item = _items[_currentIndex] as ItemShop;
            if (item == null) return;

            string name = item.nombre != null ? item.nombre.text : "";
            string desc = item.description != null ? item.description.text : "";

            if (item is ItemHackShop hackItem)
            {
                string service = "";
                var serviceField = typeof(ItemHackShop).GetField("serviceAffected", BindingFlags.NonPublic | BindingFlags.Instance);
                service = serviceField?.GetValue(hackItem) as string ?? "";
                if (string.IsNullOrEmpty(service)) service = "unknown";

                string price = GetExploitPrice(hackItem);
                ScreenReader.Say(Loc.Get("hackshop_exploit_item", _currentIndex + 1, _items.Count, name, service, price, desc));
            }
            else
            {
                string price = GetItemPrice(item);
                ScreenReader.Say(Loc.Get("hackshop_item", _currentIndex + 1, _items.Count, name, desc, price));
            }
        }

        private string GetItemPrice(ItemShop item)
        {
            try
            {
                var listaField = typeof(ItemShop).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
                var lista = listaField?.GetValue(item) as List<ItemShopAdvanced>;
                if (lista != null && lista.Count > 0)
                    return lista[0].GetPrecio().ToString();
            }
            catch { }
            return "?";
        }

        private string GetExploitPrice(ItemHackShop item)
        {
            try
            {
                var exploitField = typeof(ItemHackShop).GetField("exploit", BindingFlags.NonPublic | BindingFlags.Instance);
                var exploit = exploitField?.GetValue(item) as Exploit;
                if (exploit != null) return exploit.GetPrecio().ToString();
            }
            catch { }
            return GetItemPrice(item);
        }
    }
}
