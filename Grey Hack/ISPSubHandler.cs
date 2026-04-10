namespace GreyHackAccess
{
    /// <summary>Sub-handler for ISP configuration. Full implementation in later task.</summary>
    public class ISPSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "ISP"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
    }
}
