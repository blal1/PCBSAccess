using System.Collections.Generic;

namespace PCBSAccess
{
    /// <summary>
    /// Manages a LIFO stack of IInputContext objects.
    ///
    /// Only the topmost live context receives navigation key events each frame.
    /// Global hotkeys (F-keys, Numpad) are handled before this in Main.ProcessHotkeys()
    /// and are never blocked by the router.
    ///
    /// Thread safety: all calls happen on the Unity main thread — no locking needed.
    /// </summary>
    public static class InputRouter
    {
        private static readonly List<IInputContext> _stack = new List<IInputContext>();

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Pushes a context onto the top of the stack.
        /// Idempotent: if the context is already on the stack it is not added again.
        /// </summary>
        public static void Push(IInputContext ctx)
        {
            if (ctx == null) return;
            if (_stack.Contains(ctx)) return;
            _stack.Add(ctx);
            DebugLogger.LogState($"InputRouter: push '{ctx.ContextName}'  (depth={_stack.Count})");
        }

        /// <summary>
        /// Removes a specific context from anywhere in the stack.
        /// Safe to call even when the context is not currently on the stack.
        /// </summary>
        public static void Pop(IInputContext ctx)
        {
            if (ctx == null) return;
            bool removed = _stack.Remove(ctx);
            if (removed)
                DebugLogger.LogState($"InputRouter: pop '{ctx.ContextName}'  (depth={_stack.Count})");
        }

        /// <summary>
        /// Clears all contexts.  Must be called from Main.OnSceneLoaded on scene change
        /// so stale contexts from the previous scene do not survive.
        /// </summary>
        public static void Reset()
        {
            if (_stack.Count > 0)
                DebugLogger.LogState($"InputRouter: reset  (cleared {_stack.Count} contexts)");
            _stack.Clear();
        }

        /// <summary>
        /// Called every frame from Main.Update() after ProcessHotkeys() returns false.
        ///
        /// 1. Auto-pops any context whose IsActive has become false (e.g. menu closed
        ///    externally by a patch or polling logic before we got here).
        /// 2. Routes input to the topmost live context.
        /// </summary>
        public static void Update()
        {
            // Remove stale contexts (sweep top-down to keep index stable)
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (!_stack[i].IsActive)
                {
                    DebugLogger.LogState($"InputRouter: auto-pop stale '{_stack[i].ContextName}'");
                    _stack.RemoveAt(i);
                }
            }

            if (_stack.Count == 0) return;

            IInputContext top = _stack[_stack.Count - 1];
            top.HandleInput();
        }

        // ── Diagnostics ───────────────────────────────────────────────────────────

        /// <summary>
        /// Calls AnnounceHelp() on the topmost context that implements IHelpContext.
        /// Returns true if help was announced, false if no context supports it.
        /// </summary>
        public static bool AnnounceTopContextHelp()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i] is IHelpContext h)
                {
                    h.AnnounceHelp();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Name of the currently top context, or "none".</summary>
        public static string CurrentContextName =>
            _stack.Count > 0 ? _stack[_stack.Count - 1].ContextName : "none";

        /// <summary>Number of contexts currently on the stack (for debugging).</summary>
        public static int Depth => _stack.Count;
    }
}
