using UnityEngine;

/// <summary>
/// Controls the grappling hand projectile behavior.
/// Shoots in a straight line, grabs objects with IGrabbable component, and returns to player.
/// Settings are configured via Initialize() method from HandShooter.
/// Hand is reused (not destroyed) when it returns.
/// </summary>
public class GrapplingHand : MonoBehaviour
{
    // Event triggered when hand returns to home position
    public System.Action OnHandReturned;
    
    // All settings are now passed via Initialize()
    private float speed;
    private float maxRange;
    private float waitTimeAtMax;
    private float grabRadius;
    
    private Vector2 shootDirection;
    private Vector3 startPosition;
    private Transform playerTransform;
    private Transform homePosition; // The shoot point to return to
    private Rigidbody2D rb;
    
    private Quaternion initialRotation; // Starting rotation of the hand (its "forward")
    private Quaternion initialLocalRotation; // Local rotation relative to parent
    private Transform originalParent; // Store original parent for re-parenting
    
    private float waitTimer = 0f;
    
    private GameObject grabbedObject;
    private IGrabbable grabbedComponent;
    private Vector3 grabbedOffset; // Offset from hand to grabbed object
    
    private HandState currentState = HandState.Extending;
    
    private enum HandState
    {
        Extending,   // Moving forward
        Waiting,     // Waiting at max range
        Returning    // Coming back to player
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Store the initial rotation as the hand's "forward" direction
        initialRotation = transform.rotation;
        initialLocalRotation = transform.localRotation;
        originalParent = transform.parent;
    }
    
    /// <summary>
    /// Sets the home position (shoot point) where the hand returns to
    /// </summary>
    public void SetHomePosition(Transform home)
    {
        homePosition = home;
        // Also store initial rotation when home is set
        initialRotation = transform.rotation;
        initialLocalRotation = transform.localRotation;
        originalParent = transform.parent;
    }
    
    void Update()
    {
        switch (currentState)
        {
            case HandState.Extending:
                HandleExtending();
                break;
                
            case HandState.Waiting:
                HandleWaiting();
                break;
                
            case HandState.Returning:
                HandleReturning();
                break;
        }
        
        // Update grabbed object position if we have one
        if (grabbedObject != null)
        {
            grabbedObject.transform.position = transform.position + grabbedOffset;
        }
    }
    
    /// <summary>
    /// Initializes the hand with shooting parameters and settings
    /// </summary>
    public void Initialize(Vector2 direction, Transform player, float handSpeed, float handMaxRange, 
                          float handWaitTime, float handGrabRadius)
    {
        // Store settings
        speed = handSpeed;
        maxRange = handMaxRange;
        waitTimeAtMax = handWaitTime;
        grabRadius = handGrabRadius;
        
        shootDirection = direction.normalized;
        playerTransform = player;
        startPosition = transform.position;
        
        // Reset state
        currentState = HandState.Extending;
        waitTimer = 0f;
        
        // Clear any grabbed object
        if (grabbedObject != null && grabbedComponent != null)
        {
            grabbedComponent.OnReleased();
        }
        grabbedObject = null;
        grabbedComponent = null;
        
        // Ensure Rigidbody2D is available
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        // Detach from parent to move independently
        if (transform.parent != null)
        {
            transform.SetParent(null, true); // Keep world position
        }
        
        // Set initial velocity
        rb.linearVelocity = shootDirection * speed;
        
        // Use world rotation based on player's current rotation + initial local rotation
        if (playerTransform != null && originalParent == playerTransform)
        {
            // Calculate world rotation: player rotation + initial local rotation
            transform.rotation = playerTransform.rotation * initialLocalRotation;
        }
        else
        {
            // Keep initial world rotation
            transform.rotation = initialRotation;
        }
    }
    
