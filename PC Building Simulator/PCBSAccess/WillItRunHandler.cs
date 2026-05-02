using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the "Will It Run" tablet app.
    ///
    /// Program list page (ShowPrograms):
    ///   Announces "N programs." then allows Up/Down/Home/End navigation.
    ///   Enter: view compatibility details for focused program.
    ///
    /// Result detail page (ShowIndividualResult):
    ///   Announces game name + overall pass/fail, then all 5 categories.
    /// </summary>
    public static class WillItRunHandler
    {
        #region State

        private static WillItRunApp      _app;
        private static List<ProgramRow>  _programs = new List<ProgramRow>();
        private static int               _progIndex = -1;
        private static bool              _onProgramPage;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "WillItRun";
            public bool IsActive => _onProgramPage && _app != null;

            public bool HandleInput()
            {
                if (!_onProgramPage || _programs.Count == 0) return false;

                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);                      return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);                     return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);                    return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_programs.Count - 1);  return true; }
                if (Input.GetKeyDown(KeyCode.Return))     { ActivateFocused();                return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("wir_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        public static void Reset()
        {
            _app           = null;
            _programs.Clear();
            _progIndex     = -1;
            _onProgramPage = false;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Patches

        // Fires when program list is shown (app opens or Back pressed from details).
        [HarmonyPatch(typeof(WillItRunApp), "ShowPrograms")]
        static class WillItRunApp_ShowPrograms_Patch
        {
            static void Postfix(WillItRunApp __instance)
            {
                try
                {
                    _app           = __instance;
                    _onProgramPage = true;
                    _progIndex     = -1;
                    _programs.Clear();

                    ProgramRow[] rows = __instance.m_programs.content
                        .GetComponentsInChildren<ProgramRow>(includeInactive: false);
                    foreach (ProgramRow r in rows)
                        _programs.Add(r);

                    InputRouter.Push(_ctx);
                    ScreenReader.Say(Loc.Get("wir_programs", _programs.Count));
                    if (_programs.Count > 0)
                    {
                        _progIndex = 0;
                        AnnounceProgram(0);
                        ScreenReader.Say(Loc.Get("wir_programs_hint"), interrupt: false);
                    }
                    DebugLogger.LogState($"WillItRunHandler: {_programs.Count} programs");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WillItRunApp_ShowPrograms_Patch: {ex.Message}");
                }
            }
        }

        // Fires when a specific program's compatibility is shown.
        [HarmonyPatch(typeof(WillItRunApp), "ShowIndividualResult")]
        static class WillItRunApp_ShowIndividualResult_Patch
        {
            static void Postfix(WillItRunApp __instance, ProgramRequirementsDesc desc)
            {
                try
                {
                    _onProgramPage = false;
                    InputRouter.Pop(_ctx);

                    if (desc == null) return;
                    VirtualComputer vc = __instance.GetComponentInParent<VirtualComputer>();
                    if (vc == null) return;
                    ComputerSave comp = vc.GetComputer();
                    if (comp == null) return;

                    bool   overallPass = desc.Assess(comp);
                    string result      = overallPass ? Loc.Get("wir_pass") : Loc.Get("wir_fail");

                    ScreenReader.Say($"{desc.m_uiName}. {result}.");

                    for (int i = 0; i < 5; i++)
                    {
                        var    cat     = (ProgramRequirementsDesc.Category)i;
                        bool   catPass = desc.Assess(comp, cat);
                        string catName = Loc.Get($"wir_cat_{cat.ToString().ToLower()}");
                        string needs   = desc.GetNeedsText(cat);
                        string got     = desc.GetGotText(comp, cat);
                        string tick    = catPass ? Loc.Get("wir_ok") : Loc.Get("wir_no");

                        ScreenReader.Say(
                            $"{catName}: {Loc.Get("wir_needs")} {needs}, {Loc.Get("wir_has")} {got}. {tick}.",
                            interrupt: false);
                    }
                    DebugLogger.LogState($"WillItRunHandler: '{desc.m_uiName}' pass={overallPass}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WillItRunApp_ShowIndividualResult_Patch: {ex.Message}");
                }
            }
        }

        #endregion

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_programs.Count == 0) return;
            int next = Mathf.Clamp(_progIndex + dir, 0, _programs.Count - 1);
            if (next == _progIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _progIndex = next;
            AnnounceProgram(_progIndex);
        }

        private static void NavigateTo(int index)
        {
            if (_programs.Count == 0) return;
            _progIndex = Mathf.Clamp(index, 0, _programs.Count - 1);
            AnnounceProgram(_progIndex);
        }

        private static void ActivateFocused()
        {
            if (_progIndex < 0 || _progIndex >= _programs.Count) return;
            _programs[_progIndex].OnClick();
        }

        private static void AnnounceProgram(int index)
        {
            if (index < 0 || index >= _programs.Count) return;
            string name = _programs[index].m_name?.text ?? string.Empty;
            ScreenReader.Say($"{index + 1} {Loc.Get("nav_of")} {_programs.Count}: {name}.");
        }

        #endregion
    }
}
