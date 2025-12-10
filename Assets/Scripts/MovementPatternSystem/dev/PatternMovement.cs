using UnityEngine;

/// <summary>
/// Moves GameObject in various patterns (linear, sin wave) on X or Y axis.
/// Can destroy object when it moves outside a defined boundary area.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PatternMovement : MonoBehaviour
{
    [System.Serializable]
    public enum MovementPattern
    {
        Linear,        // Straight line
        SinWave,       // Smooth sine wave
        Zigzag,        // Sharp zigzag pattern
        Circle,        // Circular motion
        Spiral,        // Expanding/contracting spiral
        Figure8,       // Figure-8 / infinity pattern
        Square,        // Square wave (sudden jumps)
        Bounce         // Bouncing motion
    }
    
    [System.Serializable]
    public enum MovementAxis
    {
        X,
        Y
    }
    
    [System.Serializable]
    public enum BoundaryType
    {
        None,
        TwoPoints,
        SpriteRenderer
    }
    
    [Header("Movement Settings")]
    [SerializeField] private MovementPattern pattern = MovementPattern.Linear;
    [SerializeField] private MovementAxis primaryAxis = MovementAxis.X;
    [SerializeField] private float speed = 5f;
    
    [Header("Pattern-Specific Settings")]
    [SerializeField] private float waveAmplitude = 1f;
    [SerializeField] private float waveFrequency = 1f;
    [SerializeField] private float waveOffset = 0f;
    
    [Header("Circle/Spiral Settings")]
    [SerializeField] private float circleRadius = 2f;
    [SerializeField] private float spiralGrowthRate = 0.1f; // How fast spiral expands/contracts
    
    [Header("Zigzag/Square Settings")]
    [SerializeField] private float zigzagHeight = 1f;
    
    [Header("Bounce Settings")]
    [SerializeField] private float bounceHeight = 2f;
    [SerializeField] private float bounceDuration = 1f;
    
    [Header("Look At Settings")]
    [SerializeField] private bool enableLookAt = false;
    [SerializeField] private Transform lookAtTarget = null;
    [SerializeField] private float rotationSpeed = 0f; // 0 = instant, >0 = smooth rotation
    [SerializeField] private Vector3 rotationOffset = Vector3.zero; // Additional rotation offset in degrees
    
    [Header("Boundary Settings")]
    [SerializeField] private BoundaryType boundaryType = BoundaryType.None;
    [SerializeField] private bool destroyOutsideBoundary = true;
    
    [Header("Two Points Boundary")]
    [SerializeField] private Vector2 boundaryPoint1 = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 boundaryPoint2 = new Vector2(10f, 10f);
    
    [Header("Sprite Boundary")]
    [SerializeField] private GameObject boundarySprite;
    
    private Rigidbody2D rb;
    private float timeOffset;
    private Vector2 startPosition;
    private Bounds spriteBounds;
    private bool spriteBoundsCalculated = false;
    
    void Reset()
    {
        // Ensure Rigidbody2D has proper settings
        SetupRigidbody();
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupRigidbody();
    }
    
    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f); // Random offset for variety
        
        // Calculate sprite bounds if using sprite boundary
        if (boundaryType == BoundaryType.SpriteRenderer && boundarySprite != null)
        {
            CalculateSpriteBounds();
        }
    }
    
    void FixedUpdate()
    {
        MoveInPattern();
        CheckBoundary();
    }
    
    void Update()
    {
        HandleLookAt();
    }
    
    /// <summary>
    /// Sets up Rigidbody2D with appropriate settings for pattern movement
    /// </summary>
    private void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Only configure if it has default values
            if (rb.bodyType == RigidbodyType2D.Dynamic && rb.gravityScale == 1f)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }
        }
    }
    
    /// <summary>
    /// Handles rotation to look at target if enabled
    /// </summary>
    private void HandleLookAt()
    {
        if (!enableLookAt || lookAtTarget == null)
            return;
        
        // Calculate direction to target
        Vector2 direction = (lookAtTarget.position - transform.position).normalized;
        
        // Calculate target angle (in 2D, looking "up" along Y axis)
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Apply rotation offset
        targetAngle += rotationOffset.z;
        
        // Apply rotation (instant or smooth)
        if (rotationSpeed <= 0f)
        {
            // Instant rotation
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle - 90f);
        }
        else
        {
            // Smooth rotation
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle - 90f, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }
    }
    
    /// <summary>
    /// Moves the object according to the selected pattern
    /// </summary>
    private void MoveInPattern()
    {
        Vector2 velocity = Vector2.zero;
        float time = Time.time + timeOffset;
        
        switch (pattern)
        {
            case MovementPattern.Linear:
                velocity = GetLinearVelocity();
                break;
                
            case MovementPattern.SinWave:
                velocity = GetSinWaveVelocity(time);
                break;
                
            case MovementPattern.Zigzag:
                velocity = GetZigzagVelocity(time);
                break;
                
            case MovementPattern.Circle:
                velocity = GetCircleVelocity(time);
                break;
                
            case MovementPattern.Spiral:
                velocity = GetSpiralVelocity(time);
                break;
                
            case MovementPattern.Figure8:
                velocity = GetFigure8Velocity(time);
                break;
                
            case MovementPattern.Square:
                velocity = GetSquareWaveVelocity(time);
                break;
                
            case MovementPattern.Bounce:
                velocity = GetBounceVelocity(time);
                break;
        }
        
        rb.linearVelocity = velocity;
    }
    
    /// <summary>
    /// Calculates linear movement velocity
    /// </summary>
    private Vector2 GetLinearVelocity()
    {
        if (primaryAxis == MovementAxis.X)
        {
            return new Vector2(speed, 0f);
        }
        else
        {
            return new Vector2(0f, speed);
        }
    }
    
    /// <summary>
    /// Calculates sin wave movement velocity
    /// </summary>
    private Vector2 GetSinWaveVelocity(float time)
    {
        if (primaryAxis == MovementAxis.X)
        {
            // Moving along X axis, wave on Y axis
            float yVelocity = Mathf.Cos(time * waveFrequency + waveOffset) * waveFrequency * waveAmplitude;
            return new Vector2(speed, yVelocity);
        }
        else
        {
            // Moving along Y axis, wave on X axis
            float xVelocity = Mathf.Cos(time * waveFrequency + waveOffset) * waveFrequency * waveAmplitude;
            return new Vector2(xVelocity, speed);
        }
    }
    
    /// <summary>
    /// Calculates zigzag movement velocity (sharp angles)
    /// </summary>
    private Vector2 GetZigzagVelocity(float time)
    {
        // Triangle wave function for sharp zigzag
        float triangleWave = Mathf.PingPong(time * waveFrequency, 1f) * 2f - 1f;
        
        if (primaryAxis == MovementAxis.X)
        {
            float yVelocity = Mathf.Sign(Mathf.Cos(time * waveFrequency * Mathf.PI)) * waveFrequency * zigzagHeight * 2f;
            return new Vector2(speed, yVelocity);
        }
        else
        {
            float xVelocity = Mathf.Sign(Mathf.Cos(time * waveFrequency * Mathf.PI)) * waveFrequency * zigzagHeight * 2f;
            return new Vector2(xVelocity, speed);
        }
    }
    
    /// <summary>
    /// Calculates circular movement velocity
    /// </summary>
    private Vector2 GetCircleVelocity(float time)
    {
        // Circular motion around start position
        float angle = time * waveFrequency;
        float nextAngle = (time + Time.fixedDeltaTime) * waveFrequency;
        
        Vector2 currentPos = startPosition + new Vector2(
            Mathf.Cos(angle) * circleRadius,
            Mathf.Sin(angle) * circleRadius
        );
        
        Vector2 nextPos = startPosition + new Vector2(
            Mathf.Cos(nextAngle) * circleRadius,
            Mathf.Sin(nextAngle) * circleRadius
        );
        
        // Add forward movement if specified
        Vector2 forwardMovement = primaryAxis == MovementAxis.X ? 
            new Vector2(speed * Time.fixedDeltaTime, 0f) : 
            new Vector2(0f, speed * Time.fixedDeltaTime);
        
        return (nextPos - currentPos) / Time.fixedDeltaTime + forwardMovement;
    }
    
    /// <summary>
    /// Calculates spiral movement velocity (expanding circle)
    /// </summary>
    private Vector2 GetSpiralVelocity(float time)
    {
        // Spiral with expanding radius
        float angle = time * waveFrequency;
        float radius = circleRadius + (time * spiralGrowthRate);
        float nextAngle = (time + Time.fixedDeltaTime) * waveFrequency;
        float nextRadius = circleRadius + ((time + Time.fixedDeltaTime) * spiralGrowthRate);
        
        Vector2 currentPos = startPosition + new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
        
        Vector2 nextPos = startPosition + new Vector2(
            Mathf.Cos(nextAngle) * nextRadius,
            Mathf.Sin(nextAngle) * nextRadius
        );
        
        // Add forward movement
        Vector2 forwardMovement = primaryAxis == MovementAxis.X ? 
            new Vector2(speed * Time.fixedDeltaTime, 0f) : 
            new Vector2(0f, speed * Time.fixedDeltaTime);
        
        return (nextPos - currentPos) / Time.fixedDeltaTime + forwardMovement;
    }
    
    /// <summary>
    /// Calculates figure-8 movement velocity
    /// </summary>
    private Vector2 GetFigure8Velocity(float time)
    {
        // Lissajous curve for figure-8 pattern
        float t = time * waveFrequency;
        float nextT = (time + Time.fixedDeltaTime) * waveFrequency;
        
        Vector2 currentPos = startPosition + new Vector2(
            Mathf.Sin(t) * waveAmplitude,
            Mathf.Sin(t * 2f) * waveAmplitude * 0.5f
        );
        
        Vector2 nextPos = startPosition + new Vector2(
            Mathf.Sin(nextT) * waveAmplitude,
            Mathf.Sin(nextT * 2f) * waveAmplitude * 0.5f
        );
        
        // Add forward movement
        Vector2 forwardMovement = primaryAxis == MovementAxis.X ? 
            new Vector2(speed * Time.fixedDeltaTime, 0f) : 
            new Vector2(0f, speed * Time.fixedDeltaTime);
        
        return (nextPos - currentPos) / Time.fixedDeltaTime + forwardMovement;
    }
    
    /// <summary>
    /// Calculates square wave movement velocity (sudden jumps)
    /// </summary>
    private Vector2 GetSquareWaveVelocity(float time)
    {
        // Square wave - sudden position changes
        float squareWave = Mathf.Sign(Mathf.Sin(time * waveFrequency * Mathf.PI * 2f));
        
        if (primaryAxis == MovementAxis.X)
        {
            float targetY = startPosition.y + squareWave * zigzagHeight;
            float currentY = transform.position.y;
            float yVelocity = (targetY - currentY) * 10f; // Fast snapping to position
            
            return new Vector2(speed, yVelocity);
        }
        else
        {
            float targetX = startPosition.x + squareWave * zigzagHeight;
            float currentX = transform.position.x;
            float xVelocity = (targetX - currentX) * 10f;
            
            return new Vector2(xVelocity, speed);
        }
    }
    
    /// <summary>
    /// Calculates bouncing movement velocity
    /// </summary>
    private Vector2 GetBounceVelocity(float time)
    {
        // Parabolic bounce using abs(sin) for continuous bouncing
        float bouncePhase = (time / bounceDuration) % 1f;
        float bounceValue = Mathf.Abs(Mathf.Sin(bouncePhase * Mathf.PI)) * bounceHeight;
        
        float nextBouncePhase = ((time + Time.fixedDeltaTime) / bounceDuration) % 1f;
        float nextBounceValue = Mathf.Abs(Mathf.Sin(nextBouncePhase * Mathf.PI)) * bounceHeight;
        
        float bounceVelocity = (nextBounceValue - bounceValue) / Time.fixedDeltaTime;
        
        if (primaryAxis == MovementAxis.X)
        {
            return new Vector2(speed, bounceVelocity);
        }
        else
        {
            return new Vector2(bounceVelocity, speed);
        }
    }
    
    /// <summary>
    /// Checks if object is outside boundary and destroys if configured
    /// </summary>
    private void CheckBoundary()
    {
        if (!destroyOutsideBoundary || boundaryType == BoundaryType.None)
            return;
        
        bool isOutside = false;
        
        switch (boundaryType)
        {
            case BoundaryType.TwoPoints:
                isOutside = IsOutsideTwoPointsBoundary();
                break;
                
            case BoundaryType.SpriteRenderer:
                isOutside = IsOutsideSpriteBoundary();
                break;
        }
        
        if (isOutside)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Checks if position is outside the two-point boundary
    /// </summary>
    private bool IsOutsideTwoPointsBoundary()
    {
        Vector2 pos = transform.position;
        
        float minX = Mathf.Min(boundaryPoint1.x, boundaryPoint2.x);
        float maxX = Mathf.Max(boundaryPoint1.x, boundaryPoint2.x);
        float minY = Mathf.Min(boundaryPoint1.y, boundaryPoint2.y);
        float maxY = Mathf.Max(boundaryPoint1.y, boundaryPoint2.y);
        
        return pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY;
    }
    
    /// <summary>
    /// Checks if position is outside the sprite boundary
    /// </summary>
    private bool IsOutsideSpriteBoundary()
    {
        if (!spriteBoundsCalculated)
        {
            CalculateSpriteBounds();
        }
        
        if (!spriteBoundsCalculated)
        {
            return false; // Can't check boundary if no sprite
        }
        
        Vector2 pos = transform.position;
        return !spriteBounds.Contains(pos);
    }
    
    /// <summary>
    /// Calculates the bounds of the boundary sprite
    /// </summary>
    private void CalculateSpriteBounds()
    {
        if (boundarySprite == null)
        {
            Debug.LogWarning("PatternMovement: Boundary sprite not assigned!");
            return;
        }
        
        SpriteRenderer sr = boundarySprite.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteBounds = sr.bounds;
            spriteBoundsCalculated = true;
        }
        else
        {
            Debug.LogWarning("PatternMovement: Boundary GameObject has no SpriteRenderer!");
        }
    }
    
    /// <summary>
    /// Manually set the two-point boundary
    /// </summary>
    public void SetTwoPointBoundary(Vector2 point1, Vector2 point2)
    {
        boundaryType = BoundaryType.TwoPoints;
        boundaryPoint1 = point1;
        boundaryPoint2 = point2;
    }
    
    /// <summary>
    /// Manually set the sprite boundary
    /// </summary>
    public void SetSpriteBoundary(GameObject sprite)
    {
        boundaryType = BoundaryType.SpriteRenderer;
        boundarySprite = sprite;
        spriteBoundsCalculated = false;
        CalculateSpriteBounds();
    }
    
    /// <summary>
    /// Set the movement pattern at runtime
    /// </summary>
    public void SetPattern(MovementPattern newPattern)
    {
        pattern = newPattern;
    }
    
    /// <summary>
    /// Set the movement axis at runtime
    /// </summary>
    public void SetAxis(MovementAxis newAxis)
    {
        primaryAxis = newAxis;
    }
    
    /// <summary>
    /// Set the movement speed at runtime
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    /// <summary>
    /// Set sin wave parameters at runtime
    /// </summary>
    public void SetSinWaveParameters(float amplitude, float frequency, float offset = 0f)
    {
        waveAmplitude = amplitude;
        waveFrequency = frequency;
        waveOffset = offset;
    }
    
    /// <summary>
    /// Enable or disable boundary destruction
    /// </summary>
    public void SetDestroyOutsideBoundary(bool shouldDestroy)
    {
        destroyOutsideBoundary = shouldDestroy;
    }
    
    /// <summary>
    /// Enable or disable look at feature
    /// </summary>
    public void SetLookAtEnabled(bool enabled)
    {
        enableLookAt = enabled;
    }
    
    /// <summary>
    /// Set the target to look at
    /// </summary>
    public void SetLookAtTarget(Transform target)
    {
        lookAtTarget = target;
    }
    
    /// <summary>
    /// Set rotation speed (0 = instant, >0 = smooth)
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    
    /// <summary>
    /// Set rotation offset in degrees
    /// </summary>
    public void SetRotationOffset(float offsetDegrees)
    {
        rotationOffset = new Vector3(0f, 0f, offsetDegrees);
    }
    
    /// <summary>
    /// Draw boundary gizmos in editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (boundaryType == BoundaryType.TwoPoints)
        {
            // Draw two-point boundary
            Gizmos.color = Color.yellow;
            
            float minX = Mathf.Min(boundaryPoint1.x, boundaryPoint2.x);
            float maxX = Mathf.Max(boundaryPoint1.x, boundaryPoint2.x);
            float minY = Mathf.Min(boundaryPoint1.y, boundaryPoint2.y);
            float maxY = Mathf.Max(boundaryPoint1.y, boundaryPoint2.y);
            
            Vector3 bottomLeft = new Vector3(minX, minY, 0f);
            Vector3 bottomRight = new Vector3(maxX, minY, 0f);
            Vector3 topRight = new Vector3(maxX, maxY, 0f);
            Vector3 topLeft = new Vector3(minX, maxY, 0f);
            
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
        else if (boundaryType == BoundaryType.SpriteRenderer && boundarySprite != null)
        {
            // Draw sprite boundary
            SpriteRenderer sr = boundarySprite.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Gizmos.color = Color.cyan;
                Bounds bounds = sr.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}

