using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for search home page and no-network panel.
    /// </summary>
    public class SearchSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private bool _isNoNet;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _isNoNet = browser.inputSearch == null || !browser.inputSearch.gameObject.activeInHierarchy;
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public bool HandleInput()
        {
            return false;
        }

        public void AnnounceState()
        {
            if (_isNoNet)
                ScreenReader.Say(Loc.Get("search_no_net"));
            else
                ScreenReader.Say(Loc.Get("search_home"));
        }

        public string GetPanelName() => _isNoNet ? "No Network" : "Search";

        public string GetHelpText() => Loc.Get("search_help");

        /// <summary>Called when search is initiated.</summary>
        public void OnSearchStarted()
        {
            ScreenReader.Say(Loc.Get("search_searching"));
        }
    }
}
