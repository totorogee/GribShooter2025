using UnityEngine;
using GameTool.Hex;

/// <summary>
/// Tracks a GameObject's hex coordinate position.
/// Updates automatically when the GameObject moves.
/// Can optionally use HexEnvController's hex system settings.
/// </summary>
public class HexCoordinateTracker : MonoBehaviour
{
    [Header("Hex System Settings")]
    [Tooltip("Use HexEnvController's hex system settings (recommended)")]
    [SerializeField] private bool useHexEnvController = true;
    
    [Tooltip("Custom hex system (only used if useHexEnvController is false)")]
    [SerializeField] private HexSystem customHexSystem = HexSystem.GetDefault();
    
    [Header("Update Settings")]
    [Tooltip("Update hex coordinate every frame (for moving objects)")]
    [SerializeField] private bool updateEveryFrame = true;
    
    [Tooltip("Update hex coordinate only when position changes significantly")]
    [SerializeField] private bool updateOnPositionChange = true;
    
    [Tooltip("Minimum position change required to update (in world units)")]
    [SerializeField] private float positionChangeThreshold = 0.1f;
    
    [Header("Debug")]
    [Tooltip("Show current hex coordinate in Inspector")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Current hex coordinate
    private HexInt currentHexCoordinate = new HexInt(0, 0);
    private Vector3 lastPosition;
    private HexEnvController hexController;
    
    // Events
    public System.Action<HexInt, HexInt> OnHexCoordinateChanged; // (oldCoordinate, newCoordinate)
    
    void Start()
    {
        // Get HexEnvController if using it
        if (useHexEnvController)
        {
            hexController = HexEnvController.Instance;
            if (hexController == null)
            {
                Debug.LogWarning("HexCoordinateTracker: HexEnvController not found! Using custom hex system.");
                useHexEnvController = false;
            }
            else
            {
                Debug.Log($"HexCoordinateTracker: Using HexEnvController with Scale={hexController.HexSystem.Scale}, Orientation={hexController.HexSystem.Orientation}");
            }
        }
        else
        {
            Debug.Log($"HexCoordinateTracker: Using custom hex system with Scale={customHexSystem.Scale}, Orientation={customHexSystem.Orientation}");
        }
        
        // Initialize with current position
        lastPosition = transform.position;
        UpdateHexCoordinate();
        
        // Log initial coordinate
        Debug.Log($"HexCoordinateTracker: Initial hex coordinate: ({currentHexCoordinate.x}, {currentHexCoordinate.y}) at world position {transform.position}");
    }
    
    void Update()
    {
        if (updateEveryFrame)
        {
            UpdateHexCoordinate();
            lastPosition = transform.position;
        }
        else if (updateOnPositionChange)
        {
            // Check if position changed significantly
            float distance = Vector3.Distance(transform.position, lastPosition);
            if (distance >= positionChangeThreshold)
            {
                UpdateHexCoordinate();
                lastPosition = transform.position;
            }
        }
        
        // Update debug info in play mode
        #if UNITY_EDITOR
        if (showDebugInfo && Application.isPlaying)
        {
            UpdateDebugInfo();
        }
        #endif
    }
    
    /// <summary>
    /// Update the hex coordinate based on current world position
    /// </summary>
    public void UpdateHexCoordinate()
    {
        HexSystem hexSystem = GetHexSystem();
        
        // Check if hex system is valid
        if (hexSystem.Scale <= 0)
        {
            Debug.LogWarning("HexCoordinateTracker: Invalid hex system scale! Scale must be greater than 0.");
            return;
        }
        
        HexInt newHexCoordinate = GetHexCoordinateFromPosition(transform.position);
        
        // Always update current coordinate (for debug display)
        // Check if coordinate changed
        if (newHexCoordinate != currentHexCoordinate)
        {
            HexInt oldCoordinate = currentHexCoordinate;
            currentHexCoordinate = newHexCoordinate;
            
            // Debug log
            Debug.Log($"HexCoordinateTracker: Hex coordinate changed from ({oldCoordinate.x}, {oldCoordinate.y}) to ({newHexCoordinate.x}, {newHexCoordinate.y})");
            
            // Trigger event
            OnHexCoordinateChanged?.Invoke(oldCoordinate, newHexCoordinate);
        }
        else
        {
            // Update current coordinate even if it didn't change (for debug display)
            currentHexCoordinate = newHexCoordinate;
        }
    }
    
    /// <summary>
    /// Get hex coordinate from a world position
    /// </summary>
    /// <param name="worldPosition">World position to convert</param>
    /// <returns>Hex coordinate</returns>
    public HexInt GetHexCoordinateFromPosition(Vector3 worldPosition)
    {
        HexSystem hexSystem = GetHexSystem();
        
        // Convert world position to hex coordinate
        Cart cart = new Cart(worldPosition.x, worldPosition.y);
        HexInt hexInt = cart.ToHex(hexSystem.Scale, hexSystem.Orientation);
        
        return hexInt;
    }
    
    /// <summary>
    /// Get the current hex coordinate
    /// </summary>
    /// <returns>Current hex coordinate</returns>
    public HexInt GetCurrentHexCoordinate()
    {
        return currentHexCoordinate;
    }
    
    /// <summary>
    /// Get the hex system being used
    /// </summary>
    /// <returns>Hex system configuration</returns>
    public HexSystem GetHexSystem()
    {
        if (useHexEnvController && hexController != null)
        {
            return hexController.HexSystem;
        }
        return customHexSystem;
    }
    
    /// <summary>
    /// Set the hex system to use
    /// </summary>
    /// <param name="system">Hex system configuration</param>
    public void SetHexSystem(HexSystem system)
    {
        customHexSystem = system;
        useHexEnvController = false;
        UpdateHexCoordinate();
    }
    
    /// <summary>
    /// Enable or disable using HexEnvController
    /// </summary>
    /// <param name="use">True to use HexEnvController, false to use custom system</param>
    public void SetUseHexEnvController(bool use)
    {
        useHexEnvController = use;
        if (use)
        {
            hexController = HexEnvController.Instance;
        }
        UpdateHexCoordinate();
    }
    
    /// <summary>
    /// Get the world position of the center of the current hex tile
    /// </summary>
    /// <returns>World position of hex tile center</returns>
    public Vector3 GetHexTileCenterPosition()
    {
        HexSystem hexSystem = GetHexSystem();
        HexEntity hexEntity = new HexEntity(currentHexCoordinate, hexSystem);
        Cart cart = hexEntity.ToCart();
        return new Vector3(cart.x, cart.y, transform.position.z);
    }
    
    /// <summary>
    /// Check if there's a hex tile at the current coordinate
    /// </summary>
    /// <returns>True if a tile exists at current coordinate</returns>
    public bool HasTileAtCurrentCoordinate()
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        return hexController != null && hexController.TilesDic.ContainsKey(currentHexCoordinate);
    }
    
