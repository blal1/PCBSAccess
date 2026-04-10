namespace GreyHackAccess
{
    /// <summary>Sub-handler for device manual finder. Full implementation in later task.</summary>
    public class FindDeviceSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Device Manual"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
    }
}
