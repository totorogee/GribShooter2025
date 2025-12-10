using UnityEngine;

/// <summary>
/// Component that allows the player to shoot grappling hands.
/// Attach this to the player GameObject.
/// Supports two different hand types with L and K keys.
/// </summary>
public class HandShooter : MonoBehaviour
{
    [System.Serializable]
    public class HandConfig
    {
        [Header("Visual")]
        public GameObject handPrefab; // Can be null, will create simple sprite
        public Sprite handSprite; // Optional: sprite for the hand
        public Color handColor = Color.white;
        public Vector2 handSize = new Vector2(0.5f, 0.5f);
        
        [Header("Physics")]
        public float colliderRadius = 0.3f;
        
        [Header("Hand Behavior")]
        public float speed = 15f;
        public float maxRange = 10f;
        public float waitTimeAtMax = 0.3f;
        
        [Header("Grab Settings")]
        public float grabRadius = 0.5f;
        
        [Header("Cooldown")]
        public float shootCooldown = 1f;
    }
    
    [Header("Hand Configurations")]
    [SerializeField] private HandConfig leftHand; // L key
    [SerializeField] private HandConfig rightHand; // K key
    
    [Header("Shoot Points")]
    [SerializeField] private Transform leftShootPoint; // Optional: specific point for left hand
    [SerializeField] private Transform rightShootPoint; // Optional: specific point for right hand
    
    [Header("General Settings")]
    [SerializeField] private bool allowMultipleHands = true; // Can shoot multiple hands at once?
    
    private float lastLeftHandTime = -999f;
    private float lastRightHandTime = -999f;
    private GrapplingHand leftHandInstance;
    private GrapplingHand rightHandInstance;
    private bool isLeftHandActive = false;
    private bool isRightHandActive = false;
    
    void Awake()
    {
        // Initialize hand configs if null
        if (leftHand == null)
        {
            leftHand = new HandConfig();
            leftHand.handColor = Color.blue;
        }
        if (rightHand == null)
        {
            rightHand = new HandConfig();
            rightHand.handColor = Color.red;
        }
    }
    
    void Start()
    {
        // Initialize left hand
        leftHandInstance = InitializeHandInstance(leftHand, "LeftHand", leftShootPoint);
        if (leftHandInstance != null)
        {
            // Position at shoot point
            Vector3 leftPos = leftShootPoint != null ? leftShootPoint.position : transform.position + Vector3.left * 0.5f;
            leftHandInstance.transform.position = leftPos;
            leftHandInstance.gameObject.SetActive(true); // Always visible
        }
        
        // Initialize right hand
        rightHandInstance = InitializeHandInstance(rightHand, "RightHand", rightShootPoint);
        if (rightHandInstance != null)
        {
            // Position at shoot point
            Vector3 rightPos = rightShootPoint != null ? rightShootPoint.position : transform.position + Vector3.right * 0.5f;
            rightHandInstance.transform.position = rightPos;
            rightHandInstance.gameObject.SetActive(true); // Always visible
        }
    }
    
    void Update()
    {
        HandleHandShooting();
    }
    
    /// <summary>
    /// Handles input and shooting logic for grappling hands
    /// </summary>
    private void HandleHandShooting()
    {
        // Left hand (L key)
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Check cooldown
            if (Time.time - lastLeftHandTime < leftHand.shootCooldown)
                return;
            
            // Check if already active (if multiple hands not allowed)
            if (!allowMultipleHands && isLeftHandActive)
                return;
            
            ShootHandInstance(leftHandInstance, leftHand, leftShootPoint, ref isLeftHandActive, ref lastLeftHandTime);
        }
        
        // Right hand (K key)
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Check cooldown
            if (Time.time - lastRightHandTime < rightHand.shootCooldown)
                return;
            
            // Check if already active (if multiple hands not allowed)
            if (!allowMultipleHands && isRightHandActive)
                return;
            
