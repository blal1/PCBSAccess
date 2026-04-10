namespace GreyHackAccess
{
    /// <summary>Sub-handler for regular shop. Full implementation in later task.</summary>
    public class ShopSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Shop"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnShopLoaded(HtmlBrowser browser) { }
    }
}
