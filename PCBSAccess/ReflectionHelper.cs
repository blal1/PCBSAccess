using System;
using System.Reflection;
using TMPro;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Helpers for accessing private [SerializeField] fields via Reflection.
    /// The game has 1,119 private SerializeField fields — almost all UI access needs this.
    /// </summary>
    public static class ReflectionHelper
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags All     = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>Gets a private reference-type field. Returns null if not found.</summary>
        public static T GetField<T>(object obj, string fieldName) where T : class
        {
            if (obj == null) return null;
            try
            {
                return obj.GetType().GetField(fieldName, Private)?.GetValue(obj) as T;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ReflectionHelper.GetField<{typeof(T).Name}>('{fieldName}'): {ex.Message}");
                return null;
            }
        }

        /// <summary>Gets a private value-type field (int, bool, enum, struct). Returns defaultValue if not found.</summary>
        public static T GetFieldValue<T>(object obj, string fieldName, T defaultValue = default) where T : struct
        {
            if (obj == null) return defaultValue;
            try
            {
                var field = obj.GetType().GetField(fieldName, Private);
                if (field == null) return defaultValue;
                var val = field.GetValue(obj);
                return val == null ? defaultValue : (T)val;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ReflectionHelper.GetFieldValue<{typeof(T).Name}>('{fieldName}'): {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>Sets a private field value. Returns true on success.</summary>
        public static bool SetField(object obj, string fieldName, object value)
        {
            if (obj == null) return false;
            try
            {
                var field = obj.GetType().GetField(fieldName, Private);
                if (field == null) return false;
                field.SetValue(obj, value);
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ReflectionHelper.SetField('{fieldName}'): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads text from a private Text or TextMeshProUGUI field.
        /// Tries the exact name, then with "m_" prefix.
        /// Returns null if field not found.
        /// </summary>
        public static string GetText(object obj, string fieldName)
        {
            if (obj == null) return null;

            foreach (var name in new[] { fieldName, "m_" + fieldName })
            {
                var tmp = GetField<TextMeshProUGUI>(obj, name);
                if (tmp != null) return tmp.text;

                var txt = GetField<Text>(obj, name);
                if (txt != null) return txt.text;
            }

            return null;
        }

        /// <summary>Gets a FieldInfo for caching. Use when a field is accessed on every frame.</summary>
        public static FieldInfo GetFieldInfo(Type type, string fieldName) =>
            type.GetField(fieldName, All);

        /// <summary>Gets a FieldInfo, trying multiple possible names (useful when naming convention is unknown).</summary>
        public static FieldInfo GetFieldInfo(Type type, params string[] names)
        {
            foreach (var name in names)
            {
                var f = type.GetField(name, All);
                if (f != null) return f;
            }
            return null;
        }
    }
}
