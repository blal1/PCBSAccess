using System;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces PCBay tablet app — buy page offers, sell page auctions, item detail, purchase result.
    ///
    /// OnGainFocus patch: captures active app instance, announces page + first item.
    /// Up/Down: navigate buy or sell list.
    /// Enter: show item detail (buy page) or trigger action (sell page).
    /// ShowMessage patch: announces purchase feedback (success/fail/delivery info).
    /// OnShowItem patch: announces item detail when info page opens.
    /// </summary>
    public static class PCBayHandler
    {
        #region State

        private static PCBayApp _app;
        private static PCBayItem[]     _buyItems;
        private static PCBayAuction[]  _sellItems;
        private static int  _buyIndex  = -1;
        private static int  _sellIndex = -1;
        private static bool _onBuyPage;
        private static bool _onSellPage;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "PCBay";
            public bool IsActive => _app != null;

            public bool HandleInput()
            {
                if (_onBuyPage && _buyItems != null && _buyItems.Length > 0)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (_buyIndex > 0) AnnounceBuyItem(--_buyIndex);
                        else ScreenReader.Say(Loc.Get("nav_first_item"));
                        return true;
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (_buyIndex < _buyItems.Length - 1) AnnounceBuyItem(++_buyIndex);
                        else ScreenReader.Say(Loc.Get("nav_last_item"));
                        return true;
                    }
                    if (Input.GetKeyDown(KeyCode.Home))   { _buyIndex = 0; AnnounceBuyItem(_buyIndex); return true; }
                    if (Input.GetKeyDown(KeyCode.End))    { _buyIndex = _buyItems.Length - 1; AnnounceBuyItem(_buyIndex); return true; }
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        if (_buyIndex >= 0 && _buyIndex < _buyItems.Length) _buyItems[_buyIndex].OnClick();
                        return true;
                    }
                }
                else if (_onSellPage && _sellItems != null && _sellItems.Length > 0)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (_sellIndex > 0) AnnounceSellItem(--_sellIndex);
                        else ScreenReader.Say(Loc.Get("nav_first_item"));
                        return true;
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (_sellIndex < _sellItems.Length - 1) AnnounceSellItem(++_sellIndex);
                        else ScreenReader.Say(Loc.Get("nav_last_item"));
                        return true;
                    }
                    if (Input.GetKeyDown(KeyCode.Home))   { _sellIndex = 0; AnnounceSellItem(_sellIndex); return true; }
                    if (Input.GetKeyDown(KeyCode.End))    { _sellIndex = _sellItems.Length - 1; AnnounceSellItem(_sellIndex); return true; }
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        if (_sellIndex >= 0 && _sellIndex < _sellItems.Length)
                            _sellItems[_sellIndex].GetComponent<AuctionStatus>()?.OnAction();
                        return true;
                    }
                }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_pcbay")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _app       = null;
            _buyItems  = null;
            _sellItems = null;
            _buyIndex  = -1;
            _sellIndex = -1;
            _onBuyPage  = false;
            _onSellPage = false;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Helpers

        private static void AnnounceBuyItem(int index)
        {
            try
            {
                PCBayItem item = _buyItems[index];
                string name  = item.m_name  != null ? item.m_name.text  : string.Empty;
                string price = item.m_price != null ? item.m_price.text : string.Empty;
                string delivery = item.m_delivery != null ? item.m_delivery.text : string.Empty;

                string pos = $"{index + 1} {Loc.Get("nav_of")} {_buyItems.Length}";
                string msg = $"{pos}. {name}. {price}.";
                if (!string.IsNullOrEmpty(delivery)) msg += $" {delivery}.";
                ScreenReader.Say(msg);
                DebugLogger.LogState($"PCBayHandler buy [{index}]: {name}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"PCBayHandler.AnnounceBuyItem: {ex.Message}");
            }
        }

        private static void AnnounceSellItem(int index)
        {
            try
            {
                PCBayAuction item   = _sellItems[index];
                string name = item.m_name != null ? item.m_name.text : string.Empty;
                AuctionStatus status = item.GetComponent<AuctionStatus>();
                string bid       = status != null && status.m_currentBid       != null ? status.m_currentBid.text       : string.Empty;
                string remaining = status != null && status.m_remaining         != null ? status.m_remaining.text         : string.Empty;
                string bids      = status != null && status.m_currentBids       != null ? status.m_currentBids.text       : string.Empty;

                string pos = $"{index + 1} {Loc.Get("nav_of")} {_sellItems.Length}";
                ScreenReader.Say($"{pos}. {name}. {bid}. {bids}. {remaining}.");
                DebugLogger.LogState($"PCBayHandler sell [{index}]: {name}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"PCBayHandler.AnnounceSellItem: {ex.Message}");
            }
        }

        private static void StripRichText(ref string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            s = System.Text.RegularExpressions.Regex.Replace(s, "<[^>]+>", string.Empty);
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires whenever PCBay gains OS focus or the user switches Buy/Sell tabs.
        /// Captures the active instance and announces the current page.
        /// </summary>
        [HarmonyPatch(typeof(PCBayApp), "OnGainFocus")]
        static class PCBayApp_OnGainFocus_Patch
        {
            static void Postfix(PCBayApp __instance)
            {
                try
                {
                    _app = __instance;
                    InputRouter.Push(_ctx);

                    if (__instance.m_buyPage.activeSelf)
                    {
                        _onBuyPage  = true;
                        _onSellPage = false;
                        _buyItems = __instance.m_buyItems.content
                            .GetComponentsInChildren<PCBayItem>(includeInactive: false);
                        _buyIndex = _buyItems.Length > 0 ? 0 : -1;

                        ScreenReader.Say(Loc.Get("pcbay_buy_open", _buyItems.Length));
                        if (_buyIndex >= 0) AnnounceBuyItem(_buyIndex);
                        DebugLogger.LogState($"PCBayHandler: buy page, {_buyItems.Length} offers");
                    }
                    else if (__instance.m_sellItems.gameObject.activeSelf)
                    {
                        _onSellPage = true;
                        _onBuyPage  = false;
                        _sellItems = __instance.m_sellItems.content
                            .GetComponentsInChildren<PCBayAuction>(includeInactive: false);
                        _sellIndex = _sellItems.Length > 0 ? 0 : -1;

                        ScreenReader.Say(Loc.Get("pcbay_sell_open", _sellItems.Length));
                        if (_sellIndex >= 0) AnnounceSellItem(_sellIndex);
                        DebugLogger.LogState($"PCBayHandler: sell page, {_sellItems.Length} auctions");
                    }
                    else if (__instance.m_messagePage.activeSelf)
                    {
                        _onBuyPage  = false;
                        _onSellPage = false;
                        // message text announced via ShowMessage patch
                    }
                    else if (__instance.m_itemInfoPage.gameObject.activeSelf)
                    {
                        _onBuyPage  = false;
                        _onSellPage = false;
                        // item info announced via OnShowItem patch
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"PCBayApp_OnGainFocus_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when item detail page opens. Announces name, price, delivery, description.</summary>
        [HarmonyPatch(typeof(PCBayApp), "OnShowItem")]
        static class PCBayApp_OnShowItem_Patch
        {
            static void Postfix(PCBayApp __instance)
            {
                try
                {
                    PCBayItem info = __instance.m_itemInfoPage;
                    string name  = info.m_name  != null ? info.m_name.text  : string.Empty;
                    string price = info.m_price != null ? info.m_price.text : string.Empty;
                    string delivery = info.m_delivery != null ? info.m_delivery.text : string.Empty;
                    string desc  = info.m_desc  != null ? info.m_desc.text  : string.Empty;

                    StripRichText(ref price);
                    ScreenReader.Say($"{name}. {price}.");
                    if (!string.IsNullOrEmpty(delivery)) ScreenReader.Say(delivery, interrupt: false);
                    if (!string.IsNullOrEmpty(desc))     ScreenReader.Say(desc, interrupt: false);
                    ScreenReader.Say(Loc.Get("pcbay_detail_hint"), interrupt: false);

                    DebugLogger.LogState($"PCBayHandler: item detail '{name}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"PCBayApp_OnShowItem_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when PCBay shows a result message (purchase success/fail/nothing for sale).</summary>
        [HarmonyPatch(typeof(PCBayApp), "ShowMessage")]
        static class PCBayApp_ShowMessage_Patch
        {
            static void Postfix(string message)
            {
                try
                {
                    if (string.IsNullOrEmpty(message)) return;
                    string clean = System.Text.RegularExpressions.Regex.Replace(message, "<[^>]+>", string.Empty);
                    ScreenReader.Say(clean);
                    DebugLogger.LogState($"PCBayHandler: message '{clean}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"PCBayApp_ShowMessage_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
