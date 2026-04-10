using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for router help panel. Reads help text content.
    /// </summary>
    public class RouterHelpSubHandler : ISubHandler
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

        public bool HandleInput() { return false; }

        public void AnnounceState()
        {
            ScreenReader.Say(Loc.Get("router_help_panel"));
            var panel = _browser?.GetComponentInChildren<RouterConfigUI>();
            if (panel != null)
            {
                var texts = panel.GetComponentsInChildren<TMP_Text>();
                foreach (var text in texts)
                {
                    if (text != null && !string.IsNullOrWhiteSpace(text.text) && text.text.Length > 20)
                    {
                        ScreenReader.Say(text.text);
                        return;
                    }
                }
            }
        }

        public string GetPanelName() => "Router Help";

        public string GetHelpText() => Loc.Get("router_help_text");
    }
}
