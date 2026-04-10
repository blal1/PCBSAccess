namespace GreyHackAccess
{
    /// <summary>Sub-handler for firewall rules. Full implementation in later task.</summary>
    public class RouterFirewallSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Firewall"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnRouterConfigLoaded(HtmlBrowser browser) { }
    }
}
