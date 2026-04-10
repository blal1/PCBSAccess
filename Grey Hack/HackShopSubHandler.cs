namespace GreyHackAccess
{
    /// <summary>Sub-handler for hack shop. Full implementation in later task.</summary>
    public class HackShopSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Hack Shop"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnShopLoaded(HtmlBrowser browser) { }
    }
}
