using UnityEngine;

/// <summary>
/// Simple script to make any GameObject grabbable by the grappling hand.
/// Just attach this component to make an object grabbable.
/// Automatically adds and configures Rigidbody2D and Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GrabbableObject : MonoBehaviour, IGrabbable
{
    [Header("Grabbable Settings")]
    [SerializeField] private bool canBeGrabbed = true;
    [SerializeField] private bool destroyOnReturn = false; // Destroy when hand returns to player
    [SerializeField] private bool disablePhysicsWhenGrabbed = true;
    
    [Header("Physics Settings")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private RigidbodyType2D bodyTypeByDefault = RigidbodyType2D.Dynamic;
    
    [Header("Collider Settings")]
    [SerializeField] private ColliderType colliderType = ColliderType.Circle;
    [SerializeField] private float circleRadius = 0.5f;
    [SerializeField] private Vector2 boxSize = new Vector2(1f, 1f);
    
    [Header("Visual Feedback (Optional)")]
    [SerializeField] private bool changeColorWhenGrabbed = true;
    [SerializeField] private Color grabbedColor = Color.yellow;
    
    private enum ColliderType
    {
        Circle,
        Box
    }
    
    private bool isCurrentlyGrabbed = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private RigidbodyType2D originalBodyType;
    
    void Reset()
    {
        // Called when component is first added or reset
        SetupComponents();
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Ensure components are set up
        SetupComponents();
    }
    
    /// <summary>
    /// Automatically sets up required components with proper settings
    /// Only adds/configures components if they don't already exist
    /// </summary>
    private void SetupComponents()
    {
        // Setup Rigidbody2D - only configure if it wasn't manually set up
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Only apply settings if this is a newly added component
            // Check if it has default values (indicating it wasn't manually configured)
            bool isNewRigidbody = (rb.gravityScale == 1f && rb.bodyType == RigidbodyType2D.Dynamic);
            
            if (isNewRigidbody)
            {
                rb.gravityScale = useGravity ? gravityScale : 0f;
                rb.bodyType = bodyTypeByDefault;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
        
        // Setup Collider2D - only add if none exists
        Collider2D existingCollider = GetComponent<Collider2D>();
        
        if (existingCollider == null)
        {
            // No collider exists, add one based on settings
            if (colliderType == ColliderType.Circle)
            {
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.radius = circleRadius;
            }
            else
            {
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = boxSize;
            }
        }
        // If collider already exists, don't modify it - respect user's setup
    }
    
    /// <summary>
    /// Update collider settings in editor
    /// </summary>
    void OnValidate()
    {
        // Update settings when values change in inspector (editor only)
        if (Application.isPlaying)
            return;
            
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                SetupComponents();
            }
        };
        #endif
    }
    
    /// <summary>
    /// Check if this object can be grabbed
    /// </summary>
    public bool CanBeGrabbed()
    {
        return canBeGrabbed && !isCurrentlyGrabbed;
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
            rb.angularVelocity = 0f;
        }
        
        // Change color for visual feedback
        if (changeColorWhenGrabbed && spriteRenderer != null)
        {
            spriteRenderer.color = grabbedColor;
        }
        
        Debug.Log($"{gameObject.name} was grabbed!");
    }
    
    /// <summary>
    /// Called when released by the grappling hand (when hand returns)
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
        if (changeColorWhenGrabbed && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log($"{gameObject.name} was released!");
        
        // Destroy if configured
        if (destroyOnReturn)
        {
            Debug.Log($"{gameObject.name} will be destroyed");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Public method to toggle if this object can be grabbed
    /// </summary>
    public void SetGrabbable(bool canGrab)
    {
        canBeGrabbed = canGrab;
    }
    
    /// <summary>
    /// Set whether to destroy this object when released
    /// </summary>
    public void SetDestroyOnReturn(bool shouldDestroy)
    {
        destroyOnReturn = shouldDestroy;
    }
    
    /// <summary>
    /// Check if currently being grabbed
    /// </summary>
    public bool IsGrabbed()
    {
        return isCurrentlyGrabbed;
    }
}

