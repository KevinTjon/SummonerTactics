using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the Champion component that provides tools for managing champion properties and lane assignments.
/// </summary>
[CustomEditor(typeof(Champion))]
public class ChampionEditor : Editor
{
    private Champion champion;
    private SerializedProperty championNameProperty;
    private SerializedProperty teamProperty;
    private SerializedProperty assignedLaneTypeProperty;
    private SerializedProperty teamColorProperty;
    private SerializedProperty spriteRendererProperty;
    
    private bool showLaneAssignment = true;
    private bool showAppearance = true;
    
    private void OnEnable()
    {
        champion = (Champion)target;
        championNameProperty = serializedObject.FindProperty("championName");
        teamProperty = serializedObject.FindProperty("team");
        assignedLaneTypeProperty = serializedObject.FindProperty("assignedLaneType");
        teamColorProperty = serializedObject.FindProperty("teamColor");
        spriteRendererProperty = serializedObject.FindProperty("spriteRenderer");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Champion Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(championNameProperty);
        
        EditorGUILayout.Space();
        
        // Team and Lane Assignment Section
        showLaneAssignment = EditorGUILayout.Foldout(showLaneAssignment, "Team & Lane Assignment", true);
        if (showLaneAssignment)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(teamProperty);
            EditorGUILayout.PropertyField(assignedLaneTypeProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Assign to Lane"))
            {
                AssignToLane();
            }
            
            if (GUILayout.Button("Find Lane Manager"))
            {
                FindLaneManager();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Appearance Section
        showAppearance = EditorGUILayout.Foldout(showAppearance, "Appearance", true);
        if (showAppearance)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(teamColorProperty);
            EditorGUILayout.PropertyField(spriteRendererProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update Team Color"))
            {
                UpdateTeamColor();
            }
            
            if (GUILayout.Button("Find Sprite Renderer"))
            {
                FindSpriteRenderer();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void AssignToLane()
    {
        Undo.RecordObject(champion, "Assign Champion to Lane");
        
        // Find LaneManager in the scene
        LaneManager laneManager = FindObjectOfType<LaneManager>();
        if (laneManager == null)
        {
            Debug.LogError("No LaneManager found in the scene!");
            return;
        }
        
        // Get the lane based on the assigned lane type
        Lane lane = laneManager.GetLane(champion.assignedLaneType);
        if (lane == null)
        {
            Debug.LogError($"No lane found of type {champion.assignedLaneType}!");
            return;
        }
        
        // Get the ChampionMovement component
        ChampionMovement movement = champion.GetComponent<ChampionMovement>();
        if (movement == null)
        {
            // Add ChampionMovement component if it doesn't exist
            movement = Undo.AddComponent<ChampionMovement>(champion.gameObject);
        }
        
        // Assign the lane to the movement component
        movement.assignedLane = lane;
        
        // Set the movement direction based on the team
        movement.moveForward = champion.team == Team.Blue;
        
        Debug.Log($"Assigned {champion.championName} to {lane.laneName} with direction: {(movement.moveForward ? "Forward" : "Backward")}");
        
        // Position the champion at the start of the lane
        PositionAtLaneStart();
    }
    
    private void PositionAtLaneStart()
    {
        // Find LaneManager in the scene
        LaneManager laneManager = FindObjectOfType<LaneManager>();
        if (laneManager == null)
        {
            Debug.LogError("No LaneManager found in the scene!");
            return;
        }
        
        // Get the lane based on the assigned lane type
        Lane lane = laneManager.GetLane(champion.assignedLaneType);
        if (lane == null)
        {
            Debug.LogError($"No lane found of type {champion.assignedLaneType}!");
            return;
        }
        
        // Get the start waypoint index for the team
        int startIndex = lane.GetStartWaypointIndex(champion.team);
        
        // Position the champion at the start waypoint
        if (lane.waypoints.Count > 0 && startIndex >= 0 && startIndex < lane.waypoints.Count)
        {
            Vector3 startPosition = lane.waypoints[startIndex].position;
            Undo.RecordObject(champion.transform, "Position Champion at Lane Start");
            champion.transform.position = startPosition;
            Debug.Log($"Positioned {champion.championName} at the start of {lane.laneName}");
        }
        else
        {
            Debug.LogError($"Lane {lane.laneName} has no waypoints or invalid start index!");
        }
    }
    
    private void FindLaneManager()
    {
        LaneManager laneManager = FindObjectOfType<LaneManager>();
        if (laneManager == null)
        {
            Debug.LogError("No LaneManager found in the scene!");
            return;
        }
        
        Debug.Log($"Found LaneManager: {laneManager.name}");
        
        // Log lane status
        if (laneManager.topLane != null)
        {
            Debug.Log($"Top Lane: {laneManager.topLane.laneName}, Waypoints: {laneManager.topLane.waypoints.Count}");
        }
        
        if (laneManager.midLane != null)
        {
            Debug.Log($"Mid Lane: {laneManager.midLane.laneName}, Waypoints: {laneManager.midLane.waypoints.Count}");
        }
        
        if (laneManager.bottomLane != null)
        {
            Debug.Log($"Bottom Lane: {laneManager.bottomLane.laneName}, Waypoints: {laneManager.bottomLane.waypoints.Count}");
        }
    }
    
    private void UpdateTeamColor()
    {
        Undo.RecordObject(champion, "Update Champion Team Color");
        
        // Find SpriteRenderer if not assigned
        if (champion.spriteRenderer == null)
        {
            FindSpriteRenderer();
        }
        
        // Update the team color
        if (champion.spriteRenderer != null)
        {
            Color teamColor = champion.teamColor;
            
            // Set default team colors if not specified
            if (teamColor == Color.clear)
            {
                switch (champion.team)
                {
                    case Team.Blue:
                        teamColor = new Color(0.2f, 0.4f, 1.0f); // Blue team color
                        break;
                    case Team.Red:
                        teamColor = new Color(1.0f, 0.2f, 0.2f); // Red team color
                        break;
                    case Team.Neutral:
                        teamColor = new Color(0.7f, 0.7f, 0.7f); // Neutral color
                        break;
                }
                
                champion.teamColor = teamColor;
            }
            
            // Apply the color to the sprite renderer
            champion.spriteRenderer.color = teamColor;
            Debug.Log($"Updated {champion.championName}'s color to match Team {champion.team}");
        }
        else
        {
            Debug.LogError("No SpriteRenderer found on the champion!");
        }
    }
    
    private void FindSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = champion.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = champion.GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            Undo.RecordObject(champion, "Find Sprite Renderer");
            champion.spriteRenderer = spriteRenderer;
            Debug.Log($"Found SpriteRenderer on {champion.name}");
        }
        else
        {
            Debug.LogError("No SpriteRenderer found on the champion or its children!");
        }
    }
} 