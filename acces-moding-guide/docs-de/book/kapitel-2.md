# Kapitel 2: Einrichten Ihres Labors

Der Aufbau eines professionellen Accessibility-Mods erfordert die richtigen Werkzeuge. Da wir mit Windows-basierten Screenreadern und dem .NET-Ökosystem arbeiten, muss Ihre Entwicklungsumgebung korrekt konfiguriert sein.

## 1. Das .NET SDK
Um C#-Code in DLLs zu kompilieren, benötigen Sie das .NET SDK.
*   **Empfehlung:** Installieren Sie .NET 8.0 oder neuer.
*   **Befehl:** Verwenden Sie die PowerShell zur Installation über WinGet:
    ```powershell
    winget install Microsoft.DotNet.SDK.8
    ```

## 2. Der Dekompiler: Das Unsichtbare sehen
Da wir nicht über den ursprünglichen Quellcode des Spiels verfügen, müssen wir seine DLLs "dekompilieren".
*   **ILSpy / dnSpy:** Diese Tools verwandeln die DLLs des Spiels wieder in lesbaren C#-Code.
*   **Installation:**
    ```powershell
    dotnet tool install ilspycmd -g
    ```
*   **Verwendung:** Sie werden hauptsächlich `[Spielname]_Data/Managed/Assembly-CSharp.dll` dekompilieren. Hier befindet sich der Großteil der einzigartigen Logik des Spiels.

## 3. Vorbereiten des Spielverzeichnisses
Damit Ihr Mod funktioniert, muss die "Brücke" vorhanden sein.
1.  **Installieren Sie den Mod-Loader:** Führen Sie den MelonLoader-Installer aus oder entpacken Sie BepInEx in den Spielordner.
2.  **Tolk-Binärdateien:** Laden Sie die Tolk-Releases herunter und kopieren Sie zwei Dateien in denselben Ordner wie die `.exe` Ihres Spiels:
    *   `Tolk.dll` (Die Brücke)
    *   `nvdaControllerClient64.dll` (Speziell für die NVDA-Unterstützung erforderlich)

## 4. Das Scaffolding-Skript
Um Zeit zu sparen, verwenden wir ein Scaffolding-Skript, um ein neues Projekt zu erstellen.
*   **Speicherort:** `scripts/New-AccessibilityMod.ps1`
*   **Ausführung:**
    ```powershell
    pwsh scripts/New-AccessibilityMod.ps1 -ModName "SpielAccess" -Namespace "MeinName" -Loader "BepInEx"
    ```
Dies erstellt ein vorkonfiguriertes Projekt mit allen notwendigen Ordnern und Vorlagen, die wir in den kommenden Kapiteln besprechen werden.

## 5. IDE-Setup (Visual Studio oder VS Code)
Obwohl Sie Code in einem Texteditor schreiben können, bietet eine IDE IntelliSense (Code-Vervollständigung), was für das Entdecken von Spielmethoden unschätzbar wertvoll ist.
*   **Visual Studio 2022 Community:** Der Standard für die C#-Entwicklung.
*   **VS Code:** Eine leichtgewichtige Alternative (erfordert die C# Dev Kit-Erweiterung).

### Profi-Tipp: Die Logdatei
Lassen Sie die Logdatei Ihres Mod-Loaders während der Entwicklung immer geöffnet.
*   **MelonLoader:** `MelonLoader/Latest.log`
*   **BepInEx:** `BepInEx/LogOutput.log`
Hier werden alle Ihre `DebugLogger`-Meldungen angezeigt. Wenn Ihr Mod nicht spricht, sagt Ihnen das Log, warum.

---
*Weiter: [Kapitel 3: Das Nervensystem entwerfen](kapitel-3.md)*
