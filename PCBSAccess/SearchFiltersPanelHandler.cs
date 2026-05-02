using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the shop/inventory Search Filters panel.
    ///
    /// On open: announces filter count.
    /// Up/Down/Home/End: navigate category headers, checkboxes, and range labels.
    /// Space/Enter on a header: expand or collapse the category.
    /// Space on a checkbox: toggle the filter on or off.
    /// Filter change fires onFiltersChanged — announces new active filter count.
    /// </summary>
    public static class SearchFiltersPanelHandler
    {
        #region Filter item abstraction

        private abstract class FilterItem
        {
            public abstract string Announce(int pos, int total);
            public abstract void Activate();
        }

        private sealed class HeaderItem : FilterItem
        {
            private readonly SearchFilterCategoryHeader _header;
            public HeaderItem(SearchFilterCategoryHeader h) { _header = h; }

            public override string Announce(int pos, int total)
            {
                string state = _header.Expanded
                    ? Loc.Get("filter_expanded")
                    : Loc.Get("filter_collapsed");
                return $"{pos} {Loc.Get("nav_of")} {total}: {_header.Text}. {state}.";
            }

            public override void Activate() { _header.SwitchExpanded(); }
        }

        private sealed class CheckboxItem : FilterItem
        {
            private readonly SearchFilterCheckbox _cb;
            public CheckboxItem(SearchFilterCheckbox cb) { _cb = cb; }

            public override string Announce(int pos, int total)
            {
                string state = _cb.m_toggle.isOn ? Loc.Get("opt_on") : Loc.Get("opt_off");
                return $"{pos} {Loc.Get("nav_of")} {total}: {_cb.m_label.text}. {state}.";
            }

            public override void Activate()
            {
                _cb.m_toggle.isOn = !_cb.m_toggle.isOn;
            }
        }

        private sealed class RangeItem : FilterItem
        {
            private readonly string _name;
            public RangeItem(string name) { _name = name; }

            public override string Announce(int pos, int total)
                => $"{pos} {Loc.Get("nav_of")} {total}: {_name}. {Loc.Get("filter_range")}.";

            public override void Activate() { /* ranges use dropdowns — no keyboard toggle */ }
        }

        #endregion

        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static SearchFiltersPanel _panel;
        private static readonly List<FilterItem> _items = new List<FilterItem>();
        private static int _focusIndex = -1;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "SearchFilters";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);              return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);             return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);            return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_items.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    ActivateFocused();
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.F1)) { ReRead(); return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("filter_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patches

        [HarmonyPatch(typeof(SearchFiltersButton), "ShowPanel")]
        static class SearchFiltersButton_ShowPanel_Patch
        {
            static void Postfix(SearchFiltersButton __instance, bool show)
            {
                try
                {
                    if (show)
                    {
                        _panel = __instance.Panel;
                        OnOpen();
                    }
                    else
                    {
                        OnClose();
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SearchFiltersPanelHandler.ShowPanel: {ex.Message}");
                }
            }
        }

        // Re-announce active filter count whenever filters change.
        [HarmonyPatch(typeof(SearchFiltersPanel), "OnFilterCheckboxChanged")]
        static class SearchFiltersPanel_OnFilterCheckboxChanged_Patch
        {
            static void Postfix(SearchFiltersPanel __instance)
            {
                try
                {
                    if (!_active || _panel != __instance) return;
                    int active = CountActiveFilters(__instance);
                    ScreenReader.Say(Loc.Get("filter_changed", active));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SearchFiltersPanelHandler.OnFilterCheckboxChanged: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(SearchFiltersPanel), "OnCategoryExpandedStateChanged")]
        static class SearchFiltersPanel_OnCategoryExpandedStateChanged_Patch
        {
            static void Postfix(SearchFiltersPanel __instance)
            {
                try
                {
                    if (!_active || _panel != __instance) return;
                    // Rebuild list (visibility of children changes on expand/collapse).
                    BuildList(__instance);
                    // Re-clamp and re-announce focused item.
                    _focusIndex = Mathf.Clamp(_focusIndex, 0, _items.Count - 1);
                    if (_focusIndex >= 0) ReRead();
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SearchFiltersPanelHandler.OnCategoryExpandedStateChanged: {ex.Message}");
                }
            }
        }

        #endregion

        #region Poll / open / close

        public static void PollState()
        {
            if (!_wasOpen) return;
            bool open = _panel != null && _panel.gameObject.activeSelf;
            if (!open && _wasOpen) OnClose();
            _wasOpen = open;
        }

        private static void OnOpen()
        {
            BuildList(_panel);
            _focusIndex = _items.Count > 0 ? 0 : -1;
            _active     = true;
            _wasOpen    = true;
            InputRouter.Push(_ctx);

            int active = CountActiveFilters(_panel);
            ScreenReader.Say(Loc.Get("filter_open", _items.Count, active));
            if (_focusIndex >= 0)
                ScreenReader.Say(_items[0].Announce(1, _items.Count), interrupt: false);

            DebugLogger.LogState($"SearchFiltersPanelHandler: {_items.Count} filters");
        }

        private static void OnClose()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
            _panel      = null;
            InputRouter.Pop(_ctx);
            DebugLogger.LogState("SearchFiltersPanelHandler: closed");
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
            _panel      = null;
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
            ReRead();
        }

        private static void NavigateTo(int index)
        {
            if (_items.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _items.Count - 1);
            ReRead();
        }

        private static void ActivateFocused()
        {
            if (_focusIndex < 0 || _focusIndex >= _items.Count) return;
            _items[_focusIndex].Activate();
        }

        private static void ReRead()
        {
            if (_focusIndex < 0 || _focusIndex >= _items.Count) return;
            ScreenReader.Say(_items[_focusIndex].Announce(_focusIndex + 1, _items.Count));
        }

        #endregion

        #region Build list

        private static void BuildList(SearchFiltersPanel panel)
        {
            _items.Clear();
            if (panel == null) return;
            try
            {
                foreach (Transform child in panel.m_content)
                {
                    if (!child.gameObject.activeSelf) continue;

                    SearchFilterCategoryHeader hdr = child.GetComponent<SearchFilterCategoryHeader>();
                    if (hdr != null)
                    {
                        _items.Add(new HeaderItem(hdr));
                        continue;
                    }

                    SearchFilterCheckbox cb = child.GetComponent<SearchFilterCheckbox>();
                    if (cb != null)
                    {
                        _items.Add(new CheckboxItem(cb));
                        continue;
                    }

                    SearchFilterRange range = child.GetComponent<SearchFilterRange>();
                    if (range != null)
                    {
                        string name = child.GetComponentInChildren<Text>(includeInactive: false)?.text
                            ?? range.Category ?? string.Empty;
                        if (!string.IsNullOrEmpty(name))
                            _items.Add(new RangeItem(name));
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"SearchFiltersPanelHandler.BuildList: {ex.Message}");
            }
        }

        private static int CountActiveFilters(SearchFiltersPanel panel)
        {
            int count = 0;
            try
            {
                SearchFilterCheckbox[] cbs =
                    panel.GetComponentsInChildren<SearchFilterCheckbox>(includeInactive: true);
                foreach (SearchFilterCheckbox cb in cbs)
                    if (cb.m_toggle.isOn) count++;
            }
            catch { /* ignore */ }
            return count;
        }

        #endregion
    }
}