    /// <summary>
    /// Handles hand behavior while extending
    /// </summary>
    private void HandleExtending()
    {
        // Update rotation to follow player if originally parented
        UpdateRotationFromPlayer();
        
        // Check for grabbable objects
        CheckForGrabbable();
        
        // Check if reached max range
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxRange)
        {
            currentState = HandState.Waiting;
            rb.linearVelocity = Vector2.zero;
            waitTimer = 0f;
        }
    }
    
    /// <summary>
    /// Handles waiting state at max range
    /// </summary>
    private void HandleWaiting()
    {
        // Update rotation to follow player if originally parented
        UpdateRotationFromPlayer();
        
        waitTimer += Time.deltaTime;
        
        if (waitTimer >= waitTimeAtMax)
        {
            StartReturning();
        }
    }
    
    /// <summary>
    /// Handles returning to home position (shoot point)
    /// </summary>
    private void HandleReturning()
    {
        // Determine return target (home position if set, otherwise player)
        Vector3 targetPosition;
        if (homePosition != null)
        {
            targetPosition = homePosition.position;
        }
        else if (playerTransform != null)
        {
            targetPosition = playerTransform.position;
        }
        else
        {
            // No target, trigger return event
            OnHandReturned?.Invoke();
            return;
        }
        
        // Move towards target
        Vector2 directionToTarget = (targetPosition - transform.position).normalized;
        rb.linearVelocity = directionToTarget * speed;
        
        // Update rotation to follow player if originally parented
        UpdateRotationFromPlayer();
        
        // Check if reached target
        if (Vector2.Distance(transform.position, targetPosition) < 0.2f)
        {
            // Release grabbed object if any
            if (grabbedObject != null && grabbedComponent != null)
            {
                grabbedComponent.OnReleased();
            }
            grabbedObject = null;
            grabbedComponent = null;
            
            // Snap to exact position
            transform.position = targetPosition;
            
            // Stop movement
            rb.linearVelocity = Vector2.zero;
            
            // Re-parent to original parent
            if (originalParent != null)
            {
                transform.SetParent(originalParent, true);
                transform.localRotation = initialLocalRotation;
            }
            else
            {
                transform.rotation = initialRotation;
            }
            
            // Trigger event
            OnHandReturned?.Invoke();
        }
    }
    
    /// <summary>
    /// Updates hand rotation to follow player if it was originally parented to player
    /// </summary>
    private void UpdateRotationFromPlayer()
    {
        if (playerTransform != null && originalParent == playerTransform)
        {
            // Follow player rotation: player rotation + initial local rotation
            transform.rotation = playerTransform.rotation * initialLocalRotation;
        }
    }
    
    /// <summary>
    /// Checks for grabbable objects in range
    /// </summary>
    private void CheckForGrabbable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, grabRadius);
        
        foreach (Collider2D hit in hits)
        {
            // Skip if it's the player
            if (hit.transform == playerTransform)
                continue;
                
            // Check if object has IGrabbable component
            IGrabbable grabbable = hit.GetComponent<IGrabbable>();
            if (grabbable != null && grabbable.CanBeGrabbed())
            {
                GrabObject(hit.gameObject, grabbable);
                StartReturning();
                return;
            }
        }
    }
    
    /// <summary>
    /// Grabs an object
    /// </summary>
    private void GrabObject(GameObject obj, IGrabbable grabbable)
    {
        grabbedObject = obj;
        grabbedComponent = grabbable;
        grabbedOffset = obj.transform.position - transform.position;
        
        // Notify the object it's been grabbed
        grabbable.OnGrabbed(playerTransform);
    }
    
    /// <summary>
    /// Starts the return sequence
    /// </summary>
    private void StartReturning()
    {
        currentState = HandState.Returning;
    }
    
    /// <summary>
    /// Called when hand hits something (trigger collision)
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Skip if already returning or if it's the player
        if (currentState == HandState.Returning || other.transform == playerTransform)
            return;
        
        // Check for grabbable
        IGrabbable grabbable = other.GetComponent<IGrabbable>();
        if (grabbable != null && grabbable.CanBeGrabbed())
        {
            GrabObject(other.gameObject, grabbable);
            StartReturning();
        }
        else
        {
            // Hit something that's not grabbable, start returning immediately
            StartReturning();
        }
    }
    
    /// <summary>
    /// Gets the currently grabbed object
    /// </summary>
    public GameObject GetGrabbedObject()
    {
        return grabbedObject;
    }
    
    /// <summary>
    /// Checks if hand is currently grabbing something
    /// </summary>
    public bool IsGrabbing()
    {
        return grabbedObject != null;
    }
}

