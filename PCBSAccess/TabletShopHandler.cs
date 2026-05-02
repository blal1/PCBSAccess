using System;
using System.Collections.Generic;
using System.Reflection;
using FuturLab;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for all DLC tablet shops (Bargain Basement, Office Shop, Water Shop, Mothership).
    ///
    /// On shop open: announces item count.
    /// Up/Down/Home/End: navigate items.
    /// Each item: name, price, stock count, and availability.
    /// </summary>
    public static class TabletShopHandler
    {
        #region State

        private static TabletShopBase _shop;
        private static bool _active;
        private static bool _wasOpen;
        private static readonly List<TabletShopItemBase> _items = new List<TabletShopItemBase>();
        private static int _focusIndex = -1;

        private static readonly FieldInfo _fStock =
            typeof(TabletShopBase).GetField("m_stock",
                BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "TabletShop";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);              return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);             return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);            return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_items.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.F1))         { ReRead();                 return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("tabletshop_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patches

        [HarmonyPatch(typeof(TabletShopBase), "OnAppOpened")]
        static class TabletShopBase_OnAppOpened_Patch
        {
            static void Postfix(TabletShopBase __instance)
            {
                try
                {
                    _shop = __instance;
                    BuildList(__instance);
                    _focusIndex = _items.Count > 0 ? 0 : -1;
                    _active     = true;
                    _wasOpen    = true;
                    InputRouter.Push(_ctx);

                    ScreenReader.Say(Loc.Get("tabletshop_open", _items.Count));
                    if (_items.Count > 0)
                    {
                        AnnounceItem(0);
                        ScreenReader.Say(Loc.Get("tabletshop_hint"), interrupt: false);
                    }
                    DebugLogger.LogState($"TabletShopHandler: {_items.Count} items");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TabletShopHandler.OnAppOpened: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(TabletShopBase), "OnAppClosed")]
        static class TabletShopBase_OnAppClosed_Patch
        {
            static void Postfix()
            {
                try { OnClose(); }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TabletShopHandler.OnAppClosed: {ex.Message}");
                }
            }
        }

        // Announce after items are added or filtered.
        [HarmonyPatch(typeof(TabletShopBase), "UpdateStockUI")]
        static class TabletShopBase_UpdateStockUI_Patch
        {
            static void Postfix(TabletShopBase __instance)
            {
                try
                {
                    if (!_active || _shop != __instance) return;
                    BuildList(__instance);
                    _focusIndex = Mathf.Clamp(_focusIndex, 0, _items.Count - 1);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TabletShopHandler.UpdateStockUI: {ex.Message}");
                }
            }
        }

        #endregion

        #region Poll

        public static void PollState()
        {
            if (!_wasOpen || _shop == null) return;
            bool open = _shop.gameObject.activeSelf;
            if (!open && _wasOpen) OnClose();
            _wasOpen = open;
        }

        private static void OnClose()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
            _shop       = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
            _shop       = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_items.Count == 0) return;
            int next = Mathf.Clamp(_focusIndex + dir, 0, _items.Count - 1);
            if (next == _focusIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceItem(_focusIndex);
        }

        private static void NavigateTo(int index)
        {
            if (_items.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _items.Count - 1);
            AnnounceItem(_focusIndex);
        }

        private static void ReRead()
        {
            if (_focusIndex >= 0 && _focusIndex < _items.Count)
                AnnounceItem(_focusIndex);
        }

        #endregion

        #region Announce

        private static void AnnounceItem(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            TabletShopItemBase item = _items[index];

            string name  = item.itemName ?? string.Empty;
            string price = item.price.ToCash();
            string stock = item.stockCount > 0
                ? Loc.Get("tabletshop_stock", item.stockCount)
                : Loc.Get("tabletshop_out_of_stock");
            string avail = item.stockAvailability == TabletShopStockAvailability.Unavailable
                ? $" {Loc.Get("tabletshop_unavailable")}" : string.Empty;

            ScreenReader.Say($"{index + 1} {Loc.Get("nav_of")} {_items.Count}: {name}. {price}. {stock}.{avail}");
        }

        #endregion

        #region Build list

        private static void BuildList(TabletShopBase shop)
        {
            _items.Clear();
            try
            {
                var stock = _fStock?.GetValue(shop) as List<TabletShopItemBase>;
                if (stock == null) return;
                foreach (TabletShopItemBase item in stock)
                {
                    if (item != null && item.gameObject.activeSelf)
                        _items.Add(item);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TabletShopHandler.BuildList: {ex.Message}");
            }
        }

        #endregion
    }
}
