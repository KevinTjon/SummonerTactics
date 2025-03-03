using UnityEngine;
using System.Linq;
using System.Collections;

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
        
        // Ensure required layers exist
        EnsureRequiredLayers();
    }
    
    /// <summary>
    /// Ensures that all required layers exist in the project
    /// </summary>
    private void EnsureRequiredLayers()
    {
        // Check if the Champion layer exists
        int championLayer = LayerMask.NameToLayer("Champion");
        
        if (championLayer == -1)
        {
            // Layer doesn't exist, warn the user
            Debug.LogWarning("Champion layer not found! Please add a 'Champion' layer in the Unity Editor.");
            Debug.LogWarning("Go to Edit > Project Settings > Tags and Layers and add 'Champion' to the Layers list.");
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("Champion layer found at index: " + championLayer);
            }
        }
    }
    
    private void Start()
    {
        // Set up the test scene
        SetupTestScene();
        
        // Create attack effect prefab
        CreateAttackEffectPrefab();
        
        // Spawn test entities if enabled
        if (spawnTestEntitiesAtStart)
        {
            SpawnAllChampions();
            
            // Don't run the opposing champions test if we're already spawning all champions
            // This prevents duplicate champions
        }
        else
        {
            // Only spawn opposing champions for testing if we're not spawning all champions
            StartCoroutine(SpawnOpposingChampionsForTesting());
        }
        
        // Test health bar functionality
        StartCoroutine(TestHealthBarFunctionality());
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
    
    /// <summary>
    /// Spawns opposing champions in the same lane for testing interactions.
    /// This is used only when spawnTestEntitiesAtStart is false.
    /// </summary>
    private System.Collections.IEnumerator SpawnOpposingChampionsForTesting()
    {
        // Wait a bit to ensure everything is set up
        yield return new WaitForSeconds(1f);
        
        // Make sure lanes are set up
        if (laneManager == null)
        {
            Debug.LogWarning("LaneManager not found! Cannot spawn opposing champions.");
            yield break;
        }
        
        // Check if mid lane exists
        if (laneManager.midLane == null)
        {
            Debug.LogWarning("Mid lane not found! Cannot spawn opposing champions.");
            yield break;
        }
        
        Debug.Log("Spawning minimal set of champions for interaction testing...");
        
        // Spawn a blue team champion in mid lane
        GameObject blueChampion = SpawnChampion(Team.Blue, LaneType.Mid);
        if (blueChampion != null)
        {
            Debug.Log("Spawned Blue team champion in Mid lane for interaction testing");
        }
        
        // Wait a bit before spawning the red champion
        yield return new WaitForSeconds(0.5f);
        
        // Spawn a red team champion in mid lane
        GameObject redChampion = SpawnChampion(Team.Red, LaneType.Mid);
        if (redChampion != null)
        {
            Debug.Log("Spawned Red team champion in Mid lane for interaction testing");
        }
        
        Debug.Log("Champions spawned for interaction testing. They should meet in the middle of the lane.");
    }
    
    /// <summary>
    /// Spawns a champion with the specified team and lane type
    /// </summary>
    private GameObject SpawnChampion(Team team, LaneType laneType)
    {
        if (testEntityPrefab == null)
        {
            Debug.LogWarning("Test entity prefab not assigned! Cannot spawn champion.");
            return null;
        }
        
        // Determine spawn position based on team
        Vector3 spawnPosition;
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
            // Fallback to a default position
            spawnPosition = new Vector3(team == Team.Blue ? -5 : 5, 0, 0);
        }
        
        // Spawn the champion
        GameObject championObj = Instantiate(testEntityPrefab, spawnPosition, Quaternion.identity);
        championObj.name = $"{team} Champion ({laneType})";
        
        // Set up the champion
        Champion champion = championObj.GetComponent<Champion>();
        if (champion != null)
        {
            champion.team = team;
            champion.assignedLaneType = laneType;
            champion.championName = $"{team} Champion";
            
            // Set team color
            if (team == Team.Blue)
            {
                champion.teamColor = blueTeamColors[0];
            }
            else
            {
                champion.teamColor = redTeamColors[0];
            }
            
            // Make sure the champion has a collider for detection
            if (championObj.GetComponent<Collider2D>() == null)
            {
                CircleCollider2D collider = championObj.AddComponent<CircleCollider2D>();
                collider.radius = 1.5f;
                collider.isTrigger = true;
                Debug.Log($"Added CircleCollider2D to {championObj.name}");
            }
            
            // Set the champion's layer to "Champion"
            int championLayer = LayerMask.NameToLayer("Champion");
            if (championLayer != -1)
            {
                championObj.layer = championLayer;
                Debug.Log($"Set {championObj.name} to layer 'Champion'");
            }
            else
            {
                Debug.LogWarning("Champion layer not found! Please add a 'Champion' layer in the Unity Editor.");
            }
            
            // Assign to lane
            champion.AssignToLane();
            
            // Set up attack effect
            ChampionMovement movement = championObj.GetComponent<ChampionMovement>();
            if (movement != null)
            {
                // Try to load the attack effect prefab
                movement.attackEffectPrefab = Resources.Load<GameObject>("Prefabs/AttackEffect");
                
                if (movement.attackEffectPrefab != null)
                {
                    Debug.Log($"Assigned attack effect prefab to {championObj.name}");
                }
            }
        }
        
        return championObj;
    }
    
    /// <summary>
    /// Tests health bar functionality by applying damage to champions
    /// </summary>
    private System.Collections.IEnumerator TestHealthBarFunctionality()
    {
        // Wait for champions to be spawned
        yield return new WaitForSeconds(3f);
        
        // Find all champions in the scene
        Champion[] champions = FindObjectsOfType<Champion>();
        
        if (champions.Length == 0)
        {
            Debug.LogWarning("No champions found for health bar testing!");
            yield break;
        }
        
        // Log the test
        Debug.Log("Testing health bar functionality...");
        
        // Apply damage to each champion
        foreach (Champion champion in champions)
        {
            // Apply 20% damage
            float damageAmount = champion.maxHealth * 0.2f;
            champion.TakeDamage(damageAmount);
            
            Debug.Log($"Applied {damageAmount} damage to {champion.championName}. Current health: {champion.currentHealth}/{champion.maxHealth}");
            
            // Wait a bit
            yield return new WaitForSeconds(0.5f);
        }
        
        // Wait a bit
        yield return new WaitForSeconds(2f);
        
        // Heal the champions
        foreach (Champion champion in champions)
        {
            // Heal 10%
            float healAmount = champion.maxHealth * 0.1f;
            champion.Heal(healAmount);
            
            Debug.Log($"Healed {champion.championName} for {healAmount}. Current health: {champion.currentHealth}/{champion.maxHealth}");
            
            // Wait a bit
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("Health bar testing complete!");
    }
    
    /// <summary>
    /// Creates an attack effect prefab for champions to use
    /// </summary>
    private void CreateAttackEffectPrefab()
    {
        // Check if we already have a prefab in Resources
        GameObject existingPrefab = Resources.Load<GameObject>("Prefabs/AttackEffect");
        if (existingPrefab != null)
        {
            Debug.Log("Attack effect prefab already exists in Resources.");
            return;
        }
        
        // Create a new game object for the attack effect
        GameObject attackEffectObj = new GameObject("AttackEffect");
        
        // Add sprite renderer
        SpriteRenderer renderer = attackEffectObj.AddComponent<SpriteRenderer>();
        
        // Create a simple sprite
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        // Create a simple projectile shape
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (distanceFromCenter < 8)
                {
                    // Core of the projectile
                    colors[y * 32 + x] = new Color(1f, 0.8f, 0.2f, 1f);
                }
                else if (distanceFromCenter < 12)
                {
                    // Outer glow
                    float alpha = 1f - ((distanceFromCenter - 8) / 4f);
                    colors[y * 32 + x] = new Color(1f, 0.6f, 0.1f, alpha);
                }
                else
                {
                    // Transparent
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // Create sprite from texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        renderer.sprite = sprite;
        
        // Add attack effect component
        AttackEffect effect = attackEffectObj.AddComponent<AttackEffect>();
        effect.duration = 0.5f;
        effect.speed = 8f;
        effect.pulseEffect = true;
        effect.pulseFrequency = 15f;
        effect.pulseAmplitude = 0.15f;
        
        // Set scale
        attackEffectObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Create a prefab from this game object
        #if UNITY_EDITOR
        // This code only works in the editor
        if (!System.IO.Directory.Exists("Assets/Resources/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Resources/Prefabs");
        }
        
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(attackEffectObj, "Assets/Resources/Prefabs/AttackEffect.prefab");
        Debug.Log("Created attack effect prefab at Assets/Resources/Prefabs/AttackEffect.prefab");
        #endif
        
        // Destroy the temporary object
        Destroy(attackEffectObj);
    }
} 