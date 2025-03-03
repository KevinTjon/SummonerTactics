using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Ensures that required tags exist in the project.
/// This script runs in edit mode to set up tags before play mode.
/// </summary>
public class TagManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureTagsExist()
    {
        // This method runs before any scene is loaded
        Debug.Log("Checking for required tags...");
        
        // In runtime, we can't add tags, so we'll just log a warning if they don't exist
        if (!TagExists("TestEntity"))
        {
            Debug.LogWarning("The 'TestEntity' tag does not exist. Please add it in the Unity Editor under Edit > Project Settings > Tags and Layers.");
        }
    }
    
    /// <summary>
    /// Checks if a tag exists in the project
    /// </summary>
    private static bool TagExists(string tagName)
    {
        try
        {
            // Try to find a temporary GameObject with the tag
            GameObject tempObject = new GameObject();
            tempObject.tag = tagName;
            DestroyImmediate(tempObject);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
#if UNITY_EDITOR
    // This will run in the editor
    [InitializeOnLoadMethod]
    private static void EditorEnsureTagsExist()
    {
        Debug.Log("Checking for required tags in editor...");
        
        // Add the TestEntity tag if it doesn't exist
        AddTagIfNotExists("TestEntity");
    }
    
    /// <summary>
    /// Adds a tag to the project if it doesn't already exist
    /// </summary>
    private static void AddTagIfNotExists(string tagName)
    {
        // Get the TagManager asset
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        // Check if the tag already exists
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagName))
            {
                found = true;
                break;
            }
        }
        
        // If the tag doesn't exist, add it
        if (!found)
        {
            tagsProp.arraySize++;
            SerializedProperty tag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            tag.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"Added '{tagName}' tag to the project.");
        }
    }
#endif
} 