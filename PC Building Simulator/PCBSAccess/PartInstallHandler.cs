using System;
using System.Reflection;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces part installation and removal completion.
    ///
    /// Fires on InstallingPartState.OnStateDestroy (Prefix so we read the part
    /// name before the component GameObject is destroyed).
    /// m_dir  > 0 → part installed into slot.
    /// m_dir  &lt; 0 → part removed from slot (intentional or cancelled install).
    /// </summary>
    public static class PartInstallHandler
    {
        // Cached reflection fields — resolved once on first use
        private static FieldInfo _dirField;
        private static FieldInfo _componentField;

        private static FieldInfo DirField =>
            _dirField ?? (_dirField = typeof(InstallingPartState)
                .GetField("m_dir", BindingFlags.NonPublic | BindingFlags.Instance));

        private static FieldInfo ComponentField =>
            _componentField ?? (_componentField = typeof(InstallingPartState)
                .GetField("m_component", BindingFlags.NonPublic | BindingFlags.Instance));

        #region Patches

        /// <summary>
        /// Prefix: fires before OnStateDestroy so we can read the component
        /// name before it may be destroyed.
        /// </summary>
        [HarmonyPatch(typeof(InstallingPartState), "OnStateDestroy")]
        static class InstallingPartState_OnStateDestroy_Patch
        {
            static void Prefix(InstallingPartState __instance)
            {
                try
                {
                    float dir = (float)DirField.GetValue(__instance);
                    ComponentPC comp = (ComponentPC)ComponentField.GetValue(__instance);

                    if (comp == null) return;

                    string name = comp.GetPartInstance()?.GetPart()?.m_uiName;
                    if (string.IsNullOrEmpty(name)) return;

                    string key = dir > 0f ? "part_installed" : "part_removed";
                    ScreenReader.Say($"{Loc.Get(key)}: {name}.");
                    DebugLogger.LogState($"PartInstallHandler: dir={dir:F0} '{name}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"PartInstallHandler.Prefix: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
