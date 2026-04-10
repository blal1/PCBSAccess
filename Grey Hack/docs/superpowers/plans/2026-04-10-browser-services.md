# Browser Services Accessibility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make all HtmlBrowser panels (web pages, bank, shop, hack shop, router config, search, jobs, police, CCTV, ISP, currency, CTF, device finder) accessible via screen reader and keyboard navigation.

**Architecture:** BrowserHandler coordinator captures HtmlBrowser instance, detects panel changes via Harmony patch on ShowPanel(), and delegates to panel-specific sub-handler classes implementing ISubHandler interface. Each sub-handler handles its own input and announcements.

**Tech Stack:** C# (.NET 4.7.2), BepInEx 5, Harmony 2, Unity 2022.3, TMPro, PowerUI

**Spec:** `docs/superpowers/specs/2026-04-10-browser-services-design.md`

---

## File Structure

- Create: `ISubHandler.cs` — Sub-handler interface
- Create: `BrowserHandler.cs` — Coordinator: captures HtmlBrowser, routes input, manages sub-handlers
- Create: `BrowserPatches.cs` — All Harmony patches for HtmlBrowser and related classes
- Create: `WebPageSubHandler.cs` — HTML web pages with link navigation
- Create: `SearchSubHandler.cs` — Search home + no network
- Create: `BankSubHandler.cs` — Bank login/register/account with transactions
- Create: `ShopSubHandler.cs` — Regular software/hardware shop
- Create: `HackShopSubHandler.cs` — Hack shop tools/exploits
- Create: `RouterPortSubHandler.cs` — Port forwarding browse/edit
- Create: `RouterFirewallSubHandler.cs` — Firewall rules browse/edit
- Create: `RouterHelpSubHandler.cs` — Router help text
- Create: `JobsSubHandler.cs` — Jobs + police jobs with detail view
- Create: `PoliceSubHandler.cs` — Police report filing
- Create: `CCTVSubHandler.cs` — CCTV camera metadata
- Create: `ISPSubHandler.cs` — ISP configuration
- Create: `CurrencySubHandler.cs` — Cryptocurrency creation
- Create: `CTFSubHandler.cs` — CTF events
- Create: `FindDeviceSubHandler.cs` — Device manual finder
- Modify: `Main.cs` — Register BrowserHandler
- Modify: `Loc.cs` — Add browser localization strings

---

### Task 1: ISubHandler Interface

**Files:**
- Create: `ISubHandler.cs`

- [ ] **Step 1: Create interface file**

```csharp
namespace GreyHackAccess
{
    /// <summary>
    /// Interface for browser panel sub-handlers.
    /// Each sub-handler manages accessibility for one or more HtmlBrowser panel types.
    /// </summary>
    public interface ISubHandler
    {
        /// <summary>Called when this panel becomes active.</summary>
        void Activate(HtmlBrowser browser);

        /// <summary>Called when panel switches away from this handler.</summary>
        void Deactivate();

        /// <summary>Called each frame when this handler is active. Returns true if input consumed.</summary>
        bool HandleInput();

        /// <summary>Announces current state (for re-focus/Space repeat).</summary>
        void AnnounceState();

        /// <summary>Returns display name for this panel.</summary>
        string GetPanelName();

        /// <summary>Returns help text for F1.</summary>
        string GetHelpText();
    }
}
```

- [ ] **Step 2: Build to verify no errors**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded, 0 errors

- [ ] **Step 3: Commit**

```bash
git add ISubHandler.cs
git commit -m "feat: add ISubHandler interface for browser panel handlers"
```

---

### Task 2: BrowserHandler Coordinator + BrowserPatches

**Files:**
- Create: `BrowserHandler.cs`
- Create: `BrowserPatches.cs`
- Modify: `Main.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add browser localization strings to Loc.cs**

Add before the closing `}` of `InitializeStrings()` (after line 394):

```csharp
            // Browser
            _english["browser_focused"] = "Browser. {0}";
            _english["browser_loading"] = "Loading {0}";
            _english["browser_back"] = "Back.";
            _english["browser_forward"] = "Forward.";
            _english["browser_home"] = "Home.";
            _english["browser_no_history"] = "No more history.";
            _english["browser_help"] = "Browser. Alt Left and Right for history. Alt Home for search. Alt R to read content. Up Down to navigate. Enter to select. F1 for help.";
            _english["browser_prebuy"] = "Purchase: {0}. Price: ${1}. {2}";
            _english["browser_prebuy_coupons"] = "{0} coupons.";
            _english["browser_prebuy_version"] = "Version: {0}.";
            _english["browser_prebuy_source_on"] = "Source code included.";
            _english["browser_prebuy_source_off"] = "Source code not included.";
            _english["browser_prebuy_help"] = "Purchase dialog. Up Down to change version. Tab to toggle source code. Enter to buy. Escape to cancel.";
```

- [ ] **Step 2: Create BrowserPatches.cs**

```csharp
using HarmonyLib;
using System.Reflection;

namespace GreyHackAccess
{
    /// <summary>
    /// Harmony patches for HtmlBrowser accessibility.
    /// All patches delegate to static methods on BrowserHandler.
    /// </summary>

