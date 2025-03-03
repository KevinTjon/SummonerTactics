using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the ChampionMovement component that provides tools for managing champion movement properties.
/// </summary>
[CustomEditor(typeof(ChampionMovement))]
public class ChampionMovementEditor : Editor
{
    private ChampionMovement championMovement;
    private SerializedProperty moveSpeedProperty;
    private SerializedProperty waypointReachedThresholdProperty;
    private SerializedProperty loopWaypointsProperty;
    private SerializedProperty assignedLaneProperty;
    private SerializedProperty moveForwardProperty;
    private SerializedProperty showDebugProperty;
    
    private bool showMovementSettings = true;
    private bool showLaneSettings = true;
    private bool showDebugSettings = true;
    
    private void OnEnable()
    {
        championMovement = (ChampionMovement)target;
        moveSpeedProperty = serializedObject.FindProperty("moveSpeed");
        waypointReachedThresholdProperty = serializedObject.FindProperty("waypointReachedThreshold");
        loopWaypointsProperty = serializedObject.FindProperty("loopWaypoints");
        assignedLaneProperty = serializedObject.FindProperty("assignedLane");
        moveForwardProperty = serializedObject.FindProperty("moveForward");
        showDebugProperty = serializedObject.FindProperty("showDebug");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Movement Settings Section
        showMovementSettings = EditorGUILayout.Foldout(showMovementSettings, "Movement Settings", true);
        if (showMovementSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(moveSpeedProperty);
            EditorGUILayout.PropertyField(waypointReachedThresholdProperty);
            EditorGUILayout.PropertyField(loopWaypointsProperty);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Lane Settings Section
        showLaneSettings = EditorGUILayout.Foldout(showLaneSettings, "Lane Settings", true);
        if (showLaneSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(assignedLaneProperty);
            EditorGUILayout.PropertyField(moveForwardProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Closest Lane"))
            {
                FindClosestLane();
            }
            
            if (GUILayout.Button("Position At Lane Start"))
            {
                PositionAtLaneStart();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Debug Settings Section
        showDebugSettings = EditorGUILayout.Foldout(showDebugSettings, "Debug Settings", true);
        if (showDebugSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(showDebugProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Log Lane Info"))
            {
                LogLaneInfo();
            }
            
            if (GUILayout.Button("Log Waypoint Info"))
            {
                LogWaypointInfo();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void FindClosestLane()
    {
        Undo.RecordObject(championMovement, "Find Closest Lane");
        
        // Find LaneManager in the scene
        LaneManager laneManager = FindObjectOfType<LaneManager>();
        if (laneManager == null)
        {
            Debug.LogError("No LaneManager found in the scene!");
            return;
        }
        
        // Get champion position
        Vector3 championPosition = championMovement.transform.position;
        
        // Find the closest lane
        Lane closestLane = null;
        float closestDistance = float.MaxValue;
        
        // Check Top Lane
        if (laneManager.topLane != null && laneManager.topLane.waypoints.Count > 0)
        {
            float distance = GetDistanceToLane(laneManager.topLane, championPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = laneManager.topLane;
            }
        }
        
        // Check Mid Lane
        if (laneManager.midLane != null && laneManager.midLane.waypoints.Count > 0)
        {
            float distance = GetDistanceToLane(laneManager.midLane, championPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = laneManager.midLane;
            }
        }
        
        // Check Bottom Lane
        if (laneManager.bottomLane != null && laneManager.bottomLane.waypoints.Count > 0)
        {
            float distance = GetDistanceToLane(laneManager.bottomLane, championPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = laneManager.bottomLane;
            }
        }
        
        // Assign the closest lane
        if (closestLane != null)
        {
            championMovement.assignedLane = closestLane;
            
            // Get the closest waypoint index
            int closestWaypointIndex = closestLane.GetClosestWaypointIndex(championPosition);
            
            Debug.Log($"Assigned champion to {closestLane.laneName} at waypoint {closestWaypointIndex}");
            
            // Try to determine movement direction based on the closest waypoint
            if (closestWaypointIndex < closestLane.waypoints.Count / 2)
            {
                championMovement.moveForward = true;
                Debug.Log("Set movement direction to forward based on waypoint position");
            }
            else
            {
                championMovement.moveForward = false;
                Debug.Log("Set movement direction to backward based on waypoint position");
            }
            
            // Try to get the champion component to set the lane type
            Champion champion = championMovement.GetComponent<Champion>();
            if (champion != null)
            {
                // Determine lane type based on the lane name
                if (closestLane.laneName.ToLower().Contains("top"))
                {
                    champion.assignedLaneType = LaneType.Top;
                }
                else if (closestLane.laneName.ToLower().Contains("mid"))
                {
                    champion.assignedLaneType = LaneType.Mid;
                }
                else if (closestLane.laneName.ToLower().Contains("bottom"))
                {
                    champion.assignedLaneType = LaneType.Bottom;
                }
                
                Debug.Log($"Updated champion lane type to {champion.assignedLaneType}");
            }
        }
        else
        {
            Debug.LogError("No lanes found in the scene!");
        }
    }
    
    private float GetDistanceToLane(Lane lane, Vector3 position)
    {
        float minDistance = float.MaxValue;
        
        // Check distance to each waypoint
        foreach (Transform waypoint in lane.waypoints)
        {
            float distance = Vector3.Distance(position, waypoint.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        
        return minDistance;
    }
    
    private void PositionAtLaneStart()
    {
        Undo.RecordObject(championMovement.transform, "Position At Lane Start");
        
        // Check if a lane is assigned
        if (championMovement.assignedLane == null)
        {
            Debug.LogError("No lane assigned to the champion!");
            return;
        }
        
        // Get the champion component
        Champion champion = championMovement.GetComponent<Champion>();
        Team team = Team.Blue;
        
        // Get the team from the champion component if available
        if (champion != null)
        {
            team = champion.team;
        }
        
        // Get the start waypoint index for the team
        int startIndex = championMovement.assignedLane.GetStartWaypointIndex(team);
        
        // Position the champion at the start waypoint
        if (championMovement.assignedLane.waypoints.Count > 0 && 
            startIndex >= 0 && 
            startIndex < championMovement.assignedLane.waypoints.Count)
        {
            Vector3 startPosition = championMovement.assignedLane.waypoints[startIndex].position;
            championMovement.transform.position = startPosition;
            
            // Set the movement direction based on the team
            championMovement.moveForward = team == Team.Blue;
            
            Debug.Log($"Positioned champion at the start of {championMovement.assignedLane.laneName} " +
                      $"(waypoint {startIndex}) with direction: {(championMovement.moveForward ? "Forward" : "Backward")}");
        }
        else
        {
            Debug.LogError($"Lane {championMovement.assignedLane.laneName} has no waypoints or invalid start index!");
        }
    }
    
    private void LogLaneInfo()
    {
        // Check if a lane is assigned
        if (championMovement.assignedLane == null)
        {
            Debug.LogError("No lane assigned to the champion!");
            return;
        }
        
        Lane lane = championMovement.assignedLane;
        
        Debug.Log("=== Lane Info ===");
        Debug.Log($"Lane Name: {lane.laneName}");
        Debug.Log($"Lane Team: {lane.laneTeam}");
        Debug.Log($"Waypoint Count: {lane.waypoints.Count}");
        Debug.Log($"Movement Direction: {(championMovement.moveForward ? "Forward" : "Backward")}");
        Debug.Log("=================");
    }
    
    private void LogWaypointInfo()
    {
        // Check if a lane is assigned
        if (championMovement.assignedLane == null)
        {
            Debug.LogError("No lane assigned to the champion!");
            return;
        }
        
        Lane lane = championMovement.assignedLane;
        
        Debug.Log("=== Waypoint Info ===");
        
        // Log information about each waypoint
        for (int i = 0; i < lane.waypoints.Count; i++)
        {
            Transform waypoint = lane.waypoints[i];
            if (waypoint != null)
            {
                Debug.Log($"Waypoint {i}: {waypoint.name} at {waypoint.position}");
            }
            else
            {
                Debug.LogWarning($"Waypoint {i} is null!");
            }
        }
        
        // Log the current waypoint
        Champion champion = championMovement.GetComponent<Champion>();
        Team team = champion != null ? champion.team : Team.Blue;
        int startIndex = lane.GetStartWaypointIndex(team);
        
        Debug.Log($"Start Waypoint for Team {team}: {startIndex}");
        if (startIndex >= 0 && startIndex < lane.waypoints.Count)
        {
            Transform startWaypoint = lane.waypoints[startIndex];
            if (startWaypoint != null)
            {
                Debug.Log($"Start Waypoint: {startIndex} ({startWaypoint.name}) at {startWaypoint.position}");
                Debug.Log($"Distance to Start Waypoint: {Vector3.Distance(championMovement.transform.position, startWaypoint.position)}");
            }
        }
        
        Debug.Log("=====================");
    }
} 