using UnityEngine;

/// <summary>
/// A simple test entity for testing spawning and movement in the test scene.
/// </summary>
public class TestEntity : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Color of the entity")]
    public Color entityColor = Color.white;
    
    [Header("Movement Settings")]
    [Tooltip("Whether the entity should move randomly")]
    public bool moveRandomly = false;
    
    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;
    
    [Tooltip("Maximum distance to move from spawn point")]
    public float maxMoveDistance = 5f;
    
    private Vector3 spawnPosition;
    private Vector3 targetPosition;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetDefaultSprite();
        }
        
        // Set the entity color
        spriteRenderer.color = entityColor;
        
        // Try to tag the entity, but handle the case where the tag doesn't exist
        try
        {
            gameObject.tag = "TestEntity";
        }
        catch (UnityException)
        {
            Debug.LogWarning("The 'TestEntity' tag does not exist. Please add it in the Unity Editor under Edit > Project Settings > Tags and Layers.");
        }
    }
    
    private void Start()
    {
        // Store the initial spawn position
        spawnPosition = transform.position;
        
        // Set initial target position
        SetNewTargetPosition();
    }
    
    private void Update()
    {
        if (moveRandomly)
        {
            // Move towards target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // If reached target, set a new target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                SetNewTargetPosition();
            }
        }
    }
    
    /// <summary>
    /// Sets a new random target position within the max move distance
    /// </summary>
    private void SetNewTargetPosition()
    {
        // Get a random direction
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        
        // Calculate random distance (between 1 and maxMoveDistance)
        float randomDistance = Random.Range(1f, maxMoveDistance);
        
        // Set new target position
        targetPosition = spawnPosition + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;
    }
    
    /// <summary>
    /// Gets a default sprite if none is assigned
    /// </summary>
    private Sprite GetDefaultSprite()
    {
        // Try to use the built-in square sprite
        return Resources.Load<Sprite>("Sprites/Square") ?? CreateDefaultSprite();
    }
    
    /// <summary>
    /// Creates a default sprite if none can be loaded
    /// </summary>
    private Sprite CreateDefaultSprite()
    {
        // Create a simple 32x32 texture
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        // Create a sprite from the texture
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
} 