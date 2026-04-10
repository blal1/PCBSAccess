namespace GreyHackAccess
{
    /// <summary>Sub-handler for HTML web pages. Full implementation in later task.</summary>
    public class WebPageSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Web"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnPageLoaded(HtmlBrowser browser) { }
        public void OnConnectionError(string msg) { }
    }
}
