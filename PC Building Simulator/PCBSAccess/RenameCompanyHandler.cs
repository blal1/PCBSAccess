using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Rename Company dialog: current name on open, result on apply or cancel.
    ///
    /// Patches RenameCompany.OnEnable (open), OnApply (Prefix: read before navigate away), OnBack (cancel).
    /// </summary>
    public static class RenameCompanyHandler
    {
        [HarmonyPatch(typeof(RenameCompany), "OnEnable")]
        static class RenameCompany_OnEnable_Patch
        {
            static void Postfix(RenameCompany __instance)
            {
                try
                {
                    string current = __instance.m_input?.text ?? string.Empty;
                    ScreenReader.Say(Loc.Get("rename_company_open", current));
                    DebugLogger.LogState($"RenameCompanyHandler: open, current='{current}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"RenameCompany_OnEnable_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(RenameCompany), "OnApply")]
        static class RenameCompany_OnApply_Patch
        {
            static void Prefix(RenameCompany __instance)
            {
                try
                {
                    string name = __instance.m_input?.text;
                    if (!string.IsNullOrEmpty(name))
                    {
                        ScreenReader.Say(Loc.Get("rename_company_applied", name));
                        DebugLogger.LogState($"RenameCompanyHandler: applied '{name}'");
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"RenameCompany_OnApply_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(RenameCompany), "OnBack")]
        static class RenameCompany_OnBack_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("rename_company_cancelled"));
                    DebugLogger.LogState("RenameCompanyHandler: cancelled");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"RenameCompany_OnBack_Patch: {ex.Message}");
                }
            }
        }
    }
}
