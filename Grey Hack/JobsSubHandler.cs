namespace GreyHackAccess
{
    /// <summary>Sub-handler for jobs panels. Full implementation in later task.</summary>
    public class JobsSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Jobs"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnMissionSelected(ItemJobs itemJob) { }
    }
}
