namespace GreyHackAccess
{
    /// <summary>Sub-handler for CCTV camera. Full implementation in later task.</summary>
    public class CCTVSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "CCTV"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
    }
}
