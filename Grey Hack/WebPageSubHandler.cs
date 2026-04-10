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
                switch (linkId)
                {
                    case "LoginBank": _browser.ShowBankLogin(); break;
                    case "RegisterBank": _browser.ShowBankRegister(); break;
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
