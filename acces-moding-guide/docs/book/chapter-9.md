# Chapter 9: Living in the World

Moving through a 3D or 2D world without sight is a major challenge. In this chapter, we explore techniques to provide spatial and environmental information.

## 1. Proximity Audio (Beeps)
A "Beep" or a specialized sound is the most effective way to lead a player to an object.
*   **Distance = Pitch/Frequency:** As the player gets closer, the beep becomes faster or higher pitched.
*   **Stereo Position:** Use the game's own 3D audio system to place the beep exactly where the object is in the world.

## 2. Cardinal Directions (The Compass)
Provide a shortcut key (e.g., F3) to announce the player's current orientation.
`"Facing North-East."`

## 3. World Scanning
Implement a "Scan" mode that lists the nearest interactive objects or enemies.
1.  Search for all `GameObjects` within a certain radius.
2.  Filter for those that are "Interactive" (e.g., have an `IInteractable` component).
3.  Sort by distance.
4.  Announce: `"Nearest: Chest (5 meters North), Enemy (12 meters West)."`

## 4. Spatializing Information
When an event happens in the world (e.g., an explosion or an enemy cry), you can spatialize the announcement:
`"Explosion! (Far North-West)"`

### The "Beacon" Strategy
For long-distance travel, allow the player to set a "Beacon" on a quest objective. The mod then provides a continuous or triggered sound leading them to it.

### Code Pattern: Proximity Detection
```csharp
void Update() {
    if (Time.time - _lastCheck < 0.5f) return; // Throttle to 2 times a second
    _lastCheck = Time.time;

    var nearest = FindNearestObject();
    if (nearest != null && distance < 2.0f) {
        // High-priority beep or announcement when very close
        PlayProximitySound(distance);
    }
}
```

World exploration is about providing **mental geometry**. Your goal is to help the player build a map of the world in their head.

---
*Next: [Chapter 10: Advanced Interception](chapter-10.md)*
