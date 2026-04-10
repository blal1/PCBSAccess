using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for device manual finder panel.
    /// </summary>
    public class FindDeviceSubHandler : ISubHandler
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
            var deviceUI = _browser?.GetComponentInChildren<DeviceManualUI>();
            if (deviceUI != null && deviceUI.titleManual != null && !string.IsNullOrEmpty(deviceUI.titleManual.text))
            {
                string title = deviceUI.titleManual.text;
                string model = deviceUI.titleModel != null ? deviceUI.titleModel.text : "";
                ScreenReader.Say(Loc.Get("finddevice_result", title, model));

                if (deviceUI.manualText != null && !string.IsNullOrEmpty(deviceUI.manualText.text))
                    ScreenReader.Say(deviceUI.manualText.text);
            }
            else
            {
                ScreenReader.Say(Loc.Get("finddevice_panel"));
            }
        }

        public string GetPanelName() => "Device Manual";

        public string GetHelpText() => Loc.Get("finddevice_help");
    }
}
