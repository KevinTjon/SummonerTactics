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
    
    private ChampionMovement movementComponent;
    private bool laneAssigned = false;
    
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
        
        // Create engagement indicator if it doesn't exist
        CreateEngagementIndicator();
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