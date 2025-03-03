# Test Scene Setup Guide

This guide explains how to set up and use the test scene for testing map functionality and entity spawning.

## Quick Setup

1. Open the TestScene from `Assets/Scenes/TestScene.unity`
2. Create an empty GameObject and name it "TestSceneManager"
3. Add the `TestSceneManager` component to it
4. Add the `TestSceneSetup` component to the same GameObject
5. In the Inspector, click the "Set Up Test Scene" button in the TestSceneSetup component
6. Press Play to test the scene

## Manual Setup

If you prefer to set up the scene manually:

1. Open the TestScene from `Assets/Scenes/TestScene.unity`
2. Create an empty GameObject and name it "TestSceneSetup"
3. Add the `TestSceneSetup` component to it
4. Assign the Map prefab from `Assets/_Project/Prefabs/Map/Map.prefab` to the "Map Prefab" field
5. Add the `CameraController` component to the Main Camera if it doesn't have one
6. Assign the Main Camera's CameraController to the "Camera Controller" field in TestSceneSetup
7. Create a test entity prefab or use the "Create Test Entity Prefab" button in the TestSceneSetup inspector
8. Assign the test entity prefab to the "Test Entity Prefab" field
9. Press Play to test the scene

## Using the Test Scene

Once the scene is set up, you can:

- Use the "Spawn Test Entities" button to spawn test entities at the blue and red spawn points
- Use the "Clear Test Entities" button to remove all test entities from the scene
- Use the camera controls to navigate the map (mouse wheel to zoom, right-click drag to pan)
- View debug information in the top-left corner of the game view

## Customizing the Test Scene

You can customize the test scene by:

- Modifying the Map prefab to change the map layout
- Creating different test entity prefabs with different behaviors
- Adjusting the camera settings in the CameraController component
- Modifying the TestSceneSetup component settings

## Troubleshooting

If you encounter issues:

- Make sure the Map prefab has BlueSpawn and RedSpawn child objects
- Check that the CameraController is properly configured
- Ensure the TestEntity tag exists in the Tag Manager
- Check the console for error messages

## Adding New Test Functionality

To add new test functionality:

1. Modify the TestSceneSetup script to add new test methods
2. Update the TestSceneSetupEditor script to add new buttons for the new functionality
3. Modify the TestSceneManager script to expose the new functionality in the game view 