            ShootHandInstance(rightHandInstance, rightHand, rightShootPoint, ref isRightHandActive, ref lastRightHandTime);
        }
    }
    
    /// <summary>
    /// Initializes a hand instance at start (creates or uses existing)
    /// </summary>
    private GrapplingHand InitializeHandInstance(HandConfig config, string handName, Transform shootPoint)
    {
        GameObject handObj;
        
        // Check if handPrefab is assigned
        if (config.handPrefab != null)
        {
            // Check if it's a scene object or a prefab
            if (config.handPrefab.scene.name != null)
            {
                // It's a scene object, use it directly
                handObj = config.handPrefab;
                Debug.Log($"Using scene object for {handName}");
            }
            else
            {
                // It's a prefab, instantiate it
                handObj = Instantiate(config.handPrefab);
                handObj.name = handName;
                Debug.Log($"Instantiated prefab for {handName}");
            }
        }
        else
        {
            // Create a simple sprite-based hand
            handObj = CreateHandGameObject(config);
            handObj.name = handName;
            Debug.Log($"Created simple GameObject for {handName}");
        }
        
        // Ensure hand has required components
        SetupHandComponents(handObj, config);
        
        // Get or add GrapplingHand component
        GrapplingHand hand = handObj.GetComponent<GrapplingHand>();
        if (hand == null)
        {
            hand = handObj.AddComponent<GrapplingHand>();
        }
        
        // Set the home position (shoot point) for returning
        hand.SetHomePosition(shootPoint);
        
        // Register callback for when hand returns
        hand.OnHandReturned += () => OnHandReturned(hand, shootPoint);
        
        return hand;
    }
    
    /// <summary>
    /// Shoots an existing hand instance (repositions and activates it)
    /// </summary>
    private void ShootHandInstance(GrapplingHand handInstance, HandConfig config, Transform shootPoint, 
                                   ref bool isActive, ref float lastShootTime)
    {
        if (handInstance == null)
        {
            Debug.LogWarning("Hand instance is null!");
            return;
        }
        
        // Get shoot direction based on player's rotation (facing direction)
        Vector2 shootDirection = transform.up; // Assuming player's "forward" is up in 2D
        
        // Initialize the hand with all settings (hand will move from current position)
        handInstance.Initialize(shootDirection, transform, config.speed, config.maxRange, 
                               config.waitTimeAtMax, config.grabRadius);
        
        isActive = true;
        lastShootTime = Time.time;
    }
    
    /// <summary>
    /// Called when a hand returns to its home position
    /// </summary>
    private void OnHandReturned(GrapplingHand hand, Transform shootPoint)
    {
        // Hand stays visible but is no longer active
        // Update active status
        if (hand == leftHandInstance)
        {
            isLeftHandActive = false;
        }
        else if (hand == rightHandInstance)
        {
            isRightHandActive = false;
        }
    }
    
    /// <summary>
    /// Creates a simple GameObject for the hand
    /// </summary>
    private GameObject CreateHandGameObject(HandConfig config)
    {
        GameObject handObj = new GameObject("GrapplingHand");
        
        // Add sprite renderer
        SpriteRenderer sr = handObj.AddComponent<SpriteRenderer>();
        
        if (config.handSprite != null)
        {
            sr.sprite = config.handSprite;
        }
        else
        {
            // Create a simple circle sprite
            sr.sprite = CreateCircleSprite();
        }
        
        sr.color = config.handColor;
        handObj.transform.localScale = new Vector3(config.handSize.x, config.handSize.y, 1f);
        
        return handObj;
    }
    
    /// <summary>
    /// Sets up required physics components on the hand GameObject
    /// </summary>
    private void SetupHandComponents(GameObject handObj, HandConfig config)
    {
        // Add/configure Rigidbody2D
        Rigidbody2D rb = handObj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = handObj.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add/configure CircleCollider2D
        CircleCollider2D collider = handObj.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = handObj.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;
        collider.radius = config.colliderRadius;
    }
    
    /// <summary>
    /// Creates a simple circle sprite for default hand visual
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                pixels[y * resolution + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// Gets the left hand instance
    /// </summary>
    public GrapplingHand GetLeftHandInstance()
    {
        return leftHandInstance;
    }
    
    /// <summary>
    /// Gets the right hand instance
    /// </summary>
    public GrapplingHand GetRightHandInstance()
    {
        return rightHandInstance;
    }
    
    /// <summary>
    /// Checks if left hand is currently active
    /// </summary>
    public bool IsLeftHandActive()
    {
        return isLeftHandActive;
    }
    
    /// <summary>
    /// Checks if right hand is currently active
    /// </summary>
    public bool IsRightHandActive()
    {
        return isRightHandActive;
    }
    
    /// <summary>
    /// Gets the left hand cooldown progress (0-1, where 1 is ready to shoot)
    /// </summary>
    public float GetLeftHandCooldownProgress()
    {
        float timeSinceShoot = Time.time - lastLeftHandTime;
        return Mathf.Clamp01(timeSinceShoot / leftHand.shootCooldown);
    }
    
    /// <summary>
    /// Gets the right hand cooldown progress (0-1, where 1 is ready to shoot)
    /// </summary>
    public float GetRightHandCooldownProgress()
    {
        float timeSinceShoot = Time.time - lastRightHandTime;
        return Mathf.Clamp01(timeSinceShoot / rightHand.shootCooldown);
    }
}

