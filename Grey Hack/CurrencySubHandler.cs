namespace GreyHackAccess
{
    /// <summary>Sub-handler for cryptocurrency creation. Full implementation in later task.</summary>
    public class CurrencySubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Create Cryptocurrency"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
    }
}
