using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for the Lane component that allows adding, removing, and reordering waypoints.
/// </summary>
[CustomEditor(typeof(Lane))]
public class LaneEditor : Editor
{
    private Lane lane;
    private SerializedProperty waypointsProperty;
    private SerializedProperty laneNameProperty;
    private SerializedProperty laneTeamProperty;
    private SerializedProperty debugColorProperty;
    private SerializedProperty showDebugProperty;
    private SerializedProperty waypointGizmoSizeProperty;
    private SerializedProperty showWaypointIndicesProperty;
    
    private bool showWaypoints = true;
    private bool showWaypointCreation = true;
    private Vector3 newWaypointPosition = Vector3.zero;
    private bool createWaypointAtMousePosition = false;
    private int selectedWaypointIndex = -1;
    
    private void OnEnable()
    {
        lane = (Lane)target;
        waypointsProperty = serializedObject.FindProperty("waypoints");
        laneNameProperty = serializedObject.FindProperty("laneName");
        laneTeamProperty = serializedObject.FindProperty("laneTeam");
        debugColorProperty = serializedObject.FindProperty("debugColor");
        showDebugProperty = serializedObject.FindProperty("showDebug");
        waypointGizmoSizeProperty = serializedObject.FindProperty("waypointGizmoSize");
        showWaypointIndicesProperty = serializedObject.FindProperty("showWaypointIndices");
        
        // Ensure debug visualization is enabled while editing
        if (!lane.showDebug)
        {
            lane.showDebug = true;
            EditorUtility.SetDirty(lane);
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Lane Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(laneNameProperty);
        EditorGUILayout.PropertyField(laneTeamProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(debugColorProperty);
        EditorGUILayout.PropertyField(showDebugProperty);
        EditorGUILayout.PropertyField(waypointGizmoSizeProperty);
        EditorGUILayout.PropertyField(showWaypointIndicesProperty);
        
        EditorGUILayout.Space();
        
        // Waypoints section
        showWaypoints = EditorGUILayout.Foldout(showWaypoints, $"Waypoints ({waypointsProperty.arraySize})", true);
        if (showWaypoints)
        {
            EditorGUI.indentLevel++;
            
            // Display each waypoint
            for (int i = 0; i < waypointsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Waypoint label with selection highlight
                GUI.color = (selectedWaypointIndex == i) ? Color.cyan : Color.white;
                if (GUILayout.Button($"Waypoint {i}", GUILayout.Width(80)))
                {
                    // Select this waypoint
                    selectedWaypointIndex = i;
                    
                    // Focus on the waypoint in the scene view
                    SerializedProperty waypointElementProperty = waypointsProperty.GetArrayElementAtIndex(i);
                    Transform waypoint = waypointElementProperty.objectReferenceValue as Transform;
                    if (waypoint != null)
                    {
                        SceneView.lastActiveSceneView.Frame(new Bounds(waypoint.position, Vector3.one * 5f), false);
                    }
                }
                GUI.color = Color.white;
                
                // Waypoint reference
                SerializedProperty waypointProperty = waypointsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(waypointProperty, GUIContent.none);
                
                // Move up button
                GUI.enabled = i > 0;
                if (GUILayout.Button("↑", GUILayout.Width(25)))
                {
                    waypointsProperty.MoveArrayElement(i, i - 1);
                    if (selectedWaypointIndex == i)
                        selectedWaypointIndex--;
                    else if (selectedWaypointIndex == i - 1)
                        selectedWaypointIndex++;
                }
                
                // Move down button
                GUI.enabled = i < waypointsProperty.arraySize - 1;
                if (GUILayout.Button("↓", GUILayout.Width(25)))
                {
                    waypointsProperty.MoveArrayElement(i, i + 1);
                    if (selectedWaypointIndex == i)
                        selectedWaypointIndex++;
                    else if (selectedWaypointIndex == i + 1)
                        selectedWaypointIndex--;
                }
                
                // Remove button
                GUI.enabled = true;
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    // Get the waypoint transform
                    Transform waypoint = waypointProperty.objectReferenceValue as Transform;
                    
                    // Delete the waypoint GameObject
                    if (waypoint != null)
                    {
                        Undo.DestroyObjectImmediate(waypoint.gameObject);
                    }
                    
                    // Remove from the array
                    waypointsProperty.DeleteArrayElementAtIndex(i);
                    
                    // Update selection
                    if (selectedWaypointIndex == i)
                        selectedWaypointIndex = -1;
                    else if (selectedWaypointIndex > i)
                        selectedWaypointIndex--;
                    
                    i--; // Adjust index after removal
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            // Waypoint creation section
            showWaypointCreation = EditorGUILayout.Foldout(showWaypointCreation, "Waypoint Creation", true);
            if (showWaypointCreation)
            {
                EditorGUI.indentLevel++;
                
                // Add new waypoint section
                EditorGUILayout.BeginHorizontal();
                newWaypointPosition = EditorGUILayout.Vector3Field("Position:", newWaypointPosition);
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    AddWaypoint(newWaypointPosition);
                }
                EditorGUILayout.EndHorizontal();
                
                // Create waypoint at mouse position
                EditorGUILayout.BeginHorizontal();
                createWaypointAtMousePosition = EditorGUILayout.Toggle("Create at Mouse Position", createWaypointAtMousePosition);
                EditorGUILayout.HelpBox("Click in the Scene view to add waypoints", MessageType.Info);
                EditorGUILayout.EndHorizontal();
                
                // Insert waypoint between selected and next
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = selectedWaypointIndex >= 0 && selectedWaypointIndex < waypointsProperty.arraySize - 1;
                if (GUILayout.Button("Insert After Selected"))
                {
                    InsertWaypointAfterSelected();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Utility buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All Waypoints"))
            {
                if (EditorUtility.DisplayDialog("Clear Waypoints", "Are you sure you want to clear all waypoints?", "Yes", "No"))
                {
                    ClearWaypoints();
                    selectedWaypointIndex = -1;
                }
            }
            
            if (GUILayout.Button("Collect From Children"))
            {
                lane.CollectWaypointsFromChildren();
                serializedObject.Update();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Lane templates
            EditorGUILayout.LabelField("Lane Templates", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Top Lane"))
            {
                if (EditorUtility.DisplayDialog("Create Top Lane", "This will clear existing waypoints. Continue?", "Yes", "No"))
                {
                    CreateDefaultLane("Top");
                }
            }
            
            if (GUILayout.Button("Create Mid Lane"))
            {
                if (EditorUtility.DisplayDialog("Create Mid Lane", "This will clear existing waypoints. Continue?", "Yes", "No"))
                {
                    CreateDefaultLane("Mid");
                }
            }
            
            if (GUILayout.Button("Create Bottom Lane"))
            {
                if (EditorUtility.DisplayDialog("Create Bottom Lane", "This will clear existing waypoints. Continue?", "Yes", "No"))
                {
                    CreateDefaultLane("Bottom");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // Instructions
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Waypoint Creation Tips:\n" +
            "1. Enable 'Create at Mouse Position' and click in the Scene view to add waypoints\n" +
            "2. Select a waypoint to highlight it in the Scene view\n" +
            "3. Drag waypoints in the Scene view to position them\n" +
            "4. Use the arrow buttons to reorder waypoints\n" +
            "5. Use 'Insert After Selected' to add a waypoint between two existing ones",
            MessageType.Info
        );
    }
    
    private void AddWaypoint(Vector3 position)
    {
        // Create a new waypoint GameObject
        GameObject waypoint = new GameObject($"Waypoint_{waypointsProperty.arraySize}");
        Undo.RegisterCreatedObjectUndo(waypoint, "Add Waypoint");
        
        waypoint.transform.position = position;
        waypoint.transform.SetParent(lane.transform);
        
        // Add to waypoints list
        int index = waypointsProperty.arraySize;
        waypointsProperty.arraySize++;
        waypointsProperty.GetArrayElementAtIndex(index).objectReferenceValue = waypoint.transform;
        
        // Select the new waypoint
        selectedWaypointIndex = index;
        
        // Reset new waypoint position
        newWaypointPosition = Vector3.zero;
    }
    
    private void InsertWaypointAfterSelected()
    {
        if (selectedWaypointIndex < 0 || selectedWaypointIndex >= waypointsProperty.arraySize - 1)
            return;
            
        // Get the selected and next waypoints
        SerializedProperty selectedWaypointProperty = waypointsProperty.GetArrayElementAtIndex(selectedWaypointIndex);
        SerializedProperty nextWaypointProperty = waypointsProperty.GetArrayElementAtIndex(selectedWaypointIndex + 1);
        
        Transform selectedWaypoint = selectedWaypointProperty.objectReferenceValue as Transform;
        Transform nextWaypoint = nextWaypointProperty.objectReferenceValue as Transform;
        
        if (selectedWaypoint == null || nextWaypoint == null)
            return;
            
        // Calculate the midpoint position
        Vector3 midpoint = (selectedWaypoint.position + nextWaypoint.position) * 0.5f;
        
        // Create a new waypoint GameObject
        GameObject waypoint = new GameObject($"Waypoint_{waypointsProperty.arraySize}");
        Undo.RegisterCreatedObjectUndo(waypoint, "Insert Waypoint");
        
        waypoint.transform.position = midpoint;
        waypoint.transform.SetParent(lane.transform);
        
        // Insert into waypoints list
        waypointsProperty.InsertArrayElementAtIndex(selectedWaypointIndex + 1);
        waypointsProperty.GetArrayElementAtIndex(selectedWaypointIndex + 1).objectReferenceValue = waypoint.transform;
        
        // Select the new waypoint
        selectedWaypointIndex = selectedWaypointIndex + 1;
    }
    
    private void ClearWaypoints()
    {
        // Remove all waypoint GameObjects
        for (int i = waypointsProperty.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty waypointProperty = waypointsProperty.GetArrayElementAtIndex(i);
            Transform waypoint = waypointProperty.objectReferenceValue as Transform;
            
            if (waypoint != null)
            {
                Undo.DestroyObjectImmediate(waypoint.gameObject);
            }
        }
        
        // Clear the waypoints list
        waypointsProperty.ClearArray();
    }
    
    private void CreateDefaultLane(string laneType)
    {
        // Clear existing waypoints
        ClearWaypoints();
        
        // Set lane name
        laneNameProperty.stringValue = $"{laneType} Lane";
        
        // Create waypoints based on lane type
        Vector3 blueBase = Vector3.zero;
        Vector3 redBase = Vector3.zero;
        
        switch (laneType)
        {
            case "Top":
                blueBase = new Vector3(-10, 0, 0);
                redBase = new Vector3(10, 0, 0);
                break;
            case "Mid":
                blueBase = new Vector3(-10, -10, 0);
                redBase = new Vector3(10, 10, 0);
                break;
            case "Bottom":
                blueBase = new Vector3(0, -10, 0);
                redBase = new Vector3(0, 10, 0);
                break;
        }
        
        // Add Blue base waypoint
        AddWaypoint(blueBase);
        
        // Add Red base waypoint
        AddWaypoint(redBase);
        
        Debug.Log($"Created basic {laneType} Lane with start and end waypoints. Add more waypoints manually to define the path.");
    }
    
    private void OnSceneGUI()
    {
        if (!lane.showDebug)
            return;
            
        // Handle mouse clicks for waypoint creation
        if (createWaypointAtMousePosition && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            // Get the mouse position in world space
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Use the hit point
                AddWaypoint(hit.point);
            }
            else
            {
                // Use a point on a plane at z=0
                Plane plane = new Plane(Vector3.forward, Vector3.zero);
                float distance;
                if (plane.Raycast(ray, out distance))
                {
                    Vector3 point = ray.GetPoint(distance);
                    AddWaypoint(point);
                }
            }
            
            Event.current.Use();
            serializedObject.Update();
        }
        
        // Allow dragging waypoints in the scene view
        for (int i = 0; i < lane.waypoints.Count; i++)
        {
            if (lane.waypoints[i] != null)
            {
                // Highlight the selected waypoint
                if (i == selectedWaypointIndex)
                {
                    Handles.color = Color.cyan;
                    Handles.SphereHandleCap(0, lane.waypoints[i].position, Quaternion.identity, lane.waypointGizmoSize * 1.5f, EventType.Repaint);
                    Handles.color = Color.white;
                }
                
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(lane.waypoints[i].position, Quaternion.identity);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(lane.waypoints[i], "Move Waypoint");
                    lane.waypoints[i].position = newPosition;
                }
                
                // Draw waypoint label
                Handles.Label(lane.waypoints[i].position + Vector3.up * 1.5f, $"Waypoint {i}");
                
                // Draw a button to select this waypoint
                Handles.color = (i == selectedWaypointIndex) ? Color.cyan : Color.white;
                if (Handles.Button(lane.waypoints[i].position, Quaternion.identity, lane.waypointGizmoSize * 0.8f, lane.waypointGizmoSize, Handles.SphereHandleCap))
                {
                    selectedWaypointIndex = i;
                    Repaint();
                }
                Handles.color = Color.white;
            }
        }
    }
} 