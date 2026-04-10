namespace GreyHackAccess
{
    /// <summary>Sub-handler for port forwarding. Full implementation in later task.</summary>
    public class RouterPortSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Port Forwarding"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnRouterConfigLoaded(HtmlBrowser browser) { }
    }
}
