using System;
using System.Collections.Generic;
using System.Reflection;
using MailConfig;
using TMPro;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for email client accessibility.
    /// Auto-activates when a MailWindow is focused.
    /// Up/Down navigates inbox, Enter reads email, R replies,
    /// N composes, Tab switches inbox/outbox, Delete deletes.
    /// </summary>
    public class MailHandler
    {
        #region Fields

        private static MailHandler _instance;
        private MailWindow _activeMail;
        private uDialog _activeDialog;
        private bool _isActive;
        private int _currentIndex;
        private List<Mail> _currentMails;
        private bool _inReadView;

        /// <summary>
        /// Which panel state the handler last detected.
        /// </summary>
        private enum PanelState { None, Login, Inbox, Read, Compose }
        private PanelState _panelState = PanelState.None;

        #endregion

        #region Static Entry

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>Called by patch when inbox is loaded.</summary>
        public static void OnInboxLoaded(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.HandleInboxLoaded(mail);
        }

        /// <summary>Called by patch when an email is selected for reading.</summary>
        public static void OnEmailSelected(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.HandleEmailSelected(mail);
        }

        /// <summary>Called by patch when an email is deleted.</summary>
        public static void OnEmailDeleted(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.HandleEmailDeleted(mail);
        }

        /// <summary>Called by patch when compose panel opens.</summary>
        public static void OnComposeOpened()
        {
            if (_instance == null || !_instance._isActive) return;
            _instance._panelState = PanelState.Compose;
            _instance._inReadView = false;
            ScreenReader.Say(Loc.Get("mail_compose"));
        }

        /// <summary>Called by patch when returning to inbox.</summary>
        public static void OnReturnToInbox(MailWindow mail)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance._inReadView = false;
            _instance._panelState = PanelState.Inbox;
            _instance.RefreshMailList();
            ScreenReader.Say(Loc.Get("mail_back_to_inbox"));
        }

        #endregion

        #region Public Properties

        /// <summary>Whether mail handler is currently active.</summary>
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

            if (_activeMail == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            DetectPanelState();
            return HandleInput();
        }

        /// <summary>Returns help text for F1.</summary>
        public string GetHelpText()
        {
            switch (_panelState)
            {
                case PanelState.Login: return Loc.Get("mail_help_login");
                case PanelState.Compose: return Loc.Get("mail_help_compose");
                case PanelState.Read: return Loc.Get("mail_help_read");
                default: return Loc.Get("mail_help_inbox");
            }
        }

        #endregion

        #region Focus Detection

        private void CheckFocusedWindow()
        {
            var taskbar = UnityEngine.Object.FindObjectOfType<uDialog_TaskBar>();
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

            if (currentTask == _activeDialog && _isActive) return;

            MailWindow mail = currentTask.GetComponentInChildren<MailWindow>();
            if (mail != null)
            {
                if (!_isActive || mail != _activeMail)
                {
                    Activate(mail, currentTask);
                }
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(MailWindow mail, uDialog dialog)
        {
            _activeMail = mail;
            _activeDialog = dialog;
            _isActive = true;
            _currentIndex = 0;
            _inReadView = false;
            _currentMails = null;

            string title = dialog.TitleText ?? "Mail";
            ScreenReader.Say(Loc.Get("mail_focused", title));
            DetectPanelState();

            DebugLogger.LogState($"MailHandler: activated, title='{title}'");
        }

        private void Deactivate()
        {
            _isActive = false;
            _activeMail = null;
            _activeDialog = null;
            _currentMails = null;
            _panelState = PanelState.None;
        }

        #endregion

        #region Panel Detection

        private void DetectPanelState()
        {
            if (_activeMail == null) return;

            List<GameObject> panels = _activeMail.panelsMail;
            if (panels == null || panels.Count < 4) return;

            if (panels[3].activeInHierarchy)
            {
                if (_panelState != PanelState.Login)
                {
                    _panelState = PanelState.Login;
                }
            }
            else if (panels[1].activeInHierarchy)
            {
                if (_panelState != PanelState.Compose)
                {
                    _panelState = PanelState.Compose;
                }
            }
            else if (_inReadView)
            {
                _panelState = PanelState.Read;
            }
            else if (panels[0].activeInHierarchy)
            {
                _panelState = PanelState.Inbox;
            }
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            switch (_panelState)
            {
                case PanelState.Login:
                    return false;

                case PanelState.Inbox:
                    return HandleInboxInput();

                case PanelState.Read:
                    return HandleReadInput();

                case PanelState.Compose:
                    return HandleComposeInput();

                default:
                    return false;
            }
        }

        private bool HandleInboxInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateMail(1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateMail(-1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenSelectedMail();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                _activeMail.OnShowPanelRedactar();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInboxOutbox();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelectedMail();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceCurrentMail();
                return true;
            }

            return false;
        }

        private bool HandleReadInput()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R))
            {
                ReadEmailBody();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _activeMail.OnShowReplyField();
                ScreenReader.Say(Loc.Get("mail_reply"));
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                _activeMail.ShowPanelPrincipal();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                _activeMail.OnDeleteMail();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceReadView();
                return true;
            }

            return false;
        }

        private bool HandleComposeInput()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                _activeMail.OnSendNewMessage();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _activeMail.ShowPanelPrincipal();
                return true;
            }

            return false;
        }

        #endregion

        #region Mail Navigation

        private void RefreshMailList()
        {
            if (_activeMail == null) return;

            _currentMails = ReflectionHelper.GetPrivateField<List<Mail>>(_activeMail, "allMails");
            if (_currentMails == null)
            {
                _currentMails = new List<Mail>();
            }
        }

        private void NavigateMail(int direction)
        {
            RefreshMailList();

            if (_currentMails.Count == 0)
            {
                ScreenReader.Say(Loc.Get("mail_no_emails"));
                return;
            }

            int newIndex = _currentIndex + direction;
            if (newIndex < 0 || newIndex >= _currentMails.Count)
            {
                return;
            }

            _currentIndex = newIndex;
            AnnounceCurrentMail();
        }

        private void AnnounceCurrentMail()
        {
            RefreshMailList();

            if (_currentMails == null || _currentMails.Count == 0 || _currentIndex >= _currentMails.Count)
            {
                ScreenReader.Say(Loc.Get("mail_no_emails"));
                return;
            }

            Mail mail = _currentMails[_currentIndex];
            string address = mail.otherMail ?? "unknown";
            string subject = (mail.messages != null && mail.messages.Count > 0)
                ? mail.messages[0].titulo : "";
            string readStatus = mail.isUnread
                ? Loc.Get("mail_item_unread")
                : Loc.Get("mail_item_read");

            ScreenReader.Say(Loc.Get("mail_item",
                _currentIndex + 1, _currentMails.Count, address, subject, readStatus));
        }

        private void OpenSelectedMail()
        {
            RefreshMailList();

            if (_currentMails == null || _currentMails.Count == 0 || _currentIndex >= _currentMails.Count)
            {
                return;
            }

            try
            {
                var method = _activeMail.GetType().GetMethod(
                    "OnClickMail",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(int), typeof(bool) },
                    null);
                method?.Invoke(_activeMail, new object[] { _currentIndex, true });
            }
            catch (Exception ex)
            {
                DebugLogger.LogState($"MailHandler.OpenSelectedMail: reflection error: {ex.Message}");
            }

            _inReadView = true;
            _panelState = PanelState.Read;
        }

        private void DeleteSelectedMail()
        {
            if (_currentMails == null || _currentMails.Count == 0 || _currentIndex >= _currentMails.Count)
            {
                return;
            }

            OpenSelectedMail();
            _activeMail.OnDeleteMail();
        }

        private void ToggleInboxOutbox()
        {
            if (_activeMail == null) return;

            TMP_Text titlePanel = _activeMail.titleLeftPanel;
            if (titlePanel != null && titlePanel.text == "Inbox")
            {
                _activeMail.OnShowOutbox();
                ScreenReader.Say(Loc.Get("mail_switched_outbox"));
            }
            else
            {
                _activeMail.OnShowInbox();
                ScreenReader.Say(Loc.Get("mail_switched_inbox"));
            }

            _currentIndex = 0;
            RefreshMailList();
        }

        #endregion

        #region Read View

        private void HandleEmailSelected(MailWindow mail)
        {
            _inReadView = true;
            _panelState = PanelState.Read;
            AnnounceReadView();
        }

        private void AnnounceReadView()
        {
            if (_activeMail == null) return;

            string address = _activeMail.readAddress?.text ?? "unknown";
            string subject = _activeMail.readSubject?.text ?? "";

            ScreenReader.Say(Loc.Get("mail_reading", address, subject));
        }

        private void ReadEmailBody()
        {
            if (_activeMail == null) return;

            Mail selected = ReflectionHelper.GetPrivateField<Mail>(_activeMail, "selectedMail");
            if (selected == null || selected.messages == null || selected.messages.Count == 0)
            {
                ScreenReader.Say(Loc.Get("mail_empty_body"));
                return;
            }

            string body = selected.messages[selected.messages.Count - 1].mensaje;
            if (string.IsNullOrEmpty(body?.Trim()))
            {
                ScreenReader.Say(Loc.Get("mail_empty_body"));
                return;
            }

            string clean = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", "");
            string trimmed = clean.Length > 1000 ? clean.Substring(0, 1000) + "..." : clean;
            ScreenReader.Say(Loc.Get("mail_read_body", trimmed));
        }

        #endregion

        #region Inbox Loaded / Deleted

        private void HandleInboxLoaded(MailWindow mail)
        {
            _panelState = PanelState.Inbox;
            _inReadView = false;
            RefreshMailList();
            _currentIndex = 0;

            int count = _currentMails?.Count ?? 0;
            if (count > 0)
            {
                ScreenReader.Say(Loc.Get("mail_inbox", count));
            }
            else
            {
                ScreenReader.Say(Loc.Get("mail_no_emails"));
            }
        }

        private void HandleEmailDeleted(MailWindow mail)
        {
            _inReadView = false;
            _panelState = PanelState.Inbox;
            RefreshMailList();

            int count = _currentMails?.Count ?? 0;
            if (_currentIndex >= count && count > 0)
            {
                _currentIndex = count - 1;
            }

            ScreenReader.Say(Loc.Get("mail_deleted", count));
        }

        #endregion
    }
}
