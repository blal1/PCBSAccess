# Finalize Loader Entry Points Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update BepInEx and MelonLoader entry point templates to include all necessary initialization and follow loader-specific patterns.

**Architecture:** Standard Unity mod entry points for BepInEx and MelonLoader, coordinating lifecycle and global hotkeys.

**Tech Stack:** C#, BepInEx, MelonLoader, Unity.

---

### Task 1: Update BepInEx Main.cs.template

**Files:**
- Modify: `templates/bepinex/Main.cs.template`

- [ ] **Step 1: Update class attributes and namespaces**
Include correct placeholders for PLUGIN_GUID, PLUGIN_NAME, and PLUGIN_VERSION. Ensure all necessary using statements are present.

- [ ] **Step 2: Update Awake() method**
Add initialization for ScreenReader, Loc, and AccessStateManager. Include robust error logging for initialization failures.

- [ ] **Step 3: Update ProcessHotkeys() and lifecycle methods**
Ensure it uses DebugLogger for input logging and follows best practices for BepInEx logging.

- [ ] **Step 4: Verify template content**
Review the template to ensure it matches the requirements and follows the BepInEx pattern.

### Task 2: Update MelonLoader Main.cs.template

**Files:**
- Modify: `templates/melonloader/Main.cs.template`

- [ ] **Step 1: Update assembly attributes**
Include correct placeholders for PLUGIN_NAME, PLUGIN_VERSION, NAMESPACE, AUTHOR, DEVELOPER, and GAME_NAME.

- [ ] **Step 2: Update OnInitializeMelon() method**
Add initialization for ScreenReader, Loc, and AccessStateManager. Include robust error logging using MelonLogger.

- [ ] **Step 3: Sync logic with BepInEx version**
Ensure lifecycle methods (OnUpdate, OnSceneWasLoaded, OnApplicationQuit) and hotkey processing match the BepInEx logic but use MelonLoader specifics.

- [ ] **Step 4: Verify template content**
Review the template to ensure it matches the requirements and follows the MelonLoader pattern.

### Task 3: Commit and Report

- [ ] **Step 1: Commit changes**
Commit the updated templates with the message 'templates: update entry point templates for BepInEx and MelonLoader'.

- [ ] **Step 2: Report completion**
Report that the task is DONE.
