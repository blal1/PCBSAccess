using HarmonyLib;

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
        static void Postfix(HtmlBrowser __instance, int panel)
        {
            var browserPanel = (BrowserPanel)panel;
            DebugLogger.LogState($"HtmlBrowser.ShowPanel: {browserPanel}");
            BrowserHandler.OnPanelChanged(__instance, browserPanel);
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
}
