using UnityEngine;
using System.Linq;

/// <summary>
/// Handles the setup and testing functionality for the test scene.
/// Provides methods to test map functionality and entity spawning.
/// </summary>
public class TestSceneSetup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the map prefab")]
    public GameObject mapPrefab;
    
    [Tooltip("Reference to the camera controller")]
    public CameraController cameraController;
    
    [Header("Test Settings")]
    [Tooltip("Whether to spawn test entities at start")]
    public bool spawnTestEntitiesAtStart = false;
    
    [Tooltip("Prefab to spawn for testing")]
    public GameObject testEntityPrefab;
    
    // References to spawn points (will be found automatically if map is present)
    private Transform blueSpawnPoint;
    private Transform redSpawnPoint;
    
    private GameObject mapInstance;
    
    private void Awake()
    {
        // Ensure we have a map
        if (mapPrefab == null)
        {
            Debug.LogWarning("No map prefab assigned to TestSceneSetup!");
        }
    }
    
    private void Start()
    {
        // Initialize the map if not already in scene
        if (mapInstance == null && mapPrefab != null)
        {
            InitializeMap();
        }
        
        // Find spawn points
        FindSpawnPoints();
        
        // Set up camera
        SetupCamera();
        
        // Spawn test entities if enabled
        if (spawnTestEntitiesAtStart && testEntityPrefab != null)
        {
            SpawnTestEntities();
        }
    }
    
    /// <summary>
    /// Initializes the map in the scene
    /// </summary>
    private void InitializeMap()
    {
        mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        mapInstance.name = "Map";
        
        // Find spawn points after map is instantiated
        FindSpawnPoints();
    }
    
    /// <summary>
    /// Finds spawn points in the map
    /// </summary>
    private void FindSpawnPoints()
    {
        if (mapInstance != null)
        {
            // Try to find spawn points by name
            Transform blueSpawn = mapInstance.transform.Find("BlueSpawn");
            Transform redSpawn = mapInstance.transform.Find("RedSpawn");
            
            if (blueSpawn != null)
            {
                blueSpawnPoint = blueSpawn;
                Debug.Log("Found Blue Spawn Point at: " + blueSpawnPoint.position);
            }
            else
            {
                Debug.LogWarning("Blue Spawn Point not found in map!");
            }
            
            if (redSpawn != null)
            {
                redSpawnPoint = redSpawn;
                Debug.Log("Found Red Spawn Point at: " + redSpawnPoint.position);
            }
            else
            {
                Debug.LogWarning("Red Spawn Point not found in map!");
            }
        }
    }
    
    /// <summary>
    /// Sets up the camera to view the map properly
    /// </summary>
    private void SetupCamera()
    {
        if (cameraController != null && mapInstance != null)
        {
            // Assign map to camera controller
            cameraController.mapObject = mapInstance;
            
            // Reset camera position and zoom
            cameraController.ResetCamera();
        }
        else if (cameraController == null)
        {
            Debug.LogWarning("No camera controller assigned to TestSceneSetup!");
        }
    }
    
    /// <summary>
    /// Spawns test entities at the spawn points
    /// </summary>
    public void SpawnTestEntities()
    {
        if (testEntityPrefab == null)
        {
            Debug.LogError("No test entity prefab assigned!");
            return;
        }
        
        if (blueSpawnPoint != null)
        {
            GameObject blueEntity = Instantiate(testEntityPrefab, blueSpawnPoint.position, Quaternion.identity);
            blueEntity.name = "BlueTestEntity";
            Debug.Log("Spawned blue test entity at: " + blueSpawnPoint.position);
        }
        
        if (redSpawnPoint != null)
        {
            GameObject redEntity = Instantiate(testEntityPrefab, redSpawnPoint.position, Quaternion.identity);
            redEntity.name = "RedTestEntity";
            Debug.Log("Spawned red test entity at: " + redSpawnPoint.position);
        }
    }
    
    /// <summary>
    /// Clears all test entities from the scene
    /// </summary>
    public void ClearTestEntities()
    {
        try
        {
            GameObject[] testEntities = GameObject.FindGameObjectsWithTag("TestEntity");
            foreach (GameObject entity in testEntities)
            {
                Destroy(entity);
            }
            
            Debug.Log("Cleared all test entities");
        }
        catch (UnityException)
        {
            // If the tag doesn't exist, try to find entities by name
            Debug.LogWarning("The 'TestEntity' tag does not exist. Trying to find entities by name...");
            
            GameObject[] blueEntities = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.Contains("BlueTestEntity")).ToArray();
            GameObject[] redEntities = GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.Contains("RedTestEntity")).ToArray();
            
            foreach (GameObject entity in blueEntities)
            {
                Destroy(entity);
            }
            
            foreach (GameObject entity in redEntities)
            {
                Destroy(entity);
            }
            
            Debug.Log($"Cleared {blueEntities.Length + redEntities.Length} test entities by name");
        }
    }
} 