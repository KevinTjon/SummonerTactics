using UnityEngine;

/// <summary>
/// Manages the test scene and provides a central point for testing functionality.
/// Add this to an empty GameObject in your test scene.
/// </summary>
public class TestSceneManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the TestSceneSetup component")]
    public TestSceneSetup sceneSetup;
    
    [Header("Debug Settings")]
    [Tooltip("Whether to show debug information")]
    public bool showDebugInfo = true;
    
    [Tooltip("Color for debug text")]
    public Color debugTextColor = Color.white;
    
    private void Awake()
    {
        // Find the TestSceneSetup if not assigned
        if (sceneSetup == null)
        {
            sceneSetup = FindObjectOfType<TestSceneSetup>();
            if (sceneSetup == null)
            {
                Debug.LogWarning("No TestSceneSetup found in the scene. Creating one...");
                GameObject setupObj = new GameObject("TestSceneSetup");
                sceneSetup = setupObj.AddComponent<TestSceneSetup>();
            }
        }
    }
    
    private void OnGUI()
    {
        if (showDebugInfo)
        {
            // Set up GUI style
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = debugTextColor;
            style.fontSize = 14;
            style.padding = new RectOffset(10, 10, 10, 10);
            
            // Display debug info
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical(style);
            
            GUILayout.Label("Test Scene Debug Info", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
            GUILayout.Space(5);
            
            // Display camera info
            if (Camera.main != null)
            {
                GUILayout.Label($"Camera Position: {Camera.main.transform.position}");
                GUILayout.Label($"Orthographic Size: {Camera.main.orthographicSize}");
            }
            
            // Display entity count
            int entityCount = GameObject.FindGameObjectsWithTag("TestEntity").Length;
            GUILayout.Label($"Test Entities: {entityCount}");
            
            // Add buttons for testing
            GUILayout.Space(10);
            if (GUILayout.Button("Spawn Champions"))
            {
                if (sceneSetup != null)
                {
                    sceneSetup.SpawnAllChampions();
                }
            }
            
            if (GUILayout.Button("Clear Test Entities"))
            {
                if (sceneSetup != null)
                {
                    sceneSetup.ClearTestEntities();
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
} 