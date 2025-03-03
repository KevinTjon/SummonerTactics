using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for the TestSceneSetup component.
/// Provides buttons and tools to help with testing.
/// </summary>
[CustomEditor(typeof(TestSceneSetup))]
public class TestSceneSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Get the target
        TestSceneSetup testSceneSetup = (TestSceneSetup)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Test Controls", EditorStyles.boldLabel);
        
        // Add buttons for testing
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Spawn Champions"))
        {
            testSceneSetup.SpawnAllChampions();
        }
        
        if (GUILayout.Button("Clear Test Entities"))
        {
            testSceneSetup.ClearTestEntities();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Add a button to create a test entity prefab
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Create Test Entity Prefab"))
        {
            CreateTestEntityPrefab();
        }
        
        // Add a button to set up the test scene
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Set Up Test Scene"))
        {
            SetUpTestScene(testSceneSetup);
        }
    }
    
    /// <summary>
    /// Creates a test entity prefab if it doesn't exist
    /// </summary>
    private void CreateTestEntityPrefab()
    {
        // Check if the prefab already exists
        string prefabPath = "Assets/_Project/Prefabs/Testing/TestEntity.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (existingPrefab != null)
        {
            // Prefab already exists, select it
            Selection.activeObject = existingPrefab;
            EditorGUIUtility.PingObject(existingPrefab);
            Debug.Log("Test entity prefab already exists at: " + prefabPath);
            return;
        }
        
        // Create the prefab directory if it doesn't exist
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }
        
        // Create a new game object
        GameObject testEntity = new GameObject("TestEntity");
        
        // Add a sprite renderer
        SpriteRenderer spriteRenderer = testEntity.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        spriteRenderer.color = Color.cyan;
        
        // Add a circle collider
        CircleCollider2D collider = testEntity.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        
        // Add the test entity script
        TestEntity testEntityScript = testEntity.AddComponent<TestEntity>();
        testEntityScript.entityColor = Color.cyan;
        
        // Add Champion component
        Champion champion = testEntity.AddComponent<Champion>();
        champion.championName = "Test Champion";
        champion.team = Team.Blue;
        
        // Add ChampionMovement component
        ChampionMovement movement = testEntity.AddComponent<ChampionMovement>();
        movement.moveSpeed = 5f;
        movement.waypointReachedThreshold = 0.1f;
        movement.loopWaypoints = true;
        
        // Create the prefab
        #if UNITY_2018_3_OR_NEWER
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(testEntity, prefabPath);
        #else
        GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, testEntity);
        #endif
        
        // Destroy the temporary game object
        DestroyImmediate(testEntity);
        
        // Select the new prefab
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        
        Debug.Log("Created test entity prefab at: " + prefabPath);
    }
    
    /// <summary>
    /// Sets up the test scene with required components
    /// </summary>
    private void SetUpTestScene(TestSceneSetup testSceneSetup)
    {
        // Find the map prefab
        string mapPrefabPath = "Assets/_Project/Prefabs/Map/Map.prefab";
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapPrefabPath);
        
        if (mapPrefab != null)
        {
            testSceneSetup.mapPrefab = mapPrefab;
            Debug.Log("Assigned map prefab to TestSceneSetup");
        }
        else
        {
            Debug.LogWarning("Map prefab not found at: " + mapPrefabPath);
        }
        
        // Find the main camera and add a camera controller if needed
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCamera.gameObject.AddComponent<CameraController>();
                cameraController.targetCamera = mainCamera;
                Debug.Log("Added CameraController to main camera");
            }
            
            testSceneSetup.cameraController = cameraController;
            Debug.Log("Assigned camera controller to TestSceneSetup");
        }
        else
        {
            Debug.LogWarning("Main camera not found in scene");
        }
        
        // Find or create a test entity prefab
        string testEntityPrefabPath = "Assets/_Project/Prefabs/Testing/TestEntity.prefab";
        GameObject testEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(testEntityPrefabPath);
        
        if (testEntityPrefab == null)
        {
            // Create the test entity prefab
            CreateTestEntityPrefab();
            
            // Load it again
            testEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(testEntityPrefabPath);
        }
        
        if (testEntityPrefab != null)
        {
            testSceneSetup.testEntityPrefab = testEntityPrefab;
            Debug.Log("Assigned test entity prefab to TestSceneSetup");
        }
        
        // Create the TestEntity tag if it doesn't exist
        CreateTestEntityTag();
        
        // Create or find LaneManager
        string laneManagerPrefabPath = "Assets/_Project/Prefabs/Map/LaneManager.prefab";
        GameObject laneManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(laneManagerPrefabPath);
        
        if (laneManagerPrefab == null)
        {
            // Create a new LaneManager GameObject
            GameObject laneManagerObj = new GameObject("LaneManager");
            laneManagerObj.AddComponent<LaneManager>();
            
            // Create the prefab directory if it doesn't exist
            string directory = System.IO.Path.GetDirectoryName(laneManagerPrefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            
            // Create the prefab
            #if UNITY_2018_3_OR_NEWER
            laneManagerPrefab = PrefabUtility.SaveAsPrefabAsset(laneManagerObj, laneManagerPrefabPath);
            #else
            laneManagerPrefab = PrefabUtility.CreatePrefab(laneManagerPrefabPath, laneManagerObj);
            #endif
            
            // Destroy the temporary game object
            DestroyImmediate(laneManagerObj);
            
            Debug.Log("Created LaneManager prefab at: " + laneManagerPrefabPath);
        }
        
        if (laneManagerPrefab != null)
        {
            testSceneSetup.laneManagerPrefab = laneManagerPrefab;
            Debug.Log("Assigned LaneManager prefab to TestSceneSetup");
        }
        
        Debug.Log("Test scene setup complete!");
    }
    
    /// <summary>
    /// Creates the TestEntity tag if it doesn't exist
    /// </summary>
    private void CreateTestEntityTag()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals("TestEntity"))
            {
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            tagsProp.arraySize++;
            SerializedProperty tag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            tag.stringValue = "TestEntity";
            tagManager.ApplyModifiedProperties();
            Debug.Log("Added TestEntity tag");
            
            // Force Unity to save the changes
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.Log("TestEntity tag already exists");
        }
    }
} 