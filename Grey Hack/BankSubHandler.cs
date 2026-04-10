using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for bank login, registration, and account panels.
    /// </summary>
    public class BankSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private BrowserPanel _bankPanel;
        private int _transactionIndex;
        private int _transactionCount;

        private static FieldInfo _bankListAdapterField;
        private static FieldInfo _currentPanelField;

        static BankSubHandler()
        {
            _bankListAdapterField = typeof(HtmlBrowser).GetField("bankListAdapter", BindingFlags.Public | BindingFlags.Instance);
            _currentPanelField = typeof(HtmlBrowser).GetField("currentPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _transactionIndex = 0;
            _transactionCount = 0;

            if (_currentPanelField != null)
            {
                try { _bankPanel = (BrowserPanel)(int)_currentPanelField.GetValue(browser); }
                catch { _bankPanel = BrowserPanel.BankLogin; }
            }

            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (_bankPanel == BrowserPanel.BankProgram)
                return HandleAccountInput();

            return false;
        }

        public void AnnounceState()
        {
            switch (_bankPanel)
            {
                case BrowserPanel.BankLogin:
                    ScreenReader.Say(Loc.Get("bank_login"));
                    break;
                case BrowserPanel.BankRegister:
                    ScreenReader.Say(Loc.Get("bank_register"));
                    break;
                case BrowserPanel.BankProgram:
                    AnnounceAccount();
                    break;
            }
        }

        public string GetPanelName()
        {
            switch (_bankPanel)
            {
                case BrowserPanel.BankLogin: return "Bank Login";
                case BrowserPanel.BankRegister: return "Bank Registration";
                case BrowserPanel.BankProgram: return "Bank Account";
                default: return "Bank";
            }
        }

        public string GetHelpText()
        {
            switch (_bankPanel)
            {
                case BrowserPanel.BankLogin: return Loc.Get("bank_help_login");
                case BrowserPanel.BankRegister: return Loc.Get("bank_help_register");
                case BrowserPanel.BankProgram: return Loc.Get("bank_help_account");
                default: return Loc.Get("bank_help_login");
            }
        }

        #region Patch Callbacks

        /// <summary>Called when bank login succeeds.</summary>
        public void OnBankLoggedIn(HtmlBrowser browser)
        {
            _browser = browser;
            _bankPanel = BrowserPanel.BankProgram;
            AnnounceAccount();
        }

        /// <summary>Called when bank registration completes.</summary>
        public void OnBankRegistration(string message, string numCuenta)
        {
            if (!string.IsNullOrEmpty(numCuenta))
                ScreenReader.Say(Loc.Get("bank_registered", numCuenta, message));
            else
                ScreenReader.Say(message);
        }

        /// <summary>Called when account creation is submitted.</summary>
        public void OnBankAccountCreating()
        {
            ScreenReader.Say(Loc.Get("bank_creating"));
        }

        /// <summary>Called when login is submitted.</summary>
        public void OnBankLoginSubmitted()
        {
            ScreenReader.Say(Loc.Get("bank_logging_in"));
        }

        #endregion

        #region Account Panel

        private void AnnounceAccount()
        {
            string balance = _browser.balanceBank != null ? _browser.balanceBank.text : "unknown";
            _transactionCount = GetTransactionCount();
            ScreenReader.Say(Loc.Get("bank_account", balance, _transactionCount));
        }

        private bool HandleAccountInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _transactionCount = GetTransactionCount();
                if (_transactionCount == 0)
                {
                    ScreenReader.Say(Loc.Get("bank_no_transactions"));
                    return true;
                }
                _transactionIndex = Mathf.Min(_transactionIndex + 1, _transactionCount - 1);
                AnnounceTransaction();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _transactionCount = GetTransactionCount();
                if (_transactionCount == 0)
                {
                    ScreenReader.Say(Loc.Get("bank_no_transactions"));
                    return true;
                }
                _transactionIndex = Mathf.Max(_transactionIndex - 1, 0);
                AnnounceTransaction();
                return true;
            }

            return false;
        }

        private void AnnounceTransaction()
        {
            string text = GetTransactionText(_transactionIndex);
            ScreenReader.Say(Loc.Get("bank_transaction", _transactionIndex + 1, _transactionCount, text));
        }

        private int GetTransactionCount()
        {
            try
            {
                var adapter = _bankListAdapterField?.GetValue(_browser);
                if (adapter == null) return 0;

                var dataProp = adapter.GetType().GetProperty("Data");
                var data = dataProp?.GetValue(adapter);
                if (data == null) return 0;

                var countProp = data.GetType().GetProperty("Count");
                return (int)(countProp?.GetValue(data) ?? 0);
            }
            catch { return 0; }
        }

        private string GetTransactionText(int index)
        {
            try
            {
                var adapter = _bankListAdapterField?.GetValue(_browser);
                if (adapter == null) return "";

                var dataProp = adapter.GetType().GetProperty("Data");
                var data = dataProp?.GetValue(adapter);
                if (data == null) return "";

                var indexer = data.GetType().GetProperty("Item");
                var item = indexer?.GetValue(data, new object[] { index });
                if (item == null) return "";

                var transProp = item.GetType().GetField("transaction");
                return transProp?.GetValue(item) as string ?? "";
            }
            catch { return ""; }
        }

        #endregion
    }
}
