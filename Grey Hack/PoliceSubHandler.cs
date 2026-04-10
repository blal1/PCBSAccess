using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for police crime report filing.
    /// </summary>
    public class PoliceSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput()
        {
            return false;
        }

        public void AnnounceState()
        {
            ScreenReader.Say(Loc.Get("police_report"));
        }

        public string GetPanelName() => "Police Report";

        public string GetHelpText() => Loc.Get("police_help");

        /// <summary>Called when report is submitted.</summary>
        public void OnReportSubmitted()
        {
            ScreenReader.Say(Loc.Get("police_submitted"));
        }
    }
}
