# Champion Interaction Testing Setup

This document provides step-by-step instructions for setting up and testing the champion interaction feature in the test scene.

## Prerequisites

- Unity Editor (2022.3 LTS recommended)
- League Coach Simulator project

## Step 1: Set Up the Champion Layer

1. Open the Unity Editor
2. Go to Edit > Project Settings > Tags and Layers
3. Add a new layer called "Champion" (use one of the User Layers, e.g., User Layer 8)
4. Click "Apply"

## Step 2: Configure the Champion Prefab

1. Find your champion prefab in the Project window
2. Select the prefab and add the following components if they don't already exist:
   - `CircleCollider2D` (set Radius to 1.5 and check "Is Trigger")
   - `ChampionColliderHelper` script

## Step 3: Configure the TestSceneSetup

1. Open the test scene at `Assets/Scenes/TestScene.unity`
2. Find the GameObject with the TestSceneSetup component
3. Make sure the `testEntityPrefab` field is assigned with your champion prefab
4. Ensure that `setupLanesAutomatically` is checked

## Step 4: Run the Test Scene

1. Click the Play button in the Unity Editor
2. The TestSceneSetup will automatically spawn opposing champions in the mid lane
3. The champions will move toward each other along the lane
4. When they get close enough, they will stop and face each other

## Troubleshooting

### Champions don't detect each other

- Make sure the "Champion" layer exists in your project
- Verify that champions have the correct layer assigned
- Check that champions have Collider2D components
- Ensure the detection range is appropriate (default is 3 units)

### Champions don't spawn

- Check that the testEntityPrefab is assigned in the TestSceneSetup
- Verify that lanes are being created properly
- Check the console for any error messages

### Champions don't move

- Check that lanes have waypoints assigned
- Verify that champions are assigned to lanes correctly
- Check the ChampionMovement component settings

## Advanced Configuration

You can adjust the following settings to customize the champion interaction behavior:

- **ChampionMovement.championDetectionRange**: Distance at which champions detect each other
- **ChampionColliderHelper.colliderRadius**: Size of the champion's collision detection area
- **Champion.engagementColor**: Color of the engagement indicator

## Next Steps

After successfully testing the basic champion interaction feature, you can extend it with:

1. Combat mechanics between champions
2. Health and damage systems
3. Special abilities and effects
4. AI behavior for autonomous champions 