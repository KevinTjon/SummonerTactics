# Summoner Tactics

A 2D game project built in Unity.

## Project Structure

The project is organized into the following directories:

- **Scripts**: Contains all C# scripts organized by functionality
  - **Core**: Core game systems
    - **GameManager**: Central game management scripts
  - **Utils**: Utility scripts and helper functions
- **Prefabs**: Reusable game objects
  - **Map**: Map-related prefabs
- **Scenes**: Game scenes
- **Art**: Visual assets
  - **Sprites**: 2D sprite assets
- **ScriptableObjects**: Data containers

## Core Systems

### GameManager

The GameManager is responsible for:
- Managing the game state
- Handling scene transitions
- Coordinating other manager systems

### CameraController

The CameraController handles:
- Camera positioning and movement
- Zoom functionality
- Map boundary enforcement

## Setup Instructions

### Camera Setup

1. Create a new scene
2. Add a Main Camera to the scene
3. Attach the CameraController script to the camera
4. Configure the camera settings:
   - Set Projection to "Orthographic"
   - Adjust Size to fit your map
   - Set Near and Far Clipping Planes
5. If you have a map object, assign it to the Map Object field
6. Adjust the Default Map Width and Height to match your map dimensions

## Controls

- **Mouse Wheel**: Zoom in/out
- **R Key**: Reset camera position and zoom

## Troubleshooting

If you encounter issues with the camera:
1. Check that the camera is set to Orthographic mode
2. Verify that the map dimensions are set correctly
3. Ensure the camera is positioned above the map center
4. Check for any console errors related to the CameraController

## Development Guidelines

- Follow Unity's component-based architecture
- Use descriptive variable and function names
- Keep scripts focused on a single responsibility
- Document public methods and properties
- Use ScriptableObjects for configuration data 