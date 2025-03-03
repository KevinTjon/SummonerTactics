# Testing Lane Movement System

This document provides instructions on how to set up and test the lane movement system for champions in Summoner Tactics.

## Overview

The lane movement system allows champions to move along predefined lanes on the map. The system consists of the following components:

- **Lane**: Defines a lane on the map with waypoints for champions to follow.
- **LaneManager**: Manages lanes and provides utilities for lane setup.
- **Champion**: Represents a champion with team and lane assignment.
- **ChampionMovement**: Handles movement of champions along lanes.
- **TestEntity**: A simple test entity for testing champion movement.
- **TestUI**: Provides UI controls for testing champion movement.

## Setting Up the Test Scene

1. Create a new scene or use the existing test scene.
2. Add the `TestSceneSetup` script to a GameObject in the scene.
3. Assign the map prefab to the `Map Prefab` field.
4. Assign a test entity prefab to the `Test Entity Prefab` field.
5. Assign the `CameraController` to the `Camera Controller` field.
6. Add the `TestUI` script to a UI GameObject in the scene.
7. Set up UI buttons and dropdowns for testing.

## Camera Controller Integration

The test scene now integrates with the `CameraController` for better camera management:

1. The `TestSceneSetup` script automatically configures the camera based on the map bounds.
2. If a `MapBounds` object is found in the map, the camera will use its dimensions.
3. The camera will be positioned to show the entire map.
4. You can adjust camera settings in the `CameraController` component.

## Setting Up Lanes in the Map

The lane movement system requires lanes to be set up in the map. Each lane should have:

1. A parent GameObject with a `Lane` component.
2. Child GameObjects to define waypoints along the lane.

**Note:** Auto-generation of waypoints is now disabled by default. You should create waypoints manually for better control over champion movement paths.

### Manual Lane Setup (Recommended)

To set up lanes manually:

1. Create a GameObject for each lane (TopLane, MidLane, BottomLane).
2. Add the `Lane` component to each lane GameObject.
3. Create child GameObjects for each waypoint along the lane.
4. Use the Lane Editor to manage waypoints:
   - Select a Lane GameObject in the Inspector
   - Add, remove, or reorder waypoints
   - Drag waypoints in the Scene view to position them

### Automatic Lane Setup (Legacy)

If you want to use automatic waypoint generation:

1. Set `autoGenerateWaypoints` to `true` in the `LaneManager` component.
2. Create start and end points for each lane.
3. The `LaneManager` will generate waypoints between these points.

## Testing Champion Movement

1. Use the `TestUI` to spawn test entities.
2. Select the lane type and team for the test entities.
3. Click the "Start Movement" button to start movement.
4. Click the "Stop Movement" button to stop movement.
5. Use the "Clear" button to remove all test entities.

## Debugging

The lane movement system includes debug visualization features:

- Enable `showDebugInfo` in the `TestSceneSetup` component to see detailed setup information.
- Enable `showDebugInfo` in the `Lane` component to visualize waypoints.
- Enable `debugMode` in the `ChampionMovement` component to log movement information.
- The system will log warnings if lanes are missing waypoints.

## Integration with Game Logic

To integrate the lane movement system with your game logic:

1. Assign champions to lanes using the `SetLaneType` method.
2. Set the team using the `SetTeam` method.
3. Control movement using the `ChampionMovement` component.

## Example Code

```csharp
// Assign a champion to a lane
Champion champion = GetComponent<Champion>();
champion.SetTeam(Team.Blue);
champion.SetLaneType(LaneType.Bottom);

// Start movement
ChampionMovement movement = GetComponent<ChampionMovement>();
movement.enabled = true;
``` 