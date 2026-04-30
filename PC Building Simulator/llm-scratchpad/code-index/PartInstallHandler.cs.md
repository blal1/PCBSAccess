# Code Index for PartInstallHandler.cs

- Line 7: /// <summary>
- Line 8: /// Announces part installation and removal completion.
- Line 9: ///
- Line 10: /// Fires on InstallingPartState.OnStateDestroy (Prefix so we read the part
- Line 11: /// name before the component GameObject is destroyed).
- Line 12: /// m_dir  > 0 → part installed into slot.
- Line 13: /// m_dir  &lt; 0 → part removed from slot (intentional or cancelled install).
- Line 14: /// </summary>
- Line 15: public static class PartInstallHandler
- Line 31: /// <summary>
- Line 32: /// Prefix: fires before OnStateDestroy so we can read the component
- Line 33: /// name before it may be destroyed.
- Line 34: /// </summary>
- Line 36: static class InstallingPartState_OnStateDestroy_Patch
- Line 38: static void Prefix(InstallingPartState __instance)
