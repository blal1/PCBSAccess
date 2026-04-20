# Project Status: PCBSAccess

## Project Info

- **Game:** PC Building Simulator
- **Developer:** The Irregular Corporation
- **Engine:** Unity 2018.4.16f1 (Mono runtime)
- **Architecture:** 64-bit
- **Mod Loader:** BepInEx 5.x x64
- **Runtime:** net472
- **Game directory:** C:\Users\bilal\Downloads\PC Building Simulator
- **Decompiled source:** PCBS_Data\Managed\ (already decompiled by ILSpy — NOT a separate decompiled/ folder)
- **User experience level:** Little/None
- **User game familiarity:** Not at all
- **Languages:** English + French

## Setup Progress

- [x] Experience level determined
- [x] Game name and path confirmed
- [x] Game familiarity assessed
- [x] Game directory auto-check completed
- [x] Mod loader selected and installed — BepInEx 5.x x64
- [x] Tolk DLLs in place — Tolk.dll + nvdaControllerClient64.dll
- [x] .NET SDK available — 10.0.201
- [x] Decompiler tool ready — ILSpy CLI 10.0.0.8330
- [x] Game code decompiled — in PCBS_Data\Managed\
- [x] Tutorial texts extracted — docs/tutorial-texts.md created. Also found: TutorialUI.Init() Harmony postfix reads live localized text from m_title.text + m_body.text directly — no AssetBundle work needed.
- [x] Multilingual support decided — English + French
- [x] Project directory set up — PCBSAccess\ with csproj + 6 framework files
- [ ] CLAUDE.md updated with project-specific values
- [x] First build successful — 0 warnings, 0 errors
- [x] "Mod loaded" announcement working in game — F1 and F12 confirmed working

## Current Phase

**Phase:** Phase 3 — Feature Implementation
**Currently working on:** Framework complete. Next feature: HowToBuildAPC handler (Tutorial mode reader).
**Blocked by:** Nothing

## Codebase Analysis Progress

### GATE: Tier 1 MUST be complete before Phase 2 (Framework)!

- [x] 1.1 Structure overview (namespaces, singletons) → documented in game-api.md
- [x] 1.2 Input system — ALL game key bindings documented in game-api.md "Game Key Bindings"
- [x] 1.2 Input system — Safe mod keys identified and listed in game-api.md "Safe Mod Keys"
- [x] 1.3 UI system (base classes, text access patterns, Reflection needed?)
- [x] 1.4 State management decision → documented in "Architecture Decisions" below
- [x] 1.5 Localization: game's language system analyzed (multilingual mod planned)

### GATE: Relevant Tier 2 items MUST be done before implementing each feature!

- [ ] 1.6 Game mechanics (analyzed as needed per feature)
- [ ] 1.7 Status/feedback systems
- [ ] 1.8 Event system / Harmony patch points
- [ ] 1.9 Results documented in `docs/game-api.md`
- [ ] 1.10 Tutorial analysis — important! User does not know the game

## Game Key Bindings (Original)

<!-- CRITICAL: Fill this during Tier 1 analysis! Every key the game uses.
Without this list, mod keys WILL conflict with game controls. -->

- (not yet documented — MUST be done before Phase 2)

## Implemented Features

- **Phase 2 Framework** — ScreenReader (Tolk), DebugLogger, Loc (EN+FR), AccessStateManager, ReflectionHelper, Main (F1 career status, F12 debug toggle). Build script: scripts\Build-Mod.ps1.

## Pending Tests

- (none yet)

## Known Issues

- (none yet)

## Architecture Decisions

- Decompiled source is in PCBS_Data\Managed\Assembly-CSharp-firstpass\ (NOT Assembly-CSharp\ which only has shaders). Always search firstpass.
- Community uses PCBSModloader (custom patching), but we use BepInEx 5.x for better template support and cleaner injection.
- **State management: Use AccessStateManager.** Many handlers share arrow keys and Enter (main menu, building mode, inventory, job status). 3+ handlers confirmed.
- **Reflection required:** 1,119 [SerializeField] occurrences across 228 files. Almost all UI fields are private. Create ReflectionHelper in Phase 2.
- **Text components: Mixed.** Most UI classes use `UnityEngine.UI.Text`. Some (InputModule) use `TextMeshProUGUI`. Check `using` statements per class.
- **Input system: Rewired.** All game input via `PCBSInput.*` actions (Rewired named actions). Only use `Input.GetKeyDown(KeyCode.F*)` for our own mod keys.
- **Localization: I2 Localization.** `LocalizationManager.CurrentLanguage` returns "English" or "French". Use this in Loc.cs initialization.

## Key Bindings (Mod)

- F12: Toggle debug mode
<!-- Add more as features are implemented — check game-api.md Safe Mod Keys first! -->

## Notes for Next Session

- Phase 2 framework complete and tested in-game.
- Next: Phase 3 Feature 1 — HowToBuildAPC handler. Need Tier 2 analysis of HBPCState before implementing.
- CareerStatus correction: fields (m_cash etc.) are on inner State class — use GetCash(), GetKudos(), GetStarRating() methods instead.
- Key insight: TutorialUI.Init() postfix patch reads live localized strings from m_title.text + m_body.text.
- Build command: .\scripts\Build-Mod.ps1 from game directory.
