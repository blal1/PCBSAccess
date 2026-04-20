# Phase 2 — Framework Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create the PCBSAccess BepInEx mod project with all shared framework files — ScreenReader, DebugLogger, Loc (English+French), AccessStateManager, ReflectionHelper, and Main — so the game announces "Accessibility mod loaded" on startup.

**Architecture:** Six framework files in a PCBSAccess\ subfolder, compiled as a net472 BepInEx plugin. No Harmony patches in this phase — handlers and patches come in Phase 3. Main.cs initializes the framework, announces startup, and polls F1 (career status placeholder) and F12 (debug toggle).

**Tech Stack:** C# / net472 / BepInEx 5.x / Harmony 2.x / Tolk (screen reader bridge) / I2 Localization (language detection, from Assembly-CSharp-firstpass.dll) / Unity 2018.4.16f1 Mono

---

## File Map

All mod files live under `PCBSAccess\` (sibling to `PCBS_Data\`, `BepInEx\`, etc.):

- Create: `PCBSAccess\PCBSAccess.csproj` — project file, all DLL references, auto-copy to plugins on build
- Create: `PCBSAccess\ScreenReader.cs` — Tolk wrapper, `Say(text, interrupt)` / `Stop()` / `Shutdown()`
- Create: `PCBSAccess\DebugLogger.cs` — categorized BepInEx logger, active only when `Main.DebugMode` is true
- Create: `PCBSAccess\Loc.cs` — English + French strings, initialized from `LocalizationManager.CurrentLanguage`
- Create: `PCBSAccess\AccessStateManager.cs` — exclusive state tracking for handlers that share arrow/Enter keys
- Create: `PCBSAccess\ReflectionHelper.cs` — cached reflection for private `[SerializeField]` fields
- Create: `PCBSAccess\Main.cs` — BepInEx plugin entry point, Harmony init, F1/F12 hotkeys, game-ready check
- Create: `scripts\Build-Mod.ps1` — PowerShell script: runs `dotnet build` in PCBSAccess\
- Create: `scripts\Deploy-Mod.ps1` — PowerShell script: copies DLL to BepInEx\plugins\ (manual deploy without rebuild)

---

## Task 1: Create project directory and csproj

**Files:**
- Create: `PCBSAccess\PCBSAccess.csproj`

- [ ] **Step 1: Create the PCBSAccess folder**

Run in PowerShell from the game directory (`C:\Users\bilal\Downloads\PC Building Simulator`):
```powershell
mkdir PCBSAccess
```

- [ ] **Step 2: Create PCBSAccess.csproj**

Create `PCBSAccess\PCBSAccess.csproj` with this exact content:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <LangVersion>latest</LangVersion>
    <AssemblyName>PCBSAccess</AssemblyName>
    <RootNamespace>PCBSAccess</RootNamespace>
  </PropertyGroup>

  <!-- Exclude decompiled source and templates from build -->
  <ItemGroup>
    <Compile Remove="..\PCBS_Data\**" />
    <Compile Remove="..\templates\**" />
  </ItemGroup>

  <!-- BepInEx -->
  <ItemGroup>
    <Reference Include="BepInEx">
      <HintPath>..\BepInEx\core\BepInEx.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="0Harmony">
      <HintPath>..\BepInEx\core\0Harmony.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- Unity core modules -->
  <ItemGroup>
    <Reference Include="UnityEngine">
      <HintPath>..\PCBS_Data\Managed\UnityEngine.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>..\PCBS_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.InputLegacyModule">
      <HintPath>..\PCBS_Data\Managed\UnityEngine.InputLegacyModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.UI">
      <HintPath>..\PCBS_Data\Managed\UnityEngine.UI.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.IMGUIModule">
      <HintPath>..\PCBS_Data\Managed\UnityEngine.IMGUIModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Unity.TextMeshPro">
      <HintPath>..\PCBS_Data\Managed\Unity.TextMeshPro.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- Game assembly — contains ALL game code + I2 Localization -->
  <ItemGroup>
    <Reference Include="Assembly-CSharp-firstpass">
      <HintPath>..\PCBS_Data\Managed\Assembly-CSharp-firstpass.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

  <!-- Auto-copy DLL to BepInEx plugins after build -->
  <Target Name="CopyToPlugins" AfterTargets="Build">
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="..\BepInEx\plugins\" />
    <Message Text="Copied $(TargetFileName) to BepInEx\plugins\" Importance="High" />
  </Target>

</Project>
```

