# Kapitel 1: Das Modding-Ökosystem

Bevor wir unsere erste Codezeile schreiben, müssen wir die Umgebung verstehen, in der wir arbeiten. Accessibility-Modding ist die Kunst, neues Verhalten in bestehende Software zu injizieren. Für Unity-Spiele wird dies durch einen leistungsstarken Stack von Community-gesteuerten Tools ermöglicht.

## 1. Die Unity-Engine
Unity ist eine der beliebtesten Game-Engines der Welt. Sie verwendet C# für ihre Skriptlogik. Wenn ein Unity-Spiel "kompiliert" wird, wird sein Code in Intermediate Language (IL) umgewandelt und in DLL-Dateien gespeichert (wie `Assembly-CSharp.dll`).

## 2. Mod-Loader: BepInEx und MelonLoader
Ein "Mod-Loader" ist ein spezialisiertes Programm, das zwischen Ihrem Betriebssystem und dem Spiel sitzt. Seine Hauptaufgabe besteht darin, die DLL Ihres Mods beim Start in den Speicherbereich des Spiels zu "injizieren".

*   **BepInEx:** Der Industriestandard für viele Unity-Mods. Er ist leichtgewichtig, stabil und hochgradig kompatibel mit vielen älteren Spielen.
*   **MelonLoader:** Eine moderne Alternative, die sich hervorragend für "Il2Cpp"-Spiele eignet (Spiele, bei denen der C#-Code zur Leistungssteigerung in C++ umgewandelt wurde).

In diesem Buch werden wir beide unterstützen, da sie dieselbe grundlegende Logik teilen.

## 3. Harmony: Der Master-Interceptor
Harmony ist eine Bibliothek, die sowohl in BepInEx als auch in MelonLoader enthalten ist. Sie ermöglicht es Ihnen, Spielmethoden zur Laufzeit zu "patchen". Sie können Ihren Code *vor* einer Spielmethode (Prefix), *danach* (Postfix) oder sogar die Methode komplett ersetzen (Transpiler). So erkennen wir, ob ein Menü geöffnet wird oder ein Spieler Schaden erleidet.

## 4. Tolk: Die Brücke zum Screenreader
Der Screenreader (NVDA, JAWS usw.) ist eine externe Anwendung. Ihr Mod kann nicht direkt mit ihm kommunizieren. **Tolk** ist eine native C++-Bibliothek, die als Brücke dient. Sie erkennt, welchen Screenreader der Spieler verwendet, und sendet Ihren Text an diesen.

### So funktioniert die Kette:
1.  **Das Spiel** löst ein Ereignis aus (z. B. ein Mausklick auf eine Schaltfläche).
2.  **Harmony** fängt dieses Ereignis ab und ruft Ihren **Mod** auf.
3.  Ihr **Mod** identifiziert, was passiert ist, und generiert eine Textzeichenfolge.
4.  Ihr **Mod** sendet diese Zeichenfolge an **Tolk**.
5.  **Tolk** gibt den Text über den **Screenreader** aus.

Das Verständnis dieser Kette ist entscheidend. Wenn ein Glied unterbrochen ist – wenn der Mod nicht geladen wird, wenn der Patch nicht auslöst oder wenn die Tolk-DLL fehlt – erlebt der Spieler Stille.

---
*Weiter: [Kapitel 2: Einrichten Ihres Labors](kapitel-2.md)*
