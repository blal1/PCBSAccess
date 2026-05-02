namespace PCBSAccess
{
    /// <summary>
    /// Implemented by any handler that wants exclusive navigation key input
    /// while it is the topmost active context on the InputRouter stack.
    ///
    /// Usage:
    ///   1. Create a private nested class Context : IInputContext inside your handler.
    ///   2. Call InputRouter.Push(_ctx) when your handler opens.
    ///   3. Call InputRouter.Pop(_ctx) when it closes (Reset, patch, or polling close).
    ///   4. InputRouter auto-pops if IsActive returns false.
    /// </summary>
    public interface IInputContext
    {
        /// <summary>Human-readable name used in debug logs.</summary>
        string ContextName { get; }

        /// <summary>
        /// Called each frame by InputRouter when this context is on top.
        /// Return true if any key was consumed (stops further processing for this frame).
        /// </summary>
        bool HandleInput();

        /// <summary>
        /// Returns false when the context should auto-pop itself (e.g. menu closed externally).
        /// InputRouter checks this each frame before routing input.
        /// </summary>
        bool IsActive { get; }
    }
}
