using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a lane on the map with waypoints for champions to follow.
/// </summary>
public class Lane : MonoBehaviour
{
    [Tooltip("The name of this lane (Top, Mid, Bottom)")]
    public string laneName;
    
    [Tooltip("The team this lane belongs to (Blue or Red)")]
    public Team laneTeam;
    
    [Tooltip("Waypoints that define the path of this lane")]
    public List<Transform> waypoints = new List<Transform>();
    
    [Header("Debug Settings")]
    [Tooltip("Debug visualization color")]
    public Color debugColor = Color.yellow;
    
    [Tooltip("Whether to show debug visualization")]
    public bool showDebug = true;
    
    [Tooltip("Size of waypoint gizmos")]
    public float waypointGizmoSize = 0.5f;
    
    [Tooltip("Whether to show waypoint indices")]
    public bool showWaypointIndices = true;
    
    private void Awake()
    {
        // If no waypoints are manually assigned, try to find them in children
        if (waypoints.Count == 0)
        {
            CollectWaypointsFromChildren();
        }
        
        // Set default lane name if not set
        if (string.IsNullOrEmpty(laneName))
        {
            laneName = gameObject.name;
        }
    }
    
    /// <summary>
    /// Collects waypoints from child objects
    /// </summary>
    public void CollectWaypointsFromChildren()
    {
        // Clear existing waypoints
        waypoints.Clear();
        
        // Get all child transforms
        foreach (Transform child in transform)
        {
            waypoints.Add(child);
        }
        
        Debug.Log($"Lane {laneName}: Collected {waypoints.Count} waypoints from children");
    }
    
    /// <summary>
    /// Gets the position of the waypoint at the specified index
    /// </summary>
    public Vector3 GetWaypointPosition(int index)
    {
        if (index < 0 || index >= waypoints.Count)
        {
            Debug.LogWarning($"Lane {laneName}: Waypoint index {index} out of range (0-{waypoints.Count - 1})");
            return transform.position;
        }
        
        return waypoints[index].position;
    }
    
    /// <summary>
    /// Gets the total number of waypoints in this lane
    /// </summary>
    public int GetWaypointCount()
    {
        return waypoints.Count;
    }
    
    /// <summary>
    /// Gets the closest waypoint index to the specified position
    /// </summary>
    public int GetClosestWaypointIndex(Vector3 position)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(position, waypoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }
    
    /// <summary>
    /// Gets the next waypoint index based on the current index and direction
    /// </summary>
    public int GetNextWaypointIndex(int currentIndex, bool moveForward = true)
    {
        if (waypoints.Count <= 1)
            return currentIndex;
            
        if (moveForward)
        {
            // Moving from start to end (index 0 to Count-1)
            if (currentIndex >= waypoints.Count - 1)
            {
                // Reached the end, return the same index to indicate completion
                return currentIndex;
            }
            return currentIndex + 1;
        }
        else
        {
            // Moving from end to start (index Count-1 to 0)
            if (currentIndex <= 0)
            {
                // Reached the start, return the same index to indicate completion
                return currentIndex;
            }
            return currentIndex - 1;
        }
    }
    
    /// <summary>
    /// Gets the start waypoint index for a specific team
    /// </summary>
    public int GetStartWaypointIndex(Team team)
    {
        // Blue team starts at the beginning (index 0), Red team starts at the end
        return team == Team.Blue ? 0 : waypoints.Count - 1;
    }
    
    /// <summary>
    /// Gets the end waypoint index for a specific team
    /// </summary>
    public int GetEndWaypointIndex(Team team)
    {
        // Blue team ends at the end (index Count-1), Red team ends at the beginning
        return team == Team.Blue ? waypoints.Count - 1 : 0;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebug || waypoints.Count <= 1)
            return;
            
        Gizmos.color = debugColor;
        
        // Draw lines between waypoints
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i+1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }
        }
        
        // Draw spheres at waypoints
        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint != null)
            {
                // Draw different colors for start and end waypoints
                if (i == 0)
                {
                    // Blue team start
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(waypoint.position, waypointGizmoSize * 1.5f);
                }
                else if (i == waypoints.Count - 1)
                {
                    // Red team start
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(waypoint.position, waypointGizmoSize * 1.5f);
                }
                else
                {
                    // Regular waypoint
                    Gizmos.color = debugColor;
                    Gizmos.DrawSphere(waypoint.position, waypointGizmoSize);
                }
                
                // Draw waypoint indices
                if (showWaypointIndices)
                {
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(waypoint.position + Vector3.up * waypointGizmoSize, i.ToString());
#endif
                }
            }
        }
    }
}

/// <summary>
/// Enum representing the teams in the game
/// </summary>
public enum Team
{
    Blue,
    Red,
    Neutral
} 