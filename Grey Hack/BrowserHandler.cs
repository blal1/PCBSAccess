using System.Collections.Generic;
using System.Reflection;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Coordinator for HtmlBrowser accessibility.
    /// Auto-activates when browser window is focused. Delegates input to active sub-handler.
    /// </summary>
    public class BrowserHandler
    {
        #region Fields

        private static BrowserHandler _instance;
        private HtmlBrowser _activeBrowser;
        private uDialog _activeDialog;
        private bool _isActive;
        private BrowserPanel _currentPanel;
        private ISubHandler _currentSubHandler;

        private readonly Dictionary<BrowserPanel, ISubHandler> _subHandlers = new();

        // PreBuy state
        private PreBuy _activePreBuy;
        private PreBuyHardware _activePreBuyHw;
        private bool _preBuyActive;
        private int _preBuyVersionIndex;

        // Reflection cache for private fields
        private static FieldInfo _currentPanelField;
        private static FieldInfo _websHistoryField;
        private static FieldInfo _indexWebHistoryField;

        #endregion

        #region Static Entry

        /// <summary>Registers this handler instance.</summary>
        public void Register()
        {
            _instance = this;
            CacheReflection();
            RegisterSubHandlers();
        }

        private static void CacheReflection()
        {
            var browserType = typeof(HtmlBrowser);
            _currentPanelField = browserType.GetField("currentPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            _websHistoryField = browserType.GetField("websHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            _indexWebHistoryField = browserType.GetField("indexWebHistory", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void RegisterSubHandlers()
        {
            var web = new WebPageSubHandler();
            _subHandlers[BrowserPanel.Web] = web;

            var search = new SearchSubHandler();
            _subHandlers[BrowserPanel.PanelSearch] = search;
            _subHandlers[BrowserPanel.PanelNoNet] = search;

            var bank = new BankSubHandler();
            _subHandlers[BrowserPanel.BankLogin] = bank;
            _subHandlers[BrowserPanel.BankRegister] = bank;
            _subHandlers[BrowserPanel.BankProgram] = bank;

            var shop = new ShopSubHandler();
            _subHandlers[BrowserPanel.Shop] = shop;

            var hackShop = new HackShopSubHandler();
            _subHandlers[BrowserPanel.HackShopTools] = hackShop;
            _subHandlers[BrowserPanel.HackShopExploits] = hackShop;

            var routerPort = new RouterPortSubHandler();
            _subHandlers[BrowserPanel.PanelPorts] = routerPort;

            var routerFw = new RouterFirewallSubHandler();
            _subHandlers[BrowserPanel.PanelFirewall] = routerFw;

            var routerHelp = new RouterHelpSubHandler();
            _subHandlers[BrowserPanel.PanelHelp] = routerHelp;

            var jobs = new JobsSubHandler();
            _subHandlers[BrowserPanel.Jobs] = jobs;
            _subHandlers[BrowserPanel.PoliceJobs] = jobs;

            var police = new PoliceSubHandler();
            _subHandlers[BrowserPanel.PoliceReport] = police;

            var cctv = new CCTVSubHandler();
            _subHandlers[BrowserPanel.Cctv] = cctv;

            var isp = new ISPSubHandler();
            _subHandlers[BrowserPanel.ISPConfig] = isp;

            var currency = new CurrencySubHandler();
            _subHandlers[BrowserPanel.CreateCurrency] = currency;

            var ctf = new CTFSubHandler();
            _subHandlers[BrowserPanel.PanelCTF] = ctf;

            var findDevice = new FindDeviceSubHandler();
            _subHandlers[BrowserPanel.PanelFindDeviceManual] = findDevice;
        }

        #endregion

        #region Patch Callbacks

        /// <summary>Called by patch when HtmlBrowser.Start fires.</summary>
        public static void OnBrowserCreated(HtmlBrowser browser)
        {
            if (_instance == null) return;
            DebugLogger.LogState("BrowserHandler: browser instance captured");
        }

        /// <summary>Called by patch when ShowPanel fires.</summary>
        public static void OnPanelChanged(HtmlBrowser browser, BrowserPanel panel)
        {
            if (_instance == null) return;
            if (_instance._activeBrowser != browser) return;
            _instance.SwitchPanel(panel);
        }

        /// <summary>Called by patch when ResumeConnectionWeb fires.</summary>
        public static void OnWebPageLoaded(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is WebPageSubHandler web)
                web.OnPageLoaded(browser);
        }

        /// <summary>Called by patch when CloseConnection fires.</summary>
        public static void OnConnectionError(HtmlBrowser browser, string msg)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is WebPageSubHandler web)
                web.OnConnectionError(msg);
        }

        /// <summary>Called by patch when EnterWeb fires.</summary>
        public static void OnNavigationStarted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            string address = browser.barAddress != null ? browser.barAddress.text : "";
            ScreenReader.Say(Loc.Get("browser_loading", address));
        }

        /// <summary>Called by patch when EnterSearch fires.</summary>
        public static void OnSearchStarted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is SearchSubHandler search)
                search.OnSearchStarted();
        }

        /// <summary>Called by patch when PlayerLoginBank fires.</summary>
        public static void OnBankLoggedIn(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankLoggedIn(browser);
        }

        /// <summary>Called by patch when OnBankRegistration fires.</summary>
        public static void OnBankRegistration(HtmlBrowser browser, string message, string numCuenta)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankRegistration(message, numCuenta);
        }

        /// <summary>Called by patch when CreateBankAccount fires.</summary>
        public static void OnBankAccountCreating(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankAccountCreating();
        }

        /// <summary>Called by patch when BankLogin fires.</summary>
        public static void OnBankLoginSubmitted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankLoginSubmitted();
        }

        /// <summary>Called by patch when ResumeConnectionWindowFilesShop fires.</summary>
        public static void OnShopLoaded(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is ShopSubHandler shop)
                shop.OnShopLoaded(browser);
            else if (_instance._currentSubHandler is HackShopSubHandler hackShop)
                hackShop.OnShopLoaded(browser);
        }

        /// <summary>Called by patch when ResumeRouterConfig fires.</summary>
        public static void OnRouterConfigLoaded(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is RouterPortSubHandler port)
                port.OnRouterConfigLoaded(browser);
            else if (_instance._currentSubHandler is RouterFirewallSubHandler fw)
                fw.OnRouterConfigLoaded(browser);
        }

        /// <summary>Called by patch when OnMissionClick fires.</summary>
        public static void OnMissionSelected(HtmlBrowser browser, ItemJobs itemJob)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is JobsSubHandler jobs)
                jobs.OnMissionSelected(itemJob);
        }

        /// <summary>Called by patch when OnSendAttach fires.</summary>
        public static void OnPoliceReportSubmitted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is PoliceSubHandler police)
                police.OnReportSubmitted();
        }

        /// <summary>Called by patch when PreBuy.Configure fires.</summary>
        public static void OnPreBuyOpened(PreBuy preBuy)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.ActivatePreBuy(preBuy);
        }

        /// <summary>Called by patch when PreBuyHardware.Configure fires.</summary>
        public static void OnPreBuyHardwareOpened(PreBuyHardware preBuy)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.ActivatePreBuyHardware(preBuy);
        }

        #endregion

        #region Public Properties

        /// <summary>Whether browser handler is currently active.</summary>
        public bool IsActive => _isActive;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update(). Returns true if input consumed.
        /// </summary>
        public bool Update()
        {
            CheckFocusedWindow();

            if (!_isActive) return false;

            if (_activeBrowser == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            // PreBuy dialog takes priority
            if (_preBuyActive)
            {
                if (!IsPreBuyStillOpen())
                {
                    _preBuyActive = false;
                    _activePreBuy = null;
                    _activePreBuyHw = null;
                    _currentSubHandler?.AnnounceState();
                    return false;
                }
                return HandlePreBuyInput();
            }

            // Browser-level keys
            if (HandleBrowserKeys()) return true;

            // Delegate to active sub-handler
            if (_currentSubHandler != null)
                return _currentSubHandler.HandleInput();

            return false;
        }

        /// <summary>Returns help text for F1.</summary>
        public string GetHelpText()
        {
            if (_preBuyActive)
                return Loc.Get("browser_prebuy_help");

            if (_currentSubHandler != null)
                return _currentSubHandler.GetHelpText();

            return Loc.Get("browser_help");
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

            if (currentTask == _activeDialog && _isActive) return;

            HtmlBrowser browser = currentTask.GetComponentInChildren<HtmlBrowser>();
            if (browser != null)
            {
                if (!_isActive || browser != _activeBrowser)
                    Activate(browser, currentTask);
            }
            else if (_isActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(HtmlBrowser browser, uDialog dialog)
        {
            _activeBrowser = browser;
            _activeDialog = dialog;
            _isActive = true;
            _preBuyActive = false;

            // Read current panel from the browser
            var panel = GetCurrentPanel(browser);
            SwitchPanel(panel);

            DebugLogger.LogState($"BrowserHandler: activated, panel={panel}");
        }

        private void Deactivate()
        {
            _currentSubHandler?.Deactivate();
            _currentSubHandler = null;
            _isActive = false;
            _activeBrowser = null;
            _activeDialog = null;
            _preBuyActive = false;
        }

        private void SwitchPanel(BrowserPanel panel)
        {
            if (_currentSubHandler != null && panel == _currentPanel)
                return;

            _currentSubHandler?.Deactivate();
            _currentPanel = panel;

            if (_subHandlers.TryGetValue(panel, out var handler))
            {
                _currentSubHandler = handler;
                _currentSubHandler.Activate(_activeBrowser);
            }
            else
            {
                _currentSubHandler = null;
                ScreenReader.Say(Loc.Get("browser_focused", panel.ToString()));
            }
        }

        private BrowserPanel GetCurrentPanel(HtmlBrowser browser)
        {
            if (_currentPanelField != null)
            {
                try
                {
                    return (BrowserPanel)(int)_currentPanelField.GetValue(browser);
                }
                catch { }
            }
            return BrowserPanel.PanelSearch;
        }

        #endregion

        #region Browser-Level Keys

        private bool HandleBrowserKeys()
        {
            // Alt+Left: back in history
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _activeBrowser.OnBack();
                ScreenReader.Say(Loc.Get("browser_back"));
                return true;
            }

            // Alt+Right: forward in history
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.RightArrow))
            {
                _activeBrowser.OnForward();
                ScreenReader.Say(Loc.Get("browser_forward"));
                return true;
            }

            // Alt+Home: go to search home
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Home))
            {
                _activeBrowser.EnterWebHome();
                ScreenReader.Say(Loc.Get("browser_home"));
                return true;
            }

            // Alt+R: read content (delegates to sub-handler)
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R))
            {
                _currentSubHandler?.AnnounceState();
                return true;
            }

            // Space: repeat current state
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _currentSubHandler?.AnnounceState();
                return true;
            }

            return false;
        }

        #endregion

        #region PreBuy

        private void ActivatePreBuy(PreBuy preBuy)
        {
            _activePreBuy = preBuy;
            _activePreBuyHw = null;
            _preBuyActive = true;
            _preBuyVersionIndex = 0;

            string name = preBuy.nombre != null ? preBuy.nombre.text : "";
            string price = preBuy.precio != null ? preBuy.precio.text : "";
            string coupons = "";
            if (preBuy.cuponesText != null && preBuy.cuponesText.gameObject.activeInHierarchy)
                coupons = Loc.Get("browser_prebuy_coupons", preBuy.cuponesPrecio != null ? preBuy.cuponesPrecio.text : "");

            ScreenReader.Say(Loc.Get("browser_prebuy", name, price, coupons));
        }

        private void ActivatePreBuyHardware(PreBuyHardware preBuy)
        {
            _activePreBuyHw = preBuy;
            _activePreBuy = null;
            _preBuyActive = true;

            string name = preBuy.nombre != null ? preBuy.nombre.text : "";
            string price = preBuy.precio != null ? preBuy.precio.text : "";
            string coupons = "";
            if (preBuy.cuponesText != null && preBuy.cuponesText.gameObject.activeInHierarchy)
                coupons = Loc.Get("browser_prebuy_coupons", preBuy.cuponesPrecio != null ? preBuy.cuponesPrecio.text : "");

            ScreenReader.Say(Loc.Get("browser_prebuy", name, price, coupons));
        }

        private bool IsPreBuyStillOpen()
        {
            if (_activePreBuy != null)
                return _activePreBuy.gameObject != null && _activePreBuy.gameObject.activeInHierarchy;
            if (_activePreBuyHw != null)
                return _activePreBuyHw.gameObject != null && _activePreBuyHw.gameObject.activeInHierarchy;
            return false;
        }

        private bool HandlePreBuyInput()
        {
            if (_activePreBuy != null)
                return HandlePreBuySoftwareInput();
            if (_activePreBuyHw != null)
                return HandlePreBuyHardwareInput();
            return false;
        }

        private bool HandlePreBuySoftwareInput()
        {
            // Up/Down: cycle version dropdown
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_activePreBuy.dropdown != null && _activePreBuy.dropdown.options.Count > 1)
                {
                    int dir = Input.GetKeyDown(KeyCode.DownArrow) ? 1 : -1;
                    int count = _activePreBuy.dropdown.options.Count;
                    _preBuyVersionIndex = (_preBuyVersionIndex + dir + count) % count;
                    _activePreBuy.dropdown.value = _preBuyVersionIndex;
                    string ver = _activePreBuy.dropdown.options[_preBuyVersionIndex].text;
                    string price = _activePreBuy.precio != null ? _activePreBuy.precio.text : "";
                    ScreenReader.Say(Loc.Get("browser_prebuy_version", ver) + " " + price);
                }
                return true;
            }

            // Tab: toggle source code checkbox
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_activePreBuy.toggleSourceCode != null && _activePreBuy.toggleSourceCode.gameObject.activeInHierarchy)
                {
                    _activePreBuy.toggleSourceCode.isOn = !_activePreBuy.toggleSourceCode.isOn;
                    string state = _activePreBuy.toggleSourceCode.isOn
                        ? Loc.Get("browser_prebuy_source_on")
                        : Loc.Get("browser_prebuy_source_off");
                    ScreenReader.Say(state);
                }
                return true;
            }

            // Enter: buy
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _activePreBuy.OnBuy();
                return true;
            }

            // Escape: cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var dialog = _activePreBuy.GetComponentInParent<uDialog>();
                dialog?.Close();
                _preBuyActive = false;
                _activePreBuy = null;
                return true;
            }

            return false;
        }

        private bool HandlePreBuyHardwareInput()
        {
            // Enter: buy
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _activePreBuyHw.OnBuy();
                return true;
            }

            // Escape: cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var dialog = _activePreBuyHw.GetComponentInParent<uDialog>();
                dialog?.Close();
                _preBuyActive = false;
                _activePreBuyHw = null;
                return true;
            }

            return false;
        }

        #endregion
    }
}
