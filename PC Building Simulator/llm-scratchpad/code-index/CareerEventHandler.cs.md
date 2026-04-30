# Code Index for CareerEventHandler.cs

- Line 7: /// <summary>
- Line 8: /// Announces global career events via CareerStatus static delegates.
- Line 9: ///
- Line 10: /// Subscribes in Awake, unsubscribes in OnDestroy (via Main).
- Line 11: /// Events: s_onLevelUp(int zeroIndexedLevel), s_onNewReview (Action).
- Line 12: /// </summary>
- Line 13: public static class CareerEventHandler
- Line 17: /// <summary>Called from Main.Awake() — subscribe to career events.</summary>
- Line 18: public static void Subscribe()
- Line 28: /// <summary>Called from Main.OnDestroy() — unsubscribe to avoid leaks.</summary>
- Line 29: public static void Unsubscribe()
- Line 35: private static void SubscribeField(string fieldName, string handlerName)
- Line 51: private static void UnsubscribeField(string fieldName, string handlerName)
- Line 71: private static void OnLevelUp(int zeroIndexedLevel)
- Line 85: private static void OnNewReview()