    /// <summary>Captures HtmlBrowser instance on Start.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "Start")]
    public class HtmlBrowserStartPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.Start captured");
            BrowserHandler.OnBrowserCreated(__instance);
        }
    }

    /// <summary>Detects panel changes via ShowPanel.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "ShowPanel")]
    public class HtmlBrowserShowPanelPatch
    {
        static void Postfix(HtmlBrowser __instance, HtmlBrowser.SubPanelWebs panel)
        {
            DebugLogger.LogState($"HtmlBrowser.ShowPanel: {panel}");
            BrowserHandler.OnPanelChanged(__instance, panel);
        }
    }

    /// <summary>Detects web page loaded.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "ResumeConnectionWeb")]
    public class HtmlBrowserResumeWebPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.ResumeConnectionWeb");
            BrowserHandler.OnWebPageLoaded(__instance);
        }
    }

    /// <summary>Detects web page errors.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "CloseConnection")]
    public class HtmlBrowserCloseConnectionPatch
    {
        static void Postfix(HtmlBrowser __instance, string msg)
        {
            DebugLogger.LogState($"HtmlBrowser.CloseConnection: {msg}");
            BrowserHandler.OnConnectionError(__instance, msg);
        }
    }

    /// <summary>Detects navigation started.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "EnterWeb", new[] { typeof(bool), typeof(string) })]
    public class HtmlBrowserEnterWebPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.EnterWeb");
            BrowserHandler.OnNavigationStarted(__instance);
        }
    }

    /// <summary>Detects search initiated.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "EnterSearch")]
    public class HtmlBrowserEnterSearchPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.EnterSearch");
            BrowserHandler.OnSearchStarted(__instance);
        }
    }

    /// <summary>Detects bank login result.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "PlayerLoginBank")]
    public class HtmlBrowserPlayerLoginBankPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.PlayerLoginBank");
            BrowserHandler.OnBankLoggedIn(__instance);
        }
    }

    /// <summary>Detects bank registration result.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "OnBankRegistration")]
    public class HtmlBrowserBankRegistrationPatch
    {
        static void Postfix(HtmlBrowser __instance, string message, string numCuenta)
        {
            DebugLogger.LogState($"HtmlBrowser.OnBankRegistration: {message}, {numCuenta}");
            BrowserHandler.OnBankRegistration(__instance, message, numCuenta);
        }
    }

    /// <summary>Detects bank account creation submitted.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "CreateBankAccount")]
    public class HtmlBrowserCreateBankPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.CreateBankAccount");
            BrowserHandler.OnBankAccountCreating(__instance);
        }
    }

    /// <summary>Detects shop items loaded.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "ResumeConnectionWindowFilesShop")]
    public class HtmlBrowserShopLoadedPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.ResumeConnectionWindowFilesShop");
            BrowserHandler.OnShopLoaded(__instance);
        }
    }

    /// <summary>Detects router config loaded.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "ResumeRouterConfig")]
    public class HtmlBrowserRouterConfigPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.ResumeRouterConfig");
            BrowserHandler.OnRouterConfigLoaded(__instance);
        }
    }

    /// <summary>Detects job/mission selected.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "OnMissionClick")]
    public class HtmlBrowserMissionClickPatch
    {
        static void Postfix(HtmlBrowser __instance, ItemJobs itemJob)
        {
            DebugLogger.LogState("HtmlBrowser.OnMissionClick");
            BrowserHandler.OnMissionSelected(__instance, itemJob);
        }
    }

    /// <summary>Detects police report submitted.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "OnSendAttach")]
    public class HtmlBrowserSendAttachPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.OnSendAttach");
            BrowserHandler.OnPoliceReportSubmitted(__instance);
        }
    }

    /// <summary>Detects PreBuy dialog opened for software.</summary>
    [HarmonyPatch(typeof(PreBuy), "Configure")]
    public class PreBuyConfigurePatch
    {
        static void Postfix(PreBuy __instance)
        {
            DebugLogger.LogState("PreBuy.Configure");
            BrowserHandler.OnPreBuyOpened(__instance);
        }
    }

    /// <summary>Detects PreBuyHardware dialog opened.</summary>
    [HarmonyPatch(typeof(PreBuyHardware), "Configure")]
    public class PreBuyHardwareConfigurePatch
    {
        static void Postfix(PreBuyHardware __instance)
        {
            DebugLogger.LogState("PreBuyHardware.Configure");
            BrowserHandler.OnPreBuyHardwareOpened(__instance);
        }
    }

    /// <summary>Detects bank login submitted.</summary>
    [HarmonyPatch(typeof(HtmlBrowser), "BankLogin")]
    public class HtmlBrowserBankLoginPatch
    {
        static void Postfix(HtmlBrowser __instance)
        {
            DebugLogger.LogState("HtmlBrowser.BankLogin");
            BrowserHandler.OnBankLoginSubmitted(__instance);
        }
    }
}
```

- [ ] **Step 3: Create BrowserHandler.cs**

```csharp
using System.Collections.Generic;
using System.Reflection;
using TMPro;
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
        private HtmlBrowser.SubPanelWebs _currentPanel;
        private ISubHandler _currentSubHandler;

        private readonly Dictionary<HtmlBrowser.SubPanelWebs, ISubHandler> _subHandlers = new();

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
            _subHandlers[HtmlBrowser.SubPanelWebs.Web] = web;

            var search = new SearchSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelSearch] = search;
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelNoNet] = search;

            var bank = new BankSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.BankLogin] = bank;
            _subHandlers[HtmlBrowser.SubPanelWebs.BankRegister] = bank;
            _subHandlers[HtmlBrowser.SubPanelWebs.BankProgram] = bank;

            var shop = new ShopSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.Shop] = shop;

            var hackShop = new HackShopSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.HackShopTools] = hackShop;
            _subHandlers[HtmlBrowser.SubPanelWebs.HackShopExploits] = hackShop;

            var routerPort = new RouterPortSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelPorts] = routerPort;

            var routerFw = new RouterFirewallSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelFirewall] = routerFw;

            var routerHelp = new RouterHelpSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelHelp] = routerHelp;

            var jobs = new JobsSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.Jobs] = jobs;
            _subHandlers[HtmlBrowser.SubPanelWebs.PoliceJobs] = jobs;

            var police = new PoliceSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PoliceReport] = police;

            var cctv = new CCTVSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.Cctv] = cctv;

            var isp = new ISPSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.ISPConfig] = isp;

            var currency = new CurrencySubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.CreateCurrency] = currency;

            var ctf = new CTFSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelCTF] = ctf;

            var findDevice = new FindDeviceSubHandler();
            _subHandlers[HtmlBrowser.SubPanelWebs.PanelFindDeviceManual] = findDevice;
        }

        #endregion

        #region Patch Callbacks

        public static void OnBrowserCreated(HtmlBrowser browser)
        {
            if (_instance == null) return;
            DebugLogger.LogState("BrowserHandler: browser instance captured");
        }

        public static void OnPanelChanged(HtmlBrowser browser, HtmlBrowser.SubPanelWebs panel)
        {
            if (_instance == null) return;
            if (_instance._activeBrowser != browser) return;
            _instance.SwitchPanel(panel);
        }

        public static void OnWebPageLoaded(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is WebPageSubHandler web)
                web.OnPageLoaded(browser);
        }

        public static void OnConnectionError(HtmlBrowser browser, string msg)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is WebPageSubHandler web)
                web.OnConnectionError(msg);
        }

        public static void OnNavigationStarted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            string address = browser.barAddress != null ? browser.barAddress.text : "";
            ScreenReader.Say(Loc.Get("browser_loading", address));
        }

        public static void OnSearchStarted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is SearchSubHandler search)
                search.OnSearchStarted();
        }

        public static void OnBankLoggedIn(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankLoggedIn(browser);
        }

        public static void OnBankRegistration(HtmlBrowser browser, string message, string numCuenta)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankRegistration(message, numCuenta);
        }

        public static void OnBankAccountCreating(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankAccountCreating();
        }

        public static void OnBankLoginSubmitted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is BankSubHandler bank)
                bank.OnBankLoginSubmitted();
        }

        public static void OnShopLoaded(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is ShopSubHandler shop)
                shop.OnShopLoaded(browser);
            else if (_instance._currentSubHandler is HackShopSubHandler hackShop)
                hackShop.OnShopLoaded(browser);
        }

        public static void OnRouterConfigLoaded(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is RouterPortSubHandler port)
                port.OnRouterConfigLoaded(browser);
            else if (_instance._currentSubHandler is RouterFirewallSubHandler fw)
                fw.OnRouterConfigLoaded(browser);
        }

        public static void OnMissionSelected(HtmlBrowser browser, ItemJobs itemJob)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is JobsSubHandler jobs)
                jobs.OnMissionSelected(itemJob);
        }

        public static void OnPoliceReportSubmitted(HtmlBrowser browser)
        {
            if (_instance == null || !_instance._isActive) return;
            if (_instance._activeBrowser != browser) return;
            if (_instance._currentSubHandler is PoliceSubHandler police)
                police.OnReportSubmitted();
        }

        public static void OnPreBuyOpened(PreBuy preBuy)
        {
            if (_instance == null || !_instance._isActive) return;
            _instance.ActivatePreBuy(preBuy);
        }

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

        private void SwitchPanel(HtmlBrowser.SubPanelWebs panel)
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

        private HtmlBrowser.SubPanelWebs GetCurrentPanel(HtmlBrowser browser)
        {
            if (_currentPanelField != null)
            {
                try
                {
                    return (HtmlBrowser.SubPanelWebs)_currentPanelField.GetValue(browser);
                }
                catch { }
            }
            return HtmlBrowser.SubPanelWebs.PanelSearch;
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
            // Up/Down or Alt+V: cycle version dropdown
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
                var dialog = _activePreBuy.GetComponent<Ventana>()?.dialogo;
                if (dialog == null)
                    dialog = _activePreBuy.GetComponentInParent<uDialog>();
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
                var dialog = _activePreBuyHw.GetComponent<Ventana>()?.dialogo;
                if (dialog == null)
                    dialog = _activePreBuyHw.GetComponentInParent<uDialog>();
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
```

- [ ] **Step 4: Register in Main.cs**

Add field after `_tutorialHandler` (line 35):
```csharp
        private BrowserHandler _browserHandler;
```

Add registration in `InitializeHandlers()` after `_tutorialHandler.Register();` (line 123):
```csharp
            _browserHandler = new BrowserHandler();
            _browserHandler.Register();
```

Add to `UpdateHandlers()` after `if (_chatHandler.Update()) return;` (line 264):
```csharp
            // Browser consumes input when active
            if (_browserHandler.Update()) return;
```

Add to `AnnounceHelp()` after the `_chatHandler.IsActive` block (line 348):
```csharp
            if (_browserHandler.IsActive)
            {
                ScreenReader.Say(_browserHandler.GetHelpText());
                return;
            }
```

- [ ] **Step 5: Build to verify**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build will fail because sub-handler classes don't exist yet. This is expected - we need stub sub-handlers.

- [ ] **Step 6: Create stub sub-handlers**

Create minimal stubs for all 15 sub-handlers so the build passes. Each follows this pattern (shown for SearchSubHandler, repeat for all):

```csharp
namespace GreyHackAccess
{
    /// <summary>Stub handler for search panel. Full implementation in later task.</summary>
    public class SearchSubHandler : ISubHandler
    {
        public void Activate(HtmlBrowser browser) { }
        public void Deactivate() { }
        public bool HandleInput() { return false; }
        public void AnnounceState() { }
        public string GetPanelName() { return "Search"; }
        public string GetHelpText() { return Loc.Get("browser_help"); }
        public void OnSearchStarted() { }
    }
}
```

Create these stub files:
- `WebPageSubHandler.cs` (add `OnPageLoaded(HtmlBrowser)`, `OnConnectionError(string)`)
- `SearchSubHandler.cs` (add `OnSearchStarted()`)
- `BankSubHandler.cs` (add `OnBankLoggedIn(HtmlBrowser)`, `OnBankRegistration(string, string)`, `OnBankAccountCreating()`, `OnBankLoginSubmitted()`)
- `ShopSubHandler.cs` (add `OnShopLoaded(HtmlBrowser)`)
- `HackShopSubHandler.cs` (add `OnShopLoaded(HtmlBrowser)`)
- `RouterPortSubHandler.cs` (add `OnRouterConfigLoaded(HtmlBrowser)`)
- `RouterFirewallSubHandler.cs` (add `OnRouterConfigLoaded(HtmlBrowser)`)
- `RouterHelpSubHandler.cs`
- `JobsSubHandler.cs` (add `OnMissionSelected(ItemJobs)`)
- `PoliceSubHandler.cs` (add `OnReportSubmitted()`)
- `CCTVSubHandler.cs`
- `ISPSubHandler.cs`
- `CurrencySubHandler.cs`
- `CTFSubHandler.cs`
- `FindDeviceSubHandler.cs`

- [ ] **Step 7: Build to verify all stubs compile**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded, 0 errors

- [ ] **Step 8: Deploy and test basic activation**

Run: `powershell -File scripts/Deploy-Mod.ps1`
Test: Start game, open browser, verify "Browser. Search" announced when browser gets focus.

- [ ] **Step 9: Commit**

```bash
git add BrowserHandler.cs BrowserPatches.cs ISubHandler.cs Main.cs Loc.cs WebPageSubHandler.cs SearchSubHandler.cs BankSubHandler.cs ShopSubHandler.cs HackShopSubHandler.cs RouterPortSubHandler.cs RouterFirewallSubHandler.cs RouterHelpSubHandler.cs JobsSubHandler.cs PoliceSubHandler.cs CCTVSubHandler.cs ISPSubHandler.cs CurrencySubHandler.cs CTFSubHandler.cs FindDeviceSubHandler.cs
git commit -m "feat: add BrowserHandler coordinator with stubs for all sub-handlers"
```

---

### Task 3: WebPageSubHandler

**Files:**
- Modify: `WebPageSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

Add to `InitializeStrings()` in Loc.cs:

```csharp
            // Web page
            _english["web_page"] = "Web page. {0} links.";
            _english["web_page_no_links"] = "Web page. No interactive links.";
            _english["web_link"] = "{0} of {1}: {2}";
            _english["web_error_not_found"] = "Page not found.";
            _english["web_error_url_not_found"] = "URL not found.";
            _english["web_error_no_net"] = "No network access.";
            _english["web_searching"] = "Searching...";
            _english["web_help"] = "Web page. Up Down to navigate links. Enter to activate link. Alt R to read page. Space to repeat.";
```

- [ ] **Step 2: Implement WebPageSubHandler**

Replace the stub with:

```csharp
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for HTML web pages rendered by PowerUI.
    /// Navigates interactive buttons and reads page text.
    /// </summary>
    public class WebPageSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private List<string> _linkTexts = new();
        private List<string> _linkIds = new();
        private int _currentIndex;
        private string _pageText = "";
        private bool _isActive;

        private static FieldInfo _panelCustomField;
        private static FieldInfo _documentField;

        static WebPageSubHandler()
        {
            _panelCustomField = typeof(HtmlBrowser).GetField("panelCustom", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            ExtractPageContent();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _linkTexts.Clear();
            _linkIds.Clear();
            _pageText = "";
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_linkTexts.Count == 0) return true;
                _currentIndex = (_currentIndex + 1) % _linkTexts.Count;
                AnnounceCurrentLink();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_linkTexts.Count == 0) return true;
                _currentIndex = (_currentIndex - 1 + _linkTexts.Count) % _linkTexts.Count;
                AnnounceCurrentLink();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_linkTexts.Count > 0 && _currentIndex < _linkIds.Count)
                {
                    ActivateLink(_linkIds[_currentIndex]);
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            if (_linkTexts.Count > 0)
            {
                ScreenReader.Say(Loc.Get("web_page", _linkTexts.Count));
                AnnounceCurrentLink();
            }
            else if (!string.IsNullOrEmpty(_pageText))
            {
                ScreenReader.Say(Loc.Get("web_page_no_links"));
                string preview = _pageText.Length > 300 ? _pageText.Substring(0, 300) : _pageText;
                ScreenReader.Say(preview);
            }
            else
            {
                ScreenReader.Say(Loc.Get("web_page_no_links"));
            }
        }

        public string GetPanelName() => "Web";

        public string GetHelpText() => Loc.Get("web_help");

        /// <summary>Called by BrowserHandler when ResumeConnectionWeb fires.</summary>
        public void OnPageLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            ExtractPageContent();
            _currentIndex = 0;
            AnnounceState();
        }

        /// <summary>Called by BrowserHandler when CloseConnection fires.</summary>
        public void OnConnectionError(string msg)
        {
            if (string.IsNullOrEmpty(msg) || msg.Contains("ip address not found"))
                ScreenReader.Say(Loc.Get("web_error_not_found"));
            else if (msg.Contains("url_not_found"))
                ScreenReader.Say(Loc.Get("web_error_url_not_found"));
            else if (msg.Contains("no_net_access"))
                ScreenReader.Say(Loc.Get("web_error_no_net"));
            else
                ScreenReader.Say(msg);
        }

        private void ExtractPageContent()
        {
            _linkTexts.Clear();
            _linkIds.Clear();
            _pageText = "";

            if (_browser == null) return;

            try
            {
                var panelCustom = _panelCustomField?.GetValue(_browser);
                if (panelCustom == null) return;

                // Get Document property
                var docProp = panelCustom.GetType().GetProperty("Document");
                var document = docProp?.GetValue(panelCustom);
                if (document == null) return;

                // Get innerHTML
                var htmlProp = document.GetType().GetProperty("innerHTML");
                string html = htmlProp?.GetValue(document) as string;
                if (string.IsNullOrEmpty(html)) return;

                // Strip HTML tags for readable text
                _pageText = Regex.Replace(html, "<[^>]+>", " ");
                _pageText = Regex.Replace(_pageText, @"\s+", " ").Trim();

                // Find buttons with class "btn btn-primary"
                var body = document.GetType().GetProperty("body")?.GetValue(document);
                if (body == null) return;

                var getElements = body.GetType().GetMethod("getElementsByClassName");
                if (getElements == null) return;

                var elements = getElements.Invoke(body, new object[] { "btn btn-primary" });
                if (elements == null) return;

                // Iterate elements
                var enumerator = elements.GetType().GetMethod("GetEnumerator")?.Invoke(elements, null);
                if (enumerator == null) return;

                var moveNext = enumerator.GetType().GetMethod("MoveNext");
                var current = enumerator.GetType().GetProperty("Current");

                while ((bool)moveNext.Invoke(enumerator, null))
                {
                    var element = current.GetValue(enumerator);
                    if (element == null) continue;

                    var idProp = element.GetType().GetProperty("id");
                    string id = idProp?.GetValue(element) as string ?? "";

                    var textProp = element.GetType().GetProperty("textContent");
                    string text = textProp?.GetValue(element) as string ?? id;
                    text = text.Trim();

                    if (!string.IsNullOrEmpty(text))
                    {
                        _linkTexts.Add(text);
                        _linkIds.Add(id);
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLogger.LogState($"WebPageSubHandler.ExtractPageContent error: {e.Message}");
            }
        }

        private void AnnounceCurrentLink()
        {
            if (_currentIndex < _linkTexts.Count)
                ScreenReader.Say(Loc.Get("web_link", _currentIndex + 1, _linkTexts.Count, _linkTexts[_currentIndex]));
        }

        private void ActivateLink(string linkId)
        {
            if (_browser == null || string.IsNullOrEmpty(linkId)) return;

            try
            {
                // Use reflection to call OnButtonWebClick-like routing
                // The buttons use onmousedown which routes through ShowBankLogin, ShowHackShop, etc.
                // We invoke the public Show methods based on known IDs
                switch (linkId)
                {
                    case "LoginBank": _browser.ShowBankLogin(); break;
                    case "RegisterBank":
                        var showReg = typeof(HtmlBrowser).GetMethod("ShowPanel", BindingFlags.NonPublic | BindingFlags.Instance);
                        showReg?.Invoke(_browser, new object[] { HtmlBrowser.SubPanelWebs.BankRegister });
                        break;
                    case "HackShopTools": _browser.ShowHackShop(); break;
                    case "HackShopExploits": _browser.ShowHackShopExploits(); break;
                    case "InformaticaShop":
                        var showShop = typeof(HtmlBrowser).GetMethod("OnShowListInformaticaShop", BindingFlags.NonPublic | BindingFlags.Instance);
                        showShop?.Invoke(_browser, null);
                        break;
                    case "Jobs": _browser.ShowJobs(); break;
                    case "JobsPolice": _browser.ShowJobsPolice(); break;
                    case "Reports": _browser.ShowReports(); break;
                    case "CTF": _browser.ShowCTF(); break;
                    default:
                        DebugLogger.LogState($"WebPageSubHandler: unknown link ID '{linkId}'");
                        break;
                }
            }
            catch (System.Exception e)
            {
                DebugLogger.LogState($"WebPageSubHandler.ActivateLink error: {e.Message}");
            }
        }
    }
}
```

- [ ] **Step 3: Build**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded, 0 errors

- [ ] **Step 4: Commit**

```bash
git add WebPageSubHandler.cs Loc.cs
git commit -m "feat: implement WebPageSubHandler for HTML page navigation"
```

---

### Task 4: SearchSubHandler

**Files:**
- Modify: `SearchSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Search
            _english["search_home"] = "Search. Type query in address bar, Enter to search.";
            _english["search_no_net"] = "No network connection.";
            _english["search_searching"] = "Searching...";
            _english["search_help"] = "Search page. Type in the address bar and press Enter to search. Results appear as web page links.";
```

- [ ] **Step 2: Implement SearchSubHandler**

```csharp
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for search home page and no-network panel.
    /// </summary>
    public class SearchSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private bool _isNoNet;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            // Detect if we're on PanelNoNet by checking if search input is available
            _isNoNet = browser.inputSearch == null || !browser.inputSearch.gameObject.activeInHierarchy;
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public bool HandleInput()
        {
            // Search panel has no custom navigation - address bar handles typing
            return false;
        }

        public void AnnounceState()
        {
            if (_isNoNet)
                ScreenReader.Say(Loc.Get("search_no_net"));
            else
                ScreenReader.Say(Loc.Get("search_home"));
        }

        public string GetPanelName() => _isNoNet ? "No Network" : "Search";

        public string GetHelpText() => Loc.Get("search_help");

        /// <summary>Called when search is initiated.</summary>
        public void OnSearchStarted()
        {
            ScreenReader.Say(Loc.Get("search_searching"));
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add SearchSubHandler.cs Loc.cs
git commit -m "feat: implement SearchSubHandler for search and no-network panels"
```

---

### Task 5: BankSubHandler

**Files:**
- Modify: `BankSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Bank
            _english["bank_login"] = "Bank login. Tab between fields, Enter to submit.";
            _english["bank_register"] = "Bank registration. Tab between fields, Enter to submit.";
            _english["bank_logging_in"] = "Logging in...";
            _english["bank_creating"] = "Creating account...";
            _english["bank_registered"] = "Account created. Account number: {0}. {1}";
            _english["bank_account"] = "Bank account. Balance: {0}. {1} transactions.";
            _english["bank_transaction"] = "{0} of {1}: {2}";
            _english["bank_no_transactions"] = "No transactions.";
            _english["bank_balance"] = "Balance: {0}.";
            _english["bank_help_login"] = "Bank login. Tab between account and password fields. Enter to log in.";
            _english["bank_help_register"] = "Bank registration. Tab between fields. Enter to register.";
            _english["bank_help_account"] = "Bank account. Up Down to browse transactions. Space to repeat balance. Tab to transfer fields.";
```

- [ ] **Step 2: Implement BankSubHandler**

```csharp
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
        private HtmlBrowser.SubPanelWebs _bankPanel;
        private int _transactionIndex;
        private int _transactionCount;

        private static FieldInfo _bankListAdapterField;

        static BankSubHandler()
        {
            _bankListAdapterField = typeof(HtmlBrowser).GetField("bankListAdapter", BindingFlags.Public | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _transactionIndex = 0;
            _transactionCount = 0;

            // Detect which bank panel
            var panelField = typeof(HtmlBrowser).GetField("currentPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (panelField != null)
                _bankPanel = (HtmlBrowser.SubPanelWebs)panelField.GetValue(browser);

            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (_bankPanel == HtmlBrowser.SubPanelWebs.BankProgram)
                return HandleAccountInput();

            // Login and Register panels: Tab handles field cycling natively
            return false;
        }

        public void AnnounceState()
        {
            switch (_bankPanel)
            {
                case HtmlBrowser.SubPanelWebs.BankLogin:
                    ScreenReader.Say(Loc.Get("bank_login"));
                    break;
                case HtmlBrowser.SubPanelWebs.BankRegister:
                    ScreenReader.Say(Loc.Get("bank_register"));
                    break;
                case HtmlBrowser.SubPanelWebs.BankProgram:
                    AnnounceAccount();
                    break;
            }
        }

        public string GetPanelName()
        {
            switch (_bankPanel)
            {
                case HtmlBrowser.SubPanelWebs.BankLogin: return "Bank Login";
                case HtmlBrowser.SubPanelWebs.BankRegister: return "Bank Registration";
                case HtmlBrowser.SubPanelWebs.BankProgram: return "Bank Account";
                default: return "Bank";
            }
        }

        public string GetHelpText()
        {
            switch (_bankPanel)
            {
                case HtmlBrowser.SubPanelWebs.BankLogin: return Loc.Get("bank_help_login");
                case HtmlBrowser.SubPanelWebs.BankRegister: return Loc.Get("bank_help_register");
                case HtmlBrowser.SubPanelWebs.BankProgram: return Loc.Get("bank_help_account");
                default: return Loc.Get("bank_help_login");
            }
        }

        #region Patch Callbacks

        public void OnBankLoggedIn(HtmlBrowser browser)
        {
            _browser = browser;
            _bankPanel = HtmlBrowser.SubPanelWebs.BankProgram;
            AnnounceAccount();
        }

        public void OnBankRegistration(string message, string numCuenta)
        {
            if (!string.IsNullOrEmpty(numCuenta))
                ScreenReader.Say(Loc.Get("bank_registered", numCuenta, message));
            else
                ScreenReader.Say(message);
        }

        public void OnBankAccountCreating()
        {
            ScreenReader.Say(Loc.Get("bank_creating"));
        }

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

                // Data is SimpleDataHelper, use indexer
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
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add BankSubHandler.cs Loc.cs
git commit -m "feat: implement BankSubHandler for bank login, register, and account"
```

---

### Task 6: ShopSubHandler

**Files:**
- Modify: `ShopSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Shop
            _english["shop_loaded"] = "Shop. {0} items. Up Down to browse, Enter to buy.";
            _english["shop_item"] = "{0} of {1}: {2}. {3}. ${4}";
            _english["shop_item_hw"] = "{0} of {1}: {2}. {3}. {4}. ${5}";
            _english["shop_empty"] = "Shop. No items available.";
            _english["shop_filter"] = "Filter: {0}";
            _english["shop_help"] = "Shop. Up Down to browse items. Enter to buy. Alt F to change filter. Space to repeat.";
```

- [ ] **Step 2: Implement ShopSubHandler**

```csharp
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for regular software/hardware shop.
    /// </summary>
    public class ShopSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private List<Component> _items = new();
        private bool _isHardware;

        private static FieldInfo _listaItemsField;
        private static FieldInfo _listaItemsHwField;

        static ShopSubHandler()
        {
            _listaItemsField = typeof(HtmlBrowser).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
            _listaItemsHwField = typeof(HtmlBrowser).GetField("listaItemsHw", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    var buyMethod = _items[_currentIndex].GetType().GetMethod("OnBuy");
                    buyMethod?.Invoke(_items[_currentIndex], null);
                }
                return true;
            }

            // Alt+F: cycle filter
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F))
            {
                if (_browser.filterDropdown != null && _browser.filterDropdown.options.Count > 0)
                {
                    int next = (_browser.filterDropdown.value + 1) % _browser.filterDropdown.options.Count;
                    _browser.filterDropdown.value = next;
                    string filterName = _browser.filterDropdown.options[next].text;
                    ScreenReader.Say(Loc.Get("shop_filter", filterName));
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            if (_items.Count == 0)
                ScreenReader.Say(Loc.Get("shop_empty"));
            else
            {
                ScreenReader.Say(Loc.Get("shop_loaded", _items.Count));
                AnnounceCurrentItem();
            }
        }

        public string GetPanelName() => "Shop";

        public string GetHelpText() => Loc.Get("shop_help");

        /// <summary>Called when shop items are loaded from server.</summary>
        public void OnShopLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            _currentIndex = 0;
            RefreshItems();
            AnnounceState();
        }

        private void RefreshItems()
        {
            _items.Clear();

            // Try software items first
            var softItems = _listaItemsField?.GetValue(_browser) as List<ItemShop>;
            if (softItems != null && softItems.Count > 0)
            {
                _isHardware = false;
                foreach (var item in softItems)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }

            // Try hardware items
            if (_items.Count == 0)
            {
                var hwItems = _listaItemsHwField?.GetValue(_browser) as List<ItemShopHardware>;
                if (hwItems != null && hwItems.Count > 0)
                {
                    _isHardware = true;
                    foreach (var item in hwItems)
                    {
                        if (item != null && item.gameObject.activeInHierarchy)
                            _items.Add(item);
                    }
                }
            }
        }

        private void AnnounceCurrentItem()
        {
            if (_currentIndex >= _items.Count) return;

            var item = _items[_currentIndex];
            if (_isHardware)
            {
                var hw = item as ItemShopHardware;
                if (hw == null) return;
                string name = hw.nombre != null ? hw.nombre.text : "";
                string desc = hw.description != null ? hw.description.text : "";
                string tag = hw.tagText != null ? hw.tagText.text : "";
                string price = hw.precio != null ? hw.precio.text : "";
                ScreenReader.Say(Loc.Get("shop_item_hw", _currentIndex + 1, _items.Count, name, tag, desc, price));
            }
            else
            {
                var shop = item as ItemShop;
                if (shop == null) return;
                string name = shop.nombre != null ? shop.nombre.text : "";
                string desc = shop.description != null ? shop.description.text : "";
                string price = GetItemPrice(shop);
                ScreenReader.Say(Loc.Get("shop_item", _currentIndex + 1, _items.Count, name, desc, price));
            }
        }

        private string GetItemPrice(ItemShop item)
        {
            try
            {
                var listaField = typeof(ItemShop).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
                var lista = listaField?.GetValue(item) as List<ItemShopAdvanced>;
                if (lista != null && lista.Count > 0)
                    return lista[0].GetPrecio().ToString();
            }
            catch { }
            return "?";
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add ShopSubHandler.cs Loc.cs
git commit -m "feat: implement ShopSubHandler for software and hardware shop"
```

---

### Task 7: HackShopSubHandler

**Files:**
- Modify: `HackShopSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Hack shop
            _english["hackshop_tools"] = "Hack shop tools. {0} items. Up Down to browse, Enter to buy.";
            _english["hackshop_exploits"] = "Hack shop exploits. Select library and version to search.";
            _english["hackshop_exploits_results"] = "Found {0} exploits. Up Down to browse.";
            _english["hackshop_item"] = "{0} of {1}: {2}. {3}. ${4}";
            _english["hackshop_exploit_item"] = "{0} of {1}: {2}. Service: {3}. ${4}. {5}";
            _english["hackshop_empty"] = "No items available.";
            _english["hackshop_filter"] = "Library: {0}";
            _english["hackshop_perm_filter"] = "Permission: {0}";
            _english["hackshop_help_tools"] = "Hack shop tools. Up Down to browse. Enter to buy. Alt F to change filter. Space to repeat.";
            _english["hackshop_help_exploits"] = "Hack shop exploits. Alt F to select library. Type version and Enter to search. Up Down to browse results. Enter to buy. Alt P for permission filter.";
```

- [ ] **Step 2: Implement HackShopSubHandler**

```csharp
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for hack shop tools and exploits panels.
    /// </summary>
    public class HackShopSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private bool _isExploits;
        private int _currentIndex;
        private List<Component> _items = new();

        private static FieldInfo _listaItemsField;
        private static FieldInfo _currentPanelField;

        static HackShopSubHandler()
        {
            _listaItemsField = typeof(HtmlBrowser).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
            _currentPanelField = typeof(HtmlBrowser).GetField("currentPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;

            var panel = (HtmlBrowser.SubPanelWebs)(_currentPanelField?.GetValue(browser) ?? HtmlBrowser.SubPanelWebs.HackShopTools);
            _isExploits = panel == HtmlBrowser.SubPanelWebs.HackShopExploits;

            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    var buyMethod = _items[_currentIndex].GetType().GetMethod("OnBuy");
                    buyMethod?.Invoke(_items[_currentIndex], null);
                }
                return true;
            }

            // Alt+F: cycle library/filter dropdown
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F))
            {
                if (_isExploits)
                {
                    // Cycle BrowserHackshop library dropdown
                    var hackshop = _browser.GetComponentInChildren<BrowserHackshop>();
                    if (hackshop != null && hackshop.libraryId != null && hackshop.libraryId.options.Count > 0)
                    {
                        int next = (hackshop.libraryId.value + 1) % hackshop.libraryId.options.Count;
                        hackshop.libraryId.value = next;
                        ScreenReader.Say(Loc.Get("hackshop_filter", hackshop.libraryId.options[next].text));
                    }
                }
                else
                {
                    if (_browser.filterDropdown != null && _browser.filterDropdown.options.Count > 0)
                    {
                        int next = (_browser.filterDropdown.value + 1) % _browser.filterDropdown.options.Count;
                        _browser.filterDropdown.value = next;
                        ScreenReader.Say(Loc.Get("hackshop_filter", _browser.filterDropdown.options[next].text));
                    }
                }
                return true;
            }

            // Alt+P: cycle permission filter (exploits only)
            if (_isExploits && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.P))
            {
                if (_browser.filterDropdownPerm != null && _browser.filterDropdownPerm.options.Count > 0)
                {
                    int next = (_browser.filterDropdownPerm.value + 1) % _browser.filterDropdownPerm.options.Count;
                    _browser.filterDropdownPerm.value = next;
                    ScreenReader.Say(Loc.Get("hackshop_perm_filter", _browser.filterDropdownPerm.options[next].text));
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            if (_isExploits)
            {
                if (_items.Count > 0)
                    ScreenReader.Say(Loc.Get("hackshop_exploits_results", _items.Count));
                else
                    ScreenReader.Say(Loc.Get("hackshop_exploits"));
            }
            else
            {
                if (_items.Count > 0)
                    ScreenReader.Say(Loc.Get("hackshop_tools", _items.Count));
                else
                    ScreenReader.Say(Loc.Get("hackshop_empty"));
            }
        }

        public string GetPanelName() => _isExploits ? "Hack Shop Exploits" : "Hack Shop Tools";

        public string GetHelpText() => _isExploits ? Loc.Get("hackshop_help_exploits") : Loc.Get("hackshop_help_tools");

        public void OnShopLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            _currentIndex = 0;
            RefreshItems();
            AnnounceState();
        }

        private void RefreshItems()
        {
            _items.Clear();
            var softItems = _listaItemsField?.GetValue(_browser) as List<ItemShop>;
            if (softItems != null)
            {
                foreach (var item in softItems)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }
        }

        private void AnnounceCurrentItem()
        {
            if (_currentIndex >= _items.Count) return;

            var item = _items[_currentIndex] as ItemShop;
            if (item == null) return;

            string name = item.nombre != null ? item.nombre.text : "";
            string desc = item.description != null ? item.description.text : "";

            if (item is ItemHackShop hackItem)
            {
                string service = "";
                var serviceField = typeof(ItemHackShop).GetField("serviceAffected", BindingFlags.NonPublic | BindingFlags.Instance);
                service = serviceField?.GetValue(hackItem) as string ?? "";
                if (string.IsNullOrEmpty(service)) service = "unknown";

                string price = GetExploitPrice(hackItem);
                ScreenReader.Say(Loc.Get("hackshop_exploit_item", _currentIndex + 1, _items.Count, name, service, price, desc));
            }
            else
            {
                string price = GetItemPrice(item);
                ScreenReader.Say(Loc.Get("hackshop_item", _currentIndex + 1, _items.Count, name, desc, price));
            }
        }

        private string GetItemPrice(ItemShop item)
        {
            try
            {
                var listaField = typeof(ItemShop).GetField("listaItems", BindingFlags.NonPublic | BindingFlags.Instance);
                var lista = listaField?.GetValue(item) as List<ItemShopAdvanced>;
                if (lista != null && lista.Count > 0)
                    return lista[0].GetPrecio().ToString();
            }
            catch { }
            return "?";
        }

        private string GetExploitPrice(ItemHackShop item)
        {
            try
            {
                var exploitField = typeof(ItemHackShop).GetField("exploit", BindingFlags.NonPublic | BindingFlags.Instance);
                var exploit = exploitField?.GetValue(item) as Exploit;
                if (exploit != null) return exploit.GetPrecio().ToString();
            }
            catch { }
            return GetItemPrice(item);
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add HackShopSubHandler.cs Loc.cs
git commit -m "feat: implement HackShopSubHandler for hack shop tools and exploits"
```

---

### Task 8: RouterPortSubHandler

**Files:**
- Modify: `RouterPortSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Router - Port Forwarding
            _english["router_ports"] = "Port forwarding. {0} rules. Up Down to browse, Enter to edit, N to add new.";
            _english["router_port_rule"] = "Rule {0} of {1}: External {2} to {3}:{4}.{5}";
            _english["router_port_protected"] = " Protected.";
            _english["router_port_empty"] = "Port forwarding. No rules.";
            _english["router_port_editing"] = "Editing rule. Tab between fields, Enter to save, Escape to cancel.";
            _english["router_port_field"] = "{0}: {1}";
            _english["router_port_saved"] = "Rule saved.";
            _english["router_port_cancelled"] = "Edit cancelled.";
            _english["router_port_deleted"] = "Deleted {0} rules.";
            _english["router_port_protected_no_edit"] = "Protected rule. Cannot edit.";
            _english["router_port_help"] = "Port forwarding. Up Down to browse rules. Enter to edit. N to add. Delete to remove. Space to repeat.";
```

- [ ] **Step 2: Implement RouterPortSubHandler**

```csharp
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for router port forwarding configuration.
    /// Browse mode: Up/Down to navigate, Enter to edit.
    /// Edit mode: Tab to cycle fields, Enter to save, Escape to cancel.
    /// </summary>
    public class RouterPortSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private List<ItemPortForward> _items = new();
        private bool _editMode;
        private int _editFieldIndex;
        private ItemPortForward _editingItem;

        private static readonly string[] _fieldNames = { "External port", "Internal port", "LAN IP" };

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            _editMode = false;
            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _editMode = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;
            return _editMode ? HandleEditInput() : HandleBrowseInput();
        }

        public void AnnounceState()
        {
            RefreshItems();
            if (_items.Count == 0)
                ScreenReader.Say(Loc.Get("router_port_empty"));
            else
                ScreenReader.Say(Loc.Get("router_ports", _items.Count));
        }

        public string GetPanelName() => "Port Forwarding";

        public string GetHelpText() => Loc.Get("router_port_help");

        public void OnRouterConfigLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            _currentIndex = 0;
            _editMode = false;
            RefreshItems();
            AnnounceState();
        }

        private bool HandleBrowseInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentRule();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentRule();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                    EnterEditMode(_items[_currentIndex]);
                return true;
            }

            // N: add new rule
            if (Input.GetKeyDown(KeyCode.N))
            {
                var configUI = _browser.GetComponentInChildren<RouterConfigUI>();
                if (configUI != null)
                {
                    configUI.AddEntry();
                    RefreshItems();
                    if (_items.Count > 0)
                    {
                        _currentIndex = _items.Count - 1;
                        EnterEditMode(_items[_currentIndex]);
                    }
                }
                return true;
            }

            // Delete: remove selected
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    // Select current item then remove
                    var toggle = typeof(ItemPortForward).GetField("selection", BindingFlags.NonPublic | BindingFlags.Instance);
                    var sel = toggle?.GetValue(_items[_currentIndex]) as UnityEngine.UI.Toggle;
                    if (sel != null) sel.isOn = true;

                    var configUI = _browser.GetComponentInChildren<RouterConfigUI>();
                    configUI?.RemoveSelected();
                    ScreenReader.Say(Loc.Get("router_port_deleted", 1));
                    RefreshItems();
                    _currentIndex = Mathf.Min(_currentIndex, _items.Count - 1);
                }
                return true;
            }

            return false;
        }

        private bool HandleEditInput()
        {
            // Tab: cycle fields
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _editFieldIndex = (_editFieldIndex + 1) % 3;
                AnnounceEditField();
                FocusEditField();
                return true;
            }

            // Enter: save
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _editingItem.SaveChanges();
                ScreenReader.Say(Loc.Get("router_port_saved"));
                _editMode = false;
                RefreshItems();
                return true;
            }

            // Escape: cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _editMode = false;
                ScreenReader.Say(Loc.Get("router_port_cancelled"));
                return true;
            }

            return false;
        }

        private void EnterEditMode(ItemPortForward item)
        {
            // Check if protected
            var protectedField = typeof(ItemPortForward).GetField("protectedIcon", BindingFlags.Public | BindingFlags.Instance);
            var icon = protectedField?.GetValue(item) as GameObject;
            if (icon != null && icon.activeInHierarchy)
            {
                ScreenReader.Say(Loc.Get("router_port_protected_no_edit"));
                return;
            }

            _editMode = true;
            _editingItem = item;
            _editFieldIndex = 0;
            ScreenReader.Say(Loc.Get("router_port_editing"));
            AnnounceEditField();
            FocusEditField();
        }

        private void AnnounceEditField()
        {
            if (_editingItem == null) return;
            string value = "";
            switch (_editFieldIndex)
            {
                case 0: value = _editingItem.externalPort?.text ?? ""; break;
                case 1: value = _editingItem.internalPort?.text ?? ""; break;
                case 2: value = _editingItem.lanIpAddress?.text ?? ""; break;
            }
            ScreenReader.Say(Loc.Get("router_port_field", _fieldNames[_editFieldIndex], value));
        }

        private void FocusEditField()
        {
            if (_editingItem == null) return;
            TMP_InputField field = null;
            switch (_editFieldIndex)
            {
                case 0: field = _editingItem.externalPort; break;
                case 1: field = _editingItem.internalPort; break;
                case 2: field = _editingItem.lanIpAddress; break;
            }
            if (field != null)
            {
                field.Select();
                field.ActivateInputField();
            }
        }

        private void RefreshItems()
        {
            _items.Clear();
            var configUI = _browser?.GetComponentInChildren<RouterConfigUI>();
            if (configUI == null) return;

            var itemsField = typeof(RouterConfigUI).GetField("itemsRouter", BindingFlags.NonPublic | BindingFlags.Instance);
            var items = itemsField?.GetValue(configUI) as List<ItemPortForward>;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }
        }

        private void AnnounceCurrentRule()
        {
            if (_currentIndex >= _items.Count) return;
            var item = _items[_currentIndex];
            string ext = item.externalPort?.text ?? "?";
            string intern = item.internalPort?.text ?? "?";
            string ip = item.lanIpAddress?.text ?? "?";

            string protectedStr = "";
            var protectedIcon = typeof(ItemPortForward).GetField("protectedIcon", BindingFlags.Public | BindingFlags.Instance);
            var icon = protectedIcon?.GetValue(item) as GameObject;
            if (icon != null && icon.activeInHierarchy)
                protectedStr = Loc.Get("router_port_protected");

            ScreenReader.Say(Loc.Get("router_port_rule", _currentIndex + 1, _items.Count, ext, ip, intern, protectedStr));
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add RouterPortSubHandler.cs Loc.cs
git commit -m "feat: implement RouterPortSubHandler for port forwarding config"
```

---

### Task 9: RouterFirewallSubHandler

**Files:**
- Modify: `RouterFirewallSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Router - Firewall
            _english["router_firewall"] = "Firewall rules. {0} rules. Up Down to browse, Enter to edit, N to add new.";
            _english["router_fw_rule"] = "Rule {0} of {1}: {2} port {3} from {4} to {5}.";
            _english["router_fw_empty"] = "Firewall. No rules.";
            _english["router_fw_editing"] = "Editing firewall rule. Tab between fields, Enter to save, Escape to cancel.";
            _english["router_fw_field"] = "{0}: {1}";
            _english["router_fw_action_toggle"] = "Action: {0}";
            _english["router_fw_any_on"] = "{0} set to Any.";
            _english["router_fw_any_off"] = "{0} restored to {1}.";
            _english["router_fw_saved"] = "Rule saved.";
            _english["router_fw_cancelled"] = "Edit cancelled.";
            _english["router_fw_deleted"] = "Deleted {0} rules.";
            _english["router_fw_help"] = "Firewall rules. Up Down to browse. Enter to edit. N to add. Delete to remove. In edit mode: Tab between fields, Left Right for Allow Deny, Alt A to toggle Any.";
```

- [ ] **Step 2: Implement RouterFirewallSubHandler**

```csharp
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for router firewall rule configuration.
    /// Browse mode: Up/Down to navigate, Enter to edit.
    /// Edit mode: Tab to cycle fields, Left/Right for Allow/Deny, Alt+A for Any toggle.
    /// </summary>
    public class RouterFirewallSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private List<ItemHwFirewall> _items = new();
        private bool _editMode;
        private int _editFieldIndex;
        private ItemHwFirewall _editingItem;

        private static readonly string[] _fieldNames = { "Action", "Port", "Source address", "Destination address" };

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            _editMode = false;
            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _editMode = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;
            return _editMode ? HandleEditInput() : HandleBrowseInput();
        }

        public void AnnounceState()
        {
            RefreshItems();
            if (_items.Count == 0)
                ScreenReader.Say(Loc.Get("router_fw_empty"));
            else
                ScreenReader.Say(Loc.Get("router_firewall", _items.Count));
        }

        public string GetPanelName() => "Firewall";

        public string GetHelpText() => Loc.Get("router_fw_help");

        public void OnRouterConfigLoaded(HtmlBrowser browser)
        {
            _browser = browser;
            _currentIndex = 0;
            _editMode = false;
            RefreshItems();
            AnnounceState();
        }

        private bool HandleBrowseInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentRule();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentRule();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                    EnterEditMode(_items[_currentIndex]);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                var configUI = _browser.GetComponentInChildren<HwFirewallConfigUI>();
                if (configUI != null)
                {
                    configUI.AddEntry();
                    RefreshItems();
                    if (_items.Count > 0)
                    {
                        _currentIndex = _items.Count - 1;
                        EnterEditMode(_items[_currentIndex]);
                    }
                }
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    var toggle = typeof(ItemHwFirewall).GetField("selection", BindingFlags.NonPublic | BindingFlags.Instance);
                    var sel = toggle?.GetValue(_items[_currentIndex]) as UnityEngine.UI.Toggle;
                    if (sel != null) sel.isOn = true;

                    var configUI = _browser.GetComponentInChildren<HwFirewallConfigUI>();
                    configUI?.RemoveSelected();
                    ScreenReader.Say(Loc.Get("router_fw_deleted", 1));
                    RefreshItems();
                    _currentIndex = Mathf.Min(_currentIndex, _items.Count - 1);
                }
                return true;
            }

            return false;
        }

        private bool HandleEditInput()
        {
            // Tab: cycle fields
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _editFieldIndex = (_editFieldIndex + 1) % 4;
                AnnounceEditField();
                FocusEditField();
                return true;
            }

            // Left/Right: toggle Allow/Deny on action field
            if (_editFieldIndex == 0 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
            {
                if (_editingItem.dropdownAction != null)
                {
                    int newVal = _editingItem.dropdownAction.value == 0 ? 1 : 0;
                    _editingItem.dropdownAction.value = newVal;
                    string action = _editingItem.dropdownAction.options[newVal].text;
                    ScreenReader.Say(Loc.Get("router_fw_action_toggle", action));
                }
                return true;
            }

            // Alt+A: toggle Any on current field
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.A))
            {
                ToggleAny();
                return true;
            }

            // Enter: save
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _editingItem.SaveChanges();
                ScreenReader.Say(Loc.Get("router_fw_saved"));
                _editMode = false;
                RefreshItems();
                return true;
            }

            // Escape: cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _editMode = false;
                ScreenReader.Say(Loc.Get("router_fw_cancelled"));
                return true;
            }

            return false;
        }

        private void EnterEditMode(ItemHwFirewall item)
        {
            _editMode = true;
            _editingItem = item;
            _editFieldIndex = 0;
            ScreenReader.Say(Loc.Get("router_fw_editing"));
            AnnounceEditField();
        }

        private void ToggleAny()
        {
            switch (_editFieldIndex)
            {
                case 1: // Port
                    if (_editingItem.togglePort != null)
                    {
                        _editingItem.togglePort.isOn = !_editingItem.togglePort.isOn;
                        string state = _editingItem.togglePort.isOn
                            ? Loc.Get("router_fw_any_on", "Port")
                            : Loc.Get("router_fw_any_off", "Port", _editingItem.port?.text ?? "");
                        ScreenReader.Say(state);
                    }
                    break;
                case 2: // Source
                    if (_editingItem.toggleSource != null)
                    {
                        _editingItem.toggleSource.isOn = !_editingItem.toggleSource.isOn;
                        string state = _editingItem.toggleSource.isOn
                            ? Loc.Get("router_fw_any_on", "Source")
                            : Loc.Get("router_fw_any_off", "Source", _editingItem.sourceAddress?.text ?? "");
                        ScreenReader.Say(state);
                    }
                    break;
                case 3: // Dest
                    if (_editingItem.toggleDest != null)
                    {
                        _editingItem.toggleDest.isOn = !_editingItem.toggleDest.isOn;
                        string state = _editingItem.toggleDest.isOn
                            ? Loc.Get("router_fw_any_on", "Destination")
                            : Loc.Get("router_fw_any_off", "Destination", _editingItem.destAddress?.text ?? "");
                        ScreenReader.Say(state);
                    }
                    break;
            }
        }

        private void AnnounceEditField()
        {
            if (_editingItem == null) return;
            string value = "";
            switch (_editFieldIndex)
            {
                case 0:
                    value = _editingItem.dropdownAction?.options[_editingItem.dropdownAction.value].text ?? "";
                    break;
                case 1:
                    value = _editingItem.togglePort?.isOn == true ? "Any" : (_editingItem.port?.text ?? "");
                    break;
                case 2:
                    value = _editingItem.toggleSource?.isOn == true ? "Any" : (_editingItem.sourceAddress?.text ?? "");
                    break;
                case 3:
                    value = _editingItem.toggleDest?.isOn == true ? "Any" : (_editingItem.destAddress?.text ?? "");
                    break;
            }
            ScreenReader.Say(Loc.Get("router_fw_field", _fieldNames[_editFieldIndex], value));
        }

        private void FocusEditField()
        {
            if (_editingItem == null) return;
            switch (_editFieldIndex)
            {
                case 1:
                    if (_editingItem.port != null && !_editingItem.togglePort.isOn)
                    { _editingItem.port.Select(); _editingItem.port.ActivateInputField(); }
                    break;
                case 2:
                    if (_editingItem.sourceAddress != null && !_editingItem.toggleSource.isOn)
                    { _editingItem.sourceAddress.Select(); _editingItem.sourceAddress.ActivateInputField(); }
                    break;
                case 3:
                    if (_editingItem.destAddress != null && !_editingItem.toggleDest.isOn)
                    { _editingItem.destAddress.Select(); _editingItem.destAddress.ActivateInputField(); }
                    break;
            }
        }

        private void RefreshItems()
        {
            _items.Clear();
            var configUI = _browser?.GetComponentInChildren<HwFirewallConfigUI>();
            if (configUI == null) return;

            var itemsField = typeof(HwFirewallConfigUI).GetField("itemsRouter", BindingFlags.NonPublic | BindingFlags.Instance);
            var items = itemsField?.GetValue(configUI) as List<ItemHwFirewall>;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null && item.gameObject.activeInHierarchy)
                        _items.Add(item);
                }
            }
        }

        private void AnnounceCurrentRule()
        {
            if (_currentIndex >= _items.Count) return;
            var item = _items[_currentIndex];
            string action = item.dropdownAction?.options[item.dropdownAction.value].text ?? "?";
            string port = item.togglePort?.isOn == true ? "Any" : (item.port?.text ?? "?");
            string source = item.toggleSource?.isOn == true ? "Any" : (item.sourceAddress?.text ?? "?");
            string dest = item.toggleDest?.isOn == true ? "Any" : (item.destAddress?.text ?? "?");

            ScreenReader.Say(Loc.Get("router_fw_rule", _currentIndex + 1, _items.Count, action, port, source, dest));
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add RouterFirewallSubHandler.cs Loc.cs
git commit -m "feat: implement RouterFirewallSubHandler for firewall rule config"
```

---

### Task 10: RouterHelpSubHandler

**Files:**
- Modify: `RouterHelpSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Router help
            _english["router_help_panel"] = "Router help.";
            _english["router_help_text"] = "Router help. Alt R to read help content.";
```

- [ ] **Step 2: Implement RouterHelpSubHandler**

```csharp
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for router help panel. Reads help text content.
    /// </summary>
    public class RouterHelpSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput() { return false; }

        public void AnnounceState()
        {
            ScreenReader.Say(Loc.Get("router_help_panel"));
            // Try to read help text from the panel
            var panel = _browser?.GetComponentInChildren<RouterConfigUI>();
            if (panel != null)
            {
                var texts = panel.GetComponentsInChildren<TMP_Text>();
                foreach (var text in texts)
                {
                    if (text != null && !string.IsNullOrWhiteSpace(text.text) && text.text.Length > 20)
                    {
                        ScreenReader.Say(text.text);
                        return;
                    }
                }
            }
        }

        public string GetPanelName() => "Router Help";

        public string GetHelpText() => Loc.Get("router_help_text");
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add RouterHelpSubHandler.cs Loc.cs
git commit -m "feat: implement RouterHelpSubHandler for router help panel"
```

---

### Task 11: JobsSubHandler

**Files:**
- Modify: `JobsSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Jobs
            _english["jobs_panel"] = "Jobs. {0} missions. Up Down to browse, Enter for details.";
            _english["jobs_police"] = "Police jobs. {0} missions. Up Down to browse, Enter for details.";
            _english["jobs_item"] = "{0} of {1}: {2}. {3}";
            _english["jobs_item_rep"] = "{0} of {1}: {2}. Reputation {3}. {4}";
            _english["jobs_empty"] = "No missions available.";
            _english["jobs_detail"] = "Mission: {0}. {1}";
            _english["jobs_back"] = "Back to job list.";
            _english["jobs_help"] = "Jobs. Up Down to browse missions. Enter to view details. Backspace to go back. Space to repeat.";
```

- [ ] **Step 2: Implement JobsSubHandler**

```csharp
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for jobs and police jobs panels.
    /// </summary>
    public class JobsSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private bool _isPolice;
        private int _currentIndex;
        private List<ItemJobs> _items = new();
        private bool _inDetail;
        private ItemJobs _detailItem;

        private static FieldInfo _currentPanelField;

        static JobsSubHandler()
        {
            _currentPanelField = typeof(HtmlBrowser).GetField("currentPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            _inDetail = false;

            var panel = (HtmlBrowser.SubPanelWebs)(_currentPanelField?.GetValue(browser) ?? HtmlBrowser.SubPanelWebs.Jobs);
            _isPolice = panel == HtmlBrowser.SubPanelWebs.PoliceJobs;

            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _inDetail = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (_inDetail)
            {
                if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
                {
                    _inDetail = false;
                    ScreenReader.Say(Loc.Get("jobs_back"));
                    AnnounceState();
                    return true;
                }
                return false;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentJob();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentJob();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    _items[_currentIndex].OnClick();
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            RefreshItems();
            if (_items.Count == 0)
            {
                ScreenReader.Say(Loc.Get("jobs_empty"));
                return;
            }
            string key = _isPolice ? "jobs_police" : "jobs_panel";
            ScreenReader.Say(Loc.Get(key, _items.Count));
        }

        public string GetPanelName() => _isPolice ? "Police Jobs" : "Jobs";

        public string GetHelpText() => Loc.Get("jobs_help");

        /// <summary>Called when a mission is clicked for detail view.</summary>
        public void OnMissionSelected(ItemJobs itemJob)
        {
            _inDetail = true;
            _detailItem = itemJob;
            string title = itemJob.title != null ? itemJob.title.text : "";
            string content = itemJob.content != null ? itemJob.content.text : "";
            ScreenReader.Say(Loc.Get("jobs_detail", title, content));
        }

        private void RefreshItems()
        {
            _items.Clear();
            // Find all active ItemJobs in the browser's content areas
            var contentShop = _browser?.contentShop;
            if (contentShop != null)
            {
                foreach (var content in contentShop)
                {
                    if (content == null || !content.gameObject.activeInHierarchy) continue;
                    var jobItems = content.GetComponentsInChildren<ItemJobs>();
                    foreach (var item in jobItems)
                    {
                        if (item != null && item.gameObject.activeInHierarchy)
                            _items.Add(item);
                    }
                }
            }

            // Also check panelsBrowser[3] (hack shop panel which shares with jobs)
            if (_items.Count == 0)
            {
                var allJobs = _browser?.GetComponentsInChildren<ItemJobs>();
                if (allJobs != null)
                {
                    foreach (var item in allJobs)
                    {
                        if (item != null && item.gameObject.activeInHierarchy)
                            _items.Add(item);
                    }
                }
            }
        }

        private void AnnounceCurrentJob()
        {
            if (_currentIndex >= _items.Count) return;
            var item = _items[_currentIndex];
            string title = item.title != null ? item.title.text : "";
            string content = item.content != null ? item.content.text : "";
            string minRep = item.minRep != null && !string.IsNullOrEmpty(item.minRep.text)
                ? item.minRep.text : "";

            if (!string.IsNullOrEmpty(minRep))
                ScreenReader.Say(Loc.Get("jobs_item_rep", _currentIndex + 1, _items.Count, title, minRep, content));
            else
                ScreenReader.Say(Loc.Get("jobs_item", _currentIndex + 1, _items.Count, title, content));
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add JobsSubHandler.cs Loc.cs
git commit -m "feat: implement JobsSubHandler for jobs and police jobs panels"
```

---

### Task 12: PoliceSubHandler

**Files:**
- Modify: `PoliceSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Police
            _english["police_report"] = "Police report. Enter IP address and attach evidence file.";
            _english["police_submitted"] = "Report submitted.";
            _english["police_help"] = "Police report. Tab between IP field and attach button. Enter to submit. Space to repeat.";
```

- [ ] **Step 2: Implement PoliceSubHandler**

```csharp
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for police crime report filing.
    /// </summary>
    public class PoliceSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput()
        {
            // Tab/Enter handled natively by the form fields
            return false;
        }

        public void AnnounceState()
        {
            ScreenReader.Say(Loc.Get("police_report"));
        }

        public string GetPanelName() => "Police Report";

        public string GetHelpText() => Loc.Get("police_help");

        /// <summary>Called when report is submitted.</summary>
        public void OnReportSubmitted()
        {
            ScreenReader.Say(Loc.Get("police_submitted"));
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add PoliceSubHandler.cs Loc.cs
git commit -m "feat: implement PoliceSubHandler for police report filing"
```

---

### Task 13: CCTVSubHandler

**Files:**
- Modify: `CCTVSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // CCTV
            _english["cctv_active"] = "CCTV camera. {0}W A S D to pan. Zoom in and out with plus and minus.";
            _english["cctv_title"] = "Camera: {0}. ";
            _english["cctv_password"] = "Enter camera password.";
            _english["cctv_help"] = "CCTV camera. W A S D to pan camera. This is a visual-only feed.";
```

- [ ] **Step 2: Implement CCTVSubHandler**

```csharp
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for CCTV camera panel.
    /// Announces camera metadata. Visual feed cannot be described.
    /// </summary>
    public class CCTVSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput()
        {
            // Camera controls (WASD) handled natively by BrowserCam
            return false;
        }

        public void AnnounceState()
        {
            string titleInfo = "";
            if (_browser.browserCam != null && _browser.browserCam.title != null)
            {
                string camTitle = _browser.browserCam.title.text;
                if (!string.IsNullOrEmpty(camTitle))
                    titleInfo = Loc.Get("cctv_title", camTitle);
            }

            // Check if password panel is showing
            if (_browser.browserCam != null && _browser.browserCam.panelPassObj != null
                && _browser.browserCam.panelPassObj.activeInHierarchy)
            {
                ScreenReader.Say(Loc.Get("cctv_password"));
                return;
            }

            ScreenReader.Say(Loc.Get("cctv_active", titleInfo));
        }

        public string GetPanelName() => "CCTV";

        public string GetHelpText() => Loc.Get("cctv_help");
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add CCTVSubHandler.cs Loc.cs
git commit -m "feat: implement CCTVSubHandler for CCTV camera panel"
```

---

### Task 14: ISPSubHandler

**Files:**
- Modify: `ISPSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // ISP
            _english["isp_panel"] = "ISP configuration. Choose a package or manage your domain.";
            _english["isp_help"] = "ISP panel. Use Tab to navigate options. Enter to select.";
```

- [ ] **Step 2: Implement ISPSubHandler**

```csharp
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for ISP configuration panel.
    /// </summary>
    public class ISPSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput()
        {
            // Form fields handle Tab/Enter natively
            return false;
        }

        public void AnnounceState()
        {
            // Try to read ISPPanel content
            var ispPanel = _browser?.GetComponentInChildren<ISPPanel>();
            if (ispPanel != null)
            {
                string selected = ispPanel.packSelectedText != null ? ispPanel.packSelectedText.text : "";
                if (!string.IsNullOrEmpty(selected))
                {
                    ScreenReader.Say(Loc.Get("isp_panel") + " " + selected);
                    return;
                }
            }
            ScreenReader.Say(Loc.Get("isp_panel"));
        }

        public string GetPanelName() => "ISP";

        public string GetHelpText() => Loc.Get("isp_help");
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add ISPSubHandler.cs Loc.cs
git commit -m "feat: implement ISPSubHandler for ISP configuration panel"
```

---

### Task 15: CurrencySubHandler

**Files:**
- Modify: `CurrencySubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Currency
            _english["currency_panel"] = "Create cryptocurrency. Enter coin name, username, and password. Cost: $150.";
            _english["currency_help"] = "Cryptocurrency creation. Tab between fields. Enter to create. Cost is $150.";
```

- [ ] **Step 2: Implement CurrencySubHandler**

```csharp
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for cryptocurrency creation panel.
    /// </summary>
    public class CurrencySubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput()
        {
            // Form fields handle Tab/Enter natively
            return false;
        }

        public void AnnounceState()
        {
            ScreenReader.Say(Loc.Get("currency_panel"));
        }

        public string GetPanelName() => "Create Cryptocurrency";

        public string GetHelpText() => Loc.Get("currency_help");
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add CurrencySubHandler.cs Loc.cs
git commit -m "feat: implement CurrencySubHandler for cryptocurrency creation"
```

---

### Task 16: CTFSubHandler

**Files:**
- Modify: `CTFSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // CTF
            _english["ctf_panel"] = "CTF events. {0} missions. Up Down to browse, Enter for details.";
            _english["ctf_empty"] = "CTF events. No missions available.";
            _english["ctf_item"] = "{0} of {1}: {2}. By {3}. {4}";
            _english["ctf_detail"] = "CTF: {0}. {1}";
            _english["ctf_back"] = "Back to CTF list.";
            _english["ctf_help"] = "CTF events. Up Down to browse. Enter for details. Backspace to go back.";
```

- [ ] **Step 2: Implement CTFSubHandler**

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for CTF events panel.
    /// </summary>
    public class CTFSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;
        private int _currentIndex;
        private bool _inDetail;

        // ItemCTFPanel components found in the CTF panel
        private List<GameObject> _items = new();

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            _currentIndex = 0;
            _inDetail = false;
            RefreshItems();
            AnnounceState();
        }

        public void Deactivate()
        {
            _isActive = false;
            _inDetail = false;
            _items.Clear();
        }

        public bool HandleInput()
        {
            if (!_isActive) return false;

            if (_inDetail)
            {
                if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
                {
                    _inDetail = false;
                    ScreenReader.Say(Loc.Get("ctf_back"));
                    return true;
                }
                return false;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Min(_currentIndex + 1, _items.Count - 1);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_items.Count == 0) return true;
                _currentIndex = Mathf.Max(_currentIndex - 1, 0);
                AnnounceCurrentItem();
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_items.Count > 0 && _currentIndex < _items.Count)
                {
                    _inDetail = true;
                    // Read detail from CTFPanel
                    var ctfPanel = _browser?.GetComponentInChildren<CTFPanel>();
                    if (ctfPanel != null && ctfPanel.detailTitle != null)
                    {
                        string title = ctfPanel.detailTitle.text;
                        string desc = ctfPanel.detailDescription != null ? ctfPanel.detailDescription.text : "";
                        ScreenReader.Say(Loc.Get("ctf_detail", title, desc));
                    }
                }
                return true;
            }

            return false;
        }

        public void AnnounceState()
        {
            if (_items.Count == 0)
                ScreenReader.Say(Loc.Get("ctf_empty"));
            else
                ScreenReader.Say(Loc.Get("ctf_panel", _items.Count));
        }

        public string GetPanelName() => "CTF";

        public string GetHelpText() => Loc.Get("ctf_help");

        private void RefreshItems()
        {
            _items.Clear();
            var ctfPanel = _browser?.GetComponentInChildren<CTFPanel>();
            if (ctfPanel == null || ctfPanel.content == null) return;

            for (int i = 0; i < ctfPanel.content.childCount; i++)
            {
                var child = ctfPanel.content.GetChild(i);
                if (child != null && child.gameObject.activeInHierarchy)
                    _items.Add(child.gameObject);
            }
        }

        private void AnnounceCurrentItem()
        {
            if (_currentIndex >= _items.Count) return;
            var obj = _items[_currentIndex];

            // Read ItemCTFPanel fields via TMP_Text children
            var texts = obj.GetComponentsInChildren<TMP_Text>();
            string title = texts.Length > 0 ? texts[0].text : "";
            string creator = texts.Length > 2 ? texts[2].text : "";
            string desc = texts.Length > 1 ? texts[1].text : "";

            ScreenReader.Say(Loc.Get("ctf_item", _currentIndex + 1, _items.Count, title, creator, desc));
        }
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add CTFSubHandler.cs Loc.cs
git commit -m "feat: implement CTFSubHandler for CTF events panel"
```

---

### Task 17: FindDeviceSubHandler

**Files:**
- Modify: `FindDeviceSubHandler.cs`
- Modify: `Loc.cs`

- [ ] **Step 1: Add localization strings**

```csharp
            // Find Device
            _english["finddevice_panel"] = "Device manual finder. Type device model and search.";
            _english["finddevice_result"] = "Manual: {0}. Model: {1}.";
            _english["finddevice_help"] = "Device manual finder. Type a model name and press Enter to search. Alt R to read manual.";
