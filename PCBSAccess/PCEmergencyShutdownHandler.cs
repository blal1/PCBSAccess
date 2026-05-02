using System;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the reason whenever the virtual PC has an emergency shutdown (BSOD).
    /// Patches VirtualComputer.BlueScreenOfDeath before the blue screen activates,
    /// so the screen reader fires before the game locks input.
    /// </summary>
    public static class PCEmergencyShutdownHandler
    {
        [HarmonyPatch(typeof(VirtualComputer), "BlueScreenOfDeath")]
        static class BlueScreenOfDeath_Patch
        {
            static void Prefix(EmergencyShutdownReason reason)
            {
                try
                {
                    string reasonText = GetReasonText(reason);
                    ScreenReader.Say(Loc.Get("bsod_announce", reasonText));
                    DebugLogger.LogState($"PCEmergencyShutdown: {reason}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"PCEmergencyShutdownHandler: {ex.Message}");
                }
            }
        }

        private static string GetReasonText(EmergencyShutdownReason reason)
        {
            switch (reason)
            {
                case EmergencyShutdownReason.WATTAGE:
                    return Loc.Get("bsod_wattage");
                case EmergencyShutdownReason.CPU_HEAT:
                    return Loc.Get("bsod_cpu_heat");
                case EmergencyShutdownReason.GPU_HEAT:
                    return Loc.Get("bsod_gpu_heat");
                case EmergencyShutdownReason.CPU_UNDER_VOLTAGE:
                    return Loc.Get("bsod_cpu_volt");
                case EmergencyShutdownReason.GPU_UNDER_VOLTAGE:
                    return Loc.Get("bsod_gpu_volt");
                case EmergencyShutdownReason.RAM_UNDER_VOLTAGE:
                    return Loc.Get("bsod_ram_volt");
                default:
                    return reason.ToString();
            }
        }
    }
}
