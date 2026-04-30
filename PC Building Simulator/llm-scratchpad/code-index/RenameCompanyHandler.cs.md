# Code Index for RenameCompanyHandler.cs

- Line 6: /// <summary>
- Line 7: /// Announces the Rename Company dialog: current name on open, result on apply or cancel.
- Line 8: ///
- Line 9: /// Patches RenameCompany.OnEnable (open), OnApply (Prefix: read before navigate away), OnBack (cancel).
- Line 10: /// </summary>
- Line 11: public static class RenameCompanyHandler
- Line 14: static class RenameCompany_OnEnable_Patch
- Line 16: static void Postfix(RenameCompany __instance)
- Line 32: static class RenameCompany_OnApply_Patch
- Line 34: static void Prefix(RenameCompany __instance)
- Line 53: static class RenameCompany_OnBack_Patch
- Line 55: static void Postfix()
