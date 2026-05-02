using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Select Wallpaper app: tab changes and wallpaper applied.
    ///
    /// Patches private tab selection methods and OnApplyCustomWallpaper Postfix.
    /// </summary>
    public static class SelectWallpaperHandler
    {
        [HarmonyPatch(typeof(SelectWallpaperApp), "OnClientWallpaperTabSelected")]
        static class SelectWallpaper_ClientTab_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("wallpaper_tab_client"));
                    DebugLogger.LogState("SelectWallpaperHandler: client tab");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SelectWallpaper_ClientTab_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(SelectWallpaperApp), "OnCustomWallpaperTabSelected")]
        static class SelectWallpaper_CustomTab_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("wallpaper_tab_custom"));
                    DebugLogger.LogState("SelectWallpaperHandler: custom tab");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SelectWallpaper_CustomTab_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(SelectWallpaperApp), "OnWorkshopWallpaperTabSelected")]
        static class SelectWallpaper_WorkshopTab_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("wallpaper_tab_workshop"));
                    DebugLogger.LogState("SelectWallpaperHandler: workshop tab");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SelectWallpaper_WorkshopTab_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(SelectWallpaperApp), "OnApplyCustomWallpaper")]
        static class SelectWallpaper_Apply_Patch
        {
            static void Postfix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("wallpaper_applied"));
                    DebugLogger.LogState("SelectWallpaperHandler: wallpaper applied");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SelectWallpaper_Apply_Patch: {ex.Message}");
                }
            }
        }
    }
}
