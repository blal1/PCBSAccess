using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for tutorial window accessibility.
    /// Auto-activates when an AdvancedTutorial window is focused.
    /// Enter to advance, Escape to skip, Space to re-read, F1 for help.
    /// </summary>
    public class TutorialHandler
    {
        #region Fields

        private static TutorialHandler _instance;
        private AdvancedTutorial _activeTutorial;
        private uDialog _activeDialog;
        private bool _isActive;
        private string _lastPageText = "";

        /// <summary>Map of PendingAction enum values to localization keys.</summary>
        private static readonly Dictionary<AdvancedTutorial.PendingAction, string> PendingActionKeys = new()
        {
            { AdvancedTutorial.PendingAction.OPEN_TERMINAL, "tutorial_pending_open_terminal" },
            { AdvancedTutorial.PendingAction.LAUNCH_PWD, "tutorial_pending_launch_pwd" },
            { AdvancedTutorial.PendingAction.LAUNCH_LS, "tutorial_pending_launch_ls" },
            { AdvancedTutorial.PendingAction.LAUNCH_CD, "tutorial_pending_launch_cd" },
            { AdvancedTutorial.PendingAction.LAUNCH_MKDIR, "tutorial_pending_launch_mkdir" },
            { AdvancedTutorial.PendingAction.EXPLORER_ROOT, "tutorial_pending_explorer_root" },
            { AdvancedTutorial.PendingAction.EXPLORER_BIN, "tutorial_pending_explorer_bin" },
            { AdvancedTutorial.PendingAction.EXPLORER_USRBIN, "tutorial_pending_explorer_usrbin" },
            { AdvancedTutorial.PendingAction.MAIL, "tutorial_pending_mail" },
            { AdvancedTutorial.PendingAction.REMOTE_CONN, "tutorial_pending_remote_conn" },
            { AdvancedTutorial.PendingAction.TRACE_SYSTEM, "tutorial_pending_trace_system" },
            { AdvancedTutorial.PendingAction.REMOTE_EXPLORER, "tutorial_pending_remote_explorer" },
            { AdvancedTutorial.PendingAction.CREATE_MAIL_BUTTON, "tutorial_pending_create_mail_button" },
            { AdvancedTutorial.PendingAction.SELECT_LOGIN_ISSUES, "tutorial_pending_select_login_issues" },
        };

        #endregion

        #region Static Entry

        /// <summary>Registers this handler instance.</summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>Called by Harmony patch when a tutorial page is built.</summary>
        public static void OnPageBuilt(AdvancedTutorial tutorial)
        {
            if (_instance == null) return;
            _instance.HandlePageBuilt(tutorial);
        }

        /// <summary>Called by Harmony patch when a wrong action is shown.</summary>
        public static void OnWrongAction(string text)
        {
            if (_instance == null) return;
            string clean = StripRichText(text);
            ScreenReader.Say(Loc.Get("tutorial_wrong_action", clean));
        }

        #endregion

        #region Public Properties

        /// <summary>Whether tutorial handler is currently active.</summary>
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

            if (_activeTutorial == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            return HandleInput();
        }

        /// <summary>Returns help text for F1 during tutorial focus.</summary>
        public string GetHelpText()
        {
            return Loc.Get("tutorial_help");
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

            AdvancedTutorial tutorial = currentTask.GetComponentInChildren<AdvancedTutorial>();
            if (tutorial != null)
            {
                if (!_isActive || tutorial != _activeTutorial)
                {
                    Activate(tutorial, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(AdvancedTutorial tutorial, uDialog dialog)
        {
            _activeTutorial = tutorial;
            _activeDialog = dialog;
            _isActive = true;

            // Read page content on focus
            AnnouncePage(true);
            DebugLogger.LogState($"TutorialHandler: activated");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeTutorial = null;
            _activeDialog = null;
            _lastPageText = "";
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            // Enter = Next page
            if (Input.GetKeyDown(KeyCode.Return))
            {
                HandleNext();
                return true;
            }

            // Escape = Skip tutorial
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleSkip();
                return true;
            }

            // Space = Re-read page
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnouncePage(false);
                return true;
            }

            return false;
        }

        #endregion

        #region Page Reading

        private void HandlePageBuilt(AdvancedTutorial tutorial)
        {
            // Update reference in case it changed
            _activeTutorial = tutorial;

            // If we're active and focused on this tutorial, announce the new page
            if (_isActive)
            {
                AnnouncePage(true);
            }
        }

        private void AnnouncePage(bool includeHeader)
        {
            if (_activeTutorial == null) return;

            int currentPage = ReflectionHelper.GetPrivateFieldValue<int>(_activeTutorial, "pagina", 0);
            var pages = _activeTutorial.pages;
            int totalPages = pages != null ? pages.Count : 0;

            // Build announcement
            string announcement = "";

            if (includeHeader && totalPages > 0)
            {
                announcement = Loc.Get("tutorial_focused", currentPage + 1, totalPages);
            }

            // Read text content from parentPages transform
            string pageText = ReadPageText();
            if (!string.IsNullOrEmpty(pageText))
            {
                announcement += " " + pageText;
            }

            // Pending action
            var pendingAction = ReflectionHelper.GetPrivateFieldValue<AdvancedTutorial.PendingAction>(
                _activeTutorial, "pendingAction", AdvancedTutorial.PendingAction.NONE);

            if (pendingAction != AdvancedTutorial.PendingAction.NONE)
            {
                string actionDesc = GetPendingActionDescription(pendingAction);
                announcement += " " + Loc.Get("tutorial_action_required", actionDesc);
            }

            // Check if Next button exists (showNextButton on current page)
            if (totalPages > 0 && currentPage < totalPages)
            {
                var page = pages[currentPage];
                if (page.showNextButton)
                {
                    announcement += " " + Loc.Get("tutorial_next_available");
                }
            }

            _lastPageText = announcement.Trim();
            ScreenReader.Say(_lastPageText);
        }

        private string ReadPageText()
        {
            if (_activeTutorial == null) return "";

            Transform parentPages = _activeTutorial.parentPages;
            if (parentPages == null) return "";

            var texts = new List<string>();
            var tmpTexts = parentPages.GetComponentsInChildren<TMP_Text>();

            foreach (var tmp in tmpTexts)
            {
                if (tmp == null || !tmp.gameObject.activeInHierarchy) continue;

                // Skip the ButtonTutoNext text (it just says "Next" or similar)
                if (tmp.GetComponentInParent<ButtonTutoNext>() != null) continue;

                string text = StripRichText(tmp.text);
                text = StripActionTags(text);

                if (!string.IsNullOrEmpty(text.Trim()))
                {
                    texts.Add(text.Trim());
                }
            }

            return string.Join(" ", texts);
        }

        #endregion

        #region Navigation

        private void HandleNext()
        {
            if (_activeTutorial == null) return;

            // Check if there's a pending action blocking progress
            var pendingAction = ReflectionHelper.GetPrivateFieldValue<AdvancedTutorial.PendingAction>(
                _activeTutorial, "pendingAction", AdvancedTutorial.PendingAction.NONE);

            if (pendingAction != AdvancedTutorial.PendingAction.NONE)
            {
                // Check if a Next button exists on this page
                int currentPage = ReflectionHelper.GetPrivateFieldValue<int>(_activeTutorial, "pagina", 0);
                var pages = _activeTutorial.pages;
                if (pages != null && currentPage < pages.Count && !pages[currentPage].showNextButton)
                {
                    // No Next button, pending action required
                    string actionDesc = GetPendingActionDescription(pendingAction);
                    ScreenReader.Say(Loc.Get("tutorial_action_required", actionDesc));
                    return;
                }
            }

            // Try to find and click the Next button
            ButtonTutoNext nextButton = _activeTutorial.parentPages?.GetComponentInChildren<ButtonTutoNext>();
            if (nextButton != null)
            {
                nextButton.OnNext();
            }
            else
            {
                // Last page or no button - check if this is the last page
                int currentPage = ReflectionHelper.GetPrivateFieldValue<int>(_activeTutorial, "pagina", 0);
                var pages = _activeTutorial.pages;
                if (pages != null && currentPage >= pages.Count - 1)
                {
                    ScreenReader.Say(Loc.Get("tutorial_complete"));
                }
            }
        }

        private void HandleSkip()
        {
            if (_activeTutorial == null) return;

            // Call CloseTaskBar which triggers the skip confirmation dialog
            _activeTutorial.CloseTaskBar();
        }

        #endregion

        #region Helpers

        private static string GetPendingActionDescription(AdvancedTutorial.PendingAction action)
        {
            if (PendingActionKeys.TryGetValue(action, out string key))
            {
                return Loc.Get(key);
            }
            return action.ToString();
        }

        /// <summary>Strips HTML/rich text tags from a string.</summary>
        private static string StripRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return Regex.Replace(text, "<.*?>", "");
        }

        /// <summary>Strips [ACTION]...[/ACTION] markers, keeping the inner text.</summary>
        private static string StripActionTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("[ACTION]", "").Replace("[/ACTION]", "");
        }

        #endregion
    }
}
