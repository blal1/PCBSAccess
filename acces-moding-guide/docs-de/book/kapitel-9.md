# Kapitel 9: In der Welt leben

Sich ohne Sicht durch eine 3D- oder 2D-Welt zu bewegen, ist eine große Herausforderung. In diesem Kapitel untersuchen wir Techniken zur Bereitstellung räumlicher und umweltbezogener Informationen.

## 1. Proximity Audio (Pieptöne)
Ein "Piepton" oder ein spezieller Sound ist der effektivste Weg, um einen Spieler zu einem Objekt zu führen.
*   **Entfernung = Tonhöhe/Frequenz:** Je näher der Spieler kommt, desto schneller oder höher wird der Piepton.
*   **Stereo-Position:** Nutzen Sie das spielinterne 3D-Audiosystem, um den Piepton genau dort zu platzieren, wo sich das Objekt in der Welt befindet.

## 2. Kardinalrichtungen (Der Kompass)
Stellen Sie eine Tastenkombination (z. B. F3) bereit, um die aktuelle Ausrichtung des Spielers anzusagen.
`"Blickt nach Nordosten."`

## 3. Welt-Scanning
Implementieren Sie einen "Scan"-Modus, der die nächstgelegenen interaktiven Objekte oder Feinde auflistet.
1.  Suchen Sie nach allen `GameObjects` innerhalb eines bestimmten Radius.
2.  Filtern Sie nach solchen, die "interaktiv" sind (z. B. eine `IInteractable`-Komponente haben).
3.  Sortieren Sie nach Entfernung.
4.  Ansage: `"Am nächsten: Truhe (5 Meter nördlich), Feind (12 Meter westlich)."`

## 4. Informationen räumlich darstellen
Wenn ein Ereignis in der Welt passiert (z. B. eine Explosion oder ein Schrei eines Feindes), können Sie die Ansage räumlich verorten:
`"Explosion! (Weit im Nordwesten)"`

### Die "Leuchtfeuer"-Strategie (Beacon)
Ermöglichen Sie es dem Spieler bei Fernreisen, ein "Leuchtfeuer" auf ein Questziel zu setzen. Der Mod liefert dann einen kontinuierlichen oder ausgelösten Ton, der ihn dorthin führt.

### Code-Muster: Proximity-Erkennung (Näherung)
```csharp
void Update() {
    if (Time.time - _lastCheck < 0.5f) return; // Auf 2 Mal pro Sekunde drosseln
    _lastCheck = Time.time;

    var nearest = FindNearestObject();
    if (nearest != null && distance < 2.0f) {
        // Hohe Priorität: Piepton oder Ansage, wenn man sehr nahe ist
        PlayProximitySound(distance);
    }
}
```

Welterkundung bedeutet, **mentale Geometrie** bereitzustellen. Ihr Ziel ist es, dem Spieler zu helfen, eine Karte der Welt in seinem Kopf aufzubauen.

---
*Weiter: [Kapitel 10: Fortgeschrittenes Abfangen](kapitel-10.md)*
