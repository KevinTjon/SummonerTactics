using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public Camera targetCamera;
    
    [Header("Map Reference")]
    [Tooltip("Optional: Reference to the map GameObject. If not set, the camera will use manual settings.")]
    public GameObject mapObject; // Reference to the map GameObject
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 50f;
    public float defaultHeight = 40f; // Default height above the map
    
    [Header("Camera Settings")]
    public float nearClipPlane = 0.1f;
    public float farClipPlane = 1000f;
    
    [Header("Map Settings")]
    [Tooltip("These settings will be used if no map object is assigned or if automatic detection fails")]
    public Vector3 defaultMapCenter = Vector3.zero;
    public float defaultMapWidth = 100f;
    public float defaultMapHeight = 100f;
    
    private float initialSize;
    private Vector3 mapCenter;
    private float mapWidth;
    private float mapHeight;
    private bool mapDimensionsCalculated = false;
    
    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("No camera found! Please assign a camera to the CameraController or ensure there is a camera tagged as MainCamera in the scene.");
                return;
            }
        }
        
        // Set camera clipping planes
        targetCamera.nearClipPlane = nearClipPlane;
        targetCamera.farClipPlane = farClipPlane;
        
        // Calculate map dimensions and center
        CalculateMapDimensions();
        
        // Position camera above map center
        PositionCameraAboveMap();
        
        // Save initial orthographic size if using orthographic camera
        if (targetCamera.orthographic)
        {
            initialSize = targetCamera.orthographicSize;
        }
    }
    
    private void CalculateMapDimensions()
    {
        // Start with default values
        mapCenter = defaultMapCenter;
        mapWidth = defaultMapWidth;
        mapHeight = defaultMapHeight;
        
        // If no map object, use default values and return
        if (mapObject == null)
        {
            Debug.Log("No map object assigned. Using default map settings.");
            mapDimensionsCalculated = true;
            return;
        }
        
        // For a complex map with nested children, we need to calculate the bounds of all children
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
        
        // Get all renderers in children
        Renderer[] renderers = mapObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            // Initialize bounds with the first renderer
            bounds = renderers[0].bounds;
            boundsInitialized = true;
            
            // Expand bounds to include all other renderers
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            
            Debug.Log($"Map bounds calculated from {renderers.Length} renderers.");
        }
        
        // If no renderers found, try colliders
        if (!boundsInitialized)
        {
            Collider[] colliders = mapObject.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                bounds = colliders[0].bounds;
                boundsInitialized = true;
                
                for (int i = 1; i < colliders.Length; i++)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
                
                Debug.Log($"Map bounds calculated from {colliders.Length} colliders.");
            }
        }
        
        // If still no bounds, try to find a MapGenerator component
        if (!boundsInitialized)
        {
            MonoBehaviour[] components = mapObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component == null) continue;
                
                System.Type type = component.GetType();
                if (type.Name.Contains("MapGenerator") || type.Name.Contains("Map"))
                {
                    // Try to find map size properties
                    System.Reflection.FieldInfo widthField = type.GetField("width") ?? type.GetField("mapWidth") ?? type.GetField("gridWidth");
                    System.Reflection.FieldInfo heightField = type.GetField("height") ?? type.GetField("mapHeight") ?? type.GetField("gridHeight");
                    
                    if (widthField != null && heightField != null)
                    {
                        try
                        {
                            float width = (float)System.Convert.ToSingle(widthField.GetValue(component));
                            float height = (float)System.Convert.ToSingle(heightField.GetValue(component));
                            
                            mapCenter = mapObject.transform.position;
                            mapWidth = width;
                            mapHeight = height;
                            boundsInitialized = true;
                            
                            Debug.Log($"Map dimensions from {type.Name}: Center={mapCenter}, Width={mapWidth}, Height={mapHeight}");
                            break;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Error reading map dimensions from {type.Name}: {e.Message}");
                        }
                    }
                }
            }
        }
        
        // If we still don't have bounds, use transform hierarchy to estimate
        if (!boundsInitialized)
        {
            // Get all child transforms
            List<Transform> allChildren = new List<Transform>();
            GetAllChildren(mapObject.transform, allChildren);
            
            if (allChildren.Count > 0)
            {
                Vector3 min = allChildren[0].position;
                Vector3 max = allChildren[0].position;
                
                // Find min and max positions
                foreach (Transform child in allChildren)
                {
                    min = Vector3.Min(min, child.position);
                    max = Vector3.Max(max, child.position);
                }
                
                // Create bounds from min/max
                bounds = new Bounds();
                bounds.SetMinMax(min, max);
                boundsInitialized = true;
                
                Debug.Log($"Map bounds estimated from {allChildren.Count} child transforms.");
            }
        }
        
        // If we have bounds, use them
        if (boundsInitialized)
        {
            mapCenter = bounds.center;
            mapWidth = bounds.size.x;
            mapHeight = bounds.size.z;
            
            // Ensure minimum size
            mapWidth = Mathf.Max(mapWidth, 10f);
            mapHeight = Mathf.Max(mapHeight, 10f);
            
            Debug.Log($"Final map dimensions: Center={mapCenter}, Width={mapWidth}, Height={mapHeight}");
        }
        else
        {
            // Fallback to default values
            Debug.Log($"Could not determine map bounds. Using default values: Center={mapCenter}, Width={mapWidth}, Height={mapHeight}");
        }
        
        mapDimensionsCalculated = true;
    }
    
    private void GetAllChildren(Transform parent, List<Transform> result)
    {
        foreach (Transform child in parent)
        {
            result.Add(child);
            GetAllChildren(child, result);
        }
    }
    
    private void Update()
    {
        if (targetCamera == null) return;
        
        // Only handle zoom
        HandleZoom();
        
        // Handle reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera();
        }
    }
    
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;
        
        if (targetCamera.orthographic)
        {
            // Orthographic zoom changes the size
            float newSize = Mathf.Clamp(targetCamera.orthographicSize - scroll * zoomSpeed * 5f, minZoom, maxZoom);
            targetCamera.orthographicSize = newSize;
        }
        else
        {
            // Perspective zoom moves the camera position
            Vector3 direction = targetCamera.transform.forward;
            float zoomAmount = scroll * zoomSpeed;
            
            // Calculate new position
            Vector3 newPosition = targetCamera.transform.position + direction * zoomAmount;
            
            // Ensure camera stays above ground and points at map center
            newPosition.y = Mathf.Max(newPosition.y, 5f);
            targetCamera.transform.position = newPosition;
            
            // Make sure camera is looking at map center
            targetCamera.transform.LookAt(mapCenter);
        }
    }
    
    private void PositionCameraAboveMap()
    {
        // Make sure we have map dimensions
        if (!mapDimensionsCalculated)
        {
            CalculateMapDimensions();
        }
        
        // Position the camera above the map center
        Vector3 position = mapCenter;
        position.y = defaultHeight; // Height above the map
        targetCamera.transform.position = position;
        
        // Rotate camera to look down at the map
        targetCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Set initial orthographic size based on map dimensions if using orthographic camera
        if (targetCamera.orthographic)
        {
            // Calculate the orthographic size needed to view the entire map
            // We add a small margin (1.1f) to ensure the entire map is visible
            float aspectRatio = targetCamera.aspect;
            float orthoSize = Mathf.Max(mapHeight / 2, mapWidth / (2 * aspectRatio)) * 1.1f;
            
            // Clamp to min/max zoom
            orthoSize = Mathf.Clamp(orthoSize, minZoom, maxZoom);
            targetCamera.orthographicSize = orthoSize;
            initialSize = orthoSize;
        }
    }
    
    private void ResetCamera()
    {
        PositionCameraAboveMap();
    }
    
    // Call this method if the map changes during runtime
    public void RefreshMapReference()
    {
        mapDimensionsCalculated = false;
        CalculateMapDimensions();
        PositionCameraAboveMap();
    }
    
    // Call this to manually set map dimensions
    public void SetMapDimensions(Vector3 center, float width, float height)
    {
        mapCenter = center;
        mapWidth = width;
        mapHeight = height;
        mapDimensionsCalculated = true;
        PositionCameraAboveMap();
    }
} 