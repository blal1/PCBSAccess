using HarmonyLib;
using MissionConfig;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for mission panel accessibility.
    /// Captures PanelMission lifecycle events and forwards to MissionHandler.
    /// Fires after AddMission sets up the dialog content.
    /// Passes the fully-populated PanelMission and mission data to MissionHandler.
    /// </summary>
    [HarmonyPatch(typeof(PanelMission), "AddMission")]
    public class PanelMissionAddMissionPatch
    {
        static void Postfix(PanelMission __instance, DirectMission mission)
        {
            DebugLogger.LogState($"PanelMission.AddMission: type={mission.missionType} rep={mission.rep}");
            MissionHandler.OnMissionPanelOpened(__instance, mission);
        }
    }

    /// <summary>
    /// Fires before the accept logic runs, so MissionHandler can cache and announce
    /// while the panel ref is still valid.
    /// </summary>
    [HarmonyPatch(typeof(PanelMission), "OnAceptar")]
    public class PanelMissionOnAceptarPatch
    {
        static void Prefix()
        {
            DebugLogger.LogState("PanelMission.OnAceptar called");
            MissionHandler.OnMissionAccepted();
        }
    }

    /// <summary>
    /// Fires before cancel/close so MissionHandler can announce and clear state.
    /// </summary>
    [HarmonyPatch(typeof(PanelMission), "OnCancelar")]
    public class PanelMissionOnCancelarPatch
    {
        static void Prefix()
        {
            DebugLogger.LogState("PanelMission.OnCancelar called");
            MissionHandler.OnMissionDeclined();
        }
    }
}
