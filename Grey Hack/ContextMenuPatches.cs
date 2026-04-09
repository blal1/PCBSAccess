using System.Collections.Generic;
using HarmonyLib;
using TerminalPoolSystem;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for context menu accessibility.
    /// Captures menu open/close events and forwards to ContextMenuHandler.
    /// </summary>

    /// <summary>
    /// Patches ContextualMenu.OpenMenu to detect file explorer context menus opening.
    /// </summary>
    [HarmonyPatch(typeof(ContextualMenu), "OpenMenu",
        new[] { typeof(List<OpcionContextual.Opciones>), typeof(Vector3), typeof(Vector2) })]
    public class ContextualMenuOpenPatch
    {
        static void Postfix(ContextualMenu __instance)
        {
            DebugLogger.LogState("ContextualMenu.OpenMenu detected");
            ContextMenuHandler.OnContextMenuOpened(__instance);
        }
    }

    /// <summary>
    /// Patches ContextualMenuTerminal.OpenMenu to detect terminal context menus opening.
    /// </summary>
    [HarmonyPatch(typeof(ContextualMenuTerminal), "OpenMenu",
        new[] { typeof(List<OpcionContextual.Opciones>), typeof(TerminalListAdapter), typeof(SelectableTerminal) })]
    public class ContextualMenuTerminalOpenPatch
    {
        static void Postfix(ContextualMenuTerminal __instance)
        {
            DebugLogger.LogState("ContextualMenuTerminal.OpenMenu detected");
            ContextMenuHandler.OnContextMenuOpened(__instance);
        }
    }

    /// <summary>
    /// Patches ControlContextualMenus.ClearMenus to detect all context menus closing.
    /// </summary>
    [HarmonyPatch(typeof(ControlContextualMenus), "ClearMenus")]
    public class ControlContextualMenusClearPatch
    {
        static void Postfix()
        {
            DebugLogger.LogState("ControlContextualMenus.ClearMenus detected");
            ContextMenuHandler.OnContextMenusCleared();
        }
    }
}
