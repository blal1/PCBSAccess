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

            var texts = obj.GetComponentsInChildren<TMP_Text>();
            string title = texts.Length > 0 ? texts[0].text : "";
            string creator = texts.Length > 2 ? texts[2].text : "";
            string desc = texts.Length > 1 ? texts[1].text : "";

            ScreenReader.Say(Loc.Get("ctf_item", _currentIndex + 1, _items.Count, title, creator, desc));
        }
    }
}
