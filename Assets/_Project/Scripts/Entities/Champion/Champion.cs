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
    }
    
    private void Start()
    {
        // Only assign to lane if not already assigned
        if (!laneAssigned && assignedLaneType != LaneType.None)
        {
            AssignToLane();
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