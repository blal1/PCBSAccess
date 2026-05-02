using System;
using System.Reflection;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces when the player enters the peripheral-swap state (changing a monitor,
    /// keyboard, mouse, headset, or microphone on the desk).
    ///
    /// Fires once on OnStateEnter: reads the slot type and current peripheral name.
    /// "Swapping [slot type]. Current: [peripheral name or empty]."
    /// </summary>
    public static class SwapPeripheralStateHandler
    {
        private static readonly FieldInfo _fSlot =
            typeof(SwapPeripheralState).GetField("m_slot",
                BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(SwapPeripheralState), "OnStateEnter")]
        static class SwapPeripheralState_OnStateEnter_Patch
        {
            static void Postfix(SwapPeripheralState __instance)
            {
                try
                {
                    PeripheralSlot slot = _fSlot?.GetValue(__instance) as PeripheralSlot;
                    if (slot == null) return;

                    string slotType    = GetSlotTypeName(slot.Type);
                    string currentPerif = GetCurrentPerifName(slot);

                    string msg = string.IsNullOrEmpty(currentPerif)
                        ? Loc.Get("swap_perif_empty", slotType)
                        : Loc.Get("swap_perif_current", slotType, currentPerif);

                    ScreenReader.Say(msg);
                    DebugLogger.LogState($"SwapPeripheralStateHandler: slot={slot.Type} current='{currentPerif}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"SwapPeripheralStateHandler.OnStateEnter: {ex.Message}");
                }
            }
        }

        private static string GetSlotTypeName(PartDesc.Type type)
        {
            switch (type)
            {
                case PartDesc.Type.Monitors:   return Loc.Get("perif_monitor");
                case PartDesc.Type.Keyboards:  return Loc.Get("perif_keyboard");
                case PartDesc.Type.Mice:       return Loc.Get("perif_mouse");
                case PartDesc.Type.MousePads:  return Loc.Get("perif_mousepad");
                case PartDesc.Type.Headsets:   return Loc.Get("perif_headset");
                case PartDesc.Type.Microphones:return Loc.Get("perif_microphone");
                default:                       return type.ToString();
            }
        }

        private static string GetCurrentPerifName(PeripheralSlot slot)
        {
            try
            {
                Peripheral perif = slot.GetPeripheral();
                if (perif == null) return string.Empty;
                PartInstance inst = perif.GetComponentInChildren<PartInstance>();
                return inst?.GetPart()?.m_uiName ?? string.Empty;
            }
            catch { return string.Empty; }
        }
    }
}
