using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PCBS;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Keyboard navigation for the main menu (Menu_V2 scene).
    /// Scans buttons inside the mainMenu panel only — excludes OS desktop icons and HUD.
    /// Up/Down cycles through visible items; Enter clicks the focused button.
    /// </summary>
    public static class MainMenuHandler
    {
        #region State

        private static bool _active;
        private static Button[] _buttons;
        private static int _focused = -1;

        /// <summary>Frame on which this context was activated — input blocked until frame+1 to prevent bleed-in.</summary>
        private static int _activatedFrame = -1;

        /// <summary>Frame on which the last click fired — Enter blocked for 10 frames after any click.</summary>
        private static int _lastClickFrame = -1;

        /// <summary>Root transform of the main menu panel — buttons are scanned inside this.</summary>
        private static Transform _menuRoot;

        /// <summary>Direct reference to the How to Build a PC button for label override.</summary>
        private static Button _howToBuildAPC;

        /// <summary>Options and Exit buttons from MenuContext (live outside the mainMenu panel).</summary>
        private static Button _optionsBtn;
        private static Button _exitBtn;

        /// <summary>Cached FieldInfo for CareerModeButton.m_id (private SerializeField).</summary>
        private static FieldInfo _careerIdField;

        /// <summary>Cached FieldInfo for CareerModeButton.m_career (the expand Button, private SerializeField).</summary>
        private static FieldInfo _careerBtnField;

        /// <summary>Maps each m_career Button → its localized career label. Built once in OnMainMenuStart.</summary>
        private static Dictionary<Button, string> _careerBtnLabels;

        public static bool IsActive => _active;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "MainMenu";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                // Block input on the activation frame to prevent bleed-in of keys from previous scene.
                if (Time.frameCount <= _activatedFrame + 1) return false;

                if (Input.GetKeyDown(KeyCode.DownArrow))  { Navigate(1);     return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { Navigate(-1);    return true; }
                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && Time.frameCount > _lastClickFrame + 30) { ClickFocused(); return true; }
                return false;
            }

            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("mainmenu_help")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        internal static void OnMainMenuStart(MainMenu instance)
        {
            // Guard: Harmony patch and coroutine both call this — only init once per menu load.
            if (_active) return;

            _menuRoot      = instance?.mainMenu?.transform;
            _howToBuildAPC = instance?.howToBuildAPC;
            _optionsBtn    = instance?.m_context?.options;
            _exitBtn       = instance?.m_context?.exit;
            _active        = true;
            _focused       = -1;
            _activatedFrame = Time.frameCount;
            _lastClickFrame = -1;
            _buttons       = null;
            _careerIdField = typeof(CareerModeButton)
                .GetField("m_id", BindingFlags.NonPublic | BindingFlags.Instance);
            _careerBtnField = typeof(CareerModeButton)
                .GetField("m_career", BindingFlags.NonPublic | BindingFlags.Instance);
            _careerBtnLabels = BuildCareerBtnLabels(_menuRoot);

            Main.Log.LogInfo("[PCBSAccess] MainMenuHandler: OnMainMenuStart fired — _active = true");
            DebugLogger.LogState("MainMenuHandler: activated");
            InputRouter.Push(_ctx);
            ScreenReader.Say(Loc.Get("mainmenu_opened"));
        }

        public static void Reset()
        {
            _active        = false;
            _buttons       = null;
            _focused       = -1;
            _lastClickFrame = -1;
            _menuRoot      = null;
            _howToBuildAPC = null;
            _optionsBtn    = null;
            _exitBtn       = null;
            _careerIdField  = null;
            _careerBtnField = null;
            _careerBtnLabels = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void Navigate(int direction)
        {
            RefreshButtons();

            if (_buttons == null || _buttons.Length == 0)
            {
                ScreenReader.Say(Loc.Get("mainmenu_no_buttons"));
                DebugLogger.LogState("MainMenuHandler: Navigate — no buttons found");
                return;
            }

            _focused = ((_focused + direction) % _buttons.Length + _buttons.Length) % _buttons.Length;
            DebugLogger.LogState($"MainMenuHandler: Navigate to index {_focused} — '{GetButtonLabel(_buttons[_focused])}'");
            AnnounceButton(_buttons[_focused]);
        }

        private static void ClickFocused()
        {
            if (_buttons == null || _focused < 0 || _focused >= _buttons.Length)
            {
                Main.Log.LogInfo("[PCBSAccess] MainMenuHandler: ClickFocused — nothing focused");
                return;
            }

            Button btn = _buttons[_focused];
            string label = GetButtonLabel(btn);
            Main.Log.LogInfo($"[PCBSAccess] MainMenuHandler: Clicking index={_focused} label='{label}'");
            _lastClickFrame = Time.frameCount;
            ScreenReader.Say(Loc.Get("menu_clicking", label));
            ButtonHelper.Click(btn);
            
            if (Main.Instance != null)
                Main.Instance.StartCoroutine(DeferredRefresh());
        }

        private static System.Collections.IEnumerator DeferredRefresh()
        {
            // Wait for end of frame so career sub-panels (m_optionsParent) are processed.
            yield return new UnityEngine.WaitForEndOfFrame();

            Button[] prev = _buttons != null ? (Button[])_buttons.Clone() : null;
            RefreshButtons();

            // Reset cooldown whenever button list changes — even if count is the same.
            // When a career panel expands, m_career is replaced by m_newGame at the same count,
            // so checking count alone would miss it and block Enter for 30 more frames.
            if (ButtonListChanged(prev, _buttons))
                _lastClickFrame = -1;

            if (_buttons != null && _focused >= 0 && _focused < _buttons.Length)
                AnnounceButton(_buttons[_focused]);
        }

        private static bool ButtonListChanged(Button[] prev, Button[] curr)
        {
            if (prev == null && curr == null) return false;
            if (prev == null || curr == null) return true;
            if (prev.Length != curr.Length)   return true;
            for (int i = 0; i < prev.Length; i++)
                if (prev[i] != curr[i]) return true;
            return false;
        }


        #endregion

        #region Button Discovery

        /// <summary>
        /// Scans the mainMenu panel for active, interactable buttons.
        /// Falls back to full scene scan if mainMenu root is unavailable.
        /// Sorted top-to-bottom so navigation order matches visual layout.
        /// </summary>
        private static void RefreshButtons()
        {
            var active = new List<Button>();

            // Prefer scanning only inside the main menu panel to avoid OS desktop icons.
            if (_menuRoot != null)
            {
                Button[] inMenu = _menuRoot.GetComponentsInChildren<Button>(includeInactive: false);
                foreach (Button btn in inMenu)
                {
                    if (btn.IsInteractable())
                        active.Add(btn);
                }
            }
            else
            {
                // Fallback: full scene scan with no filter (original behaviour).
                Button[] all = UnityEngine.Object.FindObjectsOfType<Button>();
                foreach (Button btn in all)
                {
                    if (btn.gameObject.activeInHierarchy && btn.IsInteractable())
                        active.Add(btn);
                }
            }

            // If howToBuildAPC wasn't captured inside the panel, add it explicitly.
            if (_howToBuildAPC != null
                && _howToBuildAPC.gameObject.activeInHierarchy
                && _howToBuildAPC.IsInteractable()
                && !active.Contains(_howToBuildAPC))
            {
                active.Add(_howToBuildAPC);
            }

            // Options and Exit live in MenuContext outside the mainMenu panel — add them explicitly.
            if (_optionsBtn != null && _optionsBtn.gameObject.activeInHierarchy && _optionsBtn.IsInteractable()
                && !active.Contains(_optionsBtn))
            {
                active.Add(_optionsBtn);
            }
            if (_exitBtn != null && _exitBtn.gameObject.activeInHierarchy && _exitBtn.IsInteractable()
                && !active.Contains(_exitBtn))
            {
                active.Add(_exitBtn);
            }

            // Sort top-to-bottom (highest y first) so arrow key order feels natural.
            active.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

            _buttons = active.ToArray();

            if (_buttons.Length == 0)
                _focused = -1;
            else if (_focused >= _buttons.Length)
                _focused = _buttons.Length - 1;

            if (Main.DebugMode)
            {
                Main.Log.LogInfo($"[PCBSAccess] MainMenuHandler: {_buttons.Length} buttons found");
                for (int i = 0; i < _buttons.Length; i++)
                    Main.Log.LogInfo($"[PCBSAccess]   [{i}] '{GetButtonLabel(_buttons[i])}'");
            }
        }

        #endregion

        #region Label Resolution

        private static void AnnounceButton(Button button)
        {
            string label = GetButtonLabel(button);
            if (!string.IsNullOrEmpty(label))
                ScreenReader.Say(label);
        }

        /// <summary>
        /// Resolves a human-readable label for any main menu button.
        /// Priority: howToBuildAPC override → pre-built career label → ButtonHelper (Text/TMP) → CareerModeButton id → object name.
        /// </summary>
        private static string GetButtonLabel(Button btn)
        {
            if (btn == null) return string.Empty;

            // Explicit overrides for buttons with no text child.
            if (btn == _howToBuildAPC) return Loc.Get("menu_howtobuild");
            if (btn == _optionsBtn)    return Loc.Get("menu_options");
            if (btn == _exitBtn)       return Loc.Get("menu_exit");

            // Career expand-button labels built once at startup — safe dictionary lookup, no runtime reflection.
            if (_careerBtnLabels != null && _careerBtnLabels.TryGetValue(btn, out string cachedLabel))
                return cachedLabel;

            string label = ButtonHelper.GetLabel(btn);

            // If GetLabel fell back to the object name, try CareerModeButton Reflection.
            if (label == btn.gameObject.name)
            {
                string careerLabel = GetCareerModeLabel(btn);
                if (careerLabel != null) return careerLabel;
            }

            return label;
        }

        /// <summary>
        /// Scans all CareerModeButton components under root and maps each m_career Button → localized label.
        /// Done once at startup so GetButtonLabel stays exception-free at runtime.
        /// </summary>
        private static Dictionary<Button, string> BuildCareerBtnLabels(Transform root)
        {
            var map = new Dictionary<Button, string>();
            if (root == null || (object)_careerIdField == null || (object)_careerBtnField == null)
                return map;
            try
            {
                CareerModeButton[] cmbs = root.GetComponentsInChildren<CareerModeButton>(includeInactive: true);
                foreach (CareerModeButton cmb in cmbs)
                {
                    if (cmb == null) continue;
                    Button careerBtn = _careerBtnField.GetValue(cmb) as Button;
                    if (careerBtn == null) continue;
                    CareerID id = (CareerID)_careerIdField.GetValue(cmb);
                    string lbl = null;
                    switch (id)
                    {
                        case CareerID.None:    lbl = Loc.Get("menu_freebuild"); break;
                        case CareerID.Classic: lbl = Loc.Get("menu_career");    break;
                        case CareerID.eSports: lbl = Loc.Get("menu_esports");   break;
                        case CareerID.IT:      lbl = Loc.Get("menu_it");        break;
                    }
                    if (lbl != null)
                        map[careerBtn] = lbl;
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"BuildCareerBtnLabels: {ex.Message}");
            }
            return map;
        }

        private static string GetCareerModeLabel(Button btn)
        {
            if ((object)_careerIdField == null) return null;
            try
            {
                CareerModeButton cmb = btn.GetComponentInParent<CareerModeButton>();
                if (cmb == null) return null;
                CareerID id = (CareerID)_careerIdField.GetValue(cmb);
                switch (id)
                {
                    case CareerID.None:    return Loc.Get("menu_freebuild");
                    case CareerID.Classic: return Loc.Get("menu_career");
                    case CareerID.eSports: return Loc.Get("menu_esports");
                    case CareerID.IT:      return Loc.Get("menu_it");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"GetCareerModeLabel: {ex.Message}");
            }
            return null;
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(MainMenu), "Start")]
        static class MainMenu_Start_Patch
        {
            static void Postfix(MainMenu __instance)
            {
                try { OnMainMenuStart(__instance); }
                catch (Exception ex) { DebugLogger.LogWarning($"MainMenu_Start_Patch: {ex.Message}"); }
                try { if (__instance.optionsMenu != null) OptionsMenuHandler.InitMenu(__instance.optionsMenu); }
                catch (Exception ex) { DebugLogger.LogWarning($"MainMenu_Start_Patch OptionsMenu: {ex.Message}"); }
            }
        }

        #endregion
    }
}
