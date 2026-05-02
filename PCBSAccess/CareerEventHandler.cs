using System;
using System.Reflection;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces global career events via CareerStatus static delegates.
    ///
    /// Subscribes in Awake, unsubscribes in OnDestroy (via Main).
    /// Events: s_onLevelUp(int zeroIndexedLevel), s_onNewReview (Action).
    /// </summary>
    public static class CareerEventHandler
    {
        #region Public API

        /// <summary>Called from Main.Awake() — subscribe to career events.</summary>
        public static void Subscribe()
        {
            // Use Reflection to avoid Mono 2.0 type-identity issues with System.Action
            // across assembly boundaries (TypeLoadException: Could not load type 'System.Action'
            // from assembly 'PCBSAccess').
            SubscribeField("s_onLevelUp",   nameof(OnLevelUp));
            SubscribeField("s_onNewReview", nameof(OnNewReview));
            DebugLogger.LogState("CareerEventHandler: subscribed");
        }

        /// <summary>Called from Main.OnDestroy() — unsubscribe to avoid leaks.</summary>
        public static void Unsubscribe()
        {
            UnsubscribeField("s_onLevelUp",   nameof(OnLevelUp));
            UnsubscribeField("s_onNewReview", nameof(OnNewReview));
        }

        private static void SubscribeField(string fieldName, string handlerName)
        {
            try
            {
                var field = typeof(CareerStatus).GetField(fieldName,
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if ((object)field == null) { DebugLogger.LogWarning($"CareerEventHandler: field '{fieldName}' not found"); return; }

                var method = typeof(CareerEventHandler).GetMethod(handlerName,
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                var del = Delegate.CreateDelegate(field.FieldType, method);
                field.SetValue(null, Delegate.Combine((MulticastDelegate)field.GetValue(null), del));
            }
            catch (Exception ex) { DebugLogger.LogWarning($"CareerEventHandler.Subscribe '{fieldName}': {ex.Message}"); }
        }

        private static void UnsubscribeField(string fieldName, string handlerName)
        {
            try
            {
                var field = typeof(CareerStatus).GetField(fieldName,
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if ((object)field == null) return;

                var method = typeof(CareerEventHandler).GetMethod(handlerName,
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                var del = Delegate.CreateDelegate(field.FieldType, method);
                field.SetValue(null, Delegate.Remove((MulticastDelegate)field.GetValue(null), del));
            }
            catch (Exception ex) { DebugLogger.LogWarning($"CareerEventHandler.Unsubscribe '{fieldName}': {ex.Message}"); }
        }

        #endregion

        #region Handlers

        private static void OnLevelUp(int zeroIndexedLevel)
        {
            try
            {
                int displayLevel = zeroIndexedLevel + 1;
                ScreenReader.Say(Loc.Get("career_level_up", displayLevel));
                DebugLogger.LogState($"CareerEventHandler: level up → {displayLevel}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CareerEventHandler.OnLevelUp: {ex.Message}");
            }
        }

        private static void OnNewReview()
        {
            try
            {
                var career = CareerStatus.Get();
                if (career == null) return;

                float stars = career.GetStarRating();
                ScreenReader.Say(Loc.Get("career_new_review", stars.ToString("F1")));
                DebugLogger.LogState($"CareerEventHandler: new review, rating={stars:F1}");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"CareerEventHandler.OnNewReview: {ex.Message}");
            }
        }

        #endregion
    }
}
