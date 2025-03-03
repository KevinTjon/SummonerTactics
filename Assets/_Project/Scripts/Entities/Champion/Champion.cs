using UnityEngine;

/// <summary>
/// Represents a champion in the game with team and lane assignment.
/// </summary>
public class Champion : MonoBehaviour
{
    [Header("Champion Settings")]
    [Tooltip("The name of this champion")]
    public string championName = "Default Champion";
    
    [Tooltip("The team this champion belongs to")]
    public Team team = Team.Blue;
    
    [Tooltip("The lane this champion is assigned to")]
    public LaneType assignedLaneType = LaneType.None;
    
    [Header("Health Settings")]
    [Tooltip("Maximum health of the champion")]
    public float maxHealth = 100f;
    
    [Tooltip("Current health of the champion")]
    public float currentHealth;
    
    [Tooltip("Whether the champion is alive")]
    public bool isAlive = true;
    
    [Header("Visual Settings")]
    [Tooltip("The color to use for this champion's team")]
    public Color teamColor = Color.blue;
    
    [Tooltip("The sprite renderer for this champion")]
    public SpriteRenderer spriteRenderer;
    
    [Header("Debug Settings")]
    [Tooltip("Whether to show debug information")]
    public bool showDebug = true;
    
    [Header("Interaction Settings")]
    [Tooltip("Visual indicator for when champion is engaged with an opponent")]
    public GameObject engagementIndicator;
    
    [Tooltip("Color for the engagement indicator")]
    public Color engagementColor = Color.yellow;
    
    [Header("Health Bar Settings")]
    [Tooltip("Prefab for the health bar")]
    public GameObject healthBarPrefab;
    
    [Tooltip("Offset for the health bar position")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    
    [Tooltip("Health bar background color")]
    public Color healthBarBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Tooltip("Health bar fill color")]
    public Color healthBarFillColor = Color.green;
    
    [Tooltip("Health bar low health color (when health is below 30%)")]
    public Color healthBarLowHealthColor = Color.red;
    
    private ChampionMovement movementComponent;
    private bool laneAssigned = false;
    private GameObject healthBarInstance;
    private Transform healthBarFill;
    
    private void Awake()
    {
        // Get required components
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        movementComponent = GetComponent<ChampionMovement>();
        
        // Set initial color based on team
        UpdateTeamColor();
        
        // Set layer to "Champion" for detection
        SetChampionLayer();
        
        // Ensure champion has a collider for collision detection
        EnsureCollider();
        
        // Create engagement indicator if it doesn't exist
        CreateEngagementIndicator();
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Create health bar
        CreateHealthBar();
    }
    
    private void Start()
    {
        // Only assign to lane if not already assigned
        if (!laneAssigned && assignedLaneType != LaneType.None)
        {
            AssignToLane();
        }
    }
    
