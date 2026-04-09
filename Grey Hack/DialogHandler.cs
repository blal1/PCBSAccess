using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for error dialogs, question dialogs, kernel panic, and game over screens.
    /// Announces dialog message text and provides keyboard navigation of buttons.
    /// </summary>
    public class DialogHandler
    {
        #region Types

        private struct DialogButton
        {
            public Button Button;
            public string Label;
        }

        #endregion

        #region Fields

        private static DialogHandler _instance;

        private bool _active;
        private Component _trackedDialog;
        private List<DialogButton> _buttons = new();
        private int _currentIndex;
        private string _messageText;

        /// <summary>
        /// Tracks the last announced error message and time to prevent spam
        /// when the game calls SetMessage repeatedly for the same error.
        /// </summary>
        private string _lastErrorMessage;
        private float _lastErrorTime;

        /// <summary>
        /// Game over detail view state.
        /// </summary>
        private GameOverWindow _gameOverWindow;
        private bool _isDetailView;
        private int _traceIndex;
        private List<PassiveTrace.NodeTrace> _traceList;

        #endregion

        #region Static Entry

        /// <summary>
        /// Called when an ErrorWindow is configured with a simple message (OK only).
        /// </summary>
        public static void OnErrorShown(ErrorWindow window, string msg)
        {
            if (_instance == null) return;
            _instance.ActivateErrorWindow(window, msg);
        }

        /// <summary>
        /// Called when an ErrorWindow is configured as a Yes/No question.
        /// </summary>
        public static void OnQuestionShown(ErrorWindow window, string msg)
        {
            if (_instance == null) return;
            _instance.ActivateErrorWindow(window, msg);
        }

        /// <summary>
        /// Called when a QuestionWindow (tutorial) is configured.
        /// </summary>
        public static void OnTutorialQuestionShown(QuestionWindow window, string msg)
        {
            if (_instance == null) return;
            _instance.ActivateQuestionWindow(window, msg);
        }

        /// <summary>
        /// Called when KernelPanicWindow appears.
        /// </summary>
        public static void OnKernelPanic(KernelPanicWindow window)
        {
            if (_instance == null) return;
            _instance.ActivateKernelPanic(window);
        }

        /// <summary>
        /// Called when GameOverWindow is configured.
        /// </summary>
        public static void OnGameOver(GameOverWindow window)
        {
            if (_instance == null) return;
            _instance.ActivateGameOver(window);
        }

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether a dialog is currently active and consuming input.
        /// </summary>
        public bool IsActive => _active;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update(). Returns true if input was consumed.
        /// </summary>
        public bool Update()
        {
            if (!_active) return false;

            // Check if dialog was destroyed or closed
            if (_trackedDialog == null || !_trackedDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            if (_buttons.Count == 0) return false;

            // Game over detail view: Up/Down navigates traces
            if (_isDetailView && _traceList != null && _traceList.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    _traceIndex--;
                    if (_traceIndex < 0) _traceIndex = _traceList.Count - 1;
                    AnnounceCurrentTrace();
                    return true;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _traceIndex++;
                    if (_traceIndex >= _traceList.Count) _traceIndex = 0;
                    AnnounceCurrentTrace();
                    return true;
                }
            }

            // Up or Left = previous button
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _currentIndex--;
                if (_currentIndex < 0)
                    _currentIndex = _buttons.Count - 1;
                AnnounceCurrentButton();
                return true;
            }

            // Down or Right = next button
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _currentIndex++;
                if (_currentIndex >= _buttons.Count)
                    _currentIndex = 0;
                AnnounceCurrentButton();
                return true;
            }

            // Tab = next button (no wrap announcement)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _currentIndex++;
                if (_currentIndex >= _buttons.Count)
                    _currentIndex = 0;
                AnnounceCurrentButton();
                return true;
            }

            // Enter = click current button (with game over detail handling)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_gameOverWindow != null)
                {
                    HandleGameOverEnter();
                }
                else
                {
                    ClickCurrentButton();
                }
                return true;
            }

            // Escape = click Cancel/No/OK (dismiss the dialog)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DismissDialog();
                return true;
            }

            // Space = repeat current button and message
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceCurrentButton();
                return true;
            }

            // F1 handled by Main.cs help system, but consume other keys
            // to prevent them from passing through to the game
            return false;
        }

        /// <summary>
        /// Returns help text for dialog navigation.
        /// </summary>
        public string GetHelpText()
        {
            if (_gameOverWindow != null)
                return Loc.Get("dialog_gameover_help");
            return Loc.Get("dialog_help");
        }

        #endregion

        #region Activation

        private void ActivateErrorWindow(ErrorWindow window, string msg)
        {
            string cleanMsg = StripRichText(msg);

            // Deduplicate: skip if same error message within 1 second
            if (_active && cleanMsg == _lastErrorMessage && Time.time - _lastErrorTime < 1.0f)
            {
                return;
            }

            _trackedDialog = window;
            _messageText = cleanMsg;
            _lastErrorMessage = cleanMsg;
            _lastErrorTime = Time.time;
            _buttons.Clear();
            _currentIndex = 0;

            // Collect active buttons from the ErrorWindow's named fields
            AddButtonIfActive(window.okButton, Loc.Get("dialog_btn_ok"));
            AddButtonIfActive(window.yesButton, null);
            AddButtonIfActive(window.noButton, null);
            AddButtonIfActive(window.cancelButton, null);

            Activate();
        }

        private void ActivateQuestionWindow(QuestionWindow window, string msg)
        {
            _trackedDialog = window;
            _messageText = StripRichText(msg);
            _buttons.Clear();
            _currentIndex = 0;

            // QuestionWindow has Accept and Cancel buttons
            // Find them by looking for Button components
            var allButtons = window.GetComponentsInChildren<Button>(false);
            foreach (var btn in allButtons)
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable)
                    continue;
                string label = GetButtonLabel(btn);
                if (!string.IsNullOrEmpty(label))
                    _buttons.Add(new DialogButton { Button = btn, Label = label });
            }

            Activate();
        }

        private void ActivateKernelPanic(KernelPanicWindow window)
        {
            _trackedDialog = window;
            _messageText = Loc.Get("dialog_kernel_panic");
            _buttons.Clear();
            _currentIndex = 0;

            // Find the restart button
            var allButtons = window.GetComponentsInChildren<Button>(false);
            foreach (var btn in allButtons)
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable)
                    continue;
                string label = GetButtonLabel(btn);
                if (string.IsNullOrEmpty(label))
                    label = Loc.Get("dialog_btn_restart");
                _buttons.Add(new DialogButton { Button = btn, Label = label });
            }

            Activate();
        }

        private void ActivateGameOver(GameOverWindow window)
        {
            _trackedDialog = window;
            _gameOverWindow = window;
            _isDetailView = false;
            _traceIndex = 0;

            // Read the title and content
            string title = StripRichText(window.title?.text);
            string content = StripRichText(window.content?.text);
            _messageText = string.IsNullOrEmpty(content) ? title : $"{title}. {content}";

            // Get trace list via reflection (private field)
            _traceList = ReflectionHelper.GetPrivateField<List<PassiveTrace.NodeTrace>>(window, "traceList");

            _buttons.Clear();
            _currentIndex = 0;

            // Find all active buttons
            var allButtons = window.GetComponentsInChildren<Button>(false);
            foreach (var btn in allButtons)
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable)
                    continue;
                string label = GetButtonLabel(btn);
                if (!string.IsNullOrEmpty(label))
                    _buttons.Add(new DialogButton { Button = btn, Label = label });
            }

            Activate();
        }

        private void Activate()
        {
            _active = true;

            string announcement;
            if (_buttons.Count > 0)
            {
                string firstButton = _buttons[0].Label;
                announcement = Loc.Get("dialog_announced", _messageText, _buttons.Count, firstButton);
            }
            else
            {
                announcement = Loc.Get("dialog_announced_no_buttons", _messageText);
            }

            ScreenReader.Say(announcement);
            DebugLogger.LogState($"DialogHandler: activated with {_buttons.Count} buttons, msg='{_messageText}'");
        }

        private void Deactivate()
        {
            _active = false;
            _trackedDialog = null;
            _buttons.Clear();
            _messageText = null;
            _gameOverWindow = null;
            _isDetailView = false;
            _traceList = null;
            _traceIndex = 0;
            DebugLogger.LogState("DialogHandler: deactivated");
        }

        #endregion

        #region Button Helpers

        private void AddButtonIfActive(Button btn, string fallbackLabel)
        {
            if (btn == null || !btn.gameObject.activeInHierarchy)
                return;

            // Read the button's text, or use fallback
            string label = fallbackLabel;
            if (label == null)
                label = GetButtonLabel(btn);
            if (string.IsNullOrEmpty(label))
                label = btn.gameObject.name;

            _buttons.Add(new DialogButton { Button = btn, Label = label });
        }

        private static string GetButtonLabel(Button btn)
        {
            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null && !string.IsNullOrEmpty(tmp.text))
            {
                string text = System.Text.RegularExpressions.Regex.Replace(tmp.text, "<[^>]*>", "").Trim();
                if (!string.IsNullOrEmpty(text)) return text;
            }

            var legacyText = btn.GetComponentInChildren<Text>();
            if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
                return legacyText.text.Trim();

            return null;
        }

        private void ClickCurrentButton()
        {
            if (_currentIndex < 0 || _currentIndex >= _buttons.Count) return;

            var btn = _buttons[_currentIndex];
            ScreenReader.Say(Loc.Get("dialog_selecting", btn.Label));
            DebugLogger.LogState($"DialogHandler: clicking '{btn.Label}'");
            btn.Button.onClick.Invoke();
        }

        private void DismissDialog()
        {
            // Try to find the best dismiss button: Cancel > No > OK
            for (int i = _buttons.Count - 1; i >= 0; i--)
            {
                var btn = _buttons[i];
                string lower = btn.Label.ToLowerInvariant();
                if (lower.Contains("cancel") || lower.Contains("cancelar"))
                {
                    _currentIndex = i;
                    ClickCurrentButton();
                    return;
                }
            }

            for (int i = _buttons.Count - 1; i >= 0; i--)
            {
                var btn = _buttons[i];
                string lower = btn.Label.ToLowerInvariant();
                if (lower.Contains("no"))
                {
                    _currentIndex = i;
                    ClickCurrentButton();
                    return;
                }
            }

            // Fallback: click whatever button is there (OK, Close, etc.)
            if (_buttons.Count > 0)
            {
                _currentIndex = _buttons.Count - 1;
                ClickCurrentButton();
            }
        }

        private void AnnounceCurrentButton()
        {
            if (_currentIndex < 0 || _currentIndex >= _buttons.Count) return;

            var btn = _buttons[_currentIndex];
            ScreenReader.Say(Loc.Get("dialog_button", btn.Label, _currentIndex + 1, _buttons.Count));
        }

        private void HandleGameOverEnter()
        {
            if (_currentIndex < 0 || _currentIndex >= _buttons.Count) return;

            var btn = _buttons[_currentIndex];
            string lower = btn.Label.ToLowerInvariant();

            if (lower.Contains("show details") || lower.Contains("hide details"))
            {
                // Toggle detail view
                btn.Button.onClick.Invoke();

                _isDetailView = !_isDetailView;
                if (_isDetailView && _traceList != null && _traceList.Count > 0)
                {
                    _traceIndex = 0;
                    ScreenReader.Say(Loc.Get("dialog_gameover_details_shown", _traceList.Count));
                    AnnounceCurrentTrace();
                }
                else
                {
                    ScreenReader.Say(Loc.Get("dialog_gameover_details_hidden"));
                }

                // Update button label (it changes between Show/Hide)
                string newLabel = GetButtonLabel(btn.Button);
                if (!string.IsNullOrEmpty(newLabel))
                {
                    _buttons[_currentIndex] = new DialogButton { Button = btn.Button, Label = newLabel };
                }
            }
            else if (lower.Contains("copy"))
            {
                btn.Button.onClick.Invoke();
                ScreenReader.Say(Loc.Get("dialog_gameover_copied"));
            }
            else
            {
                // Close or other button
                ScreenReader.Say(Loc.Get("dialog_selecting", btn.Label));
                DebugLogger.LogState($"DialogHandler: clicking '{btn.Label}'");
                btn.Button.onClick.Invoke();
            }
        }

        private void AnnounceCurrentTrace()
        {
            if (_traceList == null || _traceIndex < 0 || _traceIndex >= _traceList.Count) return;

            var trace = _traceList[_traceIndex];
            string deviceType = trace.isRouter ? "Router" : "Computer";
            string action = trace.logLine.action.ToString();
            string fromIP = trace.logLine.ip;

            ScreenReader.Say(Loc.Get("dialog_gameover_trace",
                _traceIndex + 1, _traceList.Count, deviceType, trace.publicIP, action, fromIP));
        }

        #endregion

        #region Helpers

        private static string StripRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", "").Trim();
        }

        #endregion
    }
}
