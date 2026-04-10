using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for ISP configuration panel.
    /// </summary>
    public class ISPSubHandler : ISubHandler
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
            var ispPanel = _browser?.GetComponentInChildren<ISPPanel>();
            if (ispPanel != null)
            {
                string selected = ispPanel.packSelectedText != null ? ispPanel.packSelectedText.text : "";
                if (!string.IsNullOrEmpty(selected))
                {
                    ScreenReader.Say(Loc.Get("isp_panel") + " " + selected);
                    return;
                }
            }
            ScreenReader.Say(Loc.Get("isp_panel"));
        }

        public string GetPanelName() => "ISP";

        public string GetHelpText() => Loc.Get("isp_help");
    }
}
