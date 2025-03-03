using UnityEngine;
using System.Collections;

/// <summary>
/// Handles visual attack effects between champions
/// </summary>
public class AttackEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("Duration of the attack effect in seconds")]
    public float duration = 0.5f;
    
    [Tooltip("Speed of the effect movement")]
    public float speed = 5f;
    
    [Tooltip("Target position for the effect")]
    public Vector3 targetPosition;
    
    [Tooltip("Whether to destroy the effect after reaching the target")]
    public bool destroyOnReachTarget = true;
    
    [Tooltip("Whether to scale down the effect as it approaches the target")]
    public bool scaleDownOnApproach = true;
    
    [Header("Visual Settings")]
    [Tooltip("Color of the effect")]
    public Color effectColor = Color.yellow;
    
    [Tooltip("Whether to pulse the effect")]
    public bool pulseEffect = true;
    
    [Tooltip("Pulse frequency")]
    public float pulseFrequency = 10f;
    
    [Tooltip("Pulse amplitude")]
    public float pulseAmplitude = 0.2f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private float startTime;
    private bool isMoving = false;
    
    private void Awake()
    {
        // Get the sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set the color
        spriteRenderer.color = effectColor;
        
        // Store initial scale
        initialScale = transform.localScale;
    }
    
    private void Start()
    {
        // Start the effect
        startTime = Time.time;
        isMoving = true;
        
        // Destroy after duration if not moving to target
        if (targetPosition == Vector3.zero)
        {
            Destroy(gameObject, duration);
        }
    }
    
    private void Update()
    {
        // Handle pulsing effect
        if (pulseEffect)
        {
            float pulse = 1f + pulseAmplitude * Mathf.Sin(pulseFrequency * (Time.time - startTime));
            transform.localScale = initialScale * pulse;
        }
        
        // Move towards target if set
        if (isMoving && targetPosition != Vector3.zero)
        {
            // Calculate distance to target
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            // Scale down if enabled
            if (scaleDownOnApproach)
            {
                float t = 1f - Mathf.Clamp01(distanceToTarget / Vector3.Distance(transform.position, targetPosition));
                transform.localScale = Vector3.Lerp(initialScale, initialScale * 0.1f, t);
            }
            
            // Check if we've reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                
                // Destroy if set to destroy on reach
                if (destroyOnReachTarget)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        // Destroy after duration if not already destroyed
        if (Time.time - startTime > duration && !isMoving)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Sets the target position for the effect
    /// </summary>
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        isMoving = true;
    }
    
    /// <summary>
    /// Sets the color of the effect
    /// </summary>
    public void SetColor(Color color)
    {
        effectColor = color;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
} 