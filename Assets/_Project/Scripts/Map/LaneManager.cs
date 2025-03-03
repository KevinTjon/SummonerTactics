using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages lanes on the map and provides utilities for lane setup.
/// </summary>
public class LaneManager : MonoBehaviour
{
    [Header("Lane References")]
    [Tooltip("Reference to the top lane")]
    public Lane topLane;
    
    [Tooltip("Reference to the mid lane")]
    public Lane midLane;
    
    [Tooltip("Reference to the bottom lane")]
    public Lane bottomLane;
    
    [Header("Waypoint Settings")]
    [Tooltip("Number of waypoints to generate per lane")]
    public int waypointsPerLane = 10;
    
    [Tooltip("Whether to automatically generate waypoints")]
    public bool autoGenerateWaypoints = false;
    
    [Header("Team Settings")]
    [Tooltip("Whether to automatically assign teams to lanes")]
    public bool autoAssignTeams = true;
    
    [Header("Debug Settings")]
    [Tooltip("Whether to show debug information")]
    public bool showDebugInfo = true;
    
    private void Awake()
    {
        // Find lanes if not assigned
        FindLanes();
        
        // Assign teams if needed
        if (autoAssignTeams)
        {
            AssignTeamsToLanes();
        }
        
        // Generate waypoints if needed and explicitly requested
        if (autoGenerateWaypoints)
        {
            GenerateWaypoints();
        }
        
        // Log lane status
        if (showDebugInfo)
        {
            LogLaneStatus();
        }
    }
    
    /// <summary>
    /// Logs the status of all lanes
    /// </summary>
    private void LogLaneStatus()
    {
        Debug.Log("=== Lane Manager Status ===");
        Debug.Log($"Top Lane: {(topLane != null ? "Found" : "Not Found")}");
        Debug.Log($"Mid Lane: {(midLane != null ? "Found" : "Not Found")}");
        Debug.Log($"Bottom Lane: {(bottomLane != null ? "Found" : "Not Found")}");
        
        if (topLane != null)
        {
            Debug.Log($"Top Lane Waypoints: {topLane.waypoints.Count}");
            Debug.Log($"Top Lane Team: {topLane.laneTeam}");
        }
        
        if (midLane != null)
        {
            Debug.Log($"Mid Lane Waypoints: {midLane.waypoints.Count}");
            Debug.Log($"Mid Lane Team: {midLane.laneTeam}");
        }
        
        if (bottomLane != null)
        {
            Debug.Log($"Bottom Lane Waypoints: {bottomLane.waypoints.Count}");
            Debug.Log($"Bottom Lane Team: {bottomLane.laneTeam}");
        }
        Debug.Log("=========================");
    }
    
    /// <summary>
    /// Finds lanes in the scene if they are not assigned
    /// </summary>
    private void FindLanes()
    {
        if (topLane == null)
        {
            GameObject topLaneObj = GameObject.Find("TopLane");
            if (topLaneObj != null)
            {
                topLane = topLaneObj.GetComponent<Lane>();
                if (topLane == null)
                {
                    topLane = topLaneObj.AddComponent<Lane>();
                    topLane.laneName = "Top Lane";
                }
                Debug.Log($"Found TopLane GameObject: {topLaneObj.name}");
            }
            else
            {
                Debug.LogWarning("TopLane GameObject not found in scene!");
            }
        }
        
        if (midLane == null)
        {
            GameObject midLaneObj = GameObject.Find("MidLane");
            if (midLaneObj != null)
            {
                midLane = midLaneObj.GetComponent<Lane>();
                if (midLane == null)
                {
                    midLane = midLaneObj.AddComponent<Lane>();
                    midLane.laneName = "Mid Lane";
                }
                Debug.Log($"Found MidLane GameObject: {midLaneObj.name}");
            }
            else
            {
                Debug.LogWarning("MidLane GameObject not found in scene!");
            }
        }
        
        if (bottomLane == null)
        {
            GameObject bottomLaneObj = GameObject.Find("BottomLane");
            if (bottomLaneObj != null)
            {
                bottomLane = bottomLaneObj.GetComponent<Lane>();
                if (bottomLane == null)
                {
                    bottomLane = bottomLaneObj.AddComponent<Lane>();
                    bottomLane.laneName = "Bottom Lane";
                }
                Debug.Log($"Found BottomLane GameObject: {bottomLaneObj.name}");
            }
            else
            {
                Debug.LogWarning("BottomLane GameObject not found in scene!");
            }
        }
    }
    
