# Lane System Documentation

## Overview

The Lane System provides a framework for creating and managing lanes in a MOBA-style game. It includes components for defining lanes, managing lane assignments, and controlling champion movement along lanes.

## Components

### Lane

The `Lane` class represents a single lane on the map. It contains:

- A list of waypoints that define the path of the lane
- Properties for lane name and team assignment
- Methods for accessing waypoints and calculating distances
- Debug visualization in the Scene view

### LaneManager

The `LaneManager` class manages all lanes in the scene. It provides:

- References to the Top, Mid, and Bottom lanes
- Methods for finding lanes by type or name
- Automatic team assignment to lanes
- Waypoint generation for lanes
- Debug logging for lane status

### Champion

The `Champion` class represents a champion in the game. It includes:

- Team and lane type assignment
- Methods for assigning the champion to a lane
- Team color visualization
- Visual indicators for champion interactions
- Layer assignment for champion detection

### ChampionMovement

The `ChampionMovement` class controls how champions move along lanes. It includes:

- Movement speed and waypoint threshold settings
- Lane assignment and direction control
- Methods for finding the closest lane and positioning at lane start
- Champion interaction detection and handling
- Methods for engaging with and disengaging from opponents

## Champion Interactions

The Lane System now includes champion interaction features:

### Detection

- Champions detect opposing champions within a configurable range
- Detection only occurs between champions of different teams in the same lane
- Uses Unity's Physics2D system with the "Champion" layer for efficient detection

### Engagement

- When champions detect an opponent, they stop moving and face each other
- Both champions enter an "engaged" state
- Visual indicators appear to show engagement status
- Champions remain engaged until manually disengaged

### Disengagement

- Champions can be disengaged using the `DisengageFromOpponent` method
- The TestUI includes a button to disengage all champions for testing
- When disengaged, champions resume normal movement along their lanes

## Editor Tools

The Lane System includes several custom editor tools to make it easier to work with lanes and champions:

### LaneEditor

The `LaneEditor` provides a custom inspector for the `Lane` component. It allows you to:

- Add, remove, and reorder waypoints
- Visualize the lane path in the Scene view
- Set lane properties like name and team

### LaneManagerEditor

The `LaneManagerEditor` provides a custom inspector for the `LaneManager` component. It allows you to:

- Create and find lanes in the scene
- Generate waypoints for lanes
- Assign teams to lanes
- Log lane status for debugging

### ChampionEditor

The `ChampionEditor` provides a custom inspector for the `Champion` component. It allows you to:

- Set champion properties like name and team
- Assign champions to lanes
- Position champions at lane start points
- Update team colors

### ChampionMovementEditor

The `ChampionMovementEditor` provides a custom inspector for the `ChampionMovement` component. It allows you to:

- Set movement properties like speed and waypoint threshold
- Find the closest lane to a champion
- Position champions at lane start points
- Log lane and waypoint information for debugging

## Usage

### Setting Up Lanes

1. Add a `LaneManager` component to a GameObject in your scene
2. Use the "Create All Lanes" button to create the Top, Mid, and Bottom lanes
3. Use the "Generate Waypoints" button to create default waypoints for each lane
4. Adjust the waypoints as needed using the `LaneEditor`

### Assigning Champions to Lanes

1. Add a `Champion` component to your champion GameObject
2. Set the team and lane type for the champion
3. Use the "Assign to Lane" button to assign the champion to the appropriate lane
4. Use the "Position At Lane Start" button to position the champion at the start of the lane

### Controlling Champion Movement

1. Add a `ChampionMovement` component to your champion GameObject
2. Set the movement properties like speed and waypoint threshold
3. The champion will automatically move along the assigned lane

### Testing Champion Interactions

1. Ensure the "Champion" layer exists in your Unity project (Edit > Project Settings > Tags and Layers)
2. Spawn champions from different teams in the same lane
3. Wait for them to detect each other and engage
4. Use the "Disengage All Champions" button in the TestUI to disengage them

## Debugging

The Lane System includes several debugging tools:

- Debug visualization in the Scene view for lanes and waypoints
- Debug logging for lane status and waypoint information
- Custom editor buttons for testing lane assignment and movement

## Tips

- Use the `LaneEditor` to adjust waypoints visually in the Scene view
- Use the `LaneManagerEditor` to quickly create and configure lanes
- Use the `ChampionEditor` to assign champions to lanes and set team colors
- Use the `ChampionMovementEditor` to test champion movement along lanes 