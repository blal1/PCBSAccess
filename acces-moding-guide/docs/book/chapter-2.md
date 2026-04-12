# Chapter 2: Setting Up Your Laboratory

## Introduction: The Importance of the Environment

Building a professional accessibility mod requires the right tools. Because we are working with Windows-based screen readers, the .NET ecosystem, and compiled game binaries, your development environment must be configured correctly. Attempting to mod a game with a disorganized environment will lead to endless frustration, mysterious crashes, and silent failures.

Think of this process as setting up a scientific laboratory. You need a workbench (your IDE), a microscope (your decompiler), chemical reagents (the Mod Loaders), and a communication array (Tolk). If any of these instruments are miscalibrated, your experiments will fail.

This chapter will guide you through installing, configuring, and testing every piece of software you need to build accessibility mods. We will focus entirely on Windows, as Tolk (and the screen readers it communicates with, like NVDA and JAWS) are Windows-native applications.

---

## 1. The .NET SDK: The Foundation

Unity games are built using C#. To write a mod, you also must write C# code. And to turn your C# code into a `.dll` file that the game can load, you need the .NET Software Development Kit (SDK).

### SDK vs. Runtime
It is crucial to understand the difference between the .NET Runtime and the .NET SDK.
*   **The .NET Runtime** allows your computer to *run* applications built with .NET. You likely already have several versions of this installed.
*   **The .NET SDK** includes the compilers, command-line tools, and libraries necessary to *build* .NET applications. This is what we need.

### Installation
We highly recommend using the command line to install the SDK, as it ensures you get the correct path configurations automatically.

Open PowerShell as an Administrator and run the following command using the Windows Package Manager (WinGet):

```powershell
winget install Microsoft.DotNet.SDK.8
```

We recommend .NET 8.0 (or newer) because it provides modern language features, but your mod will ultimately be compiled to target the specific framework the game uses (usually .NET Framework 4.7.2 or .NET Standard 2.0/2.1). The modern SDK can compile down to these older targets effortlessly.

### Verification
After the installation completes, close your PowerShell window and open a new one. Type the following command:

```powershell
dotnet --version
```

If the command returns a version number (e.g., `8.0.100`), your foundation is solid. If it says "command not found," you may need to restart your computer to ensure the `.NET` tools are added to your system's PATH environment variable.

---

## 2. The Decompiler: Seeing the Unseen

Since we do not have access to the game's original source code, we must generate an approximation of it. A decompiler takes a compiled `.dll` file and reverses the process, turning the Intermediate Language back into readable C# code.

There are two primary tools the community uses: ILSpy and dnSpy. We recommend having both.

### Tool 1: ILSpy (Command Line & GUI)
ILSpy is an actively maintained, open-source decompiler. It is fantastic because it offers a command-line interface, allowing us to automate the decompilation process.

**Installing the Command Line Tool:**
Open PowerShell and run:
```powershell
dotnet tool install ilspycmd -g
```

**Installing the GUI (Optional but Recommended):**
```powershell
winget install icsharpcode.ILSpy
```

**How to Decompile a Game with ILSpy:**
1.  Open PowerShell.
2.  Navigate to the folder where you want to store your mod project.
3.  Create a folder named `decompiled`: `mkdir decompiled`
4.  Run the ILSpy command, pointing it to the game's main assembly and telling it to output a project file:

```powershell
ilspycmd -p -o decompiled "C:\Path\To\Game\[GameName]_Data\Managed\Assembly-CSharp.dll"
```

This process can take a few minutes. Once it finishes, your `decompiled` folder will contain hundreds of `.cs` files. This is the game's logic laid bare.

### Tool 2: dnSpy (The Interactive Debugger)
dnSpy is an older, slightly less actively maintained tool, but it has one massive advantage: it is a full debugger. It allows you to edit IL code directly and step through the game's execution line-by-line while it is running.

**Installation:**
You cannot install dnSpy via WinGet. You must download the latest release from its GitHub repository (search for "dnSpy GitHub releases"). Download the `dnSpy-net-win64.zip` file, extract it to a folder on your computer, and run `dnSpy.exe`.

