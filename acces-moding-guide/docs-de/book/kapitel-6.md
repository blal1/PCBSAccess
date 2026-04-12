# Kapitel 6: Die Matrix lesen

Reverse Engineering – der Prozess des Verstehens von Code, den jemand anderes geschrieben hat – ist der anspruchsvollste und lohnendste Teil des Moddings. Für einen blinden Modder ist dies kein visueller Prozess. Es ist eine Such-und-Vergleichs-Mission.

## 1. Die Denkweise beim Dekompilieren
Wenn Sie die DLL eines Spiels in einem Dekompiler (wie `ilspycmd` oder `dnSpy`) öffnen, sehen Sie eine Rekonstruktion des ursprünglichen C#-Codes. Er wird unordentlich sein. Variablennamen wie `k__BackingField` sind häufig. Keine Panik.

## 2. Navigieren ohne Sicht
Nutzen Sie die Suchfunktionen und "Gehe zu Definition" (Go to Definition) Ihrer IDE. Dies sind Ihre primären Augen.

### Die Suchstrategie
1.  **Nach Zeichenfolgen suchen:** Wenn Sie "Inventar" auf dem Bildschirm sehen, suchen Sie nach genau diesem Wort im dekompilierten Code. Sie finden vielleicht eine Klasse namens `UI_Inventory` oder eine Konstante wie `STRING_INVENTORY_TITLE`.
2.  **Nach Singletons suchen:** Suchen Sie nach `static [Klassenname] instance` oder `public static [Klassenname] i`. Spiele verwenden oft Singletons für die "Manager"-Klassen, die den Spielzustand steuern.
3.  **Nach Präfixen suchen:** Verwenden Sie `grep`, um alle Klassennamen aufzulisten. Suchen Sie nach gängigen Unity-Mustern: `Manager`, `Controller`, `UI`, `Screen`, `Panel`.

## 3. Die "Matrix"-Technik: Live vs. Statisch vergleichen
Verwenden Sie ein Tool wie **UnityExplorer** (wenn Sie einen sehenden Helfer haben oder durch seine textbasierten Menüs navigieren können), um die Live-Hierarchie der `GameObjects` zu sehen.
*   Wenn UnityExplorer sagt, dass eine Schaltfläche `btn_Accept` heißt, suchen Sie im Code nach `btn_Accept`.
*   Finden Sie heraus, wo diese Schaltfläche zugewiesen wird.
*   Suchen Sie nach dem `OnClick`-Listener. Dies führt Sie direkt zu dem Code, der ausgeführt wird, wenn die Schaltfläche gedrückt wird.

## 4. Die "Update"-Schleife identifizieren
Die meiste Unity-Logik findet in der `Update()`-Methode statt. Suchen Sie in einem Dekompiler nach Klassen, die von `MonoBehaviour` erben.
*   Suchen Sie nach `void Update()`.
*   Suchen Sie innerhalb von `Update()` nach `Input.GetKeyDown`. Dies sagt Ihnen, auf welche Tasten das Spiel hört.

### Profi-Tipp: Dokumentation ist alles
Jedes Mal, wenn Sie einen Klassen- oder Methodennamen finden, dokumentieren Sie ihn in Ihrer `game-api.md`.
```markdown
## Inventar-System
- Klasse: `UI_Inventory_Manager`
- Singleton: `UI_Inventory_Manager.i`
- Öffnungsmethode: `void Show()`
- Feld für ausgewählten Gegenstand: `private int _currentIndex`
```
Diese "API-Karte" ist das wertvollste Asset, das Sie aufbauen werden.

---
*Weiter: [Kapitel 7: Spiellogik identifizieren](kapitel-7.md)*
