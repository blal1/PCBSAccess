using System;
using System.Reflection;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TestMod
{
    /// <summary>
    /// Helper class for accessing private fields via Reflection.
    /// Use this when game classes use [SerializeField] private fields.
    /// See docs/unity-reflection-guide.md for usage patterns.
    /// </summary>
    public static class ReflectionHelper
    {
        private const BindingFlags PrivateFlags =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private const BindingFlags AllFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly System.Collections.Generic.Dictionary<(Type, string), FieldInfo> FieldCache =
            new System.Collections.Generic.Dictionary<(Type, string), FieldInfo>();

        private static readonly System.Collections.Generic.Dictionary<(Type, string), MethodInfo> MethodCache =
            new System.Collections.Generic.Dictionary<(Type, string), MethodInfo>();

        /// <summary>
        /// Gets a private field value as a reference type.
        /// Returns null if field not found or value is null.
        /// </summary>
        public static T GetPrivateField<T>(object obj, string fieldName) where T : class
        {
            if (obj == null) return null;

            try
            {
                var field = GetCachedField(obj.GetType(), fieldName, PrivateFlags);
                return field?.GetValue(obj) as T;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ReflectionHelper] Failed to get field '{fieldName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a private field value as a value type (int, bool, enum, etc.).
        /// Returns defaultValue if field not found.
        /// </summary>
        public static T GetPrivateFieldValue<T>(object obj, string fieldName, T defaultValue = default) where T : struct
        {
            if (obj == null) return defaultValue;

            try
            {
                var field = GetCachedField(obj.GetType(), fieldName, PrivateFlags);
                if (field == null) return defaultValue;

                var value = field.GetValue(obj);
                if (value == null) return defaultValue;

                return (T)value;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ReflectionHelper] Failed to get field '{fieldName}': {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a private field value.
        /// </summary>
        public static bool SetPrivateField(object obj, string fieldName, object value)
        {
            if (obj == null) return false;

            try
            {
                var field = GetCachedField(obj.GetType(), fieldName, PrivateFlags);
                if (field == null) return false;

                field.SetValue(obj, value);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ReflectionHelper] Failed to set field '{fieldName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shortcut: Gets text from a private TextMeshProUGUI field.
        /// Tries the field name as-is, then with m_ prefix.
        /// </summary>
        public static string GetTextFromPrivateField(object obj, string fieldName)
        {
            if (obj == null) return null;

            // Try as TextMeshProUGUI
            var tmp = GetPrivateField<TextMeshProUGUI>(obj, fieldName);
            if (tmp != null) return tmp.text;

            // Try with m_ prefix
            tmp = GetPrivateField<TextMeshProUGUI>(obj, "m_" + fieldName);
            if (tmp != null) return tmp.text;

            // Try as legacy Text component
            var text = GetPrivateField<Text>(obj, fieldName);
            if (text != null) return text.text;

            text = GetPrivateField<Text>(obj, "m_" + fieldName);
            if (text != null) return text.text;

            return null;
        }

        /// <summary>
        /// Gets FieldInfo for caching. Use this for frequently accessed fields.
        /// </summary>
        public static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            return GetCachedField(type, fieldName, AllFlags);
        }

        /// <summary>
        /// Gets FieldInfo, trying multiple possible names.
        /// Useful when field naming convention is unknown.
        /// </summary>
        public static FieldInfo GetFieldInfo(Type type, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var field = GetCachedField(type, name, AllFlags);
                if (field != null) return field;
            }
            return null;
        }

        /// <summary>
        /// Gets MethodInfo with caching.
        /// </summary>
        public static MethodInfo GetMethodInfo(Type type, string methodName)
        {
            return GetCachedMethod(type, methodName, AllFlags);
        }

        /// <summary>
        /// Helper to get cached FieldInfo.
        /// </summary>
        private static FieldInfo GetCachedField(Type type, string fieldName, BindingFlags flags)
        {
            var key = (type, fieldName);
            if (FieldCache.TryGetValue(key, out var field)) return field;

            field = type.GetField(fieldName, flags);
            if (field != null) FieldCache[key] = field;
            return field;
        }

        /// <summary>
        /// Helper to get cached MethodInfo.
        /// </summary>
        private static MethodInfo GetCachedMethod(Type type, string methodName, BindingFlags flags)
        {
            var key = (type, methodName);
            if (MethodCache.TryGetValue(key, out var method)) return method;

            method = type.GetMethod(methodName, flags);
            if (method != null) MethodCache[key] = method;
            return method;
        }

        /// <summary>
        /// Lists all private fields of a type. Useful for debugging/exploration.
        /// </summary>
        public static void LogPrivateFields(Type type)
        {
            Debug.Log($"[ReflectionHelper] Private fields of {type.Name}:");
            var fields = type.GetFields(PrivateFlags);
            foreach (var field in fields)
            {
                Debug.Log($"  - {field.Name} : {field.FieldType.Name}");
            }
        }
    }
}

