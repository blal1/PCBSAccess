using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces walking mode, interaction hints when the crosshair hits interactables,
    /// and provides a Numpad2 hotkey to teleport to the nearest customer PC.
    /// </summary>
    public static class WalkingStateHandler
    {
        private static string _lastInteractionName;
        private static bool _inWalking;

        public static bool IsInWalking => _inWalking;

        public static void Reset()
        {
            _lastInteractionName = null;
            _inWalking = false;
        }

        /// <summary>
        /// Teleports the player to the nearest BenchSlot that has a customer Case,
        /// then directly fires the DEFAULT interaction action (pick up or work on PC).
        /// Called from Main on F5 / Numpad2.
        /// </summary>
        public static void GoToCustomerPC()
        {
            if (!_inWalking)
            {
                DebugLogger.LogState("WalkingStateHandler.GoToCustomerPC: not in walking mode");
                return;
            }
            try
            {
                BenchSlot target = FindCustomerBenchSlot();
                if (target == null)
                {
                    ScreenReader.Say(Loc.Get("walking_no_customer_pc"));
                    DebugLogger.LogState("WalkingStateHandler.GoToCustomerPC: no bench slot found");
                    return;
                }

                DebugLogger.LogState($"WalkingStateHandler.GoToCustomerPC: found bench at {target.name}");

                // Teleport to 0.8m in front of the bench so the game raycast can hit it
                Vector3 benchPos = target.transform.position;
                Vector3 benchFwd = target.transform.forward;
                Vector3 standPos = benchPos - benchFwd * 0.8f;

                var tmp = new GameObject("_AccessMod_TeleportTarget");
                tmp.transform.position = standPos;
                Vector3 lookDir = benchPos - standPos;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.001f)
                    tmp.transform.rotation = Quaternion.LookRotation(lookDir);

                WorkshopController.Get().PlayerController.SetPosition(tmp.transform, _useAdjustedRotation: true);
                UnityEngine.Object.Destroy(tmp);

                // Directly fire the DEFAULT action — no need for the player to press E
                System.Collections.Generic.List<WorldInteraction> interactions = target.GetInteractions();
                DebugLogger.LogState($"WalkingStateHandler.GoToCustomerPC: {interactions?.Count ?? 0} interactions");

                WorldInteraction defaultAction = null;
                if (interactions != null)
                {
                    foreach (WorldInteraction wi in interactions)
                    {
                        if (wi.m_control == WorldInteraction.Control.DEFAULT && wi.m_action != null)
                        {
                            defaultAction = wi;
                            break;
                        }
                    }
                }

                if (defaultAction != null)
                {
                    DebugLogger.LogState($"WalkingStateHandler.GoToCustomerPC: firing '{defaultAction.m_name}'");
                    defaultAction.m_action.Invoke();
                    ScreenReader.Say(Loc.Get("walking_pc_action", defaultAction.m_name));
                }
                else
                {
                    ScreenReader.Say(Loc.Get("walking_teleported_to_pc"));
                    DebugLogger.LogState("WalkingStateHandler.GoToCustomerPC: no DEFAULT action, just teleported");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"WalkingStateHandler.GoToCustomerPC: {ex.Message}");
            }
        }

        private static BenchSlot FindCustomerBenchSlot()
        {
            BenchSlot[] slots = UnityEngine.Object.FindObjectsOfType<BenchSlot>();
            BenchSlot fallback = null;
            foreach (BenchSlot slot in slots)
            {
                if (slot.GetCaseInSlot() != null)
                    return slot;                         // case already placed
                if (slot.IncomingCaseInSlot != null)
                    fallback = slot;                     // case incoming — use if nothing better
            }
            return fallback;
        }

        // ── Patches ──────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(WalkingState), "OnStateEnter")]
        static class WalkingState_OnStateEnter_Patch
        {
            static void Postfix()
            {
                try
                {
                    _lastInteractionName = null;
                    _inWalking = true;

                    BenchSlot slot = FindCustomerBenchSlot();
                    if (slot != null)
                        ScreenReader.Say(Loc.Get("walking_entered_with_pc"));
                    else
                        ScreenReader.Say(Loc.Get("walking_entered"));

                    DebugLogger.LogState("WalkingStateHandler: entered walking");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WalkingState_OnStateEnter_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(WalkingState), "OnStateExit")]
        static class WalkingState_OnStateExit_Patch
        {
            static void Postfix()
            {
                try
                {
                    _lastInteractionName = null;
                    _inWalking = false;
                    ScreenReader.Say(Loc.Get("walking_exited"));
                    DebugLogger.LogState("WalkingStateHandler: exited walking");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WalkingState_OnStateExit_Patch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Fires every time the crosshair lands on (or leaves) an interactable object.
        /// Announces the E-key action name so blind users know what they're facing.
        /// </summary>
        [HarmonyPatch(typeof(CommonUI), "ShowWorldInteractionInfo")]
        static class CommonUI_ShowWorldInteractionInfo_Patch
        {
            static void Postfix(List<WorldInteraction> m_interactions)
            {
                try
                {
                    if (m_interactions == null || m_interactions.Count == 0)
                    {
                        _lastInteractionName = null;
                        return;
                    }

                    string name = null;
                    foreach (WorldInteraction wi in m_interactions)
                    {
                        if (wi.m_control == WorldInteraction.Control.DEFAULT ||
                            wi.m_control == WorldInteraction.Control.HOLD_DEFAULT)
                        {
                            name = wi.m_name;
                            break;
                        }
                    }

                    if (name == null || name == _lastInteractionName) return;
                    _lastInteractionName = name;

                    ScreenReader.Say(Loc.Get("walking_interact_hint", name));
                    DebugLogger.LogState($"WalkingStateHandler: interact='{name}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ShowWorldInteractionInfo_Patch: {ex.Message}");
                }
            }
        }
    }
}
