using UnityEngine;

/// <summary>
/// Helper script to ensure champions have colliders for detection and are on the correct layer.
/// </summary>
public class ChampionColliderHelper : MonoBehaviour
{
    [Tooltip("Radius of the champion collider")]
    public float colliderRadius = 1.5f;
    
    private void Awake()
    {
        // Get the Champion component
        Champion champion = GetComponent<Champion>();
        
        if (champion != null)
        {
            // Check if there's already a collider
            Collider2D existingCollider = GetComponent<Collider2D>();
            
            if (existingCollider == null)
            {
                // Add a circle collider
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.radius = colliderRadius;
                circleCollider.isTrigger = true; // Use trigger to avoid physics interactions
                
                Debug.Log($"Added CircleCollider2D to champion {champion.championName}");
            }
            
            // Set the layer to "Champion"
            int championLayer = LayerMask.NameToLayer("Champion");
            if (championLayer != -1)
            {
                gameObject.layer = championLayer;
                Debug.Log($"Set champion {champion.championName} to layer 'Champion'");
            }
            else
            {
                Debug.LogWarning("Champion layer not found! Please add a 'Champion' layer in the Unity Editor.");
            }
        }
    }
} 