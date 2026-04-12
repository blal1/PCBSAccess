# Kapitel 5: Die Sinne (Extraktion & Reflection)

Um ein Spiel barrierefrei zu machen, muss Ihr Mod "sehen" können, was auf dem Bildschirm passiert. Da wir ein Spiel von außen modifizieren, verwenden wir zwei primäre "Sinne": den **UITextExtractor** für visuelle Elemente und den **ReflectionHelper** für die interne Logik.

## 1. UITextExtractor: Den Bildschirm lesen
Die Unity-UI wird normalerweise aus Komponenten wie `Text` oder `TextMeshPro` aufgebaut. Der `UITextExtractor` ist ein Werkzeug, das die Hierarchie des Spiels durchsucht, um diese Komponenten zu finden und ihren Text zurückzugeben.

### Das Problem mit Schaltflächen (Buttons)
Oft hat eine Schaltfläche selbst keine `Text`-Eigenschaft; der Text befindet sich in einem "Child"-Objekt (untergeordnetes Objekt). Unser `UITextExtractor` ist darauf ausgelegt, die untergeordneten Objekte zu durchsuchen, um die wahrscheinlichste Beschriftung für eine Schaltfläche zu finden.

```csharp
// Einfaches Anwendungsbeispiel
GameObject currentButton = GetCurrentlyFocusedButton();
string label = UITextExtractor.GetText(currentButton);
ScreenReader.Say(label);
```

## 2. ReflectionHelper: Gedanken lesen
Manchmal befinden sich die benötigten Informationen gar nicht auf dem Bildschirm – sie sind in einem "privaten" Feld im Code des Spiels eingeschlossen. **Reflection** ist ein C#-Feature, das es uns ermöglicht, auf diese privaten Felder zuzugreifen.

### Warum ein Helper?
Reflection ist langsam. Wenn Sie jeden Frame nach einem Feldnamen suchen, wird die Leistung des Spiels massiv einbrechen. Unser `ReflectionHelper` löst dies durch **Caching** der Feldinformationen beim ersten Auffinden.

```csharp
// Zugriff auf ein privates "health"-Feld in der Player-Klasse
int health = ReflectionHelper.GetField<int>(playerInstance, "currentHealth");
```

## 3. Defensives Programmieren (Die Sinne müssen robust sein)
Die UI und der interne Code des Spiels können sich zwischen Updates ändern. Wenn Ihr Mod davon ausgeht, dass ein Objekt existiert, und dieses fehlt, wird der Mod abstürzen.

**Regel:** Prüfen Sie immer auf Null (`null`).
```csharp
var textComp = panel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
if (textComp == null) {
    DebugLogger.Log("Fehler: TextMeshPro auf dem Inventar-Panel konnte nicht gefunden werden.");
    return;
}
```

## 4. Die Sinne kombinieren
Eine hochwertige Ansage kombiniert oft beide Sinne:
1.  Verwenden Sie den `ReflectionHelper`, um die **Gegenstands-ID** zu finden.
2.  Verwenden Sie die ID, um den **Gegenstandsnamen** in der Datenbank des Spiels nachzuschlagen.
3.  Verwenden Sie den `UITextExtractor`, um zu sehen, ob ein **Preisschild** am UI-Slot angebracht ist.
4.  Ansage: "[Name], [Preis] Gold."

---
*Weiter: [Kapitel 6: Die Matrix lesen](kapitel-6.md)*
