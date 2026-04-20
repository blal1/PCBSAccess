using System.Collections.Generic;
using I2.Loc;

namespace PCBSAccess
{
    /// <summary>
    /// Localization for mod-added labels. Supports English (default) and French.
    /// Game text (tutorial body, emails, part names) comes directly from the game — Loc only covers labels the mod adds.
    /// Usage: Loc.Get("key") or Loc.Get("key", arg0, arg1)
    /// </summary>
    public static class Loc
    {
        private static bool _initialized = false;
        private static Dictionary<string, string> _current = null;

        private static readonly Dictionary<string, string> _english = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _french = new Dictionary<string, string>();

        /// <summary>Initializes localization. Call once from Main.Awake().</summary>
        public static void Initialize()
        {
            if (_initialized) return;
            PopulateStrings();
            RefreshLanguage();
            _initialized = true;
        }

        /// <summary>
        /// Refreshes the active language from the game's current setting.
        /// Call if the player changes language at runtime.
        /// </summary>
        public static void RefreshLanguage()
        {
            try
            {
                string lang = LocalizationManager.CurrentLanguage;
                _current = lang == "French" ? _french : _english;
            }
            catch
            {
                _current = _english;
            }
        }

        /// <summary>Returns the localized string for key. Falls back to English, then the key itself.</summary>
        public static string Get(string key)
        {
            if (!_initialized) Initialize();

            if (_current != null && _current.TryGetValue(key, out string val)) return val;
            if (_english.TryGetValue(key, out string eng)) return eng;
            return key;
        }

        /// <summary>Returns the localized string with format arguments substituted ({0}, {1}, ...).</summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            try { return string.Format(template, args); }
            catch { return template; }
        }

        private static void Add(string key, string english, string french)
        {
            _english[key] = english;
            _french[key] = french;
        }

        private static void PopulateStrings()
        {
            // Startup
            Add("mod_loaded",   "Accessibility mod loaded. Press F1 for status.",
                                "Mod d'accessibilité chargé. Appuyez sur F1 pour le statut.");
            Add("debug_on",     "Debug mode on.",       "Mode débogage activé.");
            Add("debug_off",    "Debug mode off.",      "Mode débogage désactivé.");

            // Career status (F1)
            Add("cash",         "Cash",         "Argent");
            Add("kudos",        "Kudos",        "Kudos");
            Add("rating",       "Rating",       "Évaluation");
            Add("stars",        "stars",        "étoiles");

            // Build mode
            Add("installed",    "Installed",    "Installé");
            Add("required",     "Required",     "Requis");
            Add("empty",        "empty",        "vide");
            Add("none",         "none",         "aucun");
        }
    }
}