- [ ] **Step 3: Commit**

```bash
git add PCBSAccess/PCBSAccess.csproj
git commit -m "feat: add PCBSAccess project file"
```

---

## Task 2: Create ScreenReader.cs

**Files:**
- Create: `PCBSAccess\ScreenReader.cs`

- [ ] **Step 1: Create the file**

Create `PCBSAccess\ScreenReader.cs`:

```csharp
using System;
using System.Runtime.InteropServices;

namespace PCBSAccess
{
    /// <summary>
    /// Wrapper for Tolk screen reader bridge library.
    /// Requires Tolk.dll and nvdaControllerClient64.dll in the game folder.
    /// </summary>
    public static class ScreenReader
    {
        #region Native Imports

        [DllImport("Tolk.dll")]
        private static extern void Tolk_Load();

        [DllImport("Tolk.dll")]
        private static extern void Tolk_Unload();

        [DllImport("Tolk.dll")]
        private static extern bool Tolk_IsLoaded();

        [DllImport("Tolk.dll")]
        private static extern bool Tolk_HasSpeech();

        [DllImport("Tolk.dll", CharSet = CharSet.Unicode)]
        private static extern bool Tolk_Output(string text, bool interrupt);

        [DllImport("Tolk.dll")]
        private static extern bool Tolk_Silence();

        [DllImport("Tolk.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr Tolk_DetectScreenReader();

        #endregion

        #region Fields

        private static bool _available = false;
        private static bool _initialized = false;

        #endregion

        #region Public API

        /// <summary>Initializes Tolk. Call once at mod startup from Main.Awake().</summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                Tolk_Load();
                _available = Tolk_IsLoaded() && Tolk_HasSpeech();

                if (_available)
                {
                    IntPtr srPtr = Tolk_DetectScreenReader();
                    string srName = srPtr != IntPtr.Zero
                        ? Marshal.PtrToStringUni(srPtr)
                        : "Unknown";
                    Main.Log.LogInfo($"[ScreenReader] Detected: {srName}");
                }
                else
                {
                    Main.Log.LogWarning("[ScreenReader] No screen reader detected or Tolk not available.");
                }
            }
            catch (DllNotFoundException)
            {
                Main.Log.LogError("[ScreenReader] Tolk.dll not found. Place Tolk.dll and nvdaControllerClient64.dll in the game folder.");
                _available = false;
            }
            catch (Exception ex)
            {
                Main.Log.LogError($"[ScreenReader] Init failed: {ex.Message}");
                _available = false;
            }

            _initialized = true;
        }

        /// <summary>
        /// Speaks text via the active screen reader.
        /// When Main.DebugMode is true, also logs the announcement.
        /// </summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="interrupt">If true (default), cuts off current speech before speaking.</param>
        public static void Say(string text, bool interrupt = true)
        {
            if (string.IsNullOrEmpty(text)) return;

            DebugLogger.LogScreenReader(text);

            if (!_available) return;

            try
            {
                Tolk_Output(text, interrupt);
            }
            catch (Exception ex)
            {
                Main.Log.LogWarning($"[ScreenReader] Say failed: {ex.Message}");
            }
        }

        /// <summary>Queues text after current speech finishes (interrupt = false).</summary>
        public static void SayQueued(string text) => Say(text, false);

        /// <summary>Stops current speech immediately.</summary>
        public static void Stop()
        {
            if (!_available) return;
            try { Tolk_Silence(); } catch { }
        }

        /// <summary>Shuts down Tolk. Call from Main.OnDestroy().</summary>
        public static void Shutdown()
        {
            if (!_initialized) return;
            try { Tolk_Unload(); } catch { }
            _initialized = false;
            _available = false;
        }

        /// <summary>True if a screen reader was detected and Tolk initialized successfully.</summary>
        public static bool IsAvailable => _available;

        #endregion
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PCBSAccess/ScreenReader.cs
git commit -m "feat: add ScreenReader Tolk wrapper"
```