    private void Update()
    {
        // Update engagement indicator visibility
        UpdateEngagementIndicator();
        
        // Update health bar
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Sets the gameObject layer to "Champion" for detection
    /// </summary>
    private void SetChampionLayer()
    {
        // Check if the Champion layer exists
        int championLayer = LayerMask.NameToLayer("Champion");
        
        if (championLayer != -1)
        {
            // Set the layer
            gameObject.layer = championLayer;
            
            if (showDebug)
            {
                Debug.Log($"Set champion {championName} to layer 'Champion'");
            }
        }
        else
        {
            // Layer doesn't exist, warn the user
            Debug.LogWarning("Champion layer not found! Please add a 'Champion' layer in the Unity Editor.");
        }
    }
    
    /// <summary>
    /// Updates the champion's color based on team
    /// </summary>
    private void UpdateTeamColor()
    {
        if (spriteRenderer != null)
        {
            if (team == Team.Blue)
            {
                teamColor = Color.blue;
            }
            else if (team == Team.Red)
            {
                teamColor = Color.red;
            }
            
            spriteRenderer.color = teamColor;
        }
    }
    
    /// <summary>
    /// Assigns the champion to a lane based on its lane type and team
    /// </summary>
    public void AssignToLane()
    {
        if (movementComponent == null)
        {
            Debug.LogWarning($"Champion {championName} has no movement component!");
            return;
        }
        
        // Skip if already assigned to a lane
        if (movementComponent.assignedLane != null && laneAssigned)
        {
            return;
        }
        
        // Skip if no lane type is assigned
        if (assignedLaneType == LaneType.None)
        {
            Debug.LogWarning($"Champion {championName} has no lane type assigned!");
            return;
        }
        
        // Find the LaneManager
        LaneManager laneManager = FindObjectOfType<LaneManager>();
        if (laneManager == null)
        {
            Debug.LogWarning("No LaneManager found in the scene!");
            return;
        }
        
        // Get the lane based on lane type
        Lane lane = laneManager.GetLane(assignedLaneType);
        
        if (lane != null)
        {
            // Determine direction based on team
            // Blue team moves from their base (index 0) towards Red base (last index)
            // Red team moves from their base (last index) towards Blue base (index 0)
            bool moveForward = (team == Team.Blue);
            
            // Assign the lane to the movement component
            movementComponent.AssignLane(lane, moveForward);
            
            if (showDebug)
            {
                Debug.Log($"Assigned champion {championName} to {lane.laneName} moving {(moveForward ? "forward" : "backward")}");
            }
            
            laneAssigned = true;
        }
        else
        {
            // If we can't find a specific lane, try to find any lane
            Lane[] lanes = FindObjectsOfType<Lane>();
            if (lanes.Length > 0)
            {
                bool moveForward = (team == Team.Blue);
                movementComponent.AssignLane(lanes[0], moveForward);
                
                if (showDebug)
                {
                    Debug.Log($"Assigned champion {championName} to fallback lane {lanes[0].laneName}");
                }
                
                laneAssigned = true;
            }
            else
            {
                Debug.LogWarning($"Could not find a matching lane for champion {championName} with lane type {assignedLaneType} and team {team}");
            }
        }
    }
    
    /// <summary>
    /// Sets the team for this champion
    /// </summary>
    public void SetTeam(Team newTeam)
    {
        team = newTeam;
        UpdateTeamColor();
        
        // If we already have a movement component and it has a lane assigned,
        // update the movement direction based on the new team
        if (movementComponent != null && movementComponent.assignedLane != null)
        {
            // Blue team moves from their base (index 0) towards Red base (last index)
            // Red team moves from their base (last index) towards Blue base (index 0)
            movementComponent.moveForward = (team == Team.Blue);
            
            // Reset the waypoint index based on the new team
            if (team == Team.Blue)
            {
                // Blue team starts at their base (index 0)
                movementComponent.AssignLane(movementComponent.assignedLane, true);
            }
            else
            {
                // Red team starts at their base (last index)
                movementComponent.AssignLane(movementComponent.assignedLane, false);
            }
        }
    }
    
    /// <summary>
    /// Sets the lane type for this champion
    /// </summary>
    public void SetLaneType(LaneType laneType)
    {
        assignedLaneType = laneType;
        AssignToLane();
    }
    
    /// <summary>
    /// Creates a visual indicator for engagement
    /// </summary>
    private void CreateEngagementIndicator()
    {
        if (engagementIndicator == null)
        {
            // Create a new game object for the indicator
            engagementIndicator = new GameObject("EngagementIndicator");
            engagementIndicator.transform.SetParent(transform);
            engagementIndicator.transform.localPosition = Vector3.zero;
            
            // Add a sprite renderer
            SpriteRenderer indicatorRenderer = engagementIndicator.AddComponent<SpriteRenderer>();
            
            // Create a simple circle sprite
            indicatorRenderer.sprite = CreateCircleSprite();
            indicatorRenderer.color = engagementColor;
            indicatorRenderer.sortingOrder = -1; // Behind the champion
            
            // Scale it appropriately
            engagementIndicator.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            // Initially hide it
            engagementIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// Updates the visibility of the engagement indicator
    /// </summary>
    private void UpdateEngagementIndicator()
    {
        if (engagementIndicator != null)
        {
            ChampionMovement movement = GetComponent<ChampionMovement>();
            if (movement != null)
            {
                // Show indicator when engaged with an opponent
                engagementIndicator.SetActive(movement.isEngagedWithOpponent);
            }
        }
    }
    
    /// <summary>
    /// Creates a simple circle sprite for the engagement indicator
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        // Set pixels to create a circle
        float radius = resolution / 2f;
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (distance < radius - 2) // Solid circle
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (distance < radius) // Border
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// Creates a health bar for the champion
    /// </summary>
    private void CreateHealthBar()
    {
        // If we already have a health bar, don't create another one
        if (healthBarInstance != null)
            return;
            
        // Create a new health bar
        if (healthBarPrefab != null)
        {
            // Instantiate from prefab
            healthBarInstance = Instantiate(healthBarPrefab, transform);
        }
        else
        {
            // Create a simple health bar
            healthBarInstance = new GameObject("HealthBar");
            healthBarInstance.transform.SetParent(transform);
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(healthBarInstance.transform);
            SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = CreateRectSprite();
            bgRenderer.color = healthBarBackgroundColor;
            bgRenderer.sortingOrder = 10;
            
            // Create fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(healthBarInstance.transform);
            SpriteRenderer fillRenderer = fill.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = CreateRectSprite();
            fillRenderer.color = healthBarFillColor;
            fillRenderer.sortingOrder = 11;
            
            // Store reference to fill transform
            healthBarFill = fill.transform;
            
            // Set up transforms
            background.transform.localScale = new Vector3(1, 0.2f, 1);
            fill.transform.localScale = new Vector3(1, 0.2f, 1);
            
            // Position the fill at the left edge of the background
            fill.transform.localPosition = new Vector3(-0.5f, 0, 0);
            
            // Set the pivot of the fill to the left
            fill.transform.localPosition = new Vector3(-0.5f, 0, 0);
        }
        
        // Position the health bar above the champion
        healthBarInstance.transform.localPosition = healthBarOffset;
        
        // Make sure the health bar is visible
        healthBarInstance.SetActive(true);
    }
    
    /// <summary>
    /// Updates the health bar based on current health
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBarInstance == null || healthBarFill == null)
            return;
            
        // Calculate health percentage
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        
        // Update fill scale
        healthBarFill.localScale = new Vector3(healthPercent, healthBarFill.localScale.y, 1);
        
        // Update fill color based on health percentage
        SpriteRenderer fillRenderer = healthBarFill.GetComponent<SpriteRenderer>();
        if (fillRenderer != null)
        {
            if (healthPercent < 0.3f)
            {
                fillRenderer.color = healthBarLowHealthColor;
            }
            else
            {
                fillRenderer.color = healthBarFillColor;
            }
        }
        
        // Hide health bar if champion is dead
        healthBarInstance.SetActive(isAlive);
    }
    
    /// <summary>
    /// Creates a simple rectangular sprite for the health bar
    /// </summary>
    private Sprite CreateRectSprite()
    {
        // Create a simple rectangle texture
        int width = 100;
        int height = 20;
        Texture2D texture = new Texture2D(width, height);
        
        // Set all pixels to white
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }
        
        texture.Apply();
        
        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// Applies damage to the champion
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (!isAlive)
            return;
            
        currentHealth -= amount;
        
        // Check if champion is dead
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        if (showDebug)
        {
            Debug.Log($"Champion {championName} took {amount} damage. Current health: {currentHealth}/{maxHealth}");
        }
    }
    
