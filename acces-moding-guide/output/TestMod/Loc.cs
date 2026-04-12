using System.Collections.Generic;
using UnityEngine;

namespace TestMod
{
    /// <summary>
    /// Central localization for the accessibility mod.
    /// Automatically detects game language.
    ///
    /// Usage:
    ///   Loc.Get("key")              - Get string
    ///   Loc.Get("key", arg1, arg2)  - String with placeholders {0}, {1}
    /// </summary>
    public static class Loc
    {
        #region Fields

        private static bool _initialized = false;
        private static string _currentLang = "en";

        // Dictionaries for each supported language
        private static readonly Dictionary<string, string> _german = new();
        private static readonly Dictionary<string, string> _english = new();
        private static readonly Dictionary<string, string> _spanish = new();
        private static readonly Dictionary<string, string> _french = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes localization. Call once at mod startup.
        /// </summary>
        public static void Initialize()
        {
            InitializeStrings();
            RefreshLanguage();
            _initialized = true;
        }

        /// <summary>
        /// Updates language based on game setting.
        /// Call when player changes language in game settings.
        /// </summary>
        public static void RefreshLanguage()
        {
            string gameLang = GetGameLanguage();

            switch (gameLang)
            {
                case "de": _currentLang = "de"; break;
                case "es": _currentLang = "es"; break;
                case "fr": _currentLang = "fr"; break;
                default:   _currentLang = "en"; break;
            }
        }

        /// <summary>
        /// Gets a localized string.
        /// </summary>
        public static string Get(string key)
        {
            if (!_initialized) Initialize();

            var dict = GetCurrentDictionary();

            // Try current language
            if (dict.TryGetValue(key, out string value))
                return value;

            // Fallback: English
            if (_english.TryGetValue(key, out string engValue))
                return engValue;

            // Last fallback: Key itself (helps with debugging)
            return key;
        }

        /// <summary>
        /// Gets a localized string with placeholders.
        /// Uses {0}, {1}, {2} etc. as placeholders.
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        /// <summary>
        /// OPTIONAL: Load strings from a JSON string.
        /// Useful if you want to allow users to provide their own translations.
        /// Format: {"translations": [{"key": "hello", "value": "Hi"}]}
        /// </summary>
        public static void LoadFromJson(string lang, string json)
        {
            try
            {
                var data = JsonUtility.FromJson<JsonTranslationList>(json);
                if (data == null || data.translations == null) return;

                var dict = GetDictionaryForLang(lang);
                foreach (var entry in data.translations)
                {
                    dict[entry.key] = entry.value;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Loc] Failed to load JSON for {lang}: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// === ADAPT THIS FOR YOUR GAME ===
        /// Reads the current game language.
        /// </summary>
        private static string GetGameLanguage()
        {
            // TODO: Search decompiled code for: Language, Localization, I18n, getAlias()

            // UNITY EXAMPLES:
            // return Language.getAlias();
            // return PlayerPrefs.GetString("language", "en");
            // return Application.systemLanguage.ToString().Substring(0, 2).ToLower();

            // FALLBACK:
            return "en";
        }

        private static Dictionary<string, string> GetCurrentDictionary()
        {
            return GetDictionaryForLang(_currentLang);
        }

        private static Dictionary<string, string> GetDictionaryForLang(string lang)
        {
            switch (lang)
            {
                case "de": return _german;
                case "es": return _spanish;
                case "fr": return _french;
                default:   return _english;
            }
        }

        /// <summary>
        /// Helper method: Adds a string in common languages.
        /// </summary>
        private static void Add(string key, string en, string de = null, string es = null, string fr = null)
        {
            _english[key] = en;
            if (de != null) _german[key] = de;
            if (es != null) _spanish[key] = es;
            if (fr != null) _french[key] = fr;
        }

        /// <summary>
        /// Define all translations here.
        /// </summary>
        private static void InitializeStrings()
        {
            // ===== GENERAL =====
            Add("mod_loaded",
                en: "[TestMod] loaded. F1 for help.",
                de: "[TestMod] geladen. F1 für Hilfe.");

            Add("help_title",
                en: "Help:",
                de: "Hilfe:");

            // ===== WITH PLACEHOLDERS =====
            // Syntax: {0}, {1}, {2} etc.
            Add("item_count",
                en: "{0} items",
                de: "{0} Gegenstände");

            // ===== HANDLER SPECIFIC =====
            // Add strings for each handler here
            // Convention: [handler]_[action]
            // Add("inventory_opened", "Inventory opened", "Inventar geöffnet");
        }

        #endregion

        #region Helper Classes for JSON

        [System.Serializable]
        private class JsonTranslation { public string key; public string value; }

        [System.Serializable]
        private class JsonTranslationList { public JsonTranslation[] translations; }

        #endregion
    }
}

