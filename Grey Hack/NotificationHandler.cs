namespace GreyHackAccess
{
    /// <summary>
    /// Handles screen reader announcements for game notifications.
    /// Stores the last notification for repeat with Alt+N.
    /// </summary>
    public class NotificationHandler
    {
        #region Fields

        private static NotificationHandler _instance;
        private string _lastNotification;

        #endregion

        #region Static Entry

        /// <summary>
        /// Called by Harmony patch when a notification spawns.
        /// </summary>
        public static void OnNotificationSpawned(string message, Notifications.NotifType notifType)
        {
            if (_instance == null) return;
            _instance.HandleNotification(message, notifType);
        }

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Repeats the last notification via screen reader.
        /// Called from Main.cs when Alt+N is pressed.
        /// </summary>
        public void RepeatLastNotification()
        {
            if (string.IsNullOrEmpty(_lastNotification))
            {
                ScreenReader.Say(Loc.Get("notif_none"));
                return;
            }

            ScreenReader.Say(Loc.Get("notif_repeat", _lastNotification));
        }

        #endregion

        #region Private Methods

        private void HandleNotification(string message, Notifications.NotifType notifType)
        {
            string announcement = BuildAnnouncement(message, notifType);
            _lastNotification = announcement;

            ScreenReader.Say(announcement);
            DebugLogger.LogState($"NotificationHandler: {notifType} - '{message}'");
        }

        private static string BuildAnnouncement(string message, Notifications.NotifType notifType)
        {
            // INFO notifications don't need a type prefix - they're generic
            if (notifType == Notifications.NotifType.INFO || notifType == Notifications.NotifType.NONE)
            {
                return Loc.Get("notif_announced_info", message);
            }

            string typeName = GetTypeName(notifType);
            return Loc.Get("notif_announced", typeName, message);
        }

        private static string GetTypeName(Notifications.NotifType notifType)
        {
            switch (notifType)
            {
                case Notifications.NotifType.WARNING: return Loc.Get("notif_type_warning");
                case Notifications.NotifType.MAIL: return Loc.Get("notif_type_mail");
                case Notifications.NotifType.NOTEPAD: return Loc.Get("notif_type_notepad");
                case Notifications.NotifType.CODEEDITOR: return Loc.Get("notif_type_codeeditor");
                case Notifications.NotifType.LOGWINDOW: return Loc.Get("notif_type_logwindow");
                case Notifications.NotifType.CONFIGLAN: return Loc.Get("notif_type_configlan");
                case Notifications.NotifType.MAP: return Loc.Get("notif_type_map");
                case Notifications.NotifType.CONNECTION: return Loc.Get("notif_type_connection");
                case Notifications.NotifType.EXPLOITREPORT: return Loc.Get("notif_type_exploitreport");
                case Notifications.NotifType.CLIPBOARD: return Loc.Get("notif_type_clipboard");
                default: return Loc.Get("notif_type_info");
            }
        }

        #endregion
    }
}