    /// <summary>
    /// Heals the champion
    /// </summary>
    public void Heal(float amount)
    {
        if (!isAlive)
            return;
            
        currentHealth += amount;
        
        // Cap health at max
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        if (showDebug)
        {
            Debug.Log($"Champion {championName} healed for {amount}. Current health: {currentHealth}/{maxHealth}");
        }
    }
    
    /// <summary>
    /// Handles champion death
    /// </summary>
    private void Die()
    {
        isAlive = false;
        
        // Note: We're not stopping movement here anymore
        // This allows other champions to continue moving after this champion dies
        
        // Fade out sprite
        if (spriteRenderer != null)
        {
            Color fadeColor = spriteRenderer.color;
            fadeColor.a = 0.5f;
            spriteRenderer.color = fadeColor;
        }
        
        if (showDebug)
        {
            Debug.Log($"Champion {championName} has died!");
        }
        
        // Respawn after a delay (5 seconds)
        Invoke("RespawnAtBase", 5f);
    }
    
    /// <summary>
    /// Respawns the champion at their team's base
    /// </summary>
    private void RespawnAtBase()
    {
        // Default spawn positions
        Vector3 blueBasePosition = new Vector3(-45f, -45f, 0f);
        Vector3 redBasePosition = new Vector3(45f, 45f, 0f);
        
        // Try to find spawn points by name first
        GameObject blueSpawn = GameObject.Find("BlueSpawn");
        GameObject redSpawn = GameObject.Find("RedSpawn");
        
        // Update default positions if spawn points were found
        if (blueSpawn != null)
        {
            blueBasePosition = blueSpawn.transform.position;
        }
        
        if (redSpawn != null)
        {
            redBasePosition = redSpawn.transform.position;
        }
        
        // Reset position to team's base
        if (team == Team.Blue)
        {
            transform.position = blueBasePosition;
            
            if (showDebug)
            {
                Debug.Log($"Champion {championName} respawning at Blue base: {blueBasePosition}");
            }
        }
        else if (team == Team.Red)
        {
            transform.position = redBasePosition;
            
            if (showDebug)
            {
                Debug.Log($"Champion {championName} respawning at Red base: {redBasePosition}");
            }
        }
        
        // Make sure any engagement state is cleared
        if (movementComponent != null)
        {
            // Force disengage from any opponents
            movementComponent.DisengageFromOpponent();
            
            // Reset the waypoint index explicitly
            if (movementComponent.assignedLane != null)
            {
                bool moveForward = (team == Team.Blue);
                int startIndex = moveForward ? 0 : movementComponent.assignedLane.GetWaypointCount() - 1;
                
                // Set the waypoint index directly
                movementComponent.ResetWaypointIndex(startIndex);
                
                if (showDebug)
                {
                    Debug.Log($"Reset waypoint index for {championName} to {startIndex}");
                }
            }
        }
        
        // Revive with full health
        Revive(1.0f);
        
        // Make sure the champion is assigned to a lane and moving
        AssignToLane();
        
        if (showDebug)
        {
            Debug.Log($"Champion {championName} has respawned at base and is moving along lane {assignedLaneType}");
        }
    }
    
