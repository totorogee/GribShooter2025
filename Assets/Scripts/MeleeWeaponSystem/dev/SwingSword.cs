using UnityEngine;

/// <summary>
/// Controls a 2D sword swinging animation.
/// Configurable rest position, swing direction, speed, and visibility.
/// Supports left and right swings with customizable angles.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SwingSword : MonoBehaviour
{
    [System.Serializable]
    public enum SwingDirection
    {
        Left,
        Right
    }
    
    [System.Serializable]
    public enum SwingState
    {
        Rest,
        Swinging,
        Returning
    }
    
    [Header("Swing Settings")]
    [SerializeField] private SwingDirection swingDirection = SwingDirection.Right;
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0;
    [SerializeField] private bool autoSwingOnInput = true;
    
    [Header("Rotation Angles")]
    [SerializeField] private float restAngle = -45f; // Angle when at rest
    [SerializeField] private float swingEndAngle = 45f; // Angle at end of swing
    [SerializeField] private float rotationSpeed = 720f; // Degrees per second
    
    [Header("Swing Behavior")]
    [SerializeField] private bool returnToRest = true; // Return to rest after swing
    [SerializeField] private float returnSpeed = 360f; // Return speed (degrees per second)
    [SerializeField] private float holdTimeAtEnd = 0f; // Hold at end before returning
    
    [Header("Visibility")]
    [SerializeField] private bool visibleWhenRest = false;
    [SerializeField] private bool visibleWhenSwinging = true;
    [SerializeField] private float fadeSpeed = 10f; // For smooth fade in/out
    
    [Header("Pivot Settings")]
    [SerializeField] private Transform pivotPoint = null; // GameObject to rotate around (null = use own transform)
    [SerializeField] private Vector2 pivotOffset = Vector2.zero; // Additional offset from pivot
    
    private SpriteRenderer spriteRenderer;
    private Vector3 pivotPosition; // Cached pivot position
    private Vector3 initialOffsetFromPivot; // Initial offset from pivot to sword
    private SwingState currentState = SwingState.Rest;
    private float currentAngle;
    private float targetAlpha;
    private float holdTimer = 0f;
    
    void Reset()
    {
        // Setup sprite renderer when component is added
        SetupComponents();
    }
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetupComponents();
    }
    
    void Start()
    {
        // Initialize at rest position
        currentAngle = restAngle;
        
        // Calculate initial offset from pivot (only if using pivot point)
        if (pivotPoint != null)
        {
            UpdatePivotPosition();
            initialOffsetFromPivot = transform.position - pivotPosition;
        }
        else
        {
            initialOffsetFromPivot = Vector3.zero;
        }
        
        // Apply initial rotation
        UpdateRotation();
        
        // Set initial visibility
        UpdateVisibility();
    }
    
    void Update()
    {
        HandleInput();
        UpdateSwing();
        UpdateVisibility();
    }
    
    /// <summary>
    /// Sets up required components with proper settings
    /// </summary>
    private void SetupComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.color.a == 1f)
        {
            // Only set initial alpha if it hasn't been customized
            Color color = spriteRenderer.color;
            color.a = visibleWhenRest ? 1f : 0f;
            spriteRenderer.color = color;
        }
    }
    
    /// <summary>
    /// Updates the cached pivot position
    /// </summary>
    private void UpdatePivotPosition()
    {
        if (pivotPoint != null)
        {
            // Use pivot point's position + offset
            pivotPosition = pivotPoint.position + (Vector3)pivotOffset;
        }
        // When pivotPoint is null, pivotPosition is not used
    }
    
    /// <summary>
    /// Updates rotation around pivot point
    /// </summary>
    private void UpdateRotation()
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, currentAngle);
        
        if (pivotPoint != null)
        {
            // Rotate around pivot point
            UpdatePivotPosition();
            
            // Rotate the initial offset vector by current angle
            Vector3 rotatedOffset = rotation * initialOffsetFromPivot;
            
            // Set position relative to pivot
            transform.position = pivotPosition + rotatedOffset;
            
            // Rotate the sword itself to face the correct direction
            transform.localRotation = rotation;
        }
        else
        {
            // No pivot point, just rotate around own center
            transform.localRotation = rotation;
        }
    }
    
    /// <summary>
    /// Handles input for swinging
    /// </summary>
    private void HandleInput()
    {
        if (!autoSwingOnInput)
            return;
        
        if (Input.GetKeyDown(swingKey) && currentState == SwingState.Rest)
        {
            StartSwing();
        }
    }
    
    /// <summary>
    /// Updates swing animation
    /// </summary>
    private void UpdateSwing()
    {
        switch (currentState)
        {
            case SwingState.Rest:
                // Do nothing, waiting for input
                break;
                
            case SwingState.Swinging:
                UpdateSwingRotation();
                break;
                
            case SwingState.Returning:
                UpdateReturnRotation();
                break;
        }
    }
    
    /// <summary>
    /// Updates rotation during swing
    /// </summary>
    private void UpdateSwingRotation()
    {
        // Determine rotation direction based on swing direction
        float targetAngle = swingEndAngle;
        float rotationDelta = rotationSpeed * Time.deltaTime;
        
        if (swingDirection == SwingDirection.Left)
        {
            // Swing left (counter-clockwise - decrease angle)
            currentAngle -= rotationDelta;
            if (currentAngle <= targetAngle)
            {
                currentAngle = targetAngle;
                OnSwingComplete();
            }
        }
        else
        {
            // Swing right (clockwise - increase angle)
            currentAngle += rotationDelta;
            if (currentAngle >= targetAngle)
            {
                currentAngle = targetAngle;
                OnSwingComplete();
            }
        }
        
        UpdateRotation();
    }
    
    /// <summary>
    /// Updates rotation when returning to rest
    /// </summary>
    private void UpdateReturnRotation()
    {
        // Check hold time
        if (holdTimer > 0f)
        {
            holdTimer -= Time.deltaTime;
            return;
        }
        
        // Return to rest angle
        float rotationDelta = returnSpeed * Time.deltaTime;
        
        if (currentAngle > restAngle)
        {
            currentAngle -= rotationDelta;
            if (currentAngle <= restAngle)
            {
                currentAngle = restAngle;
                currentState = SwingState.Rest;
            }
        }
        else if (currentAngle < restAngle)
        {
            currentAngle += rotationDelta;
            if (currentAngle >= restAngle)
            {
                currentAngle = restAngle;
                currentState = SwingState.Rest;
            }
        }
        else
        {
            currentState = SwingState.Rest;
        }
        
        UpdateRotation();
    }
    
    /// <summary>
    /// Called when swing reaches end angle
    /// </summary>
    private void OnSwingComplete()
    {
        if (returnToRest)
        {
            currentState = SwingState.Returning;
            holdTimer = holdTimeAtEnd;
        }
        else
        {
            currentState = SwingState.Rest;
        }
    }
    
    /// <summary>
    /// Updates sword visibility based on state
    /// </summary>
    private void UpdateVisibility()
    {
        if (spriteRenderer == null)
            return;
        
        // Determine target alpha
        if (currentState == SwingState.Rest)
        {
            targetAlpha = visibleWhenRest ? 1f : 0f;
        }
        else
        {
            targetAlpha = visibleWhenSwinging ? 1f : 0f;
        }
        
        // Smoothly fade to target alpha
        Color color = spriteRenderer.color;
        color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = color;
    }
    
    /// <summary>
    /// Manually trigger a swing
    /// </summary>
    public void StartSwing()
    {
        if (currentState != SwingState.Rest)
            return;
        
        currentState = SwingState.Swinging;
        currentAngle = restAngle;
    }
    
    /// <summary>
    /// Manually trigger return to rest
    /// </summary>
    public void ReturnToRest()
    {
        if (currentState == SwingState.Rest)
            return;
        
        currentState = SwingState.Returning;
        holdTimer = 0f;
    }
    
    /// <summary>
    /// Instantly snap to rest position
    /// </summary>
    public void SnapToRest()
    {
        currentState = SwingState.Rest;
        currentAngle = restAngle;
        UpdateRotation();
        holdTimer = 0f;
    }
    
    /// <summary>
    /// Set the swing direction
    /// </summary>
    public void SetSwingDirection(SwingDirection direction)
    {
        swingDirection = direction;
    }
    
    /// <summary>
    /// Set the rest angle
    /// </summary>
    public void SetRestAngle(float angle)
    {
        restAngle = angle;
        if (currentState == SwingState.Rest)
        {
            currentAngle = restAngle;
            UpdateRotation();
        }
    }
    
    /// <summary>
    /// Set the swing end angle
    /// </summary>
    public void SetSwingEndAngle(float angle)
    {
        swingEndAngle = angle;
    }
    
    /// <summary>
    /// Set the rotation speed
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    
    /// <summary>
    /// Get current swing state
    /// </summary>
    public SwingState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Check if sword is currently swinging
    /// </summary>
    public bool IsSwinging()
    {
        return currentState == SwingState.Swinging;
    }
    
    /// <summary>
    /// Check if sword is at rest
    /// </summary>
    public bool IsAtRest()
    {
        return currentState == SwingState.Rest;
    }
    
    /// <summary>
    /// Set visibility when at rest
    /// </summary>
    public void SetVisibleWhenRest(bool visible)
    {
        visibleWhenRest = visible;
    }
    
    /// <summary>
    /// Set visibility when swinging
    /// </summary>
    public void SetVisibleWhenSwinging(bool visible)
    {
        visibleWhenSwinging = visible;
    }
    
    /// <summary>
    /// Set the pivot point GameObject
    /// </summary>
    public void SetPivotPoint(Transform pivot)
    {
        pivotPoint = pivot;
        if (pivotPoint != null)
        {
            // Recalculate offset from new pivot
            UpdatePivotPosition();
            initialOffsetFromPivot = transform.position - pivotPosition;
            UpdateRotation();
        }
    }
    
    /// <summary>
    /// Get the current pivot point
    /// </summary>
    public Transform GetPivotPoint()
    {
        return pivotPoint;
    }
}