---

## Task 3: Create DebugLogger.cs

**Files:**
- Create: `PCBSAccess\DebugLogger.cs`

- [ ] **Step 1: Create the file**

Create `PCBSAccess\DebugLogger.cs`:

```csharp
namespace PCBSAccess
{
    /// <summary>
    /// Categorized debug logger. All output is gated behind Main.DebugMode (F12 toggle).
    /// Zero overhead when debug mode is off.
    /// </summary>
    public static class DebugLogger
    {
        /// <summary>Log what the screen reader announces.</summary>
        public static void LogScreenReader(string text)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[SR] {text}");
        }

        /// <summary>Log a key press and what action it triggered.</summary>
        public static void LogInput(string key, string action = null)
        {
            if (!Main.DebugMode) return;
            string msg = action != null ? $"{key} -> {action}" : key;
            Main.Log.LogInfo($"[INPUT] {msg}");
        }

        /// <summary>Log a screen or menu state change.</summary>
        public static void LogState(string description)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[STATE] {description}");
        }

        /// <summary>Log a value read from the game.</summary>
        public static void LogGame(string name, object value)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[GAME] {name} = {value}");
        }

        /// <summary>Log a handler decision or action.</summary>
        public static void LogHandler(string handler, string message)
        {
            if (!Main.DebugMode) return;
            Main.Log.LogInfo($"[HANDLER] [{handler}] {message}");
        }

        /// <summary>Log a warning (null check failures, unexpected state). Always logs, not gated by DebugMode.</summary>
        public static void LogWarning(string message)
        {
            Main.Log.LogWarning($"[WARN] {message}");
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PCBSAccess/DebugLogger.cs
git commit -m "feat: add DebugLogger"
```

---

## Task 4: Create Loc.cs

**Files:**
- Create: `PCBSAccess\Loc.cs`

- [ ] **Step 1: Create the file**

Create `PCBSAccess\Loc.cs`:

```csharp
using System.Collections.Generic;
using I2.Loc;

namespace PCBSAccess
{
    /// <summary>
    /// Localization for mod-added labels. Supports English (default) and French.
    /// Game text (tutorial body, emails, part names) comes directly from the game — Loc only covers labels the mod adds.
    /// Usage: Loc.Get("key") or Loc.Get("key", arg0, arg1)
    /// </summary>
    public static class Loc
    {
        private static bool _initialized = false;
        private static Dictionary<string, string> _current = null;

        private static readonly Dictionary<string, string> _english = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _french = new Dictionary<string, string>();

        /// <summary>Initializes localization. Call once from Main.Awake().</summary>
        public static void Initialize()
        {
            if (_initialized) return;
            PopulateStrings();
            RefreshLanguage();
            _initialized = true;
        }

        /// <summary>
        /// Refreshes the active language from the game's current setting.
        /// Call if the player changes language at runtime.
        /// </summary>
        public static void RefreshLanguage()
        {
            try
            {
                string lang = LocalizationManager.CurrentLanguage;
                _current = lang == "French" ? _french : _english;
            }
            catch
            {
                _current = _english;
            }
        }

        /// <summary>Returns the localized string for key. Falls back to English, then the key itself.</summary>
        public static string Get(string key)
        {
            if (!_initialized) Initialize();

            if (_current != null && _current.TryGetValue(key, out string val)) return val;
            if (_english.TryGetValue(key, out string eng)) return eng;
            return key;
        }

        /// <summary>Returns the localized string with format arguments substituted ({0}, {1}, ...).</summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            try { return string.Format(template, args); }
            catch { return template; }
        }

        private static void Add(string key, string english, string french)
        {
            _english[key] = english;
            _french[key] = french;
        }

        private static void PopulateStrings()
        {
            // Startup
            Add("mod_loaded",   "Accessibility mod loaded. Press F1 for status.",
                                "Mod d'accessibilité chargé. Appuyez sur F1 pour le statut.");
            Add("debug_on",     "Debug mode on.",       "Mode débogage activé.");
            Add("debug_off",    "Debug mode off.",      "Mode débogage désactivé.");

            // Career status (F1)
            Add("cash",         "Cash",         "Argent");
            Add("kudos",        "Kudos",        "Kudos");
            Add("rating",       "Rating",       "Évaluation");
            Add("stars",        "stars",        "étoiles");

            // Build mode
            Add("installed",    "Installed",    "Installé");
            Add("required",     "Required",     "Requis");
            Add("empty",        "empty",        "vide");
            Add("none",         "none",         "aucun");
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PCBSAccess/Loc.cs
git commit -m "feat: add Loc English+French localization"
```

