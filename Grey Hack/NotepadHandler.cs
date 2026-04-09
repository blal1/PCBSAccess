using NotepadPoolSystem;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for notepad window accessibility.
    /// Auto-activates when a notepad window is focused.
    /// Alt+R reads file content, F1 for help.
    /// </summary>
    public class NotepadHandler
    {
        #region Fields

        private static NotepadHandler _instance;
        private Notepad _activeNotepad;
        private uDialog _activeDialog;
        private bool _isActive;

        #endregion

        #region Static Entry

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>
        /// Called when a notepad file is loaded via Harmony patch.
        /// </summary>
        public static void OnFileLoaded(Notepad notepad)
        {
            if (_instance == null) return;
            _instance.HandleFileLoaded(notepad);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether notepad handler is currently active.
        /// </summary>
        public bool IsActive => _isActive;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update().
        /// Returns true if input was consumed.
        /// </summary>
        public bool Update()
        {
            CheckFocusedWindow();

            if (!_isActive) return false;

            if (_activeNotepad == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            return HandleInput();
        }

        /// <summary>
        /// Returns help text for F1 during notepad focus.
        /// </summary>
        public string GetHelpText()
        {
            return Loc.Get("notepad_help");
        }

        #endregion

        #region Focus Detection

        private void CheckFocusedWindow()
        {
            var taskbar = Object.FindObjectOfType<uDialog_TaskBar>();
            if (taskbar == null)
            {
                if (_isActive) Deactivate();
                return;
            }

            uDialog currentTask = taskbar.CurrentTask;
            if (currentTask == null)
            {
                if (_isActive) Deactivate();
                return;
            }

            if (currentTask == _activeDialog && _isActive)
            {
                return;
            }

            Notepad notepad = currentTask.GetComponentInChildren<Notepad>();
            if (notepad != null)
            {
                if (!_isActive || notepad != _activeNotepad)
                {
                    Activate(notepad, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(Notepad notepad, uDialog dialog)
        {
            _activeNotepad = notepad;
            _activeDialog = dialog;
            _isActive = true;

            string title = dialog.TitleText ?? "";
            string fileName = title.Contains(" - ") ? title.Substring(title.IndexOf(" - ") + 3) : "";

            if (string.IsNullOrEmpty(fileName))
            {
                ScreenReader.Say(Loc.Get("notepad_focused_new"));
            }
            else
            {
                ScreenReader.Say(Loc.Get("notepad_focused", fileName));
            }

            DebugLogger.LogState($"NotepadHandler: activated, title='{title}'");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeNotepad = null;
            _activeDialog = null;
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            // Alt+R = Read content
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R))
            {
                ReadContent();
                return true;
            }

            return false;
        }

        #endregion

        #region Content Reading

        private void ReadContent()
        {
            if (_activeNotepad == null) return;

            NotepadListAdapter adapter = ReflectionHelper.GetPrivateField<NotepadListAdapter>(_activeNotepad, "listAdapter");
            if (adapter == null)
            {
                DebugLogger.LogState("NotepadHandler: could not access listAdapter");
                return;
            }

            string content = adapter.GetText();
            if (string.IsNullOrEmpty(content?.Trim()))
            {
                ScreenReader.Say(Loc.Get("notepad_empty"));
                return;
            }

            // Limit to first 500 chars to avoid overwhelming the screen reader
            string trimmed = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
            ScreenReader.Say(Loc.Get("notepad_read_content", trimmed));
        }

        private void HandleFileLoaded(Notepad notepad)
        {
            if (notepad == null) return;

            uDialog dialog = notepad.GetComponent<uDialog>();
            if (dialog == null) return;

            string title = dialog.TitleText ?? "";
            string fileName = title.Contains(" - ") ? title.Substring(title.IndexOf(" - ") + 3) : "";

            if (!string.IsNullOrEmpty(fileName))
            {
                ScreenReader.Say(Loc.Get("notepad_file_loaded", fileName));
            }
        }

        #endregion
    }
}
