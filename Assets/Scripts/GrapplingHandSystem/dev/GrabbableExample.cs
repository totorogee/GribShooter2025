using UnityEngine;

/// <summary>
/// Example implementation of IGrabbable interface.
/// Attach this to any GameObject you want to be grabbable by the grappling hand.
/// </summary>
public class GrabbableExample : MonoBehaviour, IGrabbable
{
    [Header("Grabbable Settings")]
    [SerializeField] private bool isGrabbable = true;
    [SerializeField] private bool disablePhysicsWhenGrabbed = true;
    [SerializeField] private bool destroyWhenReleased = false;
    
    [Header("Visual Feedback (Optional)")]
    [SerializeField] private Color grabbedColor = Color.yellow;
    
    private bool isCurrentlyGrabbed = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private RigidbodyType2D originalBodyType;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    /// <summary>
    /// Check if this object can be grabbed
    /// </summary>
    public bool CanBeGrabbed()
    {
        return isGrabbable && !isCurrentlyGrabbed;
    }
    
    /// <summary>
    /// Called when grabbed by the grappling hand
    /// </summary>
    public void OnGrabbed(Transform player)
    {
        isCurrentlyGrabbed = true;
        
        // Disable physics if configured
        if (disablePhysicsWhenGrabbed && rb != null)
        {
            originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        
        // Change color for visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = grabbedColor;
        }
        
        Debug.Log($"{gameObject.name} was grabbed!");
    }
    
    /// <summary>
    /// Called when released by the grappling hand
    /// </summary>
    public void OnReleased()
    {
        isCurrentlyGrabbed = false;
        
        // Re-enable physics if it was disabled
        if (disablePhysicsWhenGrabbed && rb != null)
        {
            rb.bodyType = originalBodyType;
        }
        
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log($"{gameObject.name} was released!");
        
        // Destroy if configured
        if (destroyWhenReleased)
        {
            Destroy(gameObject, 0.1f); // Small delay to allow release logic to complete
        }
    }
    
    /// <summary>
    /// Public method to toggle if this object can be grabbed
    /// </summary>
    public void SetGrabbable(bool canGrab)
    {
        isGrabbable = canGrab;
    }
    
    /// <summary>
    /// Check if currently being grabbed
    /// </summary>
    public bool IsGrabbed()
    {
        return isCurrentlyGrabbed;
    }
}

