namespace PCBSAccess
{
    /// <summary>
    /// Optional interface for IInputContext implementations that support F1 context help.
    /// Handlers that want F1 to announce navigation hints implement this alongside IInputContext.
    /// </summary>
    public interface IHelpContext
    {
        /// <summary>Announces navigation help for this context to the screen reader.</summary>
        void AnnounceHelp();
    }
}
