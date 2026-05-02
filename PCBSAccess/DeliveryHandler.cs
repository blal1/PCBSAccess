using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces delivery events in career mode.
    ///
    /// - Polls for new deliveries arriving (day-end processing).
    ///   Announces once when DeliveriesWaiting() transitions false → true.
    /// - Patches Manifest.Init to announce collected item list.
    /// </summary>
    public static class DeliveryHandler
    {
        #region State

        private static bool _wasDeliveryWaiting;

        // Regex to strip Unity rich-text tags (e.g. <b>, </b>, <color=...>)
        private static readonly Regex s_richTag = new Regex("<[^>]+>", RegexOptions.Compiled);

        #endregion

        #region Public API

        /// <summary>Polls delivery state. Call from Main.Update each frame.</summary>
        public static void Update()
        {
            try
            {
                bool waiting = CareerStatus.Get() != null && CareerStatus.Get().DeliveriesWaiting();
                if (waiting && !_wasDeliveryWaiting)
                {
                    ScreenReader.Say(Loc.Get("delivery_arrived"));
                    DebugLogger.LogState("DeliveryHandler: delivery arrived");
                }
                _wasDeliveryWaiting = waiting;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"DeliveryHandler.Update: {ex.Message}");
            }
        }

        /// <summary>Resets on scene change.</summary>
        public static void Reset()
        {
            _wasDeliveryWaiting = false;
        }

        #endregion

        #region Patches

        /// <summary>
        /// Fires after Manifest.Init populates the scroll list.
        /// Reads item names from the created StoredComponent children and
        /// announces the full delivery list.
        /// </summary>
        [HarmonyPatch(typeof(Manifest), "Init")]
        static class Manifest_Init_Patch
        {
            static void Postfix(Manifest __instance)
            {
                try
                {
                    StoredComponent[] items = __instance.m_itemList.content
                        .GetComponentsInChildren<StoredComponent>(includeInactive: false);

                    if (items == null || items.Length == 0)
                    {
                        ScreenReader.Say(Loc.Get("delivery_no_items"));
                        return;
                    }

                    ScreenReader.Say(Loc.Get("delivery_manifest", items.Length));

                    foreach (StoredComponent item in items)
                    {
                        if (item?.m_text == null) continue;
                        string clean = s_richTag.Replace(item.m_text.text, string.Empty);
                        if (!string.IsNullOrWhiteSpace(clean))
                            ScreenReader.Say(clean, interrupt: false);
                    }

                    DebugLogger.LogState($"DeliveryHandler: manifest, {items.Length} items");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"Manifest_Init_Patch: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