    /// <summary>
    /// Get the hex tile GameObject at the current coordinate (if it exists)
    /// </summary>
    /// <returns>HexTiles component, or null if no tile exists</returns>
    public HexTiles GetTileAtCurrentCoordinate()
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        if (hexController != null && hexController.TilesDic.TryGetValue(currentHexCoordinate, out HexTiles tile))
        {
            return tile;
        }
        return null;
    }
    
    /// <summary>
    /// Get the distance to another hex coordinate (in hex units)
    /// </summary>
    /// <param name="otherCoordinate">Other hex coordinate</param>
    /// <returns>Distance in hex units</returns>
    public int GetHexDistanceTo(HexInt otherCoordinate)
    {
        // Hex distance is the maximum of the absolute differences of the three cube coordinates
        int dx = Mathf.Abs(currentHexCoordinate.x - otherCoordinate.x);
        int dy = Mathf.Abs(currentHexCoordinate.y - otherCoordinate.y);
        int dz = Mathf.Abs((currentHexCoordinate.x + currentHexCoordinate.y) - (otherCoordinate.x + otherCoordinate.y));
        
        return Mathf.Max(dx, dy, dz);
    }
    
    /// <summary>
    /// Get the ring distance from origin
    /// </summary>
    /// <returns>Ring radius from origin</returns>
    public int GetRingDistance()
    {
        return currentHexCoordinate.r;
    }
    
    // Debug display in Inspector
    #if UNITY_EDITOR
    [Header("Debug - Current Hex Coordinate (Read-Only)")]
    [SerializeField] private int debugHexX = 0;
    [SerializeField] private int debugHexY = 0;
    [SerializeField] private int debugHexZ = 0;
    [SerializeField] private int debugRingDistance = 0;
    [SerializeField] private bool debugHasTile = false;
    
    void OnValidate()
    {
        if (showDebugInfo && Application.isPlaying)
        {
            UpdateDebugInfo();
        }
    }
    
    void UpdateDebugInfo()
    {
        debugHexX = currentHexCoordinate.x;
        debugHexY = currentHexCoordinate.y;
        debugHexZ = currentHexCoordinate.z;
        debugRingDistance = currentHexCoordinate.r;
        debugHasTile = HasTileAtCurrentCoordinate();
    }
    #endif
    
    // Visualize hex coordinate in editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Preview hex coordinate in editor
            HexInt previewHex = GetHexCoordinateFromPosition(transform.position);
            HexSystem hexSystem = GetHexSystem();
            HexEntity hexEntity = new HexEntity(previewHex, hexSystem);
            Cart cart = hexEntity.ToCart();
            Vector3 hexCenter = new Vector3(cart.x, cart.y, transform.position.z);
            
            // Draw line from object to hex center
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, hexCenter);
            
            // Draw hex center
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hexCenter, 0.3f);
        }
        else
        {
            // Draw current hex coordinate center
            Vector3 hexCenter = GetHexTileCenterPosition();
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hexCenter, 0.3f);
            
            // Draw line from object to hex center
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, hexCenter);
        }
    }
}

