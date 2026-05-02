using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Market app on open: lists every category and its current day value.
    /// Patches MarketApp.Start (called when app window opens).
    /// </summary>
    public static class MarketAppHandler
    {
        public static void Reset() { }

        [HarmonyPatch(typeof(MarketApp), "Start")]
        static class MarketApp_Start_Patch
        {
            static void Postfix(MarketApp __instance)
            {
                try
                {
                    int today = CareerStatus.Get().GetToday();

                    IEnumerable<PartsDatabase.MarketEntry> entries = PartsDatabase.MarketCategories();
                    if (entries == null)
                    {
                        ScreenReader.Say(Loc.Get("market_open_empty"));
                        return;
                    }

                    ScreenReader.Say(Loc.Get("market_open"));

                    foreach (PartsDatabase.MarketEntry e in entries)
                    {
                        try
                        {
                            float value = CareerStatus.Get().GetMarketValue(e.m_key, today);
                            // Convert raw value to percentage change (100 = baseline)
                            int pct = Mathf.RoundToInt(value) - 100;
                            string sign = pct >= 0 ? "+" : "";
                            ScreenReader.Say($"{e.m_uiName}: {sign}{pct}%.", interrupt: false);
                        }
                        catch { /* skip bad entry */ }
                    }

                    DebugLogger.LogState("MarketAppHandler: opened");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MarketApp_Start_Patch: {ex.Message}");
                }
            }
        }
    }
}
