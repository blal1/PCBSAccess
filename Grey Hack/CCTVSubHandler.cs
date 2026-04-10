using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for CCTV camera panel.
    /// Announces camera metadata. Visual feed cannot be described.
    /// </summary>
    public class CCTVSubHandler : ISubHandler
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
            string titleInfo = "";
            if (_browser.browserCam != null && _browser.browserCam.title != null)
            {
                string camTitle = _browser.browserCam.title.text;
                if (!string.IsNullOrEmpty(camTitle))
                    titleInfo = Loc.Get("cctv_title", camTitle);
            }

            if (_browser.browserCam != null && _browser.browserCam.panelPassObj != null
                && _browser.browserCam.panelPassObj.activeInHierarchy)
            {
                ScreenReader.Say(Loc.Get("cctv_password"));
                return;
            }

            ScreenReader.Say(Loc.Get("cctv_active", titleInfo));
        }

        public string GetPanelName() => "CCTV";

        public string GetHelpText() => Loc.Get("cctv_help");
    }
}
