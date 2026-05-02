using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PCBSAccess
{
    public static class WorkshopNavigatorHandler
    {
        #region Reflected fields

        private static readonly FieldInfo _wcsBaseCase =
            typeof(WorkingOnComputerStateBase).GetField("m_case", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _wcsBaseCamera =
            typeof(WorkingOnComputerStateBase).GetField("m_camera", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _wcsMode =
            typeof(WorkingOnComputerState).GetField("m_mode", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _installAccept =
            typeof(InstallingPartState).GetField("m_accept", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _installDegrees =
            typeof(InstallingPartState).GetField("m_degrees", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _installDir =
            typeof(InstallingPartState).GetField("m_dir", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _installAuto =
            typeof(InstallingPartState).GetField("m_auto", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _installAutoStandoff =
            typeof(InstallingPartState).GetField("m_autoStandoff", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _cableListFrom =
            typeof(ConnectCableState).GetField("m_listFrom", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _pipeFrom =
            typeof(ConnectPipeState).GetField("m_from", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _pipeTo =
            typeof(ConnectPipeState).GetField("m_to", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _hbpcCase =
            typeof(HBPCStateBase).GetField("m_case", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _hbpcCamera =
            typeof(HBPCStateBase).GetField("m_camera", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _hbpcCandidates =
            typeof(HBPCStateBase).GetField("m_candidates", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _hbpcDoClick =
            typeof(HBPCStateBase).GetMethod("DoClick", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _hbpcInvSlot =
            typeof(HBPCInventory).GetField("m_slot", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _hbpcInvVisualInv =
            typeof(HBPCInventory).GetField("m_visualInventory", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _visualInvParts =
            typeof(VisualInventory).GetField("m_parts", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo _hbpcShortPress =
            typeof(HBPCStateBase).GetField("m_shortPress", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region State

        private static bool _active;
        private static State _lastState;
        private static WorkingOnComputerState.Mode _lastMode;
        private static readonly List<UnityEngine.Object> _targets = new List<UnityEngine.Object>();
        private static int _focusIndex = -1;
        private static bool _orientAnnounced;
        private static float _lastHbpcAnnounceTime = -999f;

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "Workshop";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                State s = GetCurrentState();
                if (s == null) return false;

                bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (s is PartInspection)       return HandlePartInspectionInput();
                if (s is HBPCInventory)        return HandleHBPCInventoryInput();
                if (s is HBPCStateBase hbpc)   return HandleHBPCInput(hbpc, shift);
                if (s is InstallingPartState ips) return HandleInstallInput(ips);
                if (s is ConnectCableState ccs)   return HandleCableInput(ccs, shift);
                if (s is ConnectPipeState cps)    return HandlePipeInput(cps, shift);
                if (s is WorkingOnComputerState w) return HandleWCSInput(w, shift);

                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_workshopnav")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API

        public static void Reset()
        {
            _active = false;
            _lastState = null;
            _targets.Clear();
            _focusIndex = -1;
            _orientAnnounced = false;
            _lastHbpcAnnounceTime = -999f;
            InputRouter.Pop(_ctx);
        }

        public static void PollState()
        {
            State current = GetCurrentState();
            bool inWorkshop = IsWorkshopState(current);

            if (inWorkshop && !_active)
            {
                _active = true;
                InputRouter.Push(_ctx);
            }
            else if (!inWorkshop && _active)
            {
                _active = false;
                _targets.Clear();
                _focusIndex = -1;
                _orientAnnounced = false;
                InputRouter.Pop(_ctx);
            }

            if (!_active) return;

            if (current != _lastState)
            {
                _targets.Clear();
                _focusIndex = -1;
                _orientAnnounced = false;
                OnEnterState(current);
            }
            else if (current is WorkingOnComputerState wcs)
            {
                WorkingOnComputerState.Mode mode = GetMode(wcs);
                if (mode != _lastMode)
                {
                    _targets.Clear();
                    _focusIndex = -1;
                    _lastMode = mode;
                    AnnounceMode(mode, GetCase(wcs));
                    RefreshWCSTargets(wcs);
                }
            }

            _lastState = current;

            // Position cursor on HBPC focus target each frame so the game's raycast hits it.
            // (Replaces the removed HBPCStateBase_Tick_Cursor_Patch which had IL compile errors.)
            if (_active && current is HBPCStateBase hbpcCurrent)
                OnBeforeHBPCTick(hbpcCurrent);
        }

        #endregion

        #region State helpers

        private static State GetCurrentState()
        {
            GameController gc = GameController.Get();
            if (gc == null) return null;
            return gc.GetCurrentStateForOneSpecificInstanceWhereINeedItDontUseThisGenerically();
        }

        private static bool IsWorkshopState(State s)
        {
            return s is WorkingOnComputerState
                || s is InstallingPartState
                || s is ConnectCableState
                || s is ConnectPipeState
                || s is HBPCStateBase
                || s is HBPCInventory
                || s is PartInspection;
        }

        private static Case GetCase(State s)     => _wcsBaseCase?.GetValue(s) as Case;
        private static Camera GetCamera(State s) => _wcsBaseCamera?.GetValue(s) as Camera;

        private static Case GetHBPCCase(HBPCStateBase s)     => _hbpcCase?.GetValue(s) as Case;
        private static Camera GetHBPCCamera(HBPCStateBase s) => _hbpcCamera?.GetValue(s) as Camera;

        private static WorkingOnComputerState.Mode GetMode(WorkingOnComputerState s)
        {
            if (_wcsMode == null) return WorkingOnComputerState.Mode.ASSEMBLY;
            return (WorkingOnComputerState.Mode)_wcsMode.GetValue(s);
        }

        private static List<ISelectable> GetHBPCCandidates(HBPCStateBase s)
            => _hbpcCandidates?.GetValue(s) as List<ISelectable>;

        #endregion

        #region State entry

        private static void OnEnterState(State s)
        {
            if (s is WorkingOnComputerState wcs)
            {
                _lastMode = GetMode(wcs);
                AnnounceMode(_lastMode, GetCase(wcs));
                RefreshWCSTargets(wcs);
            }
            else if (s is InstallingPartState ips_enter)
            {
                object rawDir = _installDir?.GetValue(ips_enter);
                float dir = rawDir is float fd ? fd : 1f;
                string msg = dir >= 0f ? Loc.Get("ws_installing_active") : Loc.Get("ws_removing_active");
                ScreenReader.Say(msg + " " + Loc.Get("ws_hbpc_hold_space"));

                // Enable auto-screw so DoScrewsStep doesn't require cursor on screw.
                // Without this, screws never tighten because the blind user can't aim the cursor.
                _installAuto?.SetValue(ips_enter, true);
                _installAutoStandoff?.SetValue(ips_enter, true);
            }
            else if (s is ConnectCableState ccs)
            {
                RefreshCableTargets(ccs);
                ScreenReader.Say(Loc.Get("ws_cable_targets", _targets.Count));
            }
            else if (s is ConnectPipeState cps)
            {
                RefreshPipeTargets(cps);
                ScreenReader.Say(Loc.Get("ws_pipe_targets", _targets.Count));
            }
            else if (s is HBPCInventory hbpcInvEnter)
            {
                RefreshVisualInventoryTargets(hbpcInvEnter);
                AnnounceVisualInventoryOpen(hbpcInvEnter);
            }
            else if (s is PartInspection)
            {
                AnnouncePartInspection();
            }
            else if (s is HBPCStateBase hbpc)
            {
                AnnounceHBPCState(hbpc);
            }
        }

        private static void AnnounceHBPCState(HBPCStateBase hbpc)
        {
            if (HBPCUI.installUI != null && HBPCUI.installUI.gameObject.activeSelf)
            {
                ScreenReader.Say(Loc.Get("ws_hbpc_inv_btn"));
                return;
            }

            // Debounce: HBPC states can transition rapidly; skip if we just announced
            if (Time.time - _lastHbpcAnnounceTime < 1.0f) return;
            _lastHbpcAnnounceTime = Time.time;

            RefreshHBPCTargets(hbpc);

            if (_targets.Count == 0)
            {
                ScreenReader.Say(Loc.Get("ws_hbpc_waiting"));
                return;
            }

            ISelectable first = _targets[0] as ISelectable;
            string action = string.Empty;
            if (first != null)
            {
                try { action = hbpc.GetInteractionName(first) ?? string.Empty; }
                catch { }
            }
            // Sanitize: GetInteractionName can return "[]" for empty collections
            if (action == "[]" || action == "[ ]") action = string.Empty;

            ScreenReader.Say(Loc.Get("ws_hbpc_action", _targets.Count, action));
        }

        private static void AnnounceMode(WorkingOnComputerState.Mode mode, Case theCase)
        {
            switch (mode)
            {
                case WorkingOnComputerState.Mode.ASSEMBLY:
                case WorkingOnComputerState.Mode.COMBO_ASSEMBLY:
                    PartInstance sel = GameController.Get().selectedItem;
                    string name = sel != null ? sel.GetPart().m_uiName : Loc.Get("ws_no_item_short");
                    ScreenReader.Say(Loc.Get("ws_assembly_mode", name));
                    break;
                case WorkingOnComputerState.Mode.DISASSEMBLY:
                case WorkingOnComputerState.Mode.COMBO_DISASSEMBLY:
                    ScreenReader.Say(Loc.Get("ws_disassembly_mode"));
                    break;
                case WorkingOnComputerState.Mode.CABLING:
                    ScreenReader.Say(Loc.Get("ws_cabling_mode"));
                    break;
                case WorkingOnComputerState.Mode.PIPING:
                    ScreenReader.Say(Loc.Get("ws_piping_mode"));
                    break;
                case WorkingOnComputerState.Mode.THERMAL_PASTE:
                    ScreenReader.Say(Loc.Get("ws_thermal_mode"));
                    break;
                case WorkingOnComputerState.Mode.CLEAN:
                    ScreenReader.Say(Loc.Get("ws_clean_mode"));
                    break;
            }
        }

        #endregion

        #region Target refresh

        private static void RefreshWCSTargets(WorkingOnComputerState wcs)
        {
            _targets.Clear();
            Case theCase = GetCase(wcs);
            if (theCase == null) return;
            WorkingOnComputerState.Mode mode = GetMode(wcs);

            switch (mode)
            {
                case WorkingOnComputerState.Mode.ASSEMBLY:
                case WorkingOnComputerState.Mode.COMBO_ASSEMBLY:
                    if (GameController.Get().selectedItem == null) return;
                    Slot[] slots = theCase.GetComponentsInChildren<Slot>();
                    for (int i = 0; i < slots.Length; i++)
                    {
                        if (!slots[i].blocked && slots[i].CanFillWithCurrentItem())
                            _targets.Add(slots[i]);
                    }
                    break;

                case WorkingOnComputerState.Mode.DISASSEMBLY:
                case WorkingOnComputerState.Mode.COMBO_DISASSEMBLY:
                    ComponentPC[] comps = theCase.GetComponentsInChildren<ComponentPC>();
                    for (int i = 0; i < comps.Length; i++)
                    {
                        ComponentPC c = comps[i];
                        if (!c.m_isPlaceholder && c.CanBeUninstalled())
                            _targets.Add(c);
                    }
                    Pin[] pins = theCase.GetComponentsInChildren<Pin>();
                    for (int i = 0; i < pins.Length; i++)
                    {
                        if (!pins[i].IsAnimating())
                            _targets.Add(pins[i]);
                    }
                    break;

                case WorkingOnComputerState.Mode.CABLING:
                    WorkStation wsC = theCase.GetComponentInParent<WorkStation>();
                    if (wsC == null) break;
                    PowerConnector[] pconns = wsC.GetComponentsInChildren<PowerConnector>();
                    for (int i = 0; i < pconns.Length; i++)
                    {
                        PowerConnector pc = pconns[i];
                        if (!pc.IsConnected() && pc.enabled && pc.IsUIStart())
                            _targets.Add(pc);
                    }
                    break;

                case WorkingOnComputerState.Mode.PIPING:
                    WaterConnector[] wconns = theCase.GetComponentsInChildren<WaterConnector>();
                    for (int i = 0; i < wconns.Length; i++)
                    {
                        if (!wconns[i].IsConnected())
                            _targets.Add(wconns[i]);
                    }
                    break;
            }

            DebugLogger.LogState($"WorkshopNav: {_targets.Count} targets, mode={mode}");
        }

        private static void RefreshHBPCTargets(HBPCStateBase hbpc)
        {
            _targets.Clear();
            List<ISelectable> candidates = GetHBPCCandidates(hbpc);
            if (candidates == null) return;
            for (int i = 0; i < candidates.Count; i++)
            {
                UnityEngine.Object obj = candidates[i] as UnityEngine.Object;
                if (obj != null) _targets.Add(obj);
            }
            DebugLogger.LogState($"WorkshopNav HBPC: {_targets.Count} candidates in {hbpc.GetType().Name}");
        }

        private static void RefreshCableTargets(ConnectCableState ccs)
        {
            _targets.Clear();
            Case theCase = GetCase(ccs);
            if (theCase == null) return;
            var listFrom = _cableListFrom?.GetValue(ccs) as List<PowerConnector>;
            if (listFrom == null) return;
            WorkStation ws = theCase.GetComponentInParent<WorkStation>();
            if (ws == null) return;

            PowerConnector[] all = ws.GetComponentsInChildren<PowerConnector>();
            for (int i = 0; i < all.Length; i++)
            {
                PowerConnector pc = all[i];
                if (!pc.enabled || listFrom.Contains(pc)) continue;
                for (int j = 0; j < listFrom.Count; j++)
                {
                    if (!pc.CanConnectTo(listFrom[j])) continue;
                    try
                    {
                        if (theCase.IsCableAvailableBetween(listFrom[j].Remote(), pc.Remote()))
                        { _targets.Add(pc); break; }
                    }
                    catch { _targets.Add(pc); break; }
                }
            }
        }

        private static void RefreshPipeTargets(ConnectPipeState cps)
        {
            _targets.Clear();
            var toList = _pipeTo?.GetValue(cps) as List<WaterConnector>;
            if (toList == null) return;
            for (int i = 0; i < toList.Count; i++)
                _targets.Add(toList[i]);
        }

        #endregion

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_targets.Count == 0)
            {
                ScreenReader.Say(Loc.Get("ws_no_targets"));
                return;
            }
            _focusIndex = (_focusIndex + dir + _targets.Count) % _targets.Count;
            AnnounceTarget(_focusIndex);
        }

        private static void AnnounceTarget(int idx)
        {
            if (idx < 0 || idx >= _targets.Count) return;
            string label = GetTargetLabel(_targets[idx]);
            ScreenReader.Say(Loc.Get("ws_target", idx + 1, _targets.Count, label));
            DebugLogger.LogState($"WorkshopNav: target {idx + 1}/{_targets.Count} '{label}'");
        }

        private static string GetTargetLabel(UnityEngine.Object t)
        {
            if (t is Slot slot)
                return !string.IsNullOrEmpty(slot.partType) ? slot.partType : slot.m_slotType.ToString();

            if (t is ComponentPC comp)
            {
                try { return comp.GetPartInstance().GetPart().m_uiName; }
                catch { return comp.gameObject.name; }
            }

            IPin pin = t as IPin;
            if (pin != null)
            {
                string st = pin.IsOpen() ? Loc.Get("ws_open") : Loc.Get("ws_closed");
                return pin.GetUIName() + st;
            }

            if (t is PowerButton)
                return Loc.Get("ws_power_button");

            if (t is PowerConnector pconn)
            {
                ComponentPC parent = pconn.GetComponentInParent<ComponentPC>();
                if (parent != null)
                {
                    try { return pconn.name + " (" + parent.GetPartInstance().GetPart().m_uiName + ")"; }
                    catch { }
                }
                return pconn.name;
            }

            if (t is WaterConnector wconn)
            {
                ComponentPC parent = wconn.GetComponentInParent<ComponentPC>();
                if (parent != null)
                {
                    try { return wconn.name + " (" + parent.GetPartInstance().GetPart().m_uiName + ")"; }
                    catch { }
                }
                return wconn.name;
            }

            return t.name;
        }

        #endregion

        #region WCS input

        private static bool HandleWCSInput(WorkingOnComputerState wcs, bool shift)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                RefreshWCSTargets(wcs);
                Navigate(shift ? -1 : 1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) && _focusIndex >= 0 && _focusIndex < _targets.Count)
            {
                InteractWCS(_targets[_focusIndex], wcs);
                return true;
            }

            return false;
        }

        private static void InteractWCS(UnityEngine.Object target, WorkingOnComputerState wcs)
        {
            Case theCase = GetCase(wcs);
            Camera theCamera = GetCamera(wcs);
            if (theCase == null || theCamera == null) return;

            try
            {
                if (target is Slot slot)
                {
                    PartInstance item = GameController.Get().selectedItem;
                    if (item == null) { ScreenReader.Say(Loc.Get("ws_no_item")); return; }
                    ScreenReader.Say(Loc.Get("ws_installing", item.GetPart().m_uiName));
                    GameController.Get().SetCurrentState(new InstallingPartState(theCase, theCamera, item, slot));
                }
                else if (target is ComponentPC comp)
                {
                    ComponentPC toRemove = comp.IsInCombo ? comp.LastComponentInCombo : comp;
                    if (!toRemove.CanBeUninstalled()) { ScreenReader.Say(Loc.Get("ws_cant_remove")); return; }
                    ScreenReader.Say(Loc.Get("ws_removing", toRemove.GetPartInstance().GetPart().m_uiName));
                    theCase.OnChangePart(toRemove.gameObject);
                    GameController.Get().SetCurrentState(InstallingPartState.UninstallState(theCase, theCamera, toRemove));
                }
                else if (target is Pin pin)
                {
                    string action = pin.IsOpen() ? Loc.Get("ws_closing") : Loc.Get("ws_opening");
                    ScreenReader.Say(Loc.Get("ws_pin_toggled", pin.GetUIName(), action));
                    pin.TogglePin();
                }
                else if (target is PowerConnector pconn)
                {
                    ScreenReader.Say(Loc.Get("ws_cabling_start", GetTargetLabel(pconn)));
                    GameController.Get().SetCurrentState(new ConnectCableState(theCase, theCamera, pconn));
                }
                else if (target is WaterConnector wconn)
                {
                    ScreenReader.Say(Loc.Get("ws_piping_start", GetTargetLabel(wconn)));
                    GameController.Get().SetCurrentState(new ConnectPipeState(theCase, theCamera, wconn));
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"WorkshopNav.InteractWCS: {ex.Message}");
            }
        }

        #endregion

        #region HBPC input

        private static bool HandleHBPCInput(HBPCStateBase hbpc, bool shift)
        {
            if (HBPCUI.installUI != null && HBPCUI.installUI.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    ScreenReader.Say(Loc.Get("ws_hbpc_going_inv"));
                    ButtonHelper.Click(HBPCUI.installUI.m_installButton);
                    return true;
                }
                return false;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                RefreshHBPCTargets(hbpc);
                Navigate(shift ? -1 : 1);
                return true;
            }

            // Enter: instant action ONLY for short-press states (toggles, latches, thermal paste).
            // Long-press states require Space hold — guide the user.
            if (Input.GetKeyDown(KeyCode.Return))
            {
                RefreshHBPCTargets(hbpc);
                if (_targets.Count == 0) { ScreenReader.Say(Loc.Get("ws_no_targets")); return true; }
                if (_focusIndex < 0 || _focusIndex >= _targets.Count) _focusIndex = 0;

                bool shortPress = _hbpcShortPress?.GetValue(hbpc) is bool b && b;
                ISelectable sel = _targets[_focusIndex] as ISelectable;
                if (sel == null) return false;

                if (shortPress)
                {
                    try
                    {
                        string action = string.Empty;
                        try { action = hbpc.GetInteractionName(sel); } catch { }
                        if (!string.IsNullOrEmpty(action)) ScreenReader.Say(action);
                        _hbpcDoClick?.Invoke(hbpc, new object[] { sel });
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.LogWarning($"WorkshopNav.HandleHBPCInput short: {ex.Message}");
                    }
                }
                else
                {
                    ScreenReader.Say(Loc.Get("ws_hbpc_hold_space"));
                }
                return true;
            }

            // Space: announce target then start the long-press directly.
            // We call DoAfterLongPress ourselves — bypasses the timing dependency on
            // HBPCStateBase.Tick()'s IsDown() which is only true for one frame.
            // The DoOnLongPressCR coroutine monitors IsHeld() which stays true via our
            // InputButton.Get() patch while Space is held.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RefreshHBPCTargets(hbpc);
                if (_targets.Count == 0) { ScreenReader.Say(Loc.Get("ws_no_targets")); return true; }
                if (_focusIndex < 0 || _focusIndex >= _targets.Count) _focusIndex = 0;

                ISelectable sel = _targets[_focusIndex] as ISelectable;
                if (sel == null) return true;

                string action = string.Empty;
                try { action = hbpc.GetInteractionName(sel) ?? string.Empty; } catch { }
                if (action == "[]" || action == "[ ]") action = string.Empty;
                if (!string.IsNullOrEmpty(action))
                    ScreenReader.Say(action);

                bool shortPress = _hbpcShortPress?.GetValue(hbpc) is bool sp && sp;
                if (shortPress)
                {
                    // Short-press: instant, no hold — fire immediately.
                    try { _hbpcDoClick?.Invoke(hbpc, new object[] { sel }); }
                    catch (Exception ex) { DebugLogger.LogWarning($"WorkshopNav.HBPC Space short: {ex.Message}"); }
                }
                else
                {
                    // Long-press: InstallingPartState uses DoAfterLongPress internally which checks
                    // m_held. Our Main.Update runs at order -100; InputButton.Update runs at order 0.
                    // On the Space-press frame m_held is still false → coroutine exits immediately.
                    // Delay DoClick by one frame so InputButton.Update sets m_held=true first.
                    HBPCStateBase capturedHbpc = hbpc;
                    ISelectable capturedSel = sel;
                    if (Main.Instance != null)
                        Main.Instance.StartCoroutine(DelayedHBPCDoClick(capturedHbpc, capturedSel));
                }
                return true;
            }

            return false;
        }

        private static System.Collections.IEnumerator DelayedHBPCDoClick(HBPCStateBase hbpc, ISelectable sel)
        {
            // Wait one frame so InputButton.Update() (order 0) sets m_held=true before
            // InstallingPartState's internal DoAfterLongPress checks IsHeld().
            yield return null;
            if (!Input.GetKey(KeyCode.Space)) yield break; // user released — cancel
            try { _hbpcDoClick?.Invoke(hbpc, new object[] { sel }); }
            catch (Exception ex) { DebugLogger.LogWarning($"WorkshopNav.HBPC delayed: {ex.Message}"); }
        }

        private static bool HandleHBPCInventoryInput()
        {
            HBPCInventory hbpcInv = GetCurrentState() as HBPCInventory;
            if (hbpcInv == null) return false;

            Slot slot = _hbpcInvSlot?.GetValue(hbpcInv) as Slot;

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Tab))
            {
                if (_targets.Count == 0) RefreshVisualInventoryTargets(hbpcInv);
                if (_targets.Count == 0) { ScreenReader.Say(Loc.Get("inv_empty")); return true; }
                if (_focusIndex < 0)
                    _focusIndex = 0;
                else if (_focusIndex < _targets.Count - 1)
                    _focusIndex++;
                else
                {
                    ScreenReader.Say(Loc.Get("nav_last_item"));
                    return true;
                }
                AnnounceVisualInvItem(slot, _focusIndex);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_targets.Count == 0) RefreshVisualInventoryTargets(hbpcInv);
                if (_targets.Count == 0) { ScreenReader.Say(Loc.Get("inv_empty")); return true; }
                if (_focusIndex <= 0)
                {
                    _focusIndex = 0;
                    ScreenReader.Say(Loc.Get("nav_first_item"));
                    return true;
                }
                _focusIndex--;
                AnnounceVisualInvItem(slot, _focusIndex);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (_targets.Count == 0) RefreshVisualInventoryTargets(hbpcInv);
                if (_targets.Count > 0) { _focusIndex = 0; AnnounceVisualInvItem(slot, _focusIndex); }
                return true;
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                if (_targets.Count == 0) RefreshVisualInventoryTargets(hbpcInv);
                if (_targets.Count > 0) { _focusIndex = _targets.Count - 1; AnnounceVisualInvItem(slot, _focusIndex); }
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (_targets.Count == 0 || _focusIndex < 0)
                {
                    ScreenReader.Say(Loc.Get("ws_hbpc_inv_none"));
                    return true;
                }
                ComponentPC comp = _targets[_focusIndex] as ComponentPC;
                if (comp == null) return true;

                VisualInventory vi = _hbpcInvVisualInv?.GetValue(hbpcInv) as VisualInventory;
                if (vi?.m_camera == null) return true;

                bool fits;
                try { fits = slot != null && slot.FitsSlot(comp.GetPartInstance().GetPart()); }
                catch { fits = false; }

                try
                {
                    GameController.Get().SetCurrentState(new PartInspection(comp, vi.m_camera, fits));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkshopNav.HBPCInvSelect: {ex.Message}");
                }
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                ScreenReader.Say(Loc.Get("ws_hbpc_inv_back"));
                try { GameController.Get().GoToPreviousState(); }
                catch (Exception ex) { DebugLogger.LogWarning($"WorkshopNav.HBPCInvBack: {ex.Message}"); }
                return true;
            }

            return false;
        }

        private static bool HandlePartInspectionInput()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (HBPCUI.inspectionUI != null
                    && HBPCUI.inspectionUI.m_use != null
                    && HBPCUI.inspectionUI.m_use.gameObject.activeSelf)
                {
                    ScreenReader.Say(Loc.Get("ws_hbpc_installing"));
                    ButtonHelper.Click(HBPCUI.inspectionUI.m_use);
                }
                else
                {
                    ScreenReader.Say(Loc.Get("ws_hbpc_not_compat"));
                }
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                ScreenReader.Say(Loc.Get("ws_hbpc_inv_back"));
                try { GameController.Get().GoToPreviousState(); }
                catch (Exception ex) { DebugLogger.LogWarning($"WorkshopNav.PartInspBack: {ex.Message}"); }
                return true;
            }

            return false;
        }

        #endregion

        #region Visual inventory helpers

        private static void RefreshVisualInventoryTargets(HBPCInventory hbpcInv)
        {
            _targets.Clear();
            VisualInventory vi = _hbpcInvVisualInv?.GetValue(hbpcInv) as VisualInventory;
            if (vi == null) return;
            var parts = _visualInvParts?.GetValue(vi) as System.Collections.Generic.List<ComponentPC>;
            if (parts == null) return;

            Slot slot = _hbpcInvSlot?.GetValue(hbpcInv) as Slot;

            // Compatible parts first so Tab lands on them first
            var compat   = new System.Collections.Generic.List<ComponentPC>();
            var incompat = new System.Collections.Generic.List<ComponentPC>();
            for (int i = 0; i < parts.Count; i++)
            {
                ComponentPC p = parts[i];
                if (p == null) continue;
                bool fits;
                try { fits = slot != null && slot.FitsSlot(p.GetPartInstance().GetPart()); }
                catch { fits = false; }
                if (fits) compat.Add(p); else incompat.Add(p);
            }
            for (int i = 0; i < compat.Count;   i++) _targets.Add(compat[i]);
            for (int i = 0; i < incompat.Count; i++) _targets.Add(incompat[i]);

            DebugLogger.LogState($"WorkshopNav VisualInv: {_targets.Count} parts ({compat.Count} compatible)");
        }

        private static void AnnounceVisualInventoryOpen(HBPCInventory hbpcInv)
        {
            Slot slot = _hbpcInvSlot?.GetValue(hbpcInv) as Slot;
            int compatible = 0;
            for (int i = 0; i < _targets.Count; i++)
            {
                ComponentPC comp = _targets[i] as ComponentPC;
                if (comp == null) continue;
                bool fits;
                try { fits = slot != null && slot.FitsSlot(comp.GetPartInstance().GetPart()); }
                catch { fits = false; }
                if (fits) compatible++;
            }

            ScreenReader.Say(Loc.Get("ws_hbpc_visual_inv", _targets.Count, compatible));

            if (_targets.Count > 0)
            {
                _focusIndex = 0;
                AnnounceVisualInvItem(slot, _focusIndex);
            }
        }

        private static void AnnounceVisualInvItem(Slot slot, int idx)
        {
            if (idx < 0 || idx >= _targets.Count) return;
            ComponentPC comp = _targets[idx] as ComponentPC;
            if (comp == null) return;

            string name;
            try { name = comp.GetPartInstance().GetPart().m_uiName; }
            catch { name = comp.gameObject.name; }

            bool fits;
            try { fits = slot != null && slot.FitsSlot(comp.GetPartInstance().GetPart()); }
            catch { fits = false; }

            string compat = fits ? Loc.Get("ws_hbpc_compatible") : Loc.Get("ws_hbpc_not_compat");
            ScreenReader.Say(Loc.Get("ws_hbpc_inv_item", idx + 1, _targets.Count, name, compat));
        }

        private static void AnnouncePartInspection()
        {
            if (HBPCUI.inspectionUI == null) return;
            string name = HBPCUI.inspectionUI.m_name?.text ?? string.Empty;
            bool canUse = HBPCUI.inspectionUI.m_use != null
                       && HBPCUI.inspectionUI.m_use.gameObject.activeSelf;
            string hint = canUse ? Loc.Get("ws_hbpc_inspect_use") : Loc.Get("ws_hbpc_not_compat");
            ScreenReader.Say(Loc.Get("ws_hbpc_inspect_open", name, hint));
        }

        #endregion

        #region Install/orient input

        private static bool HandleInstallInput(InstallingPartState ips)
        {
            // Uninstalling (m_dir < 0) never needs orientation — skip entirely
            object rawDir = _installDir?.GetValue(ips);
            if (rawDir is float dir && dir < 0f) return false;

            bool inOrient = CommonUI.RotationIndicator != null && CommonUI.RotationIndicator.activeSelf;

            if (inOrient && !_orientAnnounced)
            {
                _orientAnnounced = true;
                ScreenReader.Say(Loc.Get("ws_orient_hint"));
            }

            if (!inOrient) return false;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                _installAccept?.SetValue(ips, true);
                ScreenReader.Say(Loc.Get("ws_orient_accept"));
                return true;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                object raw = _installDegrees?.GetValue(ips);
                float cur = raw is float f ? f : 0f;
                float next = Mathf.Abs(cur) < 90f ? 180f : 0f;
                _installDegrees?.SetValue(ips, next);
                _installAccept?.SetValue(ips, true);
                ScreenReader.Say(Loc.Get("ws_orient_flip", (int)next));
                return true;
            }

            // Left/Right: continuous rotation — 90 deg/s so 2 seconds = quarter turn.
            bool rotLeft  = Input.GetKey(KeyCode.LeftArrow);
            bool rotRight = Input.GetKey(KeyCode.RightArrow);
            if (rotLeft || rotRight)
            {
                object raw = _installDegrees?.GetValue(ips);
                float cur = raw is float fc ? fc : 0f;
                float delta = 90f * UnityEngine.Time.deltaTime * (rotLeft ? 1f : -1f);
                float next = cur + delta;
                _installDegrees?.SetValue(ips, next);
                _installAccept?.SetValue(ips, false); // user is still choosing angle

                // Announce angle only when key first pressed (GetKeyDown)
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                    ScreenReader.Say(Loc.Get("ws_orient_angle", (int)Mathf.Round(next)));
                return true;
            }

            return false;
        }

        #endregion

        #region Cable/pipe input

        private static bool HandleCableInput(ConnectCableState ccs, bool shift)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Navigate(shift ? -1 : 1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (_focusIndex < 0 || _focusIndex >= _targets.Count) return false;
                PowerConnector to = _targets[_focusIndex] as PowerConnector;
                if (to == null) return false;

                Case theCase = GetCase(ccs);
                var listFrom = _cableListFrom?.GetValue(ccs) as List<PowerConnector>;
                if (theCase == null || listFrom == null) return false;

                try
                {
                    PowerConnector from = null;
                    for (int i = 0; i < listFrom.Count; i++)
                    {
                        PowerConnector f = listFrom[i];
                        if (!to.CanConnectTo(f)) continue;
                        try { if (!theCase.IsCableAvailableBetween(f.Remote(), to.Remote())) continue; } catch { }
                        from = f;
                        break;
                    }

                    if (from == null) { ScreenReader.Say(Loc.Get("ws_cant_connect")); return true; }
                    theCase.CreateCable(GameController.Get().GetCableType(), from, to);
                    ScreenReader.Say(Loc.Get("ws_connected"));
                    GameController.Get().GoToPreviousState();
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkshopNav.HandleCableInput: {ex.Message}");
                }
                return true;
            }

            return false;
        }

        private static bool HandlePipeInput(ConnectPipeState cps, bool shift)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Navigate(shift ? -1 : 1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (_focusIndex < 0 || _focusIndex >= _targets.Count) return false;
                WaterConnector to = _targets[_focusIndex] as WaterConnector;
                if (to == null) return false;

                Case theCase = GetCase(cps);
                Camera theCamera = GetCamera(cps);
                WaterConnector from = _pipeFrom?.GetValue(cps) as WaterConnector;
                if (theCase == null || theCamera == null || from == null) return false;

                try
                {
                    ScreenReader.Say(Loc.Get("ws_connected"));
                    GameController.Get().GoToPreviousState();
                    GameController.Get().SetCurrentState(new RoutePipeState(theCase, theCamera, from, to));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"WorkshopNav.HandlePipeInput: {ex.Message}");
                }
                return true;
            }

            return false;
        }

        #endregion

        #region Space-as-mouse-hold injection

        /// <summary>Returns true when Space should simulate the game's action button (left mouse hold).</summary>
        public static bool IsActionInjecting()
        {
            if (!Input.GetKey(KeyCode.Space)) return false;
            if (!_active) return false;
            // Never inject during orientation — Enter handles that separately
            if (CommonUI.RotationIndicator != null && CommonUI.RotationIndicator.activeSelf) return false;
            State s = GetCurrentState();
            return s is HBPCStateBase || s is InstallingPartState;
        }

        /// <summary>Called as Prefix on HBPCStateBase.Tick — positions cursor on focused target before raycast.</summary>
        public static void OnBeforeHBPCTick(HBPCStateBase hbpc)
        {
            if (!_active || _focusIndex < 0 || _focusIndex >= _targets.Count) return;
            if (!Input.GetKey(KeyCode.Space)) return;

            MonoBehaviour mb = _targets[_focusIndex] as MonoBehaviour;
            Camera cam = GetHBPCCamera(hbpc);
            if (mb == null || cam == null) return;

            Vector3 sp = cam.WorldToScreenPoint(mb.transform.position);
            if (sp.z > 0)
                InputModule.SetLocalCursorPos(new Vector2(sp.x, sp.y));
        }

        /// <summary>Harmony patch: makes PCBSInput.m_action.Get() return 1f (held) when Space is injecting.</summary>
        [HarmonyLib.HarmonyPatch(typeof(InputButton), "Get")]
        static class InputButton_Get_Inject_Patch
        {
            static bool Prefix(InputButton __instance, ref float __result)
            {
                if (__instance == PCBSInput.m_action && IsActionInjecting())
                {
                    __result = 1f;
                    return false;
                }
                return true;
            }
        }

        // HBPCStateBase.Tick() contains LINQ lambdas that cause HarmonyX IL compile errors.
        // OnBeforeHBPCTick is now called directly from PollState() every frame instead.

        #endregion
    }
}