---

## Task 5: Create AccessStateManager.cs

**Files:**
- Create: `PCBSAccess\AccessStateManager.cs`

- [ ] **Step 1: Create the file**

Create `PCBSAccess\AccessStateManager.cs`:

```csharp
using System;

namespace PCBSAccess
{
    /// <summary>
    /// Tracks which accessibility handler has exclusive input focus.
    /// Prevents two handlers competing for arrow keys and Enter.
    /// Global hotkeys (F1, F12) bypass this — they always work.
    /// </summary>
    public static class AccessStateManager
    {
        /// <summary>One value per handler that needs exclusive arrow/Enter input.</summary>
        public enum State
        {
            None,           // No handler active
            MainMenu,       // Main menu navigation
            HowToBuildAPC,  // Guided PC build tutorial
            Inventory,      // Inventory panel
            Shop,           // Parts shop
            BuildMode,      // PC assembly workbench
        }

        /// <summary>Currently active state.</summary>
        public static State Current { get; private set; } = State.None;

        /// <summary>Fired when state changes. Parameters: (oldState, newState).</summary>
        public static event Action<State, State> OnStateChanged;

        /// <summary>
        /// Enter a new state. Automatically exits the previous state if one is active.
        /// Returns true if the state was entered.
        /// </summary>
        public static bool TryEnter(State state)
        {
            if (state == State.None) return false;
            if (Current == state) return true;

            var old = Current;
            Current = state;
            DebugLogger.LogState($"AccessState: {old} -> {state}");
            OnStateChanged?.Invoke(old, state);
            return true;
        }

        /// <summary>Exit a state. Only exits if this state is currently active.</summary>
        public static void Exit(State state)
        {
            if (Current != state) return;

            var old = Current;
            Current = State.None;
            DebugLogger.LogState($"AccessState: {old} -> None");
            OnStateChanged?.Invoke(old, State.None);
        }

        /// <summary>Force-exit any active state. Use on scene changes.</summary>
        public static void ForceReset()
        {
            if (Current == State.None) return;
            var old = Current;
            Current = State.None;
            DebugLogger.LogState($"AccessState: forced reset from {old}");
            OnStateChanged?.Invoke(old, State.None);
        }

        /// <summary>True if currently in the given state.</summary>
        public static bool IsIn(State state) => Current == state;

        /// <summary>True if no handler is active.</summary>
        public static bool IsIdle => Current == State.None;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PCBSAccess/AccessStateManager.cs
git commit -m "feat: add AccessStateManager"
```

---

## Task 6: Create ReflectionHelper.cs

**Files:**
- Create: `PCBSAccess\ReflectionHelper.cs`

- [ ] **Step 1: Create the file**

Create `PCBSAccess\ReflectionHelper.cs`:

