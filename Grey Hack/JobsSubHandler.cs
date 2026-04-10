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

            BrowserPanel panel = BrowserPanel.Jobs;
            if (_currentPanelField != null)
            {
                try { panel = (BrowserPanel)(int)_currentPanelField.GetValue(browser); }
                catch { }
            }
            _isPolice = panel == BrowserPanel.PoliceJobs;

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