**Using dnSpy for Exploration:**
1.  Open `dnSpy.exe`.
2.  Drag and drop the game's `Assembly-CSharp.dll` into the left-hand "Assembly Explorer" pane.
3.  Expand the assembly, expand the `-` (empty namespace) node, and you will see the game's classes.
4.  Use `Ctrl+Shift+K` to open the search bar. This is how you will find UI panels and game managers.

---

## 3. Preparing the Game Directory (The Bridge)

Your development environment is set up, but the game itself must be prepared to accept your mod. This involves installing the Mod Loader and the Tolk native binaries.

### Step 3A: Installing the Mod Loader

You must choose between BepInEx and MelonLoader based on the game's architecture (Mono vs. IL2CPP) and community consensus (see Chapter 1).

**Installing BepInEx:**
1.  Go to the BepInEx GitHub Releases page.
2.  Download the correct zip file. 
    *   If the game is 64-bit (look for a `MonoBleedingEdge` folder or an `x64` suffix), download `BepInEx_x64_...zip`.
    *   If the game is 32-bit, download `BepInEx_x86_...zip`.
3.  Extract the *contents* of the zip file directly into the game's root folder (the same folder that contains the game's `.exe` file).
4.  **Crucial Step:** Run the game once. BepInEx needs to initialize and generate its folder structure. You should see a terminal window pop up briefly. Close the game.
5.  Look in the game folder. You should now see a `BepInEx` folder containing `core`, `config`, and `plugins` subdirectories.

