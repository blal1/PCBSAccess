using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for regular software/hardware shop.
    /// </summary>
    public class ShopSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private List<Component> _items = new();
        private bool _isHardware;

        private static FieldInfo _listaItemsField;
        private static FieldInfo _listaItemsHwField;

        static ShopSubHandler()
        {
            _listaItemsField = typeof(HtmlBrowser).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
            _listaItemsHwField = typeof(HtmlBrowser).GetField("listaItemsHw", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
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

            // Alt+F: cycle filter
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F))
            {
                if (_browser.filterDropdown != null && _browser.filterDropdown.options.Count > 0)
                {
                    int next = (_browser.filterDropdown.value + 1) % _browser.filterDropdown.options.Count;
                    _browser.filterDropdown.value = next;
                    string filterName = _browser.filterDropdown.options[next].text;
                    ScreenReader.Say(Loc.Get("shop_filter", filterName));
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            if (_items.Count == 0)
                ScreenReader.Say(Loc.Get("shop_empty"));
            else
            {
                ScreenReader.Say(Loc.Get("shop_loaded", _items.Count));
                AnnounceCurrentItem();
            }
        }

        public string GetPanelName() => "Shop";

        public string GetHelpText() => Loc.Get("shop_help");

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
            if (softItems != null && softItems.Count > 0)
            {
                _isHardware = false;
                foreach (var item in softItems)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }

            if (_items.Count == 0)
            {
                var hwItems = _listaItemsHwField?.GetValue(_browser) as List<ItemShopHardware>;
                if (hwItems != null && hwItems.Count > 0)
                {
                    _isHardware = true;
                    foreach (var item in hwItems)
                    {
                        if (item != null && item.gameObject.activeInHierarchy)
                            _items.Add(item);
                    }
                }
            }
        }

        private void AnnounceCurrentItem()
        {
            if (_currentIndex >= _items.Count) return;

            var item = _items[_currentIndex];
            if (_isHardware)
            {
                var hw = item as ItemShopHardware;
                if (hw == null) return;
                string name = hw.nombre != null ? hw.nombre.text : "";
                string desc = hw.description != null ? hw.description.text : "";
                string tag = hw.tagText != null ? hw.tagText.text : "";
                string price = hw.precio != null ? hw.precio.text : "";
                ScreenReader.Say(Loc.Get("shop_item_hw", _currentIndex + 1, _items.Count, name, tag, desc, price));
            }
            else
            {
                var shop = item as ItemShop;
                if (shop == null) return;
                string name = shop.nombre != null ? shop.nombre.text : "";
                string desc = shop.description != null ? shop.description.text : "";
                string price = GetItemPrice(shop);
                ScreenReader.Say(Loc.Get("shop_item", _currentIndex + 1, _items.Count, name, desc, price));
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
    }
}
