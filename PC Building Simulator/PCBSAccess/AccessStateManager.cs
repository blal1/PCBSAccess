using System;

namespace PCBSAccess
{
    /// <summary>
    /// Tracks which accessibility handler has exclusive input focus.
    /// Prevents two handlers competing for arrow keys and Enter.
    /// Global hotkeys (F1, F12) bypass this — they always work.
    /// </summary>
    public static class AccessStateManager
    {
        /// <summary>One value per handler that needs exclusive arrow/Enter input.</summary>
        public enum State
        {
            None,           // No handler active
            MainMenu,       // Main menu navigation
            HowToBuildAPC,  // Guided PC build tutorial
            Inventory,      // Inventory panel
            Shop,           // Parts shop
            BuildMode,      // PC assembly workbench
        }

        /// <summary>Currently active state.</summary>
        public static State Current { get; private set; } = State.None;

        /// <summary>Fired when state changes. Parameters: (oldState, newState).</summary>
        public static event Action<State, State> OnStateChanged;

        /// <summary>
        /// Enter a new state. Automatically exits the previous state if one is active.
        /// Returns true if the state was entered.
        /// </summary>
        public static bool TryEnter(State state)
        {
            if (state == State.None) return false;
            if (Current == state) return true;

            var old = Current;
            Current = state;
            DebugLogger.LogState($"AccessState: {old} -> {state}");
            OnStateChanged?.Invoke(old, state);
            return true;
        }

        /// <summary>Exit a state. Only exits if this state is currently active.</summary>
        public static void Exit(State state)
        {
            if (Current != state) return;

            var old = Current;
            Current = State.None;
            DebugLogger.LogState($"AccessState: {old} -> None");
            OnStateChanged?.Invoke(old, State.None);
        }

        /// <summary>Force-exit any active state. Use on scene changes.</summary>
        public static void ForceReset()
        {
            if (Current == State.None) return;
            var old = Current;
            Current = State.None;
            DebugLogger.LogState($"AccessState: forced reset from {old}");
            OnStateChanged?.Invoke(old, State.None);
        }

        /// <summary>True if currently in the given state.</summary>
        public static bool IsIn(State state) => Current == state;

        /// <summary>True if no handler is active.</summary>
        public static bool IsIdle => Current == State.None;
    }
}
