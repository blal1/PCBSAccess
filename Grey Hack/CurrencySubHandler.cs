using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for cryptocurrency creation panel.
    /// </summary>
    public class CurrencySubHandler : ISubHandler
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
            ScreenReader.Say(Loc.Get("currency_panel"));
        }

        public string GetPanelName() => "Create Cryptocurrency";

        public string GetHelpText() => Loc.Get("currency_help");
    }
}
