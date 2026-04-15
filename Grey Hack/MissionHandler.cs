using MissionConfig;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handles PanelMission contract dialog accessibility.
    /// Announces mission details on open and provides keyboard navigation
    /// for Accept and Decline. Alt+M re-reads the last accepted mission.
    /// </summary>
    public class MissionHandler
    {
        #region Fields

        private static MissionHandler _instance;

        private PanelMission _panel;
        private int _buttonIndex; // 0 = Accept, 1 = Decline

        // Panel announcement state (populated in Activate, used for Space repeat)
        private string _panelTitle;
        private string _panelType;
        private string _panelDifficulty;
        private string _panelReward;
        private string _panelDescription;

        // Active mission cache (updated on accept, read by Alt+M)
        private string _cachedTitle;
        private string _cachedType;
        private string _cachedDescription;

        #endregion

        #region Static Entry Points

        /// <summary>Called from MissionPatches when PanelMission.AddMission runs.</summary>
        public static void OnMissionPanelOpened(PanelMission panel, DirectMission mission)
        {
            _instance?.Activate(panel, mission);
        }

        /// <summary>Called from MissionPatches Prefix when PanelMission.OnAceptar runs.</summary>
        public static void OnMissionAccepted()
        {
            _instance?.HandleAccepted();
        }

        /// <summary>Called from MissionPatches Prefix when PanelMission.OnCancelar runs.</summary>
        public static void OnMissionDeclined()
        {
            _instance?.HandleDeclined();
        }

        /// <summary>Registers this instance as the active singleton.</summary>
        public void Register()
        {
            _instance = this;
        }

        #endregion

        #region Properties

        /// <summary>True when a PanelMission dialog is open and being tracked.</summary>
        public bool IsActive => _panel != null && _panel.gameObject.activeInHierarchy;

        #endregion

        #region Public Methods

        /// <summary>Called every frame from Main.UpdateHandlers(). Returns true if input was consumed.</summary>
        public bool Update()
        {
            // Detect panel closed externally (X button / server close) — no OnCancelar fired
            if (_panel != null && !IsActive)
            {
                ScreenReader.Say(Loc.Get("mission_declined"));
                _panel = null;
                return false;
            }

            if (!IsActive)
            {
                _panel = null;
                return false;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _buttonIndex = _buttonIndex == 0 ? 1 : 0;
                AnnounceCurrentButton();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_buttonIndex == 0)
                    _panel.OnAceptar();
                else
                    _panel.OnCancelar();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _panel.OnCancelar();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnouncePanel();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Announces the current panel state (if open) or the last accepted mission.
        /// Bound to Alt+M in Main.cs.
        /// </summary>
        public void AnnounceActiveMission()
        {
            if (IsActive)
            {
                AnnouncePanel();
                return;
            }

            if (!string.IsNullOrEmpty(_cachedTitle))
                ScreenReader.Say(Loc.Get("mission_active", _cachedType, _cachedTitle, _cachedDescription));
            else
                ScreenReader.Say(Loc.Get("mission_none"));
        }

        /// <summary>Returns F1 help text for the mission panel.</summary>
        public string GetHelpText() => Loc.Get("mission_help");

        #endregion

        #region Private Methods

        private void Activate(PanelMission panel, DirectMission mission)
        {
            _panel = panel;
            _buttonIndex = 0;

            _panelTitle = panel.titleLabel != null ? panel.titleLabel.text : string.Empty;
            _panelDescription = panel.contentMission != null ? panel.contentMission.text : string.Empty;
            _panelReward = panel.rewardText != null ? panel.rewardText.text : string.Empty;
            _panelType = GetTypeLabel(mission.missionType);
            _panelDifficulty = GetDifficultyLabel(mission.rep);

            AnnouncePanel();
            DebugLogger.LogState($"MissionHandler: activated '{_panelTitle}' type={mission.missionType} rep={mission.rep}");
        }

        private void HandleAccepted()
        {
            _cachedTitle = _panelTitle;
            _cachedType = _panelType;
            _cachedDescription = _panelDescription;
            ScreenReader.Say(Loc.Get("mission_accepted"));
            _panel = null;
            DebugLogger.LogState($"MissionHandler: accepted '{_cachedTitle}'");
        }

        private void HandleDeclined()
        {
            ScreenReader.Say(Loc.Get("mission_declined"));
            _panel = null;
            DebugLogger.LogState("MissionHandler: declined");
        }

        private void AnnouncePanel()
        {
            if (string.IsNullOrEmpty(_panelDescription))
                ScreenReader.Say(Loc.Get("mission_panel_nodesc", _panelTitle, _panelType, _panelReward, _panelDifficulty));
            else
                ScreenReader.Say(Loc.Get("mission_panel", _panelTitle, _panelType, _panelReward, _panelDifficulty, _panelDescription));
        }

        private void AnnounceCurrentButton()
        {
            string label = _buttonIndex == 0
                ? Loc.Get("mission_nav_accept")
                : Loc.Get("mission_nav_decline");
            ScreenReader.Say(label);
        }

        private static string GetTypeLabel(TypeMissionDirect type)
        {
            switch (type)
            {
                case TypeMissionDirect.Tutorial:        return Loc.Get("mission_type_tutorial");
                case TypeMissionDirect.Credentials:     return Loc.Get("mission_type_credentials");
                case TypeMissionDirect.AcademicRecord:  return Loc.Get("mission_type_academic");
                case TypeMissionDirect.PoliceRecord:    return Loc.Get("mission_type_police");
                case TypeMissionDirect.DestroyComputer: return Loc.Get("mission_type_destroy");
                case TypeMissionDirect.StealFile:       return Loc.Get("mission_type_stealfile");
                case TypeMissionDirect.DeleteFile:      return Loc.Get("mission_type_deletefile");
                case TypeMissionDirect.FindHacker:      return Loc.Get("mission_type_findhacker");
                case TypeMissionDirect.FindEvidence:    return Loc.Get("mission_type_findevidence");
                default:                                return Loc.Get("unknown");
            }
        }

        // rep is generated by DirectMission from preview.minRep/maxRep (typically 0, 1, or 2).
        // Values <=0 = Easy, 1 = Medium, >=2 = Hard.
        private static string GetDifficultyLabel(int rep)
        {
            if (rep <= 0) return Loc.Get("mission_diff_easy");
            if (rep == 1) return Loc.Get("mission_diff_medium");
            return Loc.Get("mission_diff_hard");
        }

        #endregion
    }
}
