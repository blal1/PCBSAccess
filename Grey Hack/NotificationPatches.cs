using HarmonyLib;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patch for notification accessibility.
    /// Intercepts ItemNotification.Spawn to announce notifications via screen reader.
    /// </summary>
    [HarmonyPatch(typeof(ItemNotification), "Spawn")]
    public class ItemNotificationSpawnPatch
    {
        static void Postfix(string text, Notifications.NotifType notifType)
        {
            DebugLogger.LogState($"ItemNotification.Spawn: type={notifType}, text='{text}'");
            NotificationHandler.OnNotificationSpawned(text, notifType);
        }
    }
}
