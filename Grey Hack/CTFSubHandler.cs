namespace GreyHackAccess
{
    /// <summary>Sub-handler for CTF events. Full implementation in later task.</summary>
    public class CTFSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "CTF"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
    }
}