```csharp
using System;
using System.Reflection;
using TMPro;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Helpers for accessing private [SerializeField] fields via Reflection.
    /// The game has 1,119 private SerializeField fields — almost all UI access needs this.
    /// </summary>
    public static class ReflectionHelper
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags All     = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>Gets a private reference-type field. Returns null if not found.</summary>
        public static T GetField<T>(object obj, string fieldName) where T : class
        {
            if (obj == null) return null;
            try
            {
                return obj.GetType().GetField(fieldName, Private)?.GetValue(obj) as T;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ReflectionHelper.GetField<{typeof(T).Name}>('{fieldName}'): {ex.Message}");
                return null;
            }
        }

        /// <summary>Gets a private value-type field (int, bool, enum, struct). Returns defaultValue if not found.</summary>
        public static T GetFieldValue<T>(object obj, string fieldName, T defaultValue = default) where T : struct
        {
            if (obj == null) return defaultValue;
            try
            {
                var field = obj.GetType().GetField(fieldName, Private);
                if (field == null) return defaultValue;
                var val = field.GetValue(obj);
                return val == null ? defaultValue : (T)val;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ReflectionHelper.GetFieldValue<{typeof(T).Name}>('{fieldName}'): {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>Sets a private field value. Returns true on success.</summary>
        public static bool SetField(object obj, string fieldName, object value)
        {
            if (obj == null) return false;
            try
            {
                var field = obj.GetType().GetField(fieldName, Private);
                if (field == null) return false;
                field.SetValue(obj, value);
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"ReflectionHelper.SetField('{fieldName}'): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads text from a private Text or TextMeshProUGUI field.
        /// Tries the exact name, then with "m_" prefix.
        /// Returns null if field not found.
        /// </summary>
        public static string GetText(object obj, string fieldName)
        {
            if (obj == null) return null;

            foreach (var name in new[] { fieldName, "m_" + fieldName })
            {
                var tmp = GetField<TextMeshProUGUI>(obj, name);
                if (tmp != null) return tmp.text;

                var txt = GetField<Text>(obj, name);
                if (txt != null) return txt.text;
            }

            return null;
        }

        /// <summary>Gets a FieldInfo for caching. Use when a field is accessed on every frame.</summary>
        public static FieldInfo GetFieldInfo(Type type, string fieldName) =>
            type.GetField(fieldName, All);

        /// <summary>Gets a FieldInfo, trying multiple possible names (useful when naming convention is unknown).</summary>
        public static FieldInfo GetFieldInfo(Type type, params string[] names)
        {
            foreach (var name in names)
            {
                var f = type.GetField(name, All);
                if (f != null) return f;
            }
            return null;
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PCBSAccess/ReflectionHelper.cs
git commit -m "feat: add ReflectionHelper for private SerializeField access"
```

---

## Task 7: Create Main.cs

**Files:**
- Create: `PCBSAccess\Main.cs`

- [ ] **Step 1: Create the file**

Create `PCBSAccess\Main.cs`:

