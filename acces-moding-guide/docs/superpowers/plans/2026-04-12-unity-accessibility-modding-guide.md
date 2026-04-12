# Unity Accessibility Modding Guide Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a complete, professional-grade guide and template system for Unity accessibility modding.

**Architecture:** A modular framework using a "Handler" pattern, a centralized "AccessStateManager", and a "ScreenReader" wrapper for Tolk.

**Tech Stack:** C#, Unity, BepInEx, MelonLoader, Harmony, Tolk (Native C++).

---

### Task 1: Finalize Core Templates (Shared)

**Files:**
- Modify: `templates/shared/AccessStateManager.cs.template`
- Modify: `templates/shared/ReflectionHelper.cs.template`
- Create: `templates/shared/UITextExtractor.cs.template`

- [ ] **Step 1: Refine AccessStateManager.cs.template**
Ensure it supports context-aware "Escape" handling and event-driven state changes.

- [ ] **Step 2: Refine ReflectionHelper.cs.template**
Add caching for `FieldInfo` and `MethodInfo` to prevent performance hits in `Update` loops.

- [ ] **Step 3: Create UITextExtractor.cs.template**
Implement a utility to find and read text from `Text`, `TextMeshProUGUI`, and `Button` components.

```csharp
// Example for Step 3
public static string GetText(GameObject obj) {
    if (obj == null) return null;
    var tmp = obj.GetComponent<TMPro.TextMeshProUGUI>();
    if (tmp != null) return tmp.text;
    var txt = obj.GetComponent<UnityEngine.UI.Text>();
    if (txt != null) return txt.text;
    return null;
}
```

- [ ] **Step 4: Commit**
```bash
git add templates/shared/
git commit -m "templates: finalize shared modular utilities"
```

### Task 2: Finalize Loader Entry Points

**Files:**
- Modify: `templates/bepinex/Main.cs.template`
- Modify: `templates/melonloader/Main.cs.template`

- [ ] **Step 1: Update BepInEx Main.cs.template**
Include initialization for `ScreenReader`, `AccessStateManager`, and the `DebugLogger`.

- [ ] **Step 2: Update MelonLoader Main.cs.template**
Sync with BepInEx version but using `MelonMod` lifecycle and logging.

- [ ] **Step 3: Commit**
```bash
git add templates/bepinex/Main.cs.template templates/melonloader/Main.cs.template
git commit -m "templates: update entry point templates for BepInEx and MelonLoader"
```

### Task 3: Create Scaffolding Script

**Files:**
- Create: `scripts/New-AccessibilityMod.ps1`

- [ ] **Step 1: Write New-AccessibilityMod.ps1**
A script that asks for: Mod Name, Namespace, Game Name, and Loader (BepInEx/MelonLoader), then copies templates and replaces placeholders.

```powershell
param($ModName, $Namespace, $Loader)
# Copy templates to new folder, replace NAMESPACE with $Namespace, etc.
```

- [ ] **Step 2: Test script**
Run: `pwsh scripts/New-AccessibilityMod.ps1 -ModName "TestMod" -Namespace "Test" -Loader "BepInEx"`
Expected: Folder "TestMod" created with populated files.

- [ ] **Step 3: Commit**
```bash
git add scripts/New-AccessibilityMod.ps1
git commit -m "scripts: add mod scaffolding script"
```

### Task 4: Author Documentation Chapters

**Files:**
- Modify: `docs/ACCESSIBILITY_MODDING_GUIDE.md` (Update with Modular Framework details)
- Modify: `docs/setup-guide.md` (Add Tolk/Native binary setup)
- Modify: `docs/technical-reference.md` (Document Handler pattern)

- [ ] **Step 1: Update ACCESSIBILITY_MODDING_GUIDE.md**
Add the "Modular Framework" section and the "Playability, Not Simplification" philosophy.

- [ ] **Step 2: Update setup-guide.md**
Add explicit instructions for `Tolk.dll` and `nvdaControllerClient64.dll` placement.

- [ ] **Step 3: Update technical-reference.md**
Provide code examples for the `Handler` pattern and state detection.

- [ ] **Step 4: Commit**
```bash
git add docs/*.md
git commit -m "docs: complete core documentation chapters"
```

### Task 5: Localization & Internationalization Guide

**Files:**
- Create: `docs/localization-guide.md`
- Modify: `templates/shared/Loc.cs.template`

- [ ] **Step 1: Create localization-guide.md**
Explain how to use the `Loc` utility and how to hook into the game's native localization.

- [ ] **Step 2: Finalize Loc.cs.template**
Ensure it supports simple JSON or dictionary-based translations.

- [ ] **Step 3: Commit**
```bash
git add docs/localization-guide.md templates/shared/Loc.cs.template
git commit -m "docs: add localization guide and utility template"
```

### Task 6: Final Review & Polish

- [ ] **Step 1: Verify all links**
Check that cross-references between MD files work.

- [ ] **Step 2: German Localization check**
Ensure `docs-de/` contains translated versions of the key guides.

- [ ] **Step 3: Final Commit**
```bash
git add .
git commit -m "chore: final polish and link verification"
```
