using UnityEngine;

/// <summary>
/// A simple test entity for testing champion movement and lane functionality.
/// </summary>
public class TestEntity : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("Sprite renderer for the entity")]
    public SpriteRenderer spriteRenderer;
    
    [Tooltip("Color for the entity")]
    public Color entityColor = Color.white;
    
    [Header("Components")]
    [Tooltip("Reference to the Champion component")]
    public Champion champion;
    
    [Tooltip("Reference to the ChampionMovement component")]
    public ChampionMovement movement;
    
    [Header("Test Settings")]
    [Tooltip("Whether to start moving automatically")]
    public bool startMovingAutomatically = true;
    
    [Tooltip("Lane type to assign to (only used if no lane is already assigned)")]
    public LaneType laneType = LaneType.Bottom;
    
    [Tooltip("Team to assign to (only used if no team is already assigned)")]
    public Team team = Team.Blue;
    
    [Tooltip("Whether to respect existing lane and team assignments")]
    public bool respectExistingAssignments = true;
    
    [Header("Debug Settings")]
    [Tooltip("Whether to show debug information")]
    public bool showDebug = true;
    
    private void Awake()
    {
        // Get or add required components
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        if (champion == null)
        {
            champion = GetComponent<Champion>();
            if (champion == null)
            {
                champion = gameObject.AddComponent<Champion>();
            }
        }
        
        if (movement == null)
        {
            movement = GetComponent<ChampionMovement>();
            if (movement == null)
            {
                movement = gameObject.AddComponent<ChampionMovement>();
            }
        }
    }
    
    private void Start()
    {
        // Set up the entity
        SetupEntity();
        
        // Start moving if enabled
        if (startMovingAutomatically)
        {
            StartMoving();
        }
    }
    
    /// <summary>
    /// Sets up the entity with the specified team and lane
    /// </summary>
    private void SetupEntity()
    {
        // Set sprite color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = entityColor;
        }
        
        // Set team and lane only if not already set or if we're not respecting existing assignments
        if (champion != null)
        {
            if (showDebug)
            {
                Debug.Log($"TestEntity {gameObject.name} - Current team: {champion.team}, Lane type: {champion.assignedLaneType}");
            }
            
            // Only set team if not already set or if we're not respecting existing assignments
            if (!respectExistingAssignments || champion.team == Team.Neutral)
            {
                if (showDebug)
                {
                    Debug.Log($"TestEntity {gameObject.name} - Setting team to {team}");
                }
                champion.SetTeam(team);
            }
            
            // Only set lane type if not already set or if we're not respecting existing assignments
            if (!respectExistingAssignments || champion.assignedLaneType == LaneType.None)
            {
                if (showDebug)
                {
                    Debug.Log($"TestEntity {gameObject.name} - Setting lane type to {laneType}");
                }
                champion.SetLaneType(laneType);
            }
            else
            {
                // Ensure the champion's lane assignment is properly initialized
                if (showDebug)
                {
                    Debug.Log($"TestEntity {gameObject.name} - Respecting existing lane assignment: {champion.assignedLaneType}");
                }
                champion.AssignToLane();
            }
        }
    }
    
    /// <summary>
    /// Starts the entity moving along its assigned lane
    /// </summary>
    public void StartMoving()
    {
        if (movement != null)
        {
            // Ensure the champion has a lane assigned
            if (movement.assignedLane == null)
            {
                Debug.LogWarning($"Test entity {gameObject.name} has no lane assigned. Trying to assign now...");
                
                // Try to assign a lane based on the champion's lane type
                if (champion != null)
                {
                    LaneManager laneManager = FindObjectOfType<LaneManager>();
                    if (laneManager != null)
                    {
                        Lane lane = laneManager.GetLane(champion.assignedLaneType);
                        if (lane != null)
                        {
                            movement.AssignLane(lane, champion.team != Team.Red);
                            Debug.Log($"Assigned {gameObject.name} to {lane.laneName} based on lane type {champion.assignedLaneType}");
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find lane of type {champion.assignedLaneType} for team {champion.team}");
                        }
                    }
                }
                
                // If still no lane, try to find any lane
                if (movement.assignedLane == null)
                {
                    Lane[] lanes = FindObjectsOfType<Lane>();
                    if (lanes.Length > 0)
                    {
                        movement.AssignLane(lanes[0], champion.team != Team.Red);
                        Debug.Log($"Assigned {gameObject.name} to first available lane: {movement.assignedLane.laneName}");
                    }
                    else
                    {
                        Debug.LogError("No lanes found in the scene. Cannot start moving.");
                        return;
                    }
                }
            }
            
            // Start moving
            movement.enabled = true;
            Debug.Log($"Test entity {gameObject.name} started moving along {movement.assignedLane.laneName}");
        }
    }
    
    /// <summary>
    /// Stops the entity from moving
    /// </summary>
    public void StopMoving()
    {
        if (movement != null)
        {
            movement.enabled = false;
            Debug.Log($"Test entity {gameObject.name} stopped moving");
        }
    }
} 