**Installing MelonLoader:**
1.  Go to the MelonLoader GitHub Releases page or wiki.
2.  Download the `MelonLoader.Installer.exe`.
3.  Run the installer.
4.  Click the "Select" button and browse to the game's `.exe` file.
5.  Leave the settings on "Auto-Detect" and click "Install".
6.  **Crucial Step:** Run the game once. MelonLoader will perform a lengthy "unhollowing" process (especially if it's an IL2CPP game). Wait for the game to reach the main menu, then close it.
7.  Look in the game folder. You should now see `MelonLoader`, `Mods`, and `Plugins` subdirectories.

### Step 3B: Installing Tolk

Tolk is the library that talks to the screen reader. Because it communicates directly with Windows services and native applications (like NVDA), it requires unmanaged C++ DLLs.

1.  Go to the Tolk GitHub Releases page.
2.  Download the latest `tolk-v...zip`.
3.  Extract the zip file. You will see an `x86` (32-bit) folder and an `x64` (64-bit) folder.
4.  Determine your game's architecture (almost all modern games are 64-bit).
5.  Open the corresponding Tolk folder (e.g., `x64`).
6.  Copy **BOTH** `Tolk.dll` and `nvdaControllerClient64.dll` (or `nvdaControllerClient32.dll`).
7.  Paste these two files **directly into the game's root folder** (next to the `.exe`).

**Why both files?**
`Tolk.dll` is the main router. When it tries to talk to JAWS, it uses Microsoft COM, which is built into Windows. However, when it tries to talk to NVDA, it requires a specific bridge provided by the NVDA developers. That bridge is the `nvdaControllerClient` DLL. If you omit it, your mod will work for JAWS users but remain completely silent for NVDA users.

---

## 4. IDE Setup: Visual Studio or VS Code

Writing C# in a plain text editor is a recipe for disaster. The Unity API is massive, and you need an IDE (Integrated Development Environment) to provide IntelliSense (autocomplete), syntax checking, and automatic formatting.

### Option A: Visual Studio 2022 Community (Recommended)
Visual Studio is the heavyweight champion of C# development. It requires a large download but provides the most frictionless experience.
1.  Download the Visual Studio Installer from Microsoft.
2.  During installation, check the box for **".NET desktop development"**.
3.  (Optional but helpful): Check the box for **"Game development with Unity"**.

### Option B: Visual Studio Code (Lightweight)
VS Code is faster and uses fewer resources, but requires a bit more manual configuration.
1.  Install VS Code.
2.  Open VS Code and go to the Extensions tab (`Ctrl+Shift+X`).
3.  Install the **"C# Dev Kit"** extension by Microsoft.

### A Note on Project Files (`.csproj`)
Whether you use VS or VS Code, your project is defined by a `.csproj` file. This XML file tells the compiler which version of C# to use and where to find the game's DLLs (so your IDE knows what `GameObject` or `MonoBehaviour` means). Our scaffolding script (discussed next) will generate this file for you automatically.

---

## 5. The Scaffolding Script: Automating the Boilerplate

Setting up a mod project from scratch involves creating a half-dozen files, configuring XML references, and writing boilerplate initialization code. To prevent errors and save time, this framework provides a PowerShell scaffolding script.

### Running the Script

1.  Open PowerShell.
2.  Navigate to the folder where you saved the accessibility framework (the folder containing `scripts`, `templates`, and `docs`).
3.  Run the script with the required parameters:

```powershell
pwsh scripts/New-AccessibilityMod.ps1 -ModName "GameAccess" -Namespace "GameAccess" -Loader "BepInEx" -GameName "My Awesome Game"
```

*   `-ModName`: The name of your project folder and output DLL (e.g., `SkyrimAccess`).
*   `-Namespace`: The C# namespace for your code. Keep it simple and identical to the ModName.
*   `-Loader`: Must be exactly `BepInEx` or `MelonLoader`.
*   `-GameName`: A human-readable name for the game.

### What the Script Does
The script creates a new folder in the `output/` directory containing a fully functioning, compile-ready project. It pulls files from the `templates/` folder and replaces placeholder text (like `NAMESPACE` or `PLUGIN_VERSION`) with your specific details.

It generates:
*   `[ModName].csproj`: The project configuration.
*   `Main.cs`: The entry point for the Mod Loader.
*   `ScreenReader.cs`: The Tolk wrapper.
*   `AccessStateManager.cs`: The input context manager.
*   `UITextExtractor.cs` & `ReflectionHelper.cs`: Your "senses".
*   `Loc.cs`: The localization engine.

### Post-Scaffolding Setup
After the script finishes, you have one manual step remaining before you can compile:

1.  Open the newly created `[ModName].csproj` file in your IDE or a text editor.
2.  Find the `<PropertyGroup>` at the top.
3.  Locate the `<GameFolder>` or `<GamePath>` property.
4.  Change its value to the absolute path of your game installation (e.g., `C:\Program Files (x86)\Steam\steamapps\common\MyGame`).

This step is crucial. It allows your project to find the `UnityEngine.dll` and the mod loader DLLs it needs to compile.

---

## 6. Testing the Build Pipeline

Before writing any custom game logic, you must verify that your laboratory is fully functional. We will do this by compiling the scaffolded project and verifying that it speaks when the game launches.

1.  Open PowerShell and navigate to your new project folder (`output/[ModName]`).
2.  Run the build command:
    ```powershell
    dotnet build
    ```
3.  If the build succeeds, you will see a message saying "Build succeeded" and no errors.
4.  Navigate to the output folder (usually `bin/Debug/net472/` or similar, as defined in the `.csproj`).
5.  Copy your newly compiled `[ModName].dll`.
6.  Paste it into the game's mod folder:
    *   For BepInEx: `[GameFolder]\BepInEx\plugins\`
    *   For MelonLoader: `[GameFolder]\Mods\`
7.  Launch the game.
8.  Listen closely. As the game reaches the main menu, you should hear your screen reader say: "[GameName] Accessibility Mod Loaded."

If you hear this message, congratulations. Your laboratory is complete. Your compiler works, the mod loader is successfully injecting your code, and Tolk is successfully communicating with your screen reader. 

### Troubleshooting the Silence
If you do not hear the message, do not panic. This is where your Mod Loader's log file becomes your best friend.

1.  Open `BepInEx/LogOutput.log` or `MelonLoader/Latest.log`.
2.  Search the log for the name of your mod.
    *   If you don't see your mod mentioned anywhere, the DLL is in the wrong folder, or it was compiled for the wrong .NET framework.
    *   If you see your mod load, but it throws an exception (a block of error text), read the error carefully. Common errors include `DllNotFoundException: Tolk.dll` (you forgot to copy the native binaries to the game folder) or `TypeLoadException` (your mod is trying to access a game class that doesn't exist yet).

In the next chapter, we will dive deep into the code that made that first announcement possible. We will explore the `ScreenReader` wrapper and learn how to manage the flow of information from the game to the player.