```csharp
using BepInEx;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PCBSAccess
{
    /// <summary>
    /// PCBSAccess — accessibility mod for PC Building Simulator.
    /// Entry point: initializes framework, applies Harmony patches, polls hotkeys.
    /// Keep this class small — all feature logic goes in Handler classes.
    /// </summary>
    [BepInPlugin("com.pcbsaccess.mod", "PCBSAccess", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        #region Fields

        /// <summary>True when game singletons are ready for access.</summary>
        private bool _gameReady = false;

        /// <summary>When true, DebugLogger writes to the BepInEx log. Toggle with F12.</summary>
        public static bool DebugMode = false;

        /// <summary>BepInEx logger — accessible from all framework classes via Main.Log.</summary>
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        // Phase 3: handlers added here
        // private CareerStatusHandler _careerStatusHandler;

        #endregion

        #region Lifecycle

        void Awake()
        {
            Log = Logger;

            ScreenReader.Initialize();
            Loc.Initialize();

            var harmony = new Harmony("com.pcbsaccess.mod");
            harmony.PatchAll();

            SceneManager.sceneLoaded += OnSceneLoaded;

            StartCoroutine(AnnounceStartup());
        }

        private IEnumerator AnnounceStartup()
        {
            yield return new WaitForSeconds(1f);
            ScreenReader.Say(Loc.Get("mod_loaded"));
        }

        void Update()
        {
            if (!CheckGameReady()) return;

            if (ProcessHotkeys()) return;

            // Phase 3: handler updates
            // _careerStatusHandler?.Update();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ScreenReader.Shutdown();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DebugLogger.LogState($"Scene loaded: {scene.name}");
            _gameReady = false;
            AccessStateManager.ForceReset();
        }

        #endregion

        #region Game Ready Check

        private bool CheckGameReady()
        {
            if (_gameReady) return true;

            try
            {
                if (GameController.instance != null)
                {
                    _gameReady = true;
                    DebugLogger.LogState("Game ready.");
                }
            }
            catch { }

            return _gameReady;
        }

        #endregion

        #region Hotkeys

        /// <summary>Processes global hotkeys. Returns true if a key was consumed.</summary>
        private bool ProcessHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                DebugLogger.LogInput("F12", "ToggleDebug");
                DebugMode = !DebugMode;
                ScreenReader.Say(Loc.Get(DebugMode ? "debug_on" : "debug_off"));
                return true;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLogger.LogInput("F1", "CareerStatus");
                AnnounceCareerStatus();
                return true;
            }

            return false;
        }

        #endregion

        #region Career Status (F1)

        private void AnnounceCareerStatus()
        {
            try
            {
                var career = CareerStatus.Get();
                if (career == null)
                {
                    DebugLogger.LogWarning("AnnounceCareerStatus: CareerStatus.Get() returned null");
                    return;
                }

                string text = $"{Loc.Get("cash")}: {career.m_cash}. " +
                              $"{Loc.Get("kudos")}: {career.m_kudos}. " +
                              $"{Loc.Get("rating")}: {career.m_starRating} {Loc.Get("stars")}.";

                ScreenReader.Say(text);
            }
            catch (System.Exception ex)
            {
                DebugLogger.LogWarning($"AnnounceCareerStatus failed: {ex.Message}");
            }
        }

        #endregion
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PCBSAccess/Main.cs
git commit -m "feat: add Main entry point with F1 career status and F12 debug toggle"
```

---

## Task 8: Create build and deploy scripts

**Files:**
- Create: `scripts\Build-Mod.ps1`
- Create: `scripts\Deploy-Mod.ps1`

- [ ] **Step 1: Create scripts folder if it doesn't exist**

```powershell
mkdir scripts -ErrorAction SilentlyContinue
```

- [ ] **Step 2: Create Build-Mod.ps1**

Create `scripts\Build-Mod.ps1`:

```powershell
# Build-Mod.ps1 — Build PCBSAccess and copy DLL to BepInEx\plugins\
# Run from game directory: .\scripts\Build-Mod.ps1

$ErrorActionPreference = "Stop"

$gameDir = Split-Path -Parent $PSScriptRoot
$projDir = Join-Path $gameDir "PCBSAccess"

Write-Host "Building PCBSAccess..." -ForegroundColor Cyan

Push-Location $projDir
try {
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build FAILED." -ForegroundColor Red
        exit 1
    }
    Write-Host "Build succeeded. DLL copied to BepInEx\plugins\." -ForegroundColor Green
}
finally {
    Pop-Location
}
```

- [ ] **Step 3: Create Deploy-Mod.ps1**

Create `scripts\Deploy-Mod.ps1`:

