using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the delivery manifest screen (list of received parts/jobs).
    ///
    /// On open (Manifest.Init patch): announces item count.
    /// Up/Down: navigate StoredComponent rows and announce each item.
    /// Home/End: jump to first/last item.
    /// Escape: handled by the game's back button automatically.
    /// </summary>
    public static class ManifestHandler
    {
        #region State

        private static bool _active;
        private static bool _wasOpen;
        private static readonly List<string> _items = new List<string>();
        private static int _focusIndex = -1;

        private static readonly Regex s_richTag = new Regex("<[^>]+>", RegexOptions.Compiled);

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "Manifest";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.DownArrow)) { Navigate(1);                     return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))   { Navigate(-1);                    return true; }
                if (Input.GetKeyDown(KeyCode.Home))      { NavigateTo(0);                   return true; }
                if (Input.GetKeyDown(KeyCode.End))       { NavigateTo(_items.Count - 1);    return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("manifest_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patch — fires when manifest is filled

        [HarmonyPatch(typeof(Manifest), "Init",
            new[] { typeof(System.Collections.Generic.IEnumerable<PartInstance>), typeof(bool) })]
        static class Manifest_Init_Patch
        {
            static void Postfix(Manifest __instance)
            {
                try
                {
                    BuildItemList(__instance);
                    _active     = true;
                    _focusIndex = -1;
                    _wasOpen    = true;
                    InputRouter.Push(_ctx);

                    ScreenReader.Say(Loc.Get("manifest_open", _items.Count));
                    if (_items.Count > 0)
                        ScreenReader.Say(Loc.Get("manifest_hint"), interrupt: false);

                    DebugLogger.LogState($"ManifestHandler: {_items.Count} items");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ManifestHandler.Init: {ex.Message}");
                }
            }
        }

        #endregion

        #region Close detection

        /// <summary>Called every frame from Main.Update().</summary>
        public static void PollState()
        {
            if (!_wasOpen) return;
            bool open = IsOpen();
            if (!open && _wasOpen) OnClose();
            _wasOpen = open;
        }

        private static bool IsOpen()
        {
            try
            {
                return WorkshopUI.m_manifest != null
                    && WorkshopUI.m_manifest.gameObject.activeSelf;
            }
            catch { return false; }
        }

        private static void OnClose()
        {
            _active     = false;
            _wasOpen    = false;
            _focusIndex = -1;
            _items.Clear();
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

        private static void AnnounceItem(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            ScreenReader.Say($"{index + 1} {Loc.Get("nav_of")} {_items.Count}: {_items[index]}");
        }

        #endregion

        #region Build list from UI

        private static void BuildItemList(Manifest manifest)
        {
            _items.Clear();
            try
            {
                StoredComponent[] rows = manifest.GetComponentsInChildren<StoredComponent>(includeInactive: false);
                foreach (StoredComponent row in rows)
                {
                    string raw = row.m_text?.text ?? string.Empty;
                    string clean = s_richTag.Replace(raw, string.Empty).Trim();
                    // Replace newlines with comma-space for linear reading
                    clean = clean.Replace('\n', ',').Replace(",,", ",").Trim(',').Trim();
                    if (!string.IsNullOrEmpty(clean))
                        _items.Add(clean);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ManifestHandler.BuildItemList: {ex.Message}");
            }
        }

        #endregion
    }
}
