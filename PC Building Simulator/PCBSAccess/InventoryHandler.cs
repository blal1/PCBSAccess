using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces inventory items when the inventory panel is open.
    ///
    /// Up/Down arrows: navigate items in the current category.
    /// F5 (wired in Main): re-read the currently focused item.
    /// Announces category name + item count on open and category change.
    /// Announces "Inventory closed." on exit.
    /// </summary>
    public static class InventoryHandler
    {
        #region State

        private static bool _active;
        private static int _focusIndex = -1;
        private static string _lastItemSummary;

        // Cached reflection fields
        private static FieldInfo _currentCategoryField;
        private static FieldInfo _currentCategoriesField;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "Inventory";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))            { Navigate(1);       return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))              { Navigate(-1);      return true; }
                if (Input.GetKeyDown(KeyCode.Home))                 { NavigateTo(0);     return true; }
                if (Input.GetKeyDown(KeyCode.End))                  { NavigateToEnd();   return true; }
                if (Input.GetKeyDown(KeyCode.RightArrow))           { CycleCategory(1);  return true; }
                if (Input.GetKeyDown(KeyCode.LeftArrow))            { CycleCategory(-1); return true; }
                if (Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.Space))                { SelectFocused();   return true; }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_inventory")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        /// <summary>Re-reads the focused item. Called from Main on F5.</summary>
        public static void AnnounceCurrentItem()
        {
            if (!_active) return;

            if (_focusIndex >= 0)
            {
                ItemDisplay[] items = GetItems();
                if (items != null && _focusIndex < items.Length)
                {
                    AnnounceItem(items[_focusIndex], items.Length);
                    return;
                }
            }
            ScreenReader.Say(Loc.Get("inv_no_item"));
        }

        /// <summary>Resets on scene change. Called from Main.OnSceneLoaded.</summary>
        public static void Reset()
        {
            _active = false;
            _focusIndex = -1;
            _lastItemSummary = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int direction)
        {
            try
            {
                ItemDisplay[] items = GetItems();
                if (items == null || items.Length == 0)
                {
                    ScreenReader.Say(Loc.Get("inv_empty"));
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
                DebugLogger.LogWarning($"InventoryHandler.Navigate: {ex.Message}");
            }
        }

        private static void NavigateTo(int index)
        {
            try
            {
                ItemDisplay[] items = GetItems();
                if (items == null || items.Length == 0) { ScreenReader.Say(Loc.Get("inv_empty")); return; }
                _focusIndex = Mathf.Clamp(index, 0, items.Length - 1);
                AnnounceItem(items[_focusIndex], items.Length);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InventoryHandler.NavigateTo: {ex.Message}");
            }
        }

        private static void NavigateToEnd()
        {
            ItemDisplay[] items = GetItems();
            if (items != null && items.Length > 0)
                NavigateTo(items.Length - 1);
        }

        private static void CycleCategory(int dir)
        {
            try
            {
                Inventory inv = WorkshopUI.m_inventory;
                if (inv == null) return;

                if (_currentCategoriesField == null)
                    _currentCategoriesField = typeof(Inventory).GetField("m_currentCategories",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                var cats = _currentCategoriesField?.GetValue(inv) as PartDesc.ShopCategory[];
                if (cats == null || cats.Length == 0) return;

                if (_currentCategoryField == null)
                    _currentCategoryField = typeof(Inventory).GetField("m_currentCategory",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                var cur = (PartDesc.ShopCategory)(_currentCategoryField?.GetValue(inv) ?? cats[0]);
                int idx = Array.IndexOf(cats, cur);
                if (idx < 0) idx = 0;
                int next = (idx + dir + cats.Length) % cats.Length;

                inv.SetCategory(cats[next]);
                _focusIndex = -1;
                AnnounceCategoryAndCount();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InventoryHandler.CycleCategory: {ex.Message}");
            }
        }

        /// <summary>
        /// Invokes the install button on the focused item — puts the part in the player's hand
        /// and closes the inventory. For cases, picks up the case from storage.
        /// </summary>
        private static void SelectFocused()
        {
            if (_focusIndex < 0) return;
            try
            {
                ItemDisplay[] items = GetItems();
                if (items == null || _focusIndex >= items.Length) return;
                ItemDisplay item = items[_focusIndex];
                if (item == null || item.installBtn == null) return;

                if (!item.installBtn.IsInteractable())
                {
                    // Announce why — incompatible or no computer in bay
                    string why = item.incompatible.gameObject.activeSelf
                        ? Loc.Get("inv_incompatible")
                        : Loc.Get("inv_cant_select");
                    ScreenReader.Say(why);
                    return;
                }

                ButtonHelper.Click(item.installBtn);
                DebugLogger.LogState($"InventoryHandler: selected '{item.title.text}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InventoryHandler.SelectFocused: {ex.Message}");
            }
        }

        #endregion

        #region Announce helpers

        private static void AnnounceItem(ItemDisplay item, int total)
        {
            if (item == null) return;

            try
            {
                string name       = item.title.text;
                string condition  = GetCondition(item);
                string price      = item.m_priceText.gameObject.activeSelf ? item.m_priceText.text : string.Empty;
                string specs      = item.details.text;
                int pos           = _focusIndex + 1;
                bool incompatible = item.incompatible.gameObject.activeSelf;
                string compat     = incompatible ? $" {Loc.Get("inv_incompatible")}." : string.Empty;

                string detail = string.IsNullOrEmpty(price)
                    ? $"{condition}. {specs}"
                    : $"{condition}. {price}. {specs}";

                string text = $"{Loc.Get("inv_label")}, {pos} {Loc.Get("nav_of")} {total}: {name}. {detail}{compat}";

                _lastItemSummary = text;
                ScreenReader.Say(text);
                DebugLogger.LogState($"InventoryHandler: {pos}/{total} '{name}' incompatible={incompatible}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InventoryHandler.AnnounceItem: {ex.Message}");
            }
        }

        private static string GetCondition(ItemDisplay item)
        {
            if (item.broken.activeSelf) return Loc.Get("inv_broken");
            if (item.used.activeSelf)   return Loc.Get("inv_used");
            return Loc.Get("inv_new");
        }

        private static void AnnounceCategoryAndCount()
        {
            try
            {
                Inventory inv = WorkshopUI.m_inventory;
                if (inv == null) return;

                string categoryName = GetCategoryName(inv);
                ItemDisplay[] items = GetItems();
                int count = items != null ? items.Length : 0;

                string text = count == 0
                    ? $"{categoryName}. {Loc.Get("inv_empty")}"
                    : $"{categoryName}. {count} {Loc.Get("inv_items")}.";

                ScreenReader.Say(text);
                DebugLogger.LogState($"InventoryHandler: category={categoryName} count={count}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InventoryHandler.AnnounceCategoryAndCount: {ex.Message}");
            }
        }

        #endregion

        #region Item list helpers

        private static ItemDisplay[] GetItems()
        {
            Inventory inv = WorkshopUI.m_inventory;
            if (inv == null) return null;

            Transform content = inv.m_itemList?.content;
            if (content == null) return null;

            return content.GetComponentsInChildren<ItemDisplay>(includeInactive: false);
        }

        #endregion

        #region Category name helper

        private static string GetCategoryName(Inventory inv)
        {
            try
            {
                if (_currentCategoryField == null)
                    _currentCategoryField = typeof(Inventory).GetField("m_currentCategory",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                if (_currentCategoryField == null) return string.Empty;

                var category = (PartDesc.ShopCategory)_currentCategoryField.GetValue(inv);
                return CategoryToLocKey(category);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"InventoryHandler.GetCategoryName: {ex.Message}");
                return string.Empty;
            }
        }

        private static string CategoryToLocKey(PartDesc.ShopCategory cat)
        {
            switch (cat)
            {
                case PartDesc.ShopCategory.All:           return Loc.Get("inv_cat_all");
                case PartDesc.ShopCategory.Misc:          return Loc.Get("inv_cat_misc");
                case PartDesc.ShopCategory.CPU:           return Loc.Get("inv_cat_cpu");
                case PartDesc.ShopCategory.Cooling:       return Loc.Get("inv_cat_cooling");
                case PartDesc.ShopCategory.Motherboard:   return Loc.Get("inv_cat_motherboard");
                case PartDesc.ShopCategory.Memory:        return Loc.Get("inv_cat_memory");
                case PartDesc.ShopCategory.GPU:           return Loc.Get("inv_cat_gpu");
                case PartDesc.ShopCategory.Storage:       return Loc.Get("inv_cat_storage");
                case PartDesc.ShopCategory.PSU:           return Loc.Get("inv_cat_psu");
                case PartDesc.ShopCategory.Cables:        return Loc.Get("inv_cat_cables");
                case PartDesc.ShopCategory.Case:          return Loc.Get("inv_cat_case");
                case PartDesc.ShopCategory.CaseCooling:   return Loc.Get("inv_cat_casecooling");
                case PartDesc.ShopCategory.CaseParts:     return Loc.Get("inv_cat_caseparts");
                case PartDesc.ShopCategory.Radiators:     return Loc.Get("inv_cat_radiators");
                case PartDesc.ShopCategory.Reservoir:     return Loc.Get("inv_cat_reservoir");
                case PartDesc.ShopCategory.CPUBlock:      return Loc.Get("inv_cat_cpublock");
                case PartDesc.ShopCategory.WaterCooledGPU:return Loc.Get("inv_cat_watergpu");
                case PartDesc.ShopCategory.Pipes:         return Loc.Get("inv_cat_pipes");
                case PartDesc.ShopCategory.PipeConnectors:return Loc.Get("inv_cat_pipeconn");
                case PartDesc.ShopCategory.Coolant:       return Loc.Get("inv_cat_coolant");
                default:                                  return cat.ToString();
            }
        }

        #endregion

        #region Patches

        /// <summary>Fires when the inventory state is entered. Announces open + category + count.</summary>
        [HarmonyPatch(typeof(InventoryState), "OnStateEnter")]
        static class InventoryState_OnStateEnter_Patch
        {
            static void Postfix()
            {
                try
                {
                    _active = true;
                    _focusIndex = -1;
                    _lastItemSummary = null;
                    InputRouter.Push(_ctx);

                    // Items may not exist yet (lazy coroutine) — announce after one frame via helper
                    Inventory inv = WorkshopUI.m_inventory;
                    if (inv != null)
                        inv.StartCoroutine(AnnounceOpenAfterFrame());
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"InventoryState_OnStateEnter_Patch: {ex.Message}");
                }
            }
        }

        private static System.Collections.IEnumerator AnnounceOpenAfterFrame()
        {
            yield return null;
            try
            {
                Inventory inv = WorkshopUI.m_inventory;
                if (inv == null) yield break;

                string categoryName = GetCategoryName(inv);
                ItemDisplay[] items = GetItems();
                int count = items != null ? items.Length : 0;

                string text = count == 0
                    ? $"{Loc.Get("inv_opened_prefix")} {categoryName}. {Loc.Get("inv_empty")}"
                    : $"{Loc.Get("inv_opened_prefix")} {categoryName}. {count} {Loc.Get("inv_items")}.";

                ScreenReader.Say(text);
                DebugLogger.LogState($"InventoryHandler: opened, category={categoryName}, count={count}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"AnnounceOpenAfterFrame: {ex.Message}");
            }
        }

        /// <summary>Fires when the inventory state is exited. Announces closed.</summary>
        [HarmonyPatch(typeof(InventoryState), "OnStateExit")]
        static class InventoryState_OnStateExit_Patch
        {
            static void Postfix()
            {
                try
                {
                    _active = false;
                    _focusIndex = -1;
                    _lastItemSummary = null;
                    InputRouter.Pop(_ctx);
                    ScreenReader.Say(Loc.Get("inv_closed"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"InventoryState_OnStateExit_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>Fires when the user switches category. Resets index and announces new category.</summary>
        [HarmonyPatch(typeof(Inventory), "SetCategory")]
        static class Inventory_SetCategory_Patch
        {
            static void Postfix(Inventory __instance)
            {
                if (!_active) return;
                try
                {
                    _focusIndex = -1;
                    _lastItemSummary = null;

                    // Items rebuild asynchronously — wait one frame
                    __instance.StartCoroutine(AnnounceCategoryAfterFrame());
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Inventory_SetCategory_Patch: {ex.Message}");
                }
            }
        }

        private static System.Collections.IEnumerator AnnounceCategoryAfterFrame()
        {
            yield return null;
            AnnounceCategoryAndCount();
        }

        /// <summary>Fires when items are rebuilt (search, filter, category change). Resets focus index.</summary>
        [HarmonyPatch(typeof(Inventory), "ConstructInventory")]
        static class Inventory_ConstructInventory_Patch
        {
            static void Postfix()
            {
                if (!_active) return;
                _focusIndex = -1;
                _lastItemSummary = null;
            }
        }

        #endregion
    }
}