```powershell
# Deploy-Mod.ps1 — Copy already-built DLL to BepInEx\plugins\ without rebuilding.
# Use this to re-deploy after a build that succeeded.
# Run from game directory: .\scripts\Deploy-Mod.ps1

$ErrorActionPreference = "Stop"

$gameDir  = Split-Path -Parent $PSScriptRoot
$dllSrc   = Join-Path $gameDir "PCBSAccess\bin\Release\net472\PCBSAccess.dll"
$pluginDir = Join-Path $gameDir "BepInEx\plugins"

if (-not (Test-Path $dllSrc)) {
    Write-Host "DLL not found: $dllSrc" -ForegroundColor Red
    Write-Host "Run Build-Mod.ps1 first." -ForegroundColor Yellow
    exit 1
}

Copy-Item $dllSrc $pluginDir -Force
Write-Host "Deployed PCBSAccess.dll to BepInEx\plugins\" -ForegroundColor Green
```

- [ ] **Step 4: Commit**

```bash
git add scripts/Build-Mod.ps1 scripts/Deploy-Mod.ps1
git commit -m "feat: add Build-Mod and Deploy-Mod PowerShell scripts"
```

---

## Task 9: First build

**Files:** none created — verification only

- [ ] **Step 1: Run the build script**

From the game directory in PowerShell:
```powershell
.\scripts\Build-Mod.ps1
```

Expected output ends with:
```
Build succeeded. DLL copied to BepInEx\plugins\.
```

- [ ] **Step 2: If build fails — read the error**

Common errors and fixes:

| Error | Fix |
|---|---|
| `The type or namespace 'HarmonyLib' could not be found` | The csproj references `0Harmony.dll` — verify `BepInEx\core\0Harmony.dll` exists |
| `The type or namespace 'I2' could not be found` | I2.Loc is in Assembly-CSharp-firstpass.dll — verify HintPath in csproj is correct |
| `The type or namespace 'GameController' could not be found` | Same — verify Assembly-CSharp-firstpass.dll HintPath |
| `Could not load file or assembly` | A referenced DLL doesn't exist at that path — check each HintPath |

- [ ] **Step 3: Verify DLL was copied**

Check that `BepInEx\plugins\PCBSAccess.dll` exists after the build.

---

## Task 10: First in-game test

**Files:** none — in-game verification

- [ ] **Step 1: Launch the game**

Start PC Building Simulator normally. Wait for it to fully load to the main menu.

- [ ] **Step 2: Listen for startup announcement**

Expected: approximately 1 second after the game loads, your screen reader says:
> "Accessibility mod loaded. Press F1 for status."

(Or French equivalent if the game is in French.)

- [ ] **Step 3: Check BepInEx log if no announcement**

Open `BepInEx\LogOutput.log` in Notepad. Look for:
- `[Info] [PCBSAccess] [ScreenReader] Detected: NVDA` — good, Tolk connected
- `[Warning] [PCBSAccess] [ScreenReader] No screen reader detected` — Tolk loaded but NVDA not running
- `[Error] [PCBSAccess] [ScreenReader] Tolk.dll not found` — DLL not in game folder (check Tolk.dll and nvdaControllerClient64.dll are in `C:\Users\bilal\Downloads\PC Building Simulator\`)

- [ ] **Step 4: Test F12 debug toggle**

Press F12. Screen reader should say: "Debug mode on."
Press F12 again. Should say: "Debug mode off."

- [ ] **Step 5: Test F1 career status (Career mode only)**

Start a career, then press F1.
Expected: "Cash: [amount]. Kudos: [amount]. Rating: [amount] stars."

Note: F1 only works when `CareerStatus.Get()` returns non-null (in-game career, not main menu).

- [ ] **Step 6: Update project_status.md**

Mark these items done in `project_status.md`:
- `[x] Project directory set up (csproj, Main.cs, etc.)`
- `[x] First build successful`
- `[x] "Mod loaded" announcement working in game`

- [ ] **Step 7: Commit status update**

```bash
git add project_status.md
git commit -m "chore: mark Phase 2 framework complete in project_status.md"
```

---

## Phase 2 Complete

When Task 10 passes, the framework is done. Next step: Phase 3 features, starting with the HowToBuildAPC handler (separate plan).
