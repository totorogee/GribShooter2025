# Game Systems Documentation

This document describes all the new systems included in the "basic game function draft" commit.

---

## Table of Contents

1. [Melee Weapon System](#melee-weapon-system)
2. [Grappling Hand System](#grappling-hand-system)
3. [Item Spawner System](#item-spawner-system)
4. [Movement Pattern System](#movement-pattern-system)
5. [Player System](#player-system)
6. [Score System](#score-system)
7. [Utility Systems](#utility-systems)

---

## Melee Weapon System

**Location:** `Assets/Scripts/MeleeWeaponSystem/dev/`

### Overview
The Melee Weapon System provides a flexible sword swinging animation system for 2D games. It supports configurable swing directions, speeds, visibility states, and pivot-based rotation.

### Components

#### `SwingSword.cs`
Controls a 2D sword swinging animation with configurable rest position, swing direction, speed, and visibility.

**Key Features:**
- Left/Right swing directions
- Configurable rest and swing end angles
- Automatic return to rest position
- Visibility control (visible when resting vs. swinging)
- Pivot point rotation support
- Smooth fade in/out transitions

**Usage:**
```csharp
// Attach to a GameObject with a SpriteRenderer
// Configure in Inspector:
// - Swing Direction: Left or Right
// - Rest Angle: -45 degrees
// - Swing End Angle: 45 degrees
// - Rotation Speed: 720 degrees/second
// - Auto Swing On Input: true (uses Mouse0 by default)

// Manual control:
sword.StartSwing();        // Trigger a swing
sword.ReturnToRest();      // Return to rest position
sword.SnapToRest();        // Instantly snap to rest
```

**Public Methods:**
- `StartSwing()` - Manually trigger a swing
- `ReturnToRest()` - Start returning to rest position
- `SnapToRest()` - Instantly snap to rest position
- `SetSwingDirection(SwingDirection)` - Change swing direction
- `SetRestAngle(float)` - Set rest angle
- `SetSwingEndAngle(float)` - Set swing end angle
- `IsSwinging()` - Check if currently swinging
- `IsAtRest()` - Check if at rest position

---

## Grappling Hand System

**Location:** `Assets/Scripts/GrapplingHandSystem/dev/`

### Overview
A complete grappling hook system that allows players to shoot hands that can grab objects and return to the player. Supports dual-hand gameplay with independent controls.

### Components

#### `HandShooter.cs`
Main component for shooting grappling hands. Supports two independent hands (left/right) with separate configurations.

**Key Features:**
- Dual hand support (L key for left, K key for right)
- Configurable hand appearance (prefab or auto-generated sprite)
- Independent cooldowns per hand
- Optional multiple hands at once
- Custom shoot points per hand

**Usage:**
```csharp
// Attach to player GameObject
// Configure in Inspector:
// - Left Hand Config: Speed, Range, Cooldown, etc.
// - Right Hand Config: Speed, Range, Cooldown, etc.
// - Left/Right Shoot Points: Optional Transform references

// Controls:
// L key - Shoot left hand
// K key - Shoot right hand
```

**Public Methods:**
- `GetLeftHandInstance()` - Get left hand GrapplingHand component
- `GetRightHandInstance()` - Get right hand GrapplingHand component
- `IsLeftHandActive()` - Check if left hand is currently active
- `IsRightHandActive()` - Check if right hand is currently active
- `GetLeftHandCooldownProgress()` - Get cooldown progress (0-1)
- `GetRightHandCooldownProgress()` - Get cooldown progress (0-1)

#### `GrapplingHand.cs`
Controls the grappling hand projectile behavior. Handles shooting, grabbing, and returning to player.

**Key Features:**
- Straight-line projectile movement
- Automatic object detection and grabbing
- Returns to home position (shoot point)
- Hand reuse (not destroyed on return)
- Follows player rotation when parented

**States:**
- `Extending` - Moving forward from player
- `Waiting` - Waiting at max range
- `Returning` - Coming back to player

**Public Methods:**
- `Initialize(Vector2 direction, Transform player, float speed, float maxRange, float waitTime, float grabRadius)` - Initialize hand with settings
- `SetHomePosition(Transform)` - Set return position
- `GetGrabbedObject()` - Get currently grabbed object
- `IsGrabbing()` - Check if currently grabbing something

#### `IGrabbable.cs`
Interface for objects that can be grabbed by the grappling hand.

**Required Methods:**
- `bool CanBeGrabbed()` - Check if object can be grabbed
- `void OnGrabbed(Transform player)` - Called when grabbed
- `void OnReleased()` - Called when released

**Example Implementation:**
```csharp
public class GrabbableObject : MonoBehaviour, IGrabbable
{
    public bool CanBeGrabbed() { return true; }
    public void OnGrabbed(Transform player) { /* Handle grab */ }
    public void OnReleased() { /* Handle release */ }
}
```

#### `GrabbableObject.cs` & `GrabbableExample.cs`
Example implementations of the `IGrabbable` interface for reference.

---

## Item Spawner System

**Location:** `Assets/Scripts/ItemSpawnerSystem/dev/`

### Overview
A flexible item spawning system that can spawn multiple item types with configurable spawn chances, counts, and behaviors when objects are destroyed or disabled.

### Components

#### `ItemSpawner.cs`
Spawns items on destroy or when manually triggered. Supports multiple spawn items with configurable spawn chances.

**Key Features:**
- Multiple spawn items with individual chances
- Spawn on destroy/disable triggers
- Random spawn counts (min/max)
- Spawn position offsets
- Velocity inheritance from parent object
- Random velocity application

**Usage:**
```csharp
// Attach to any GameObject
// Configure in Inspector:
// - Spawn Items: List of items to spawn
//   - Prefab: Item to spawn
//   - Spawn Chance: 0-100%
//   - Min/Max Count: How many to spawn
//   - Spawn Offset Range: Random position offset
//   - Inherit Velocity: Use parent's velocity
//   - Random Velocity: Additional random velocity

// Manual spawning:
itemSpawner.Spawn();                    // Spawn all items
itemSpawner.SpawnSpecificItem(0);       // Spawn item at index 0
```

**Public Methods:**
- `Spawn()` - Manually trigger spawning
- `SpawnSpecificItem(int index)` - Spawn specific item by index
- `AddSpawnItem(GameObject, float, int, int)` - Add spawn item at runtime
- `ClearSpawnItems()` - Remove all spawn items
- `GetSpawnItemCount()` - Get number of configured items
- `SetSpawnOnDestroy(bool)` - Enable/disable spawn on destroy
- `SetSpawnOnDisable(bool)` - Enable/disable spawn on disable

---

## Movement Pattern System

**Location:** `Assets/Scripts/MovementPatternSystem/dev/`

### Overview
A comprehensive movement system that allows GameObjects to move in various patterns (linear, sin wave, zigzag, circle, spiral, figure-8, square, bounce). Includes boundary checking and look-at functionality.

### Components

#### `PatternMovement.cs`
Moves GameObject in various patterns on X or Y axis. Can destroy object when it moves outside a defined boundary area.

**Movement Patterns:**
- `Linear` - Straight line movement
- `SinWave` - Smooth sine wave motion
- `Zigzag` - Sharp zigzag pattern
- `Circle` - Circular motion
- `Spiral` - Expanding/contracting spiral
- `Figure8` - Figure-8 / infinity pattern
- `Square` - Square wave (sudden jumps)
- `Bounce` - Bouncing motion

**Key Features:**
- 8 different movement patterns
- X or Y axis primary movement
- Configurable speed, amplitude, frequency
- Boundary checking (two points or sprite renderer)
- Automatic destruction outside boundaries
- Look-at target functionality
- Smooth or instant rotation

**Usage:**
```csharp
// Attach to GameObject with Rigidbody2D
// Configure in Inspector:
// - Pattern: Choose movement pattern
// - Primary Axis: X or Y
// - Speed: Movement speed
// - Wave Amplitude/Frequency: For wave patterns
// - Boundary Type: None, TwoPoints, or SpriteRenderer
// - Destroy Outside Boundary: Auto-destroy when out of bounds

// Runtime control:
patternMovement.SetPattern(MovementPattern.SinWave);
patternMovement.SetSpeed(10f);
patternMovement.SetLookAtTarget(playerTransform);
```

**Public Methods:**
- `SetPattern(MovementPattern)` - Change movement pattern
- `SetAxis(MovementAxis)` - Change primary axis
- `SetSpeed(float)` - Set movement speed
- `SetSinWaveParameters(float, float, float)` - Set wave parameters
- `SetTwoPointBoundary(Vector2, Vector2)` - Set boundary points
- `SetSpriteBoundary(GameObject)` - Set sprite boundary
- `SetDestroyOutsideBoundary(bool)` - Enable/disable boundary destruction
- `SetLookAtEnabled(bool)` - Enable/disable look-at
- `SetLookAtTarget(Transform)` - Set look-at target
- `SetRotationSpeed(float)` - Set rotation speed (0 = instant)

---

## Player System

**Location:** `Assets/Scripts/Player/dev/`

### Overview
Complete player control system with 8-directional movement, rotation, and burst-fire shooting mechanics.

### Components

#### `PlayerController.cs`
Controls player movement and rotation in 8 directions on the XY plane.

**Key Features:**
- 8-directional movement (QWEADZXC keys)
- Alternative 4-directional mode (WASD)
- Manual rotation (O/P keys)
- Auto-snap to 45-degree angles
- Grid snapping support

**Controls:**
- **Movement (8-directional):**
  - Q=↖, W=↑, E=↗
  - A=←, D=→
  - Z=↙, X=↓, C=↘
- **Movement (4-directional WASD mode):**
  - W=↑, A=←, S=↓, D=→
- **Rotation:**
  - O = Counter-clockwise
  - P = Clockwise
  - Auto-snaps to nearest 45° when input stops

**Public Methods:**
- `SnapToGrid()` - Snap player to nearest grid point
- `SnapPositionToGrid(Vector2)` - Snap position to grid
- `GetNearestGridPoint()` - Get nearest grid point

#### `PlayerShooting.cs`
Controls player shooting behavior with burst fire mechanics.

**Key Features:**
- Burst fire (configurable bullets per burst)
- Cooldown between bursts
- Configurable bullet interval
- Optional audio support
- Fire point support

**Controls:**
- **Space** - Hold to shoot (triggers bursts)

**Usage:**
```csharp
// Attach to player GameObject
// Configure in Inspector:
// - Bullet Prefab: Bullet GameObject
// - Fire Point: Optional spawn position
// - Bullet Speed: Projectile speed
// - Burst Size: Bullets per burst (default: 5)
// - Bullet Interval: Time between bullets
// - Burst Cooldown: Cooldown after burst
```

**Public Methods:**
- `GetBulletsRemainingInBurst()` - Get remaining bullets in current burst
- `IsInCooldown()` - Check if in cooldown
- `GetCooldownTimeRemaining()` - Get cooldown time left

#### `Bullet.cs`
Bullet projectile component (implementation details in codebase).

---

## Score System

**Location:** `Assets/Scripts/ScoreSystem/dev/`

### Overview
A flexible score management system that supports multiple score types (gold, hp, energy, etc.) and integrates with the EventManager for event-driven score updates.

### Components

#### `ScoreManager.cs`
Manages multiple score types dynamically. Listens to EventManager for score updates.

**Key Features:**
- Dynamic score type creation
- Event-driven updates via EventManager
- PrefabSingleton pattern (auto-loads from Resources)
- Score clamping support
- Debug display in Inspector

**Event Integration:**
```csharp
// Add score via event:
EventManager.TriggerEvent("ScoreChange", "gold", 100);  // Add 100 gold
EventManager.TriggerEvent("ScoreSet", "hp", 50);        // Set hp to 50
EventManager.TriggerEvent("ScoreReset", "gold");        // Reset gold to 0

// Direct access:
int gold = ScoreManager.Instance.GetScore("gold");
ScoreManager.Instance.AddScore("hp", -10);  // Subtract 10 hp
```

**Public Methods:**
- `GetScore(string)` - Get score value
- `AddScore(string, int)` - Add/subtract score
- `SetScore(string, int)` - Set score to value
- `ResetScore(string)` - Reset score to 0
- `ResetAllScores()` - Reset all scores
- `ClearAllScores()` - Remove all scores
- `HasScore(string)` - Check if score exists
- `GetAllScoreNames()` - Get all score type names
- `GetAllScores()` - Get all scores as dictionary
- `ClampScore(string, int, int)` - Clamp score between min/max
- `RemoveScore(string)` - Remove specific score

**Event Names (configurable):**
- `ScoreChange` - (string scoreName, int amount)
- `ScoreSet` - (string scoreName, int value)
- `ScoreReset` - (string scoreName)

**Broadcast Events:**
- `Score_{scoreName}_Changed` - (int oldValue, int newValue)
- `ScoreUpdated` - (string scoreName, int newValue)

---

## Utility Systems

### SceneSingleton

**Location:** `Assets/Scripts/SceneSingleton.cs`

Generic singleton pattern for MonoBehaviour components that exist only in the current scene.

**Usage:**
```csharp
public class GameManager : SceneSingleton<GameManager>
{
    public void DoSomething() { ... }
}

// Access:
GameManager.Instance.DoSomething();
```

**Features:**
- Thread-safe instance creation
- Scene-specific (doesn't persist across scenes)
- Auto-creates instance if none exists
- Prevents duplicate instances

---

## System Integration

### Event System
Many systems integrate with the EventManager located at `Assets/Scripts/Tools/Event/EventManager.cs`:

- **ScoreSystem** - Listens for score change events
- **Other systems** - Can trigger events for inter-system communication

### File Organization
All systems follow the project's folder structure:
- Main scripts in `dev/` subfolder
- Each system in its own folder
- Tools in `Assets/Scripts/Tools/`

---

## Setup Instructions

### Melee Weapon System
1. Create a GameObject with SpriteRenderer
2. Add `SwingSword` component
3. Configure swing settings in Inspector
4. Optionally set pivot point for rotation

### Grappling Hand System
1. Add `HandShooter` to player GameObject
2. Configure left/right hand settings
3. Set shoot points (optional)
4. Implement `IGrabbable` on objects you want to grab

### Item Spawner System
1. Add `ItemSpawner` to any GameObject
2. Configure spawn items list
3. Set spawn triggers (on destroy/disable)
4. Assign item prefabs

### Movement Pattern System
1. Add `PatternMovement` to GameObject with Rigidbody2D
2. Select movement pattern
3. Configure speed and pattern parameters
4. Set boundaries if needed

### Player System
1. Add `PlayerController` to player GameObject
2. Add `PlayerShooting` to player GameObject
3. Assign bullet prefab to shooting component
4. Configure movement and shooting settings

### Score System
1. Create prefab at `Resources/Singleton/ScoreManager.prefab`
2. Add `ScoreManager` component
3. Configure event names (optional)
4. Access via `ScoreManager.Instance`

---

## Notes

- All systems are designed for 2D games
- Systems use Unity's new Input System where applicable
- Event-driven architecture is preferred for inter-system communication
- All systems include comprehensive Inspector configuration options
- Debug logging is included for development

---

## Future Enhancements

Potential improvements for each system:
- **MeleeWeaponSystem**: Damage detection, combo system
- **GrapplingHandSystem**: Chain mechanics, swing physics
- **ItemSpawnerSystem**: Weighted spawn tables, spawn pools
- **MovementPatternSystem**: Custom pattern scripting
- **PlayerSystem**: Dash mechanics, weapon switching
- **ScoreSystem**: Score multipliers, achievements

