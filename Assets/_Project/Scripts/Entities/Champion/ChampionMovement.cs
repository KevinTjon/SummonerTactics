using UnityEngine;
using System.Collections.Generic;

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
    
    [Header("Interaction Settings")]
    [Tooltip("Whether this champion is currently engaged with an opponent")]
    public bool isEngagedWithOpponent = false;
    
    [Tooltip("The current opponent this champion is engaged with")]
    public Champion currentOpponent;
    
    [Header("Combat Settings")]
    [Tooltip("Whether to automatically attack when engaged")]
    public bool autoAttack = true;
    
    [Tooltip("Base damage per attack")]
    public float attackDamage = 10f;
    
    [Tooltip("Time between attacks in seconds")]
    public float attackCooldown = 1.5f;
    
    [Tooltip("Visual effect prefab for attacks")]
    public GameObject attackEffectPrefab;
    
    [Header("Debug")]
    [Tooltip("Whether to show debug information")]
    public bool showDebug = true;
    
    // Current waypoint index
    private int currentWaypointIndex = 0;
    
    // Whether the champion has reached the end of the lane
    private bool reachedEndOfLane = false;
    
    // Reference to the champion component
    private Champion championComponent;
    
    // Layer mask for detecting champions
    private LayerMask championLayerMask;
    
    // Time until next attack
    private float nextAttackTime = 0f;
    
    private void Awake()
    {
        // Get the champion component
        championComponent = GetComponent<Champion>();
        
        // Set up layer mask for champion detection
        championLayerMask = LayerMask.GetMask("Champion");
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
                        Debug.Log($"Champion {championComponent.championName} starting at Blue base (index 0)");
                    }
                }
                else
                {
                    // Red team starts at their base (last index)
                    currentWaypointIndex = assignedLane.waypoints.Count - 1;
                    if (showDebug)
                    {
                        Debug.Log($"Champion {championComponent.championName} starting at Red base (index {currentWaypointIndex})");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"Champion {gameObject.name} has no assigned lane!");
        }
        
        // Try to load attack effect prefab from Resources if none is assigned
        if (attackEffectPrefab == null)
        {
            attackEffectPrefab = Resources.Load<GameObject>("Prefabs/AttackEffect");
            
            if (attackEffectPrefab != null && showDebug)
            {
                Debug.Log("Loaded attack effect prefab from Resources");
            }
        }
    }
    
    /// <summary>
    /// Attempts to find a lane for this champion
    /// </summary>
    private void FindLane()
    {
        // Try to get lane from champion component
        if (championComponent != null && championComponent.assignedLaneType != LaneType.None)
        {
            // Find the LaneManager
            LaneManager laneManager = FindObjectOfType<LaneManager>();
            if (laneManager != null)
            {
                // Get the lane based on lane type
                Lane lane = laneManager.GetLane(championComponent.assignedLaneType);
                
                if (lane != null)
                {
                    // Assign the lane
                    AssignLane(lane, championComponent.team == Team.Blue);
                    
                    if (showDebug)
                    {
                        Debug.Log($"Found lane {lane.laneName} for champion {championComponent.championName}");
                    }
                    
                    return;
                }
            }
        }
        
        // If we couldn't find a specific lane, try to find any lane
        Lane[] lanes = FindObjectsOfType<Lane>();
        if (lanes.Length > 0)
        {
            // Assign the first lane
            AssignLane(lanes[0], championComponent != null && championComponent.team == Team.Blue);
            
            if (showDebug)
            {
                Debug.Log($"Assigned champion to fallback lane {lanes[0].laneName}");
            }
        }
        else
        {
            Debug.LogWarning("No lanes found in the scene!");
        }
    }
    
    private void Update()
    {
        // Don't do anything if no lane is assigned or we've reached the end of the lane
        if (assignedLane == null || reachedEndOfLane)
            return;
            
        // Don't do anything if the champion is dead
        if (!championComponent.isAlive)
            return;
        
        // Handle engagement logic if already engaged
        if (isEngagedWithOpponent)
        {
            // If engaged with an opponent, face them
            if (currentOpponent != null && currentOpponent.isAlive)
            {
                FaceOpponent(currentOpponent.transform.position);
                
                // Auto attack if enabled
                if (autoAttack && Time.time >= nextAttackTime)
                {
                    AttackOpponent();
                }
            }
            else
            {
                // If opponent is no longer valid or is dead, disengage
                DisengageFromOpponent();
                
                // Resume movement immediately
                MoveAlongLane();
                
                if (showDebug)
                {
                    Debug.Log($"Champion {championComponent.championName} disengaged from invalid opponent and resumed movement");
                }
            }
            
            // Don't move while engaged with a valid opponent
            return;
        }
        
        // Move along the lane if not engaged with an opponent
        MoveAlongLane();
    }
    
    /// <summary>
    /// Called when this champion collides with another object
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Always log collision for debugging
        Debug.Log($"Champion {championComponent.championName} collided with {other.gameObject.name}");
        
        // Skip if already engaged
        if (isEngagedWithOpponent)
        {
            Debug.Log($"Champion {championComponent.championName} is already engaged with an opponent, ignoring collision with {other.gameObject.name}");
            return;
        }
            
        // Check if the other object is a champion
        Champion otherChampion = other.GetComponent<Champion>();
        if (otherChampion == null)
        {
            Debug.Log($"Object {other.gameObject.name} is not a champion, ignoring collision");
            return;
        }
            
        // Skip if it's on the same team
        if (otherChampion.team == championComponent.team)
        {
            Debug.Log($"Champion {otherChampion.championName} is on the same team as {championComponent.championName}, ignoring collision");
            return;
        }
            
        // Check if they're in the same lane
        ChampionMovement otherMovement = otherChampion.GetComponent<ChampionMovement>();
        if (otherMovement != null && otherMovement.assignedLane == assignedLane)
        {
            // Engage with this opponent
            EngageWithOpponent(otherChampion);
            
            // Also make the opponent engage with us
            otherMovement.EngageWithOpponent(championComponent);
            
            Debug.Log($"Champion {championComponent.championName} collided with and engaged opponent {otherChampion.championName} in lane {assignedLane.laneName}");
        }
        else
        {
            Debug.Log($"Champion {otherChampion.championName} is not in the same lane as {championComponent.championName}, ignoring collision");
        }
    }
    
    /// <summary>
    /// Called when this champion stops colliding with another object
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Always log collision exit for debugging
        Debug.Log($"Champion {championComponent.championName} stopped colliding with {other.gameObject.name}");
        
        // Skip if not engaged
        if (!isEngagedWithOpponent || currentOpponent == null)
        {
            Debug.Log($"Champion {championComponent.championName} is not engaged with an opponent, ignoring collision exit with {other.gameObject.name}");
            return;
        }
            
        // Check if the other object is our current opponent
        Champion otherChampion = other.GetComponent<Champion>();
        if (otherChampion == null || otherChampion != currentOpponent)
        {
            Debug.Log($"Object {other.gameObject.name} is not the current opponent of {championComponent.championName}, ignoring collision exit");
            return;
        }
            
        // If we're no longer colliding with our opponent and they're still alive,
        // disengage and resume movement
        if (otherChampion.isAlive)
        {
            DisengageFromOpponent();
            
            // Also make the opponent disengage from us
            ChampionMovement otherMovement = otherChampion.GetComponent<ChampionMovement>();
            if (otherMovement != null && otherMovement.currentOpponent == championComponent)
            {
                otherMovement.DisengageFromOpponent();
            }
            
            Debug.Log($"Champion {championComponent.championName} stopped colliding with opponent {otherChampion.championName} and disengaged");
        }
        else
        {
            Debug.Log($"Champion {championComponent.championName} stopped colliding with opponent {otherChampion.championName} but opponent is dead, not disengaging");
        }
    }
    
    /// <summary>
    /// Engages with an opponent champion
    /// </summary>
    public void EngageWithOpponent(Champion opponent)
    {
        isEngagedWithOpponent = true;
        currentOpponent = opponent;
        
        // Face the opponent
        FaceOpponent(opponent.transform.position);
    }
    
    /// <summary>
    /// Disengages from the current opponent
    /// </summary>
    public void DisengageFromOpponent()
    {
        // Store reference to opponent for logging
        Champion previousOpponent = currentOpponent;
        
        // Clear engagement state
        isEngagedWithOpponent = false;
        currentOpponent = null;
        
        // Only resume movement if the champion is alive
        if (championComponent != null && championComponent.isAlive)
        {
            // Resume movement
            ResumeMovement();
            
            if (showDebug && previousOpponent != null)
            {
                Debug.Log($"Champion {championComponent.championName} disengaged from opponent {previousOpponent.championName} and resumed movement");
            }
        }
        else if (showDebug && previousOpponent != null)
        {
            Debug.Log($"Champion {championComponent.championName} disengaged from opponent {previousOpponent.championName} but is dead, not resuming movement");
        }
    }
    
    /// <summary>
    /// Makes the champion face the opponent
    /// </summary>
    private void FaceOpponent(Vector3 opponentPosition)
    {
        // Calculate direction to opponent
        Vector3 direction = (opponentPosition - transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            // Calculate angle to face opponent
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 to face forward
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
    
    /// <summary>
    /// Attacks the current opponent
    /// </summary>
    private void AttackOpponent()
    {
        if (currentOpponent == null || !currentOpponent.isAlive)
        {
            // If opponent is null or already dead, disengage and resume movement
            DisengageFromOpponent();
            return;
        }
            
        // Set next attack time
        nextAttackTime = Time.time + attackCooldown;
        
        // Apply damage to opponent
        currentOpponent.TakeDamage(attackDamage);
        
        // Show attack effect
        ShowAttackEffect();
        
        if (showDebug)
        {
            Debug.Log($"Champion {championComponent.championName} attacked {currentOpponent.championName} for {attackDamage} damage");
        }
        
        // Check if opponent died
        if (!currentOpponent.isAlive)
        {
            Debug.Log($"Champion {championComponent.championName} defeated {currentOpponent.championName} and is disengaging");
            
            // Disengage from dead opponent
            DisengageFromOpponent();
            
            // Force resume movement
            ResumeMovement();
            
            if (showDebug)
            {
                Debug.Log($"Champion {championComponent.championName} defeated {currentOpponent.championName} and resumed movement");
            }
        }
    }
    
    /// <summary>
    /// Shows a visual effect for attacks
    /// </summary>
    private void ShowAttackEffect()
    {
        if (currentOpponent == null)
            return;
            
        // Create a simple effect if no prefab is assigned
        if (attackEffectPrefab == null)
        {
            // Create a simple circle for the attack effect
            GameObject effectObj = new GameObject("AttackEffect");
            effectObj.transform.position = transform.position;
            
            // Add sprite renderer
            SpriteRenderer renderer = effectObj.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateCircleSprite(16, Color.yellow);
            renderer.sortingOrder = 5;
            
            // Add attack effect component
            AttackEffect effect = effectObj.AddComponent<AttackEffect>();
            effect.targetPosition = currentOpponent.transform.position;
            effect.duration = 0.3f;
            effect.effectColor = Color.yellow;
            
            // Set team color if champion component exists
            if (championComponent != null)
            {
                effect.SetColor(championComponent.teamColor);
            }
            
            // Set scale
            effectObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        else
        {
            // Instantiate attack effect from prefab
            GameObject effectObj = Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
            
            // Set up the effect
            AttackEffect effect = effectObj.GetComponent<AttackEffect>();
            if (effect != null)
            {
                effect.SetTarget(currentOpponent.transform.position);
                
                // Set team color if champion component exists
                if (championComponent != null)
                {
                    effect.SetColor(championComponent.teamColor);
                }
            }
            else
            {
                // If the prefab doesn't have the AttackEffect component, add it
                effect = effectObj.AddComponent<AttackEffect>();
                effect.SetTarget(currentOpponent.transform.position);
                
                // Set team color if champion component exists
                if (championComponent != null)
                {
                    effect.SetColor(championComponent.teamColor);
                }
                
                Debug.LogWarning("Attack effect prefab doesn't have AttackEffect component. Adding one automatically.");
            }
        }
        
        if (showDebug)
        {
            Debug.Log($"Champion {championComponent.championName} attacked {currentOpponent.championName}");
        }
    }
    
    /// <summary>
    /// Creates a simple circle sprite for the attack effect
    /// </summary>
    private Sprite CreateCircleSprite(int resolution, Color color)
    {
        // Create a simple circle texture
        Texture2D texture = new Texture2D(resolution, resolution);
        
        // Set pixels to create a circle
        float radius = resolution / 2f;
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (distance < radius)
                {
                    texture.SetPixel(x, y, color);
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
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
    
    /// <summary>
    /// Explicitly resets the waypoint index
    /// </summary>
    public void ResetWaypointIndex(int newIndex)
    {
        if (assignedLane != null)
        {
            // Ensure the index is within valid range
            int waypointCount = assignedLane.GetWaypointCount();
            if (waypointCount > 0)
            {
                currentWaypointIndex = Mathf.Clamp(newIndex, 0, waypointCount - 1);
                
                // Also reset the reached end of lane flag
                reachedEndOfLane = false;
                
                if (showDebug)
                {
                    Debug.Log($"Champion {championComponent.championName} waypoint index reset to {currentWaypointIndex}");
                }
            }
        }
    }
} 