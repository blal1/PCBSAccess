namespace GreyHackAccess
{
    /// <summary>Sub-handler for router help. Full implementation in later task.</summary>
    public class RouterHelpSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Router Help"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
    }
}
