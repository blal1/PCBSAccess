using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the focused day and its events when the calendar widget changes focus.
    /// Only fires when the day actually changes, and only 1 second after startup
    /// so the initial calendar build during scene load stays silent.
    /// </summary>
    public static class CalendarHandler
    {
        private static int  _lastAnnouncedDay = -1;
        private static bool _ready;
        private static float _startTime;

        /// <summary>Called from Main when a workshop scene loads.</summary>
        public static void Reset()
        {
            _lastAnnouncedDay = -1;
            _ready = false;
            _startTime = Time.realtimeSinceStartup;
        }

        // Delayed ready: ignore the first 1.5 s after scene load (calendar builds silently).
        private static bool IsReady()
        {
            if (_ready) return true;
            if (Time.realtimeSinceStartup - _startTime > 1.5f)
            {
                _ready = true;
                return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(CalendarWidget), "SetFocusDay")]
        static class CalendarWidget_SetFocusDay_Patch
        {
            static void Postfix(int day)
            {
                try
                {
                    if (!IsReady())       return;
                    if (day == _lastAnnouncedDay) return;
                    _lastAnnouncedDay = day;
                    AnnounceDay(day);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"CalendarHandler.SetFocusDay: {ex.Message}");
                }
            }
        }

        private static void AnnounceDay(int day)
        {
            try
            {
                var career = CareerStatus.Get();
                if (career == null) return;

                Calendar cal = career.GetCalendar();
                if (cal == null) return;

                string dateStr = cal.GetLongDateString(day);

                IEnumerable<CalendarEvent> events = cal.GetEventsOnDay(day);
                var sb = new StringBuilder();
                sb.Append(dateStr);

                bool hasEvent = false;
                foreach (CalendarEvent ev in events)
                {
                    if (!ev.IsVisible()) continue;
                    string desc = ev.GetDescription();
                    if (string.IsNullOrEmpty(desc)) continue;
                    if (!hasEvent)
                    {
                        sb.Append(". ");
                        hasEvent = true;
                    }
                    else
                    {
                        sb.Append(". ");
                    }
                    sb.Append(desc);
                }

                if (!hasEvent)
                    sb.Append(". ").Append(Loc.Get("calendar_no_events"));

                ScreenReader.Say(sb.ToString());
                DebugLogger.LogState($"CalendarHandler: day={day} events={hasEvent}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CalendarHandler.AnnounceDay: {ex.Message}");
            }
        }
    }
}
