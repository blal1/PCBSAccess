namespace GreyHackAccess
{
    /// <summary>
    /// Interface for browser panel sub-handlers.
    /// Each sub-handler manages accessibility for one or more HtmlBrowser panel types.
    /// </summary>
    public interface ISubHandler
    {
        /// <summary>Called when this panel becomes active.</summary>
        void Activate(HtmlBrowser browser);

        /// <summary>Called when panel switches away from this handler.</summary>
        void Deactivate();

        /// <summary>Called each frame when this handler is active. Returns true if input consumed.</summary>
        bool HandleInput();

        /// <summary>Announces current state (for re-focus/Space repeat).</summary>
        void AnnounceState();

        /// <summary>Returns display name for this panel.</summary>
        string GetPanelName();

        /// <summary>Returns help text for F1.</summary>
        string GetHelpText();
    }
}
