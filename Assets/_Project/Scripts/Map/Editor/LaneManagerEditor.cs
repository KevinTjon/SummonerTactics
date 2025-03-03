using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the LaneManager component that provides tools for managing lanes.
/// </summary>
[CustomEditor(typeof(LaneManager))]
public class LaneManagerEditor : Editor
{
    private LaneManager laneManager;
    private SerializedProperty topLaneProperty;
    private SerializedProperty midLaneProperty;
    private SerializedProperty bottomLaneProperty;
    private SerializedProperty waypointsPerLaneProperty;
    private SerializedProperty autoGenerateWaypointsProperty;
    private SerializedProperty autoAssignTeamsProperty;
    private SerializedProperty showDebugInfoProperty;
    
    private bool showLaneCreation = true;
    private bool showLaneAssignment = true;
    
    private void OnEnable()
    {
        laneManager = (LaneManager)target;
        topLaneProperty = serializedObject.FindProperty("topLane");
        midLaneProperty = serializedObject.FindProperty("midLane");
        bottomLaneProperty = serializedObject.FindProperty("bottomLane");
        waypointsPerLaneProperty = serializedObject.FindProperty("waypointsPerLane");
        autoGenerateWaypointsProperty = serializedObject.FindProperty("autoGenerateWaypoints");
        autoAssignTeamsProperty = serializedObject.FindProperty("autoAssignTeams");
        showDebugInfoProperty = serializedObject.FindProperty("showDebugInfo");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Lane References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(topLaneProperty);
        EditorGUILayout.PropertyField(midLaneProperty);
        EditorGUILayout.PropertyField(bottomLaneProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Waypoint Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(waypointsPerLaneProperty);
        EditorGUILayout.PropertyField(autoGenerateWaypointsProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Team Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(autoAssignTeamsProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showDebugInfoProperty);
        
        EditorGUILayout.Space();
        
        // Lane Creation Section
        showLaneCreation = EditorGUILayout.Foldout(showLaneCreation, "Lane Creation Tools", true);
        if (showLaneCreation)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create All Lanes"))
            {
                CreateAllLanes();
            }
            
            if (GUILayout.Button("Find Existing Lanes"))
            {
                FindLanes();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Top Lane"))
            {
                CreateLane("Top");
            }
            
            if (GUILayout.Button("Create Mid Lane"))
            {
                CreateLane("Mid");
            }
            
            if (GUILayout.Button("Create Bottom Lane"))
            {
                CreateLane("Bottom");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Lane Assignment Section
        showLaneAssignment = EditorGUILayout.Foldout(showLaneAssignment, "Lane Assignment Tools", true);
        if (showLaneAssignment)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Assign Teams to Lanes"))
            {
                AssignTeamsToLanes();
            }
            
            if (GUILayout.Button("Generate Waypoints"))
            {
                GenerateWaypoints();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Log Lane Status"))
            {
                LogLaneStatus();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void CreateAllLanes()
    {
        CreateLane("Top");
        CreateLane("Mid");
        CreateLane("Bottom");
    }
    
    private void CreateLane(string laneType)
    {
        // Check if lane already exists
        SerializedProperty laneProperty = null;
        switch (laneType)
        {
            case "Top":
                laneProperty = topLaneProperty;
                break;
            case "Mid":
                laneProperty = midLaneProperty;
                break;
            case "Bottom":
                laneProperty = bottomLaneProperty;
                break;
        }
        
        if (laneProperty.objectReferenceValue != null)
        {
            if (!EditorUtility.DisplayDialog("Replace Lane", $"{laneType} Lane already exists. Replace it?", "Yes", "No"))
            {
                return;
            }
        }
        
        // Create lane GameObject
        GameObject laneObj = new GameObject($"{laneType}Lane");
        laneObj.transform.SetParent(laneManager.transform);
        
        // Add Lane component
        Lane lane = laneObj.AddComponent<Lane>();
        lane.laneName = $"{laneType} Lane";
        lane.laneTeam = Team.Neutral;
        
        // Set lane reference
        laneProperty.objectReferenceValue = lane;
        
        Debug.Log($"Created {laneType} Lane without waypoints. Use the Lane Editor to add waypoints manually.");
    }
    
    private void CreateDefaultWaypoints(Lane lane, string laneType)
    {
        // Clear existing waypoints
        foreach (Transform child in lane.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        lane.waypoints.Clear();
        
        // Create just two empty waypoints for reference
        GameObject blueWaypoint = new GameObject("BlueBaseWaypoint");
        blueWaypoint.transform.SetParent(lane.transform);
        
        GameObject redWaypoint = new GameObject("RedBaseWaypoint");
        redWaypoint.transform.SetParent(lane.transform);
        
        // Set positions based on lane type
        switch (laneType)
        {
            case "Top":
                blueWaypoint.transform.position = new Vector3(-10, 0, 0);
                redWaypoint.transform.position = new Vector3(10, 0, 0);
                break;
            case "Mid":
                blueWaypoint.transform.position = new Vector3(-10, -10, 0);
                redWaypoint.transform.position = new Vector3(10, 10, 0);
                break;
            case "Bottom":
                blueWaypoint.transform.position = new Vector3(0, -10, 0);
                redWaypoint.transform.position = new Vector3(0, 10, 0);
                break;
        }
        
        // Collect waypoints
        lane.CollectWaypointsFromChildren();
        
        Debug.Log($"Created minimal waypoints for {lane.laneName}. Use the Lane Editor to add more waypoints manually.");
    }
    
    private void FindLanes()
    {
        // Find Top Lane
        GameObject topLaneObj = GameObject.Find("TopLane");
        if (topLaneObj != null)
        {
            Lane topLane = topLaneObj.GetComponent<Lane>();
            if (topLane == null)
            {
                topLane = topLaneObj.AddComponent<Lane>();
                topLane.laneName = "Top Lane";
            }
            topLaneProperty.objectReferenceValue = topLane;
        }
        
        // Find Mid Lane
        GameObject midLaneObj = GameObject.Find("MidLane");
        if (midLaneObj != null)
        {
            Lane midLane = midLaneObj.GetComponent<Lane>();
            if (midLane == null)
            {
                midLane = midLaneObj.AddComponent<Lane>();
                midLane.laneName = "Mid Lane";
            }
            midLaneProperty.objectReferenceValue = midLane;
        }
        
        // Find Bottom Lane
        GameObject bottomLaneObj = GameObject.Find("BottomLane");
        if (bottomLaneObj != null)
        {
            Lane bottomLane = bottomLaneObj.GetComponent<Lane>();
            if (bottomLane == null)
            {
                bottomLane = bottomLaneObj.AddComponent<Lane>();
                bottomLane.laneName = "Bottom Lane";
            }
            bottomLaneProperty.objectReferenceValue = bottomLane;
        }
        
        Debug.Log("Found existing lanes in the scene");
    }
    
    private void AssignTeamsToLanes()
    {
        // Assign teams to lanes
        if (topLaneProperty.objectReferenceValue != null)
        {
            Lane topLane = topLaneProperty.objectReferenceValue as Lane;
            topLane.laneTeam = Team.Neutral;
            topLane.laneName = "Top Lane";
        }
        
        if (midLaneProperty.objectReferenceValue != null)
        {
            Lane midLane = midLaneProperty.objectReferenceValue as Lane;
            midLane.laneTeam = Team.Neutral;
            midLane.laneName = "Mid Lane";
        }
        
        if (bottomLaneProperty.objectReferenceValue != null)
        {
            Lane bottomLane = bottomLaneProperty.objectReferenceValue as Lane;
            bottomLane.laneTeam = Team.Neutral;
            bottomLane.laneName = "Bottom Lane";
        }
        
        Debug.Log("Assigned teams to lanes");
    }
    
    private void GenerateWaypoints()
    {
        // Generate minimal waypoints for each lane if they have no waypoints
        if (topLaneProperty.objectReferenceValue != null)
        {
            Lane topLane = topLaneProperty.objectReferenceValue as Lane;
            if (topLane.waypoints.Count == 0)
            {
                CreateDefaultWaypoints(topLane, "Top");
            }
        }
        
        if (midLaneProperty.objectReferenceValue != null)
        {
            Lane midLane = midLaneProperty.objectReferenceValue as Lane;
            if (midLane.waypoints.Count == 0)
            {
                CreateDefaultWaypoints(midLane, "Mid");
            }
        }
        
        if (bottomLaneProperty.objectReferenceValue != null)
        {
            Lane bottomLane = bottomLaneProperty.objectReferenceValue as Lane;
            if (bottomLane.waypoints.Count == 0)
            {
                CreateDefaultWaypoints(bottomLane, "Bottom");
            }
        }
        
        Debug.Log("Generated minimal waypoints for lanes. Use the Lane Editor to add more waypoints manually.");
    }
    
    private void LogLaneStatus()
    {
        Debug.Log("=== Lane Manager Status ===");
        
        if (topLaneProperty.objectReferenceValue != null)
        {
            Lane topLane = topLaneProperty.objectReferenceValue as Lane;
            Debug.Log($"Top Lane: Found, Waypoints: {topLane.waypoints.Count}, Team: {topLane.laneTeam}");
        }
        else
        {
            Debug.Log("Top Lane: Not Found");
        }
        
        if (midLaneProperty.objectReferenceValue != null)
        {
            Lane midLane = midLaneProperty.objectReferenceValue as Lane;
            Debug.Log($"Mid Lane: Found, Waypoints: {midLane.waypoints.Count}, Team: {midLane.laneTeam}");
        }
        else
        {
            Debug.Log("Mid Lane: Not Found");
        }
        
        if (bottomLaneProperty.objectReferenceValue != null)
        {
            Lane bottomLane = bottomLaneProperty.objectReferenceValue as Lane;
            Debug.Log($"Bottom Lane: Found, Waypoints: {bottomLane.waypoints.Count}, Team: {bottomLane.laneTeam}");
        }
        else
        {
            Debug.Log("Bottom Lane: Not Found");
        }
        
        Debug.Log("=========================");
    }
} 