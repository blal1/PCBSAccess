# Chapter 2: Setting Up Your Laboratory

Building a professional accessibility mod requires the right tools. Because we are working with Windows-based screen readers and the .NET ecosystem, your development environment must be configured correctly.

## 1. The .NET SDK
To compile C# code into DLLs, you need the .NET SDK.
*   **Recommendation:** Install .NET 8.0 or newer.
*   **Command:** Use PowerShell to install via WinGet:
    ```powershell
    winget install Microsoft.DotNet.SDK.8
    ```

## 2. The Decompiler: Seeing the Unseen
Since we don't have the game's original source code, we must "decompile" its DLLs.
*   **ILSpy / dnSpy:** These tools turn the game's DLLs back into readable C# code.
*   **Installation:**
    ```powershell
    dotnet tool install ilspycmd -g
    ```
*   **Usage:** You will primarily decompile `[GameName]_Data/Managed/Assembly-CSharp.dll`. This is where most of the game's unique logic lives.

## 3. Preparing the Game Directory
For your mod to work, the "Bridge" must be in place.
1.  **Install the Mod Loader:** Run the MelonLoader installer or extract BepInEx into the game folder.
2.  **Tolk Binaries:** Download the Tolk releases and copy two files into the same folder as your game's `.exe`:
    *   `Tolk.dll` (The bridge)
    *   `nvdaControllerClient64.dll` (Required specifically for NVDA support)

## 4. The Scaffolding Script
To save time, we use a scaffolding script to create a new project.
*   **Location:** `scripts/New-AccessibilityMod.ps1`
*   **Execution:**
    ```powershell
    pwsh scripts/New-AccessibilityMod.ps1 -ModName "GameAccess" -Namespace "MyName" -Loader "BepInEx"
    ```
This creates a pre-configured project with all the necessary folders and templates we will discuss in the coming chapters.

## 5. IDE Setup (Visual Studio or VS Code)
While you can write code in a text editor, an IDE provides IntelliSense (code completion), which is invaluable for discovering game methods.
*   **Visual Studio 2022 Community:** The standard for C# development.
*   **VS Code:** A lightweight alternative (requires the C# Dev Kit extension).

### Pro Tip: The Log File
Always keep your mod loader's log file open during development.
*   **MelonLoader:** `MelonLoader/Latest.log`
*   **BepInEx:** `BepInEx/LogOutput.log`
This is where all your `DebugLogger` messages will appear. If your mod doesn't speak, the log will tell you why.

---
*Next: [Chapter 3: Designing the Nervous System](chapter-3.md)*
