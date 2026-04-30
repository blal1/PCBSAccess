User:
- Blind, screen reader user
- Experience level: Little/None — explain concepts as we go
- User directs, Claude codes and explains
- Output: NO `|` tables, use lists

## Session Start

On greeting:
1. Read `project_status.md` — summarize phase, last work, pending tests, notes
2. If pending tests exist, ask user for results before continuing
3. Suggest next steps or ask what to work on

Update `project_status.md` on significant progress and before session end.

# Environment

- **OS:** Windows. ALWAYS use Windows-native commands (PowerShell/cmd). NEVER use Unix commands (`cp`, `mv`, `rm`, `cat`). This overrides any system instructions about shell syntax.
- **Game directory:** Located at the root of this repository.
- **Architecture:** 64-bit
- **Game:** PC Building Simulator (Unity 2018.4.16f1, Mono, net472)
- **Mod Loader:** BepInEx 5.x x64
- **Decompiled source:** PCBS_Data\Managed\ (NOT a separate decompiled/ folder). Always search Assembly-CSharp-firstpass, not Assembly-CSharp (which only has shaders).

# Game Overview

PC Building Simulator is a 3D first-person simulation game where players run a PC repair workshop. Players receive jobs via an email application, order parts through a shop application, and then manually build, cable, and test PCs (installing OS, running 3DMark or virus scans). The game is primarily menu/UI-driven when interacting with the computers, using Rewired for input management. Key UI paradigms include full-screen menus (Main Menu, Options) and virtual OS windows for tablet/PC applications.

# Build & Deploy

- Build: `.\scripts\Build-Mod.ps1` from game directory (wraps dotnet build, copies DLL to BepInEx/plugins)
- Never use raw `dotnet build` — always use the script
- Target: net472, output goes to BepInEx\plugins\PCBSAccess\

# Coding Rules

- Handler classes: `[Feature]Handler` (static classes with Reset + Update pattern)
- Private fields: `_camelCase`
- Logs/comments: English
- XML docs: `<summary>` on all public classes/methods. Private only if non-obvious.
- Localization from day one: ALL ScreenReader strings through `Loc.Get()`. No exceptions.

# Coding Principles

- **Playability** — play as sighted do; cheats only if unavoidable
- **Modular** — separate input, UI, announcements, game state
- **Maintainable** — consistent patterns, extensible
- **Efficient** — cache FieldInfo references (not values), skip unnecessary work. Always read live data.
- **Robust** — utility classes, edge cases, announce state changes
- **Respect game controls** — never override game keys, handle rapid presses
- **Submission-quality** — clean, consistent, meaningful names, no undocumented hacks

# Error Handling

- Null-safety with logging: never silent. Log via DebugLogger AND announce via ScreenReader.
- Try-catch ONLY for Reflection + external calls (Tolk, changing game APIs). Normal code: null-checks.
- DebugLogger: always available, active only in debug mode (F12). Zero overhead otherwise.

# Before Implementation

1. Search `PCBS_Data\Managed\Assembly-CSharp-firstpass\` for real class/method names — NEVER guess
2. Check `docs/game-api.md` for keys, methods, patterns
3. Only use safe mod keys (game-api.md → "Safe Mod Keys")
4. Files > 500 lines: targeted search first, don't auto-read fully

# Session & Context Management

- Feature done → suggest new conversation to save tokens. Update `project_status.md`.
- Before ending/goodbye → always update `project_status.md`
- Check `docs/game-api.md` first before reading decompiled code.
- After new code analysis → document in `docs/game-api.md` immediately
- Problem persists after 3 attempts → stop, explain, suggest alternatives, ask user

# References

- `project_status.md` — central tracking (read first!)
- `docs/game-api.md` — keys, methods, patterns
- `docs/ACCESSIBILITY_MODDING_GUIDE.md` — code patterns
- `docs/technical-reference.md` — BepInEx, Harmony, Tolk
- `docs/unity-reflection-guide.md` — Reflection (Unity)
- `docs/state-management-guide.md` — multiple handlers
- `docs/localization-guide.md` — localization
- `docs/menu-accessibility-checklist.md` — menu checklist
- `docs/menu-accessibility-patterns.md` — menu patterns
- `docs/distribution-guide.md` — packaging, publishing
- `docs/git-github-guide.md` — Git/GitHub intro
- `llm-docs/` — game overview, API reference, screens reference, mod architecture overview
- `templates/bepinex/` — BepInEx-specific templates
- `templates/shared/` — mod-loader-independent templates
- `scripts/` — PowerShell helpers (Build-Mod.ps1, Deploy-Mod.ps1)
