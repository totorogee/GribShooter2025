using UnityEngine;
using GameTool.Hex;

/// <summary>
/// Places hex tiles using the HexEnvController system.
/// Supports placing tiles in various patterns (ring, plane, or individual tiles).
/// </summary>
public class HexTilePlacer : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("Hex system configuration for tile placement")]
    [SerializeField] private HexSystem hexSystem = HexSystem.GetDefault();
    
    [Header("Placement Patterns")]
    [Tooltip("Generate a hex plane (filled hexagon) of this size on Start")]
    [SerializeField] private bool generatePlaneOnStart = false;
    [SerializeField] private int planeSize = 5;
    
    [Tooltip("Generate a hex ring of this size on Start")]
    [SerializeField] private bool generateRingOnStart = false;
    [SerializeField] private int ringSize = 3;
    
    [Header("Manual Placement")]
    [Tooltip("Place a single tile at origin (0,0) on Start")]
    [SerializeField] private bool placeSingleTileOnStart = false;
    
    [Header("Tile Prefab")]
    [Tooltip("Optional: Override the prefab used by HexEnvController")]
    [SerializeField] private HexTiles customTilePrefab = null;
    
    private HexEnvController hexController;
    
    void Start()
    {
        // Get or find HexEnvController
        hexController = HexEnvController.Instance;
        
        if (hexController == null)
        {
            Debug.LogError("HexTilePlacer: HexEnvController not found! Make sure it exists in the scene.");
            return;
        }
        
        // Override prefab if custom one is provided
        if (customTilePrefab != null)
        {
            if (hexSystem.Orientation == Orientations.Flat)
            {
                hexController.PrefabsHexagonTilesFlat = customTilePrefab;
            }
            else
            {
                hexController.PrefabsHexagonTilesPointy = customTilePrefab;
            }
        }
        
        // Generate tiles based on settings
        if (generatePlaneOnStart)
        {
            PlaceHexPlane(planeSize);
        }
        else if (generateRingOnStart)
        {
            PlaceHexRing(ringSize);
        }
        else if (placeSingleTileOnStart)
        {
            PlaceTileAtHex(new HexInt(0, 0));
        }
    }
    
    /// <summary>
    /// Place a single hex tile at the specified hex coordinates
    /// </summary>
    /// <param name="hexCoordinates">Hex coordinates where tile should be placed</param>
    public void PlaceTileAtHex(HexInt hexCoordinates)
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        if (hexController != null)
        {
            hexController.GenerateTileByHexInt(hexCoordinates);
        }
    }
    
    /// <summary>
    /// Place a single hex tile at world position (converts to hex coordinates)
    /// </summary>
    /// <param name="worldPosition">World position to place tile</param>
    public void PlaceTileAtWorldPosition(Vector2 worldPosition)
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        if (hexController != null)
        {
            Cart cart = new Cart(worldPosition.x, worldPosition.y);
            HexInt hexInt = cart.ToHex(hexSystem.Scale, hexSystem.Orientation);
            hexController.GenerateTileByHexInt(hexInt);
        }
    }
    
    /// <summary>
    /// Place a hexagonal ring of tiles at the specified distance from center
    /// </summary>
    /// <param name="ringSize">Distance from center (0 = center tile only)</param>
    public void PlaceHexRing(int ringSize)
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        if (hexController != null)
        {
            hexController.GenerateTilesHexRing(ringSize);
        }
    }
    
    /// <summary>
    /// Place a complete hexagonal plane (filled hexagon) of tiles
    /// </summary>
    /// <param name="planeSize">Radius of the hex plane (number of rings from center)</param>
    public void PlaceHexPlane(int planeSize)
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        if (hexController != null)
        {
            hexController.GenerateTilesHexPlane(planeSize);
        }
    }
    
    /// <summary>
    /// Place a circular area of tiles
    /// </summary>
    /// <param name="radius">Radius of the circular area in hex units</param>
    public void PlaceCircularPlane(float radius)
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        if (hexController != null)
        {
            hexController.GenerateTilesCirPlane(radius);
        }
    }
    
    /// <summary>
    /// Get the world position of the center of a hex tile at the given coordinates
    /// </summary>
    /// <param name="hexCoordinates">Hex coordinates</param>
    /// <returns>World position of the tile center</returns>
    public Vector3 GetTileWorldPosition(HexInt hexCoordinates)
    {
        HexEntity hexEntity = new HexEntity(hexCoordinates, hexSystem);
        Cart cart = hexEntity.ToCart();
        return new Vector3(cart.x, cart.y, 0f);
    }
    
    /// <summary>
    /// Get all currently placed hex tiles
    /// </summary>
    /// <returns>Dictionary of hex coordinates to tile GameObjects</returns>
    public System.Collections.Generic.Dictionary<HexInt, HexTiles> GetAllTiles()
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        return hexController != null ? hexController.TilesDic : new System.Collections.Generic.Dictionary<HexInt, HexTiles>();
    }
    
    /// <summary>
    /// Check if a tile exists at the given hex coordinates
    /// </summary>
    /// <param name="hexCoordinates">Hex coordinates to check</param>
    /// <returns>True if tile exists at these coordinates</returns>
    public bool HasTileAt(HexInt hexCoordinates)
    {
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
        }
        
        return hexController != null && hexController.TilesDic.ContainsKey(hexCoordinates);
    }
    
    /// <summary>
    /// Get a random hex coordinate from all placed tiles
    /// </summary>
    /// <returns>Random hex coordinates, or (0,0) if no tiles exist</returns>
    public HexInt GetRandomTileCoordinates()
    {
        var tiles = GetAllTiles();
        if (tiles.Count == 0)
        {
            return new HexInt(0, 0);
        }
        
        var keys = new System.Collections.Generic.List<HexInt>(tiles.Keys);
        return keys[Random.Range(0, keys.Count)];
    }
}

