using System;
using System.Reflection;
using FuturLab;
using HarmonyLib;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the destination floor when the player presses a lift button
    /// in the IT Support DLC building.
    ///
    /// Patches ITSupportLiftController.MoveToNewFloor(string buttonName) Prefix.
    /// Reads the floor description from the DLC floor registry and announces
    /// the localized floor name before the lift starts moving.
    /// </summary>
    public static class ITSupportLiftHandler
    {
        private static readonly FieldInfo _fFloorOfficeLookup =
            typeof(ITSupportLiftController).GetField("m_floorOfficeLookup",
                BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPatch(typeof(ITSupportLiftController), "MoveToNewFloor")]
        static class MoveToNewFloor_Patch
        {
            static void Prefix(ITSupportLiftController __instance, string buttonName)
            {
                try
                {
                    OfficeFloor[] lookup = _fFloorOfficeLookup?.GetValue(__instance) as OfficeFloor[];
                    if (lookup == null) return;

                    string floorName = null;
                    foreach (OfficeFloor floor in lookup)
                    {
                        if (floor.buttonName == buttonName)
                        {
                            string floorId = floor.associatedFloor.ToString();
                            try
                            {
                                // Try to get the localised floor name via ITSupportFloors.
                                ITSupportFloors floors = UnityEngine.Object.FindObjectOfType<ITSupportFloors>();
                                if (floors != null)
                                {
                                    ITSupportFloorDesc desc = floors.GetFloor(floorId, showError: false);
                                    if (desc != null)
                                        floorName = desc.GetNameLocalised();
                                }
                            }
                            catch { /* floor registry may not be initialised — fall through */ }

                            if (string.IsNullOrEmpty(floorName))
                            {
                                // Fall back: format the enum name (e.g. FLOOR_2 → "Floor 2").
                                floorName = floorId.Replace('_', ' ').ToLower();
                                if (floorName.Length > 0)
                                    floorName = char.ToUpper(floorName[0]) + floorName.Substring(1);
                            }
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(floorName))
                    {
                        ScreenReader.Say(Loc.Get("lift_going_to", floorName));
                        DebugLogger.LogState($"ITSupportLiftHandler: going to '{floorName}'");
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ITSupportLiftHandler.MoveToNewFloor: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(ITSupportLiftController), "ExitLiftControllerState")]
        static class ExitLiftControllerState_Patch
        {
            static void Prefix()
            {
                try
                {
                    ScreenReader.Say(Loc.Get("lift_arrived"));
                    DebugLogger.LogState("ITSupportLiftHandler: arrived");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ITSupportLiftHandler.ExitLiftControllerState: {ex.Message}");
                }
            }
        }
    }
}
