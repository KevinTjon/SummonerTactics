using UnityEngine;

/// <summary>
/// Handles movement for champions along lanes.
/// </summary>
public class ChampionMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the champion")]
    public float moveSpeed = 5f;
    
    [Tooltip("Distance threshold to consider a waypoint reached")]
    public float waypointReachedThreshold = 0.5f;
    
    [Tooltip("Whether to loop through waypoints")]
    public bool loopWaypoints = false;
    
    [Header("Lane Settings")]
    [Tooltip("The lane this champion should follow")]
    public Lane assignedLane;
    
    [Tooltip("Whether to move forward along the lane (true for Blue team, false for Red team)")]
    public bool moveForward = true;
    
    [Header("Debug")]
    [Tooltip("Whether to show debug information")]
    public bool showDebug = true;
    
    // Current waypoint index
    private int currentWaypointIndex = 0;
    
    // Whether the champion has reached the end of the lane
    private bool reachedEndOfLane = false;
    
    // Reference to the champion component
    private Champion championComponent;
    
    private void Awake()
    {
        // Get the champion component
        championComponent = GetComponent<Champion>();
    }
    
    private void Start()
    {
        // If no lane is assigned, try to find one
        if (assignedLane == null)
        {
            FindLane();
        }
        
        // Set initial waypoint based on team
        if (assignedLane != null)
        {
            // Blue team starts at their base (index 0), Red team starts at their base (last index)
            if (championComponent != null)
            {
                // Blue team moves from index 0 to last index
                // Red team moves from last index to index 0
                moveForward = (championComponent.team == Team.Blue);
                
                if (championComponent.team == Team.Blue)
                {
                    // Blue team starts at their base (index 0)
                    currentWaypointIndex = 0;
                    if (showDebug)
                    {
                        Debug.Log($"Blue champion {gameObject.name} starting at waypoint {currentWaypointIndex} (Blue base) in lane {assignedLane.laneName}, moving forward");
                    }
                }
                else
                {
                    // Red team starts at their base (last index)
                    currentWaypointIndex = assignedLane.GetWaypointCount() - 1;
                    if (showDebug)
                    {
                        Debug.Log($"Red champion {gameObject.name} starting at waypoint {currentWaypointIndex} (Red base) in lane {assignedLane.laneName}, moving backward");
                    }
                }
            }
            else
            {
                // Fallback to closest waypoint if no champion component
                currentWaypointIndex = assignedLane.GetClosestWaypointIndex(transform.position);
                if (showDebug)
                {
                    Debug.Log($"Champion {gameObject.name} starting at closest waypoint {currentWaypointIndex} in lane {assignedLane.laneName}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Champion {gameObject.name} has no assigned lane!");
        }
    }
    
    private void Update()
    {
        if (assignedLane == null || reachedEndOfLane)
            return;
            
        MoveAlongLane();
    }
    
    /// <summary>
    /// Finds a lane to follow based on the champion's position
    /// </summary>
    private void FindLane()
    {
        // Find all lanes in the scene
        Lane[] lanes = FindObjectsOfType<Lane>();
        
        if (lanes.Length == 0)
        {
            Debug.LogWarning("No lanes found in the scene!");
            return;
        }
        
        // Find the closest lane
        float closestDistance = float.MaxValue;
        Lane closestLane = null;
        
        foreach (Lane lane in lanes)
        {
            // Skip lanes that don't match our team
            if (championComponent != null && lane.laneTeam != Team.Neutral && lane.laneTeam != championComponent.team)
                continue;
                
            // Find the closest waypoint in this lane
            int closestWaypointIndex = lane.GetClosestWaypointIndex(transform.position);
            float distance = Vector3.Distance(transform.position, lane.GetWaypointPosition(closestWaypointIndex));
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = lane;
            }
        }
        
        if (closestLane != null)
        {
            assignedLane = closestLane;
            Debug.Log($"Champion {gameObject.name} assigned to lane {assignedLane.laneName}");
        }
    }
    
    /// <summary>
    /// Moves the champion along the assigned lane
    /// </summary>
    private void MoveAlongLane()
    {
        if (assignedLane == null || assignedLane.GetWaypointCount() == 0)
            return;
            
        // Get current target waypoint
        Vector3 targetPosition = assignedLane.GetWaypointPosition(currentWaypointIndex);
        
        // Move towards the waypoint
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
        
        // Check if waypoint is reached
        float distanceToWaypoint = Vector3.Distance(transform.position, targetPosition);
        if (distanceToWaypoint < waypointReachedThreshold)
        {
            // Get next waypoint
            int nextWaypointIndex = GetNextWaypointIndex();
            
            // Check if we've completed the lane
            if (nextWaypointIndex == currentWaypointIndex)
            {
                reachedEndOfLane = true;
                if (showDebug)
                {
                    Debug.Log($"Champion {gameObject.name} reached the end of lane {assignedLane.laneName}");
                }
            }
            else
            {
                // Move to next waypoint
                currentWaypointIndex = nextWaypointIndex;
                if (showDebug)
                {
                    Debug.Log($"Champion {gameObject.name} moving to waypoint {currentWaypointIndex} in lane {assignedLane.laneName}");
                }
            }
        }
        
        // Rotate towards movement direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 to face forward
        }
    }
    
    /// <summary>
    /// Gets the next waypoint index based on the current index and team
    /// </summary>
    private int GetNextWaypointIndex()
    {
        if (assignedLane.GetWaypointCount() <= 1)
            return currentWaypointIndex;
            
        if (moveForward)
        {
            // Blue team moves forward (increasing index)
            if (currentWaypointIndex >= assignedLane.GetWaypointCount() - 1)
            {
                // Reached the end (Red base)
                return currentWaypointIndex;
            }
            return currentWaypointIndex + 1;
        }
        else
        {
            // Red team moves backward (decreasing index)
            if (currentWaypointIndex <= 0)
            {
                // Reached the end (Blue base)
                return currentWaypointIndex;
            }
            return currentWaypointIndex - 1;
        }
    }
    
    /// <summary>
    /// Assigns a lane to this champion
    /// </summary>
    public void AssignLane(Lane lane, bool forward = true)
    {
        assignedLane = lane;
        moveForward = forward;
        reachedEndOfLane = false;
        
        // Reset waypoint index based on team
        if (assignedLane != null)
        {
            if (championComponent != null)
            {
                if (championComponent.team == Team.Blue)
                {
                    // Blue team starts at their base (index 0)
                    currentWaypointIndex = 0;
                }
                else
                {
                    // Red team starts at their base (last index)
                    currentWaypointIndex = assignedLane.GetWaypointCount() - 1;
                }
            }
            else
            {
                // Fallback if no champion component
                currentWaypointIndex = moveForward ? 0 : assignedLane.GetWaypointCount() - 1;
            }
            
            if (showDebug)
            {
                Debug.Log($"Champion {gameObject.name} assigned to lane {assignedLane.laneName}, starting at waypoint {currentWaypointIndex}, moving {(moveForward ? "forward" : "backward")}");
            }
        }
    }
    
    /// <summary>
    /// Stops the champion's movement
    /// </summary>
    public void StopMovement()
    {
        reachedEndOfLane = true;
    }
    
    /// <summary>
    /// Resumes the champion's movement
    /// </summary>
    public void ResumeMovement()
    {
        reachedEndOfLane = false;
    }
} 