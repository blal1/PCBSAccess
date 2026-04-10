namespace GreyHackAccess
{
    /// <summary>Sub-handler for bank panels. Full implementation in later task.</summary>
    public class BankSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Bank"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnBankLoggedIn(HtmlBrowser browser) { }
        public void OnBankRegistration(string message, string numCuenta) { }
        public void OnBankAccountCreating() { }
        public void OnBankLoginSubmitted() { }
    }
}