    /// <summary>
    /// Revives the champion
    /// </summary>
    public void Revive(float healthPercent = 1f)
    {
        isAlive = true;
        currentHealth = maxHealth * healthPercent;
        
        // Restore sprite
        if (spriteRenderer != null)
        {
            Color restoreColor = spriteRenderer.color;
            restoreColor.a = 1f;
            spriteRenderer.color = restoreColor;
        }
        
        // Resume movement
        if (movementComponent != null)
        {
            movementComponent.ResumeMovement();
        }
        
        if (showDebug)
        {
            Debug.Log($"Champion {championName} has been revived with {currentHealth}/{maxHealth} health!");
        }
    }
    
    /// <summary>
    /// Ensures the champion has a collider for collision detection
    /// </summary>
    private void EnsureCollider()
    {
        // Check if the champion already has a collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // Add a circle collider
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = 1.0f;
            circleCollider.isTrigger = true; // Use trigger for overlap detection without physics
            
            if (showDebug)
            {
                Debug.Log($"Added CircleCollider2D to champion {championName}");
            }
        }
        else if (!collider.isTrigger)
        {
            // Make sure the collider is a trigger
            collider.isTrigger = true;
            
            if (showDebug)
            {
                Debug.Log($"Set collider to trigger on champion {championName}");
            }
        }
        
        // Check if the champion has a Rigidbody2D component
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            // Add a Rigidbody2D component
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; // Set to kinematic to prevent physics from affecting movement
            rb.gravityScale = 0f;  // Disable gravity
            
            if (showDebug)
            {
                Debug.Log($"Added Rigidbody2D to champion {championName}");
            }
        }
    }
}

/// <summary>
/// Enum representing the different lane types
/// </summary>
public enum LaneType
{
    None,
    Top,
    Mid,
    Bottom
} 