    /// <summary>
    /// Assigns teams to lanes based on standard MOBA conventions
    /// </summary>
    private void AssignTeamsToLanes()
    {
        // In a standard MOBA, lanes are mirrored for each team
        // Each lane is used by both teams, but in opposite directions
        
        if (topLane != null)
        {
            // Top lane is typically Blue team's top and Red team's bottom
            topLane.laneTeam = Team.Neutral; // Neutral means both teams use it
            topLane.laneName = "Top Lane";
            Debug.Log($"Assigned {topLane.laneName} to Team {topLane.laneTeam}");
        }
        
        if (midLane != null)
        {
            // Mid lane is used by both teams
            midLane.laneTeam = Team.Neutral;
            midLane.laneName = "Mid Lane";
            Debug.Log($"Assigned {midLane.laneName} to Team {midLane.laneTeam}");
        }
        
        if (bottomLane != null)
        {
            // Bottom lane is typically Blue team's bottom and Red team's top
            bottomLane.laneTeam = Team.Neutral;
            bottomLane.laneName = "Bottom Lane";
            Debug.Log($"Assigned {bottomLane.laneName} to Team {bottomLane.laneTeam}");
        }
    }
    
    /// <summary>
    /// Generates waypoints for each lane
    /// </summary>
    private void GenerateWaypoints()
    {
        // Only generate waypoints if explicitly requested
        if (!autoGenerateWaypoints)
        {
            Debug.Log("Waypoint auto-generation is disabled. Use the Lane Editor to create waypoints manually.");
            return;
        }
        
        if (topLane != null && topLane.waypoints.Count == 0)
        {
            GenerateWaypointsForLane(topLane);
        }
        
        if (midLane != null && midLane.waypoints.Count == 0)
        {
            GenerateWaypointsForLane(midLane);
        }
        
        if (bottomLane != null && bottomLane.waypoints.Count == 0)
        {
            GenerateWaypointsForLane(bottomLane);
        }
        
        Debug.Log("Generated waypoints for lanes. Consider using the Lane Editor for more precise waypoint placement.");
    }
    
    /// <summary>
    /// Generates waypoints for a specific lane
    /// </summary>
    private void GenerateWaypointsForLane(Lane lane)
    {
        // Clear existing waypoints
        lane.waypoints.Clear();
        
        // Get all child objects
        List<Transform> childObjects = new List<Transform>();
        foreach (Transform child in lane.transform)
        {
            childObjects.Add(child);
        }
        
        // If there are at least 2 child objects, use them as start and end points
        if (childObjects.Count >= 2)
        {
            Transform startPoint = childObjects[0];
            Transform endPoint = childObjects[1];
            
            // Generate waypoints between start and end
            for (int i = 0; i < waypointsPerLane; i++)
            {
                float t = i / (float)(waypointsPerLane - 1);
                Vector3 position = Vector3.Lerp(startPoint.position, endPoint.position, t);
                
                // Create a waypoint
                GameObject waypoint = new GameObject($"Waypoint_{i}");
                waypoint.transform.position = position;
                waypoint.transform.SetParent(lane.transform);
                
                // Add to waypoints list
                lane.waypoints.Add(waypoint.transform);
            }
            
            Debug.Log($"Generated {waypointsPerLane} waypoints for {lane.laneName}");
        }
        else
        {
            Debug.LogWarning($"Lane {lane.laneName} does not have enough child objects to generate waypoints");
        }
    }
    
    /// <summary>
    /// Gets a lane by type
    /// </summary>
    public Lane GetLane(LaneType laneType)
    {
        Lane result = null;
        
        switch (laneType)
        {
            case LaneType.Top:
                result = topLane;
                if (result != null)
                {
                    Debug.Log($"Found Top Lane: {result.laneName}");
                }
                break;
            case LaneType.Mid:
                result = midLane;
                if (result != null)
                {
                    Debug.Log($"Found Mid Lane: {result.laneName}");
                }
                break;
            case LaneType.Bottom:
                result = bottomLane;
                if (result != null)
                {
                    Debug.Log($"Found Bottom Lane: {result.laneName}");
                }
                break;
            case LaneType.None:
                Debug.LogWarning("Lane type is None, cannot find a matching lane");
                break;
        }
        
        if (result == null)
        {
            Debug.LogWarning($"Could not find lane of type {laneType}. Falling back to any available lane.");
            
            // Fall back to any available lane
            if (topLane != null)
            {
                Debug.Log($"Falling back to Top Lane: {topLane.laneName}");
                return topLane;
            }
            if (midLane != null)
            {
                Debug.Log($"Falling back to Mid Lane: {midLane.laneName}");
                return midLane;
            }
            if (bottomLane != null)
            {
                Debug.Log($"Falling back to Bottom Lane: {bottomLane.laneName}");
                return bottomLane;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets a lane by name
    /// </summary>
    public Lane GetLane(string laneName)
    {
        if (topLane != null && topLane.laneName.ToLower().Contains(laneName.ToLower()))
            return topLane;
            
        if (midLane != null && midLane.laneName.ToLower().Contains(laneName.ToLower()))
            return midLane;
            
        if (bottomLane != null && bottomLane.laneName.ToLower().Contains(laneName.ToLower()))
            return bottomLane;
            
        return null;
    }
} 