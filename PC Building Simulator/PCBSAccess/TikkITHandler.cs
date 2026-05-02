using System;
using System.Collections.Generic;
using System.Reflection;
using FuturLab;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the IT Support DLC TikkIT Kanban board.
    ///
    /// Board view:
    ///   On open: announces column summary (N jobs per column).
    ///   Left/Right: cycle columns (Backlog → In Progress → Blocked → Completed → Nightshift).
    ///   Up/Down: navigate cards within the current column.
    ///   Home/End: first/last card in current column.
    ///   Enter: open the focused card detail popup.
    ///   F1: re-read focused card.
    ///
    /// Card popup:
    ///   Announced automatically on open: staff name, role, subject, request, objectives,
    ///   payment, budget, and available actions.
    ///   Enter: positive action (Accept/Collect).
    ///   Escape: close popup (back to board).
    ///   N: negative action (Decline/Discard).
    ///
    /// New job notification: "New IT job: [subject] from [name]."
    /// </summary>
    public static class TikkITHandler
    {
        #region Reflection

        private static readonly FieldInfo _fBoard =
            typeof(TabletAppTikkIT).GetField("m_tikkITBoard",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fNameText =
            typeof(TabletTikkITCard).GetField("m_nameText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fSubjectText =
            typeof(TabletTikkITCard).GetField("m_subjectText",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _fPopupNameText =
            typeof(TabletTikkITCardPopup).GetField("m_nameText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupSubjectText =
            typeof(TabletTikkITCardPopup).GetField("m_subjectText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupRequestText =
            typeof(TabletTikkITCardPopup).GetField("m_requestText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupObjectivesText =
            typeof(TabletTikkITCardPopup).GetField("m_objectivesText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupLabourText =
            typeof(TabletTikkITCardPopup).GetField("m_labourText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupBudgetText =
            typeof(TabletTikkITCardPopup).GetField("m_budgetText",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupPositiveBtn =
            typeof(TabletTikkITCardPopup).GetField("m_positiveActionButton",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupNegativeBtn =
            typeof(TabletTikkITCardPopup).GetField("m_negativeActionButton",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _fPopupCurrentCard =
            typeof(TabletTikkITCardPopup).GetField("m_currentCard",
                BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region State

        private static TabletTikkITBoard _board;
        private static TabletTikkITCardPopup _popup;
        private static bool _boardActive;
        private static bool _popupActive;

        // Board navigation
        private static int _colIndex;   // 0=Backlog 1=InProgress 2=Blocked 3=Completed 4=Nightshift
        private static int _cardIndex;
        private static readonly string[] _colNames = { "Backlog", "In Progress", "Blocked", "Completed", "Nightshift" };

        #endregion

        #region IInputContext — Board

        private sealed class BoardContext : IInputContext, IHelpContext
        {
            public string ContextName => "TikkITBoard";
            public bool IsActive => _boardActive && !_popupActive;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.RightArrow)) { CycleColumn(1);   return true; }
                if (Input.GetKeyDown(KeyCode.LeftArrow))  { CycleColumn(-1);  return true; }
                if (Input.GetKeyDown(KeyCode.DownArrow))  { NavigateCard(1);  return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))    { NavigateCard(-1); return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateCard(-999); return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateCard(999);  return true; }
                if (Input.GetKeyDown(KeyCode.Return))     { OpenFocusedCard();  return true; }
                if (Input.GetKeyDown(KeyCode.F1))         { ReReadCard();       return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("tikkit_board_help"));
            }
        }

        #endregion

        #region IInputContext — Popup

        private sealed class PopupContext : IInputContext, IHelpContext
        {
            public string ContextName => "TikkITPopup";
            public bool IsActive => _popupActive;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.Return))     { PopupPositive();  return true; }
                if (Input.GetKeyDown(KeyCode.N))          { PopupNegative();  return true; }
                if (Input.GetKeyDown(KeyCode.Escape))     { ClosePopup();     return true; }
                if (Input.GetKeyDown(KeyCode.F1))         { ReReadPopup();    return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("tikkit_popup_help"));
            }
        }

        private static readonly BoardContext _boardCtx = new BoardContext();
        private static readonly PopupContext _popupCtx = new PopupContext();

        #endregion

        #region Patches

        [HarmonyPatch(typeof(TabletAppTikkIT), "OnStartShowing")]
        static class TabletAppTikkIT_OnStartShowing_Patch
        {
            static void Postfix(TabletAppTikkIT __instance)
            {
                try
                {
                    _board = _fBoard?.GetValue(__instance) as TabletTikkITBoard;
                    if (_board == null) return;

                    // Find the popup reference via the board.
                    _popup = _board.GetComponentInChildren<TabletTikkITCardPopup>(includeInactive: true);

                    _boardActive  = true;
                    _popupActive  = false;
                    _colIndex     = 0;
                    _cardIndex    = 0;

                    InputRouter.Push(_boardCtx);
                    AnnounceBoardSummary();
                    DebugLogger.LogState("TikkITHandler: board opened");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TikkITHandler.OnStartShowing: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(TabletAppTikkIT), "OnClosed")]
        static class TabletAppTikkIT_OnClosed_Patch
        {
            static void Postfix()
            {
                try
                {
                    CloseAll();
                    DebugLogger.LogState("TikkITHandler: board closed");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TikkITHandler.OnClosed: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(TabletTikkITBoard), "OnCardOpened")]
        static class TabletTikkITBoard_OnCardOpened_Patch
        {
            static void Postfix(TabletTikkITCard _card)
            {
                try
                {
                    if (_card == null) return;
                    _popupActive = true;
                    InputRouter.Push(_popupCtx);

                    if (_popup == null && _board != null)
                        _popup = _board.GetComponentInChildren<TabletTikkITCardPopup>(includeInactive: false);

                    AnnouncePopup(_card);
                    DebugLogger.LogState($"TikkITHandler: card opened '{_card.job?.GetSubject()}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TikkITHandler.OnCardOpened: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(TabletTikkITBoard), "OnCardClosed")]
        static class TabletTikkITBoard_OnCardClosed_Patch
        {
            static void Postfix()
            {
                try
                {
                    DismissPopup();
                    ScreenReader.Say(Loc.Get("tikkit_popup_closed"));
                    DebugLogger.LogState("TikkITHandler: popup closed");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TikkITHandler.OnCardClosed: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(TabletTikkITBoard), "CreateNewCard")]
        static class TabletTikkITBoard_CreateNewCard_Patch
        {
            static void Postfix(ITSupportJob _job)
            {
                try
                {
                    if (_job == null) return;
                    string name    = _job.GetStaffNameLocalised();
                    string subject = _job.GetSubject();
                    ScreenReader.Say(Loc.Get("tikkit_new_job", subject, name));
                    DebugLogger.LogState($"TikkITHandler: new card '{subject}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"TikkITHandler.CreateNewCard: {ex.Message}");
                }
            }
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            CloseAll();
        }

        #endregion

        #region Board navigation

        private static void CycleColumn(int dir)
        {
            _colIndex  = (_colIndex + dir + 5) % 5;
            _cardIndex = 0;
            AnnounceColumn();
        }

        private static void NavigateCard(int dir)
        {
            List<TabletTikkITCard> cards = GetCurrentColumnCards();
            if (cards.Count == 0)
            {
                ScreenReader.Say(Loc.Get("tikkit_col_empty", _colNames[_colIndex]));
                return;
            }

            int next = Mathf.Clamp(_cardIndex + dir, 0, cards.Count - 1);
            if (dir < -1)  next = 0;                 // Home
            if (dir > 1)   next = cards.Count - 1;   // End

            if (next == _cardIndex && Math.Abs(dir) == 1)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _cardIndex = next;
            AnnounceCard(cards[_cardIndex], _cardIndex + 1, cards.Count);
        }

        private static void OpenFocusedCard()
        {
            if (_board == null) return;
            List<TabletTikkITCard> cards = GetCurrentColumnCards();
            if (cards.Count == 0 || _cardIndex >= cards.Count) return;
            _board.OnCardOpened(cards[_cardIndex]);
        }

        private static void ReReadCard()
        {
            List<TabletTikkITCard> cards = GetCurrentColumnCards();
            if (cards.Count == 0)
            {
                ScreenReader.Say(Loc.Get("tikkit_col_empty", _colNames[_colIndex]));
                return;
            }
            _cardIndex = Mathf.Clamp(_cardIndex, 0, cards.Count - 1);
            AnnounceCard(cards[_cardIndex], _cardIndex + 1, cards.Count);
        }

        private static List<TabletTikkITCard> GetCurrentColumnCards()
        {
            var result = new List<TabletTikkITCard>();
            if (_board == null) return result;
            try
            {
                TabletTikkITList list = _board.lists[_colIndex];
                foreach (Transform child in list.cardContainer)
                {
                    TabletTikkITCard card = child.GetComponent<TabletTikkITCard>();
                    if (card != null && card.gameObject.activeSelf)
                        result.Add(card);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.GetCurrentColumnCards: {ex.Message}");
            }
            return result;
        }

        #endregion

        #region Popup actions

        private static void PopupPositive()
        {
            try
            {
                if (_popup == null) return;
                Button btn = _fPopupPositiveBtn?.GetValue(_popup) as Button;
                if (btn != null && btn.IsInteractable() && btn.gameObject.activeSelf)
                    btn.onClick.Invoke();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.PopupPositive: {ex.Message}");
            }
        }

        private static void PopupNegative()
        {
            try
            {
                if (_popup == null) return;
                Button btn = _fPopupNegativeBtn?.GetValue(_popup) as Button;
                if (btn != null && btn.IsInteractable() && btn.gameObject.activeSelf)
                    btn.onClick.Invoke();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.PopupNegative: {ex.Message}");
            }
        }

        private static void ClosePopup()
        {
            if (_board != null)
                _board.OnCardClosed();
        }

        private static void DismissPopup()
        {
            _popupActive = false;
            InputRouter.Pop(_popupCtx);
        }

        private static void ReReadPopup()
        {
            try
            {
                if (_popup == null) return;
                TabletTikkITCard card = _fPopupCurrentCard?.GetValue(_popup) as TabletTikkITCard;
                if (card != null) AnnouncePopup(card);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.ReReadPopup: {ex.Message}");
            }
        }

        #endregion

        #region Announce helpers

        private static void AnnounceBoardSummary()
        {
            if (_board == null) return;
            try
            {
                int total = 0;
                var parts = new System.Text.StringBuilder();
                for (int i = 0; i < _board.lists.Length; i++)
                {
                    int count = 0;
                    foreach (Transform child in _board.lists[i].cardContainer)
                    {
                        if (child.GetComponent<TabletTikkITCard>() != null) count++;
                    }
                    total += count;
                    if (count > 0)
                    {
                        if (parts.Length > 0) parts.Append(", ");
                        parts.Append($"{count} {_colNames[i]}");
                    }
                }
                string summary = parts.Length > 0 ? parts.ToString() : Loc.Get("tikkit_no_jobs");
                ScreenReader.Say(Loc.Get("tikkit_board_open", total, summary));
                ScreenReader.Say(Loc.Get("tikkit_board_hint"), interrupt: false);

                // Auto-focus first column with cards.
                for (int i = 0; i < _board.lists.Length; i++)
                {
                    int count = 0;
                    foreach (Transform child in _board.lists[i].cardContainer)
                        if (child.GetComponent<TabletTikkITCard>() != null) count++;
                    if (count > 0) { _colIndex = i; break; }
                }
                AnnounceColumn();
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.AnnounceBoardSummary: {ex.Message}");
            }
        }

        private static void AnnounceColumn()
        {
            List<TabletTikkITCard> cards = GetCurrentColumnCards();
            string colName = _colNames[_colIndex];
            if (cards.Count == 0)
            {
                ScreenReader.Say(Loc.Get("tikkit_col_empty", colName));
                return;
            }
            ScreenReader.Say(Loc.Get("tikkit_col_selected", colName, cards.Count));
            _cardIndex = Mathf.Clamp(_cardIndex, 0, cards.Count - 1);
            AnnounceCard(cards[_cardIndex], _cardIndex + 1, cards.Count);
        }

        private static void AnnounceCard(TabletTikkITCard card, int pos, int total)
        {
            try
            {
                string name    = ((_fNameText?.GetValue(card)    as Text)?.text) ?? string.Empty;
                string subject = ((_fSubjectText?.GetValue(card) as Text)?.text) ?? string.Empty;
                string status  = card.job?.GetStatus().ToString() ?? string.Empty;
                string isReady = card.job?.GetStatus() == Job.Status.FINISHED
                    ? Loc.Get("tikkit_ready") : string.Empty;

                string msg = $"{pos} {Loc.Get("nav_of")} {total}: {name}. {subject}.";
                if (!string.IsNullOrEmpty(isReady)) msg += $" {isReady}.";
                ScreenReader.Say(msg);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.AnnounceCard: {ex.Message}");
            }
        }

        private static void AnnouncePopup(TabletTikkITCard card)
        {
            if (_popup == null) return;
            try
            {
                string name       = (_fPopupNameText?.GetValue(_popup)       as Text)?.text ?? string.Empty;
                string subject    = (_fPopupSubjectText?.GetValue(_popup)    as Text)?.text ?? string.Empty;
                string request    = (_fPopupRequestText?.GetValue(_popup)    as Text)?.text ?? string.Empty;
                string objectives = (_fPopupObjectivesText?.GetValue(_popup) as Text)?.text ?? string.Empty;
                string labour     = (_fPopupLabourText?.GetValue(_popup)     as Text)?.text ?? string.Empty;
                string budget     = (_fPopupBudgetText?.GetValue(_popup)     as Text)?.text ?? string.Empty;

                ScreenReader.Say($"{name}. {subject}.");
                if (!string.IsNullOrEmpty(request))    ScreenReader.Say(request,    interrupt: false);
                if (!string.IsNullOrEmpty(objectives)) ScreenReader.Say(objectives, interrupt: false);
                if (!string.IsNullOrEmpty(labour))     ScreenReader.Say(labour,     interrupt: false);
                if (!string.IsNullOrEmpty(budget))     ScreenReader.Say(budget,     interrupt: false);

                // Announce available actions.
                Button posBtn = _fPopupPositiveBtn?.GetValue(_popup) as Button;
                Button negBtn = _fPopupNegativeBtn?.GetValue(_popup) as Button;
                string posText = (posBtn != null && posBtn.gameObject.activeSelf)
                    ? posBtn.GetComponentInChildren<Text>()?.text : null;
                string negText = (negBtn != null && negBtn.gameObject.activeSelf)
                    ? negBtn.GetComponentInChildren<Text>()?.text : null;

                string actions = Loc.Get("tikkit_popup_actions",
                    string.IsNullOrEmpty(posText) ? Loc.Get("tikkit_none") : posText,
                    string.IsNullOrEmpty(negText) ? Loc.Get("tikkit_none") : negText);
                ScreenReader.Say(actions, interrupt: false);
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"TikkITHandler.AnnouncePopup: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        private static void CloseAll()
        {
            _boardActive = false;
            _popupActive = false;
            _board       = null;
            _popup       = null;
            _colIndex    = 0;
            _cardIndex   = 0;
            InputRouter.Pop(_boardCtx);
            InputRouter.Pop(_popupCtx);
        }

        #endregion
    }
}