```

- [ ] **Step 2: Implement FindDeviceSubHandler**

```csharp
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Sub-handler for device manual finder panel.
    /// </summary>
    public class FindDeviceSubHandler : ISubHandler
    {
        private HtmlBrowser _browser;
        private bool _isActive;

        public void Activate(HtmlBrowser browser)
        {
            _browser = browser;
            _isActive = true;
            AnnounceState();
        }

        public void Deactivate() { _isActive = false; }

        public bool HandleInput()
        {
            // Form handles Tab/Enter natively
            return false;
        }

        public void AnnounceState()
        {
            var deviceUI = _browser?.GetComponentInChildren<DeviceManualUI>();
            if (deviceUI != null && deviceUI.titleManual != null && !string.IsNullOrEmpty(deviceUI.titleManual.text))
            {
                string title = deviceUI.titleManual.text;
                string model = deviceUI.titleModel != null ? deviceUI.titleModel.text : "";
                ScreenReader.Say(Loc.Get("finddevice_result", title, model));

                // Also read manual text if available
                if (deviceUI.manualText != null && !string.IsNullOrEmpty(deviceUI.manualText.text))
                    ScreenReader.Say(deviceUI.manualText.text);
            }
            else
            {
                ScreenReader.Say(Loc.Get("finddevice_panel"));
            }
        }

        public string GetPanelName() => "Device Manual";

        public string GetHelpText() => Loc.Get("finddevice_help");
    }
}
```

- [ ] **Step 3: Build and commit**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded

```bash
git add FindDeviceSubHandler.cs Loc.cs
git commit -m "feat: implement FindDeviceSubHandler for device manual finder"
```

---

### Task 18: Full Build, Deploy, and Integration Test

**Files:**
- Modify: `project_status.md`

- [ ] **Step 1: Full build**

Run: `powershell -File scripts/Build-Mod.ps1`
Expected: Build succeeded, 0 errors, 0 warnings

- [ ] **Step 2: Deploy**

Run: `powershell -File scripts/Deploy-Mod.ps1`

- [ ] **Step 3: Update project_status.md**

Add BrowserHandler to implemented features and add pending test checklist. Update "Currently working on" to reflect browser handler testing phase.

- [ ] **Step 4: Commit**

```bash
git add project_status.md
git commit -m "docs: add BrowserHandler to project status and test checklist"
```

- [ ] **Step 5: In-game testing checklist**

Test the following in-game:

**Browser activation:**
- Open browser: hear panel announcement
- Switch panels: hear new panel name
- Alt+Left/Right: history navigation announced
- Alt+Home: search home announced
- F1: hear browser help text

**Web pages:**
- Visit a website: hear "Web page. N links."
- Up/Down: navigate links
- Enter: activate link, panel switches

**Bank:**
- Navigate to bank login: hear "Bank login."
- Log in: hear "Logging in..." then "Logged in. Balance: $X."
- Up/Down in account: navigate transactions

**Shop:**
- Open shop: hear "Shop. N items."
- Up/Down: browse items with name, description, price
- Enter on item: PreBuy dialog announced

**Hack shop:**
- Open hack shop tools: hear item count
- Switch to exploits: hear search prompt
- Alt+F: cycle library

**Router config:**
- Open port forwarding: hear rule count
- Up/Down: browse rules
- Enter: edit mode with Tab field cycling
- Open firewall: hear rule count
- Left/Right: toggle Allow/Deny

**Jobs:**
- Open jobs: hear mission count
- Up/Down: browse jobs
- Enter: detail view

**Simple panels (CCTV, ISP, Currency, CTF, FindDevice):**
- Each panel announced on activation
- F1: panel-specific help
