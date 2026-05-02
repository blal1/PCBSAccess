using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces shop items when browsing the in-game shop.
    ///
    /// Up/Down: navigate items (X of Y format).
    /// Home: jump to first item.
    /// End:  jump to last item.
    /// Numpad1 (wired in Main): re-read the focused item.
    /// Announces category name + item count on category select.
    /// </summary>
    public static class ShopHandler
    {
        #region State

        private static Shop _shop;
        private static int _focusIndex = -1;
        private static string _lastItemSummary;

        // Cached reflection
        private static FieldInfo _lockedField;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "Shop";
            public bool IsActive => IsShopItemListVisible();

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);           return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);          return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);         return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateToEnd();       return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_shop")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Re-reads the focused shop item. Called from Main on Numpad1.</summary>
        public static void AnnounceCurrentItem()
        {
            if (!IsShopItemListVisible())
            {
                ScreenReader.Say(Loc.Get("shop_not_open"));
                return;
            }
            if (!string.IsNullOrEmpty(_lastItemSummary))
                ScreenReader.Say(Loc.Get("shop_reread", _lastItemSummary));
            else
                ScreenReader.Say(Loc.Get("shop_no_item"));
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _shop = null;
            _focusIndex = -1;
            _lastItemSummary = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Helpers

        private static bool IsShopItemListVisible()
        {
            try
            {
                return _shop != null
                    && _shop.gameObject.activeSelf
                    && _shop.m_itemList != null
                    && _shop.m_itemList.gameObject.activeSelf;
            }
            catch
            {
                return false;
            }
        }

        private static ShopItem[] GetItems()
        {
            if (_shop?.m_itemList?.ShopItems == null) return null;
            return _shop.m_itemList.ShopItems.ToArray();
        }

        #endregion

        #region Navigation

        private static void Navigate(int direction)
        {
            try
            {
                ShopItem[] items = GetItems();
                if (items == null || items.Length == 0)
                {
                    ScreenReader.Say(Loc.Get("shop_empty"));
                    return;
                }

                int next = Mathf.Clamp(_focusIndex + direction, 0, items.Length - 1);

                if (next == _focusIndex)
                {
                    string edge = direction > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item");
                    ScreenReader.Say(edge);
                    return;
                }

                _focusIndex = next;
                AnnounceItem(items[_focusIndex], items.Length);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ShopHandler.Navigate: {ex.Message}");
            }
        }

        private static void NavigateTo(int index)
        {
            try
            {
                ShopItem[] items = GetItems();
                if (items == null || items.Length == 0) { ScreenReader.Say(Loc.Get("shop_empty")); return; }
                _focusIndex = Mathf.Clamp(index, 0, items.Length - 1);
                AnnounceItem(items[_focusIndex], items.Length);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ShopHandler.NavigateTo: {ex.Message}");
            }
        }

        private static void NavigateToEnd()
        {
            ShopItem[] items = GetItems();
            if (items != null && items.Length > 0)
                NavigateTo(items.Length - 1);
        }

        #endregion

        #region Announce helpers

        private static void AnnounceItem(ShopItem item, int total)
        {
            if (item == null) return;
            try
            {
                string name  = item.m_name.text;
                string price = item.m_price.text;
                string spec  = item.m_desc.text;
                bool locked  = IsLocked(item);
                int pos      = _focusIndex + 1;

                string lockedStr = locked ? $" {Loc.Get("shop_locked")}." : string.Empty;
                string text = $"{Loc.Get("shop_label")}, {pos} {Loc.Get("nav_of")} {total}: {name}. {price}.{lockedStr} {spec}";

                _lastItemSummary = text;
                ScreenReader.Say(text);
                DebugLogger.LogState($"ShopHandler: {pos}/{total} '{name}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ShopHandler.AnnounceItem: {ex.Message}");
            }
        }

        private static bool IsLocked(ShopItem item)
        {
            try
            {
                if (_lockedField == null)
                    _lockedField = typeof(ShopItem).GetField("m_locked",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                return _lockedField != null && (bool)_lockedField.GetValue(item);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires when a shop category is selected or items are reconstructed.
        /// Captures Shop reference, resets index, announces category + count.
        /// </summary>
        [HarmonyPatch(typeof(Shop), "ConstructItems")]
        static class Shop_ConstructItems_Patch
        {
            static void Postfix(Shop __instance)
            {
                try
                {
                    _shop = __instance;
                    _focusIndex = -1;
                    _lastItemSummary = null;
                    InputRouter.Push(_ctx);

                    if (!__instance.m_itemList.gameObject.activeSelf) return;

                    // Items built lazily — wait one frame
                    __instance.StartCoroutine(AnnounceCategoryAfterFrame());
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Shop_ConstructItems_Patch: {ex.Message}");
                }
            }
        }

        private static System.Collections.IEnumerator AnnounceCategoryAfterFrame()
        {
            yield return null;
            try
            {
                if (_shop == null) yield break;
                ShopItem[] items = GetItems();
                int count = items != null ? items.Length : 0;
                string location = _shop.m_location?.text ?? string.Empty;

                string text = count == 0
                    ? $"{location}. {Loc.Get("shop_empty")}"
                    : $"{location}. {count} {Loc.Get("shop_items")}. {Loc.Get("shop_nav_hint")}";

                ScreenReader.Say(text);
                DebugLogger.LogState($"ShopHandler: category '{location}', {count} items");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ShopHandler.AnnounceCategoryAfterFrame: {ex.Message}");
            }
        }

        /// <summary>Fires when the shop item detail page is shown. Announces full item details.</summary>
        [HarmonyPatch(typeof(Shop), "OnShowItem")]
        static class Shop_OnShowItem_Patch
        {
            static void Postfix(ShopEntry item)
            {
                try
                {
                    if (item?.m_part == null) return;
                    PartDesc part = item.m_part;
                    string locked = part.m_kudosUnlock > CareerStatus.Get().GetKudos()
                        ? $" {Loc.Get("shop_locked")}."
                        : string.Empty;
                    string text = $"{part.m_uiName}. {item.m_cost.ToCash()}.{locked} {part.m_uiLongSpec}";
                    ScreenReader.Say(text);
                    DebugLogger.LogState($"ShopHandler: item page '{part.m_uiName}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Shop_OnShowItem_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when an item is added to cart. Announces confirmation.</summary>
        [HarmonyPatch(typeof(Shop), "AddItem")]
        static class Shop_AddItem_Patch
        {
            static void Postfix(ShopEntry entry)
            {
                try
                {
                    if (entry?.m_part == null) return;
                    ScreenReader.Say(Loc.Get("shop_added_to_cart", entry.m_part.m_uiName, entry.m_cost.ToCash()));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Shop_AddItem_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the checkout page is built. Reads cart contents and total.</summary>
        [HarmonyPatch(typeof(Shop), "ConstructCheckout")]
        static class Shop_ConstructCheckout_Patch
        {
            static void Postfix(Shop __instance)
            {
                try
                {
                    _shop = __instance;

                    CheckoutRow[] rows = __instance.m_checkoutList.content
                        .GetComponentsInChildren<CheckoutRow>(includeInactive: false);

                    CheckoutFinalRow finalRow = __instance.m_checkoutList.content
                        .GetComponentInChildren<CheckoutFinalRow>(includeInactive: false);

                    if (rows == null || rows.Length == 0)
                    {
                        ScreenReader.Say(Loc.Get("shop_cart_empty"));
                        return;
                    }

                    string intro = Loc.Get("shop_checkout_header", rows.Length);
                    ScreenReader.Say(intro);

                    // Queue each item
                    foreach (var row in rows)
                    {
                        string line = $"{row.m_name.text}. {row.m_quantity.text}x {row.m_price.text}. {Loc.Get("shop_total_label")}: {row.m_total.text}.";
                        ScreenReader.Say(line, interrupt: false);
                    }

                    if (finalRow != null)
                    {
                        string summary = Loc.Get("shop_checkout_total", finalRow.m_total.text, finalRow.m_cash.text);
                        ScreenReader.Say(summary, interrupt: false);
                        ScreenReader.Say(Loc.Get("shop_checkout_hint"), interrupt: false);
                    }

                    DebugLogger.LogState($"ShopHandler: checkout, {rows.Length} items");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Shop_ConstructCheckout_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
