namespace GreyHackAccess
{
    /// <summary>Sub-handler for search and no-network panels. Full implementation in later task.</summary>
    public class SearchSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Search"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnSearchStarted() { }
    }
}
