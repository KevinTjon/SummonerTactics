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
    
    [Header("Lane Settings")]
    [Tooltip("Whether to set up lanes automatically")]
    public bool setupLanesAutomatically = true;
    
    [Tooltip("Reference to the LaneManager prefab")]
    public GameObject laneManagerPrefab;
    
    [Header("Champion Settings")]
    [Tooltip("Number of champions to spawn per team")]
    public int championsPerTeam = 3;
    
    [Tooltip("Blue team champion colors")]
    public Color[] blueTeamColors = new Color[] { 
        new Color(0, 0, 1),      // Blue
        new Color(0, 0.5f, 1),   // Light Blue
        new Color(0, 0.2f, 0.8f) // Dark Blue
    };
    
    [Tooltip("Red team champion colors")]
    public Color[] redTeamColors = new Color[] { 
        new Color(1, 0, 0),      // Red
        new Color(1, 0.5f, 0),   // Orange-Red
        new Color(0.8f, 0, 0.2f) // Dark Red
    };
    
    [Header("Debug Settings")]
    [Tooltip("Whether to show debug information")]
    public bool showDebugInfo = true;
    
    private Transform blueSpawnPoint;
    private Transform redSpawnPoint;
    
    private GameObject mapInstance;
    private LaneManager laneManager;
    
    private void Awake()
    {
        // Initialize references
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
    }
    
    private void Start()
    {
        // Set up the test scene
        SetupTestScene();
        
        // Spawn test entities if enabled
        if (spawnTestEntitiesAtStart)
        {
            SpawnAllChampions();
        }
    }
    
    /// <summary>
    /// Sets up the test scene with map and lanes
    /// </summary>
    private void SetupTestScene()
    {
        // Set up the map
        InitializeMap();
        
        // Set up lanes
        if (setupLanesAutomatically)
        {
            SetupLanes();
        }
        
        // Set up the camera
        SetupCamera();
    }
    
    /// <summary>
    /// Initializes the map in the scene
    /// </summary>
    private void InitializeMap()
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map prefab is not assigned!");
            return;
        }
        
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
                // Create a default spawn point if not found
                GameObject blueSpawnObj = new GameObject("BlueSpawn");
                blueSpawnObj.transform.SetParent(mapInstance.transform);
                blueSpawnObj.transform.position = new Vector3(-10, -10, 0);
                blueSpawnPoint = blueSpawnObj.transform;
                Debug.Log("Created default Blue Spawn Point at: " + blueSpawnPoint.position);
            }
            
            if (redSpawn != null)
            {
                redSpawnPoint = redSpawn;
                Debug.Log("Found Red Spawn Point at: " + redSpawnPoint.position);
            }
            else
            {
                Debug.LogWarning("Red Spawn Point not found in map!");
                // Create a default spawn point if not found
                GameObject redSpawnObj = new GameObject("RedSpawn");
                redSpawnObj.transform.SetParent(mapInstance.transform);
                redSpawnObj.transform.position = new Vector3(10, 10, 0);
                redSpawnPoint = redSpawnObj.transform;
                Debug.Log("Created default Red Spawn Point at: " + redSpawnPoint.position);
            }
        }
    }
    
    /// <summary>
    /// Sets up the lanes for testing
    /// </summary>
    private void SetupLanes()
    {
        // Find or create the LaneManager
        laneManager = FindObjectOfType<LaneManager>();
        if (laneManager == null && laneManagerPrefab != null)
        {
            GameObject laneManagerObj = Instantiate(laneManagerPrefab);
            laneManagerObj.name = "LaneManager";
            laneManager = laneManagerObj.GetComponent<LaneManager>();
        }
        
        if (laneManager != null)
        {
            // Ensure lanes are set up but don't force waypoint generation
            laneManager.autoGenerateWaypoints = false;
            
            // Verify that lanes have waypoints
            VerifyLaneWaypoints(laneManager);
        }
        else
        {
            Debug.LogError("No LaneManager found or created. Lanes will not be set up.");
        }
    }
    
    /// <summary>
    /// Verifies that lanes have waypoints and logs warnings if they don't
    /// </summary>
    private void VerifyLaneWaypoints(LaneManager laneManager)
    {
        bool allLanesHaveWaypoints = true;
        
        if (laneManager.topLane == null || laneManager.topLane.waypoints.Count == 0)
        {
            Debug.LogWarning("Top Lane has no waypoints. Champions assigned to this lane may not move correctly.");
            allLanesHaveWaypoints = false;
        }
        
        if (laneManager.midLane == null || laneManager.midLane.waypoints.Count == 0)
        {
            Debug.LogWarning("Mid Lane has no waypoints. Champions assigned to this lane may not move correctly.");
            allLanesHaveWaypoints = false;
        }
        
        if (laneManager.bottomLane == null || laneManager.bottomLane.waypoints.Count == 0)
        {
            Debug.LogWarning("Bottom Lane has no waypoints. Champions assigned to this lane may not move correctly.");
            allLanesHaveWaypoints = false;
        }
        
        if (!allLanesHaveWaypoints)
        {
            Debug.LogWarning("Some lanes are missing waypoints. Use the Lane Editor to add waypoints manually.");
        }
        else
        {
            Debug.Log("All lanes have waypoints. Champions should move correctly.");
        }
    }
    
    /// <summary>
    /// Sets up the camera for the test scene
    /// </summary>
    private void SetupCamera()
    {
        if (cameraController != null)
        {
            // Set initial camera position
            cameraController.transform.position = new Vector3(0, 0, -10);
            
            // Set camera bounds if map exists
            if (mapInstance != null)
            {
                // Try to find a map bounds object
                Transform mapBounds = mapInstance.transform.Find("MapBounds");
                if (mapBounds != null)
                {
                    // Set camera bounds based on map bounds
                    Renderer boundsRenderer = mapBounds.GetComponent<Renderer>();
                    if (boundsRenderer != null)
                    {
                        // Use SetMapDimensions instead of SetBounds
                        CameraController camera = cameraController.GetComponent<CameraController>();
                        if (camera != null)
                        {
                            Bounds bounds = boundsRenderer.bounds;
                            camera.SetMapDimensions(bounds.center, bounds.size.x, bounds.size.z);
                            if (showDebugInfo)
                            {
                                Debug.Log($"Set camera bounds to center: {bounds.center}, width: {bounds.size.x}, height: {bounds.size.z}");
                            }
                        }
                    }
                    else if (showDebugInfo)
                    {
                        Debug.Log("MapBounds not found in map instance. Using default camera settings.");
                    }
                }
            }
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("Camera controller not found. Camera will not be set up for the test scene.");
        }
    }
    
    /// <summary>
    /// Spawns champions for both teams
    /// </summary>
    public void SpawnAllChampions()
    {
        // Find the LaneManager if not already assigned
        if (laneManager == null)
        {
            laneManager = FindObjectOfType<LaneManager>();
            if (laneManager == null)
            {
                Debug.LogError("No LaneManager found. Cannot spawn champions.");
                return;
            }
        }
        
        // Verify that lanes have waypoints before spawning champions
        VerifyLaneWaypoints(laneManager);
        
        // Spawn champions for each team
        SpawnTeamChampions(Team.Blue);
        SpawnTeamChampions(Team.Red);
    }
    
    /// <summary>
    /// Spawns champions for a specific team
    /// </summary>
    private void SpawnTeamChampions(Team team)
    {
        if (testEntityPrefab == null)
        {
            Debug.LogError("Test entity prefab is not assigned!");
            return;
        }
        
        // Determine spawn position based on team
        Vector3 spawnPosition = Vector3.zero;
        Color[] teamColors = team == Team.Blue ? blueTeamColors : redTeamColors;
        
        if (team == Team.Blue && blueSpawnPoint != null)
        {
            spawnPosition = blueSpawnPoint.position;
        }
        else if (team == Team.Red && redSpawnPoint != null)
        {
            spawnPosition = redSpawnPoint.position;
        }
        else
        {
            Debug.LogWarning($"No spawn point found for {team} team. Using default position.");
        }
        
        // Define lane types for the champions
        LaneType[] laneTypes = new LaneType[] { LaneType.Top, LaneType.Mid, LaneType.Bottom };
        
        // Spawn champions for each lane
        for (int i = 0; i < Mathf.Min(championsPerTeam, laneTypes.Length); i++)
        {
            // Calculate offset position to avoid champions spawning on top of each other
            Vector3 offsetPosition = spawnPosition + new Vector3(i * 2.0f, 0, 0);
            
            // Spawn the champion
            GameObject championObj = Instantiate(testEntityPrefab, offsetPosition, Quaternion.identity);
            championObj.name = $"{team}_{laneTypes[i]}_Champion";
            
            // Get or add Champion component
            Champion champion = championObj.GetComponent<Champion>();
            if (champion == null)
            {
                champion = championObj.AddComponent<Champion>();
            }
            
            // Set team and lane
            champion.championName = $"{team} {laneTypes[i]} Champion";
            champion.SetTeam(team);
            champion.SetLaneType(laneTypes[i]);
            
            // Set color
            if (i < teamColors.Length)
            {
                champion.teamColor = teamColors[i];
                
                // Update sprite renderer color
                SpriteRenderer spriteRenderer = championObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = teamColors[i];
                    
                    // Ensure the sprite renderer has a higher sorting order
                    spriteRenderer.sortingOrder = 10;
                }
            }
            
            // Add ChampionMovement component if it doesn't exist
            ChampionMovement movement = championObj.GetComponent<ChampionMovement>();
            if (movement == null)
            {
                movement = championObj.AddComponent<ChampionMovement>();
            }
            
            // Configure TestEntity component if it exists
            TestEntity testEntity = championObj.GetComponent<TestEntity>();
            if (testEntity != null)
            {
                // Make sure TestEntity respects the lane assignments we just made
                testEntity.respectExistingAssignments = true;
                testEntity.champion = champion;
                testEntity.movement = movement;
                testEntity.team = team;
                testEntity.laneType = laneTypes[i];
                testEntity.entityColor = champion.teamColor;
            }
            
            Debug.Log($"Spawned {team} {laneTypes[i]} champion at: {offsetPosition}");
        }
    }
    
    /// <summary>
    /// Spawns test entities at the spawn points (legacy method, use SpawnAllChampions instead)
    /// </summary>
    public void SpawnTestEntities()
    {
        // Redirect to the new method
        SpawnAllChampions();
    }
    
    /// <summary>
    /// Clears all test entities from the scene
    /// </summary>
    public void ClearTestEntities()
    {
        // Find all test entities
        TestEntity[] testEntities = FindObjectsOfType<TestEntity>();
        foreach (TestEntity entity in testEntities)
        {
            Destroy(entity.gameObject);
        }
        
        // Also find all champions
        Champion[] champions = FindObjectsOfType<Champion>();
        foreach (Champion champion in champions)
        {
            Destroy(champion.gameObject);
        }
        
        Debug.Log($"Cleared {testEntities.Length + champions.Length} test entities from the scene");
    }
} 