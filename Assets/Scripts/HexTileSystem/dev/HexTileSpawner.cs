using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTool.Hex;

/// <summary>
/// Spawns objects on hex tiles at random positions every few seconds.
/// Objects are spawned in the middle of tiles and positioned on top of them.
/// </summary>
public class HexTileSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab to spawn on hex tiles")]
    [SerializeField] private GameObject spawnPrefab;
    
    [Tooltip("Time interval between spawns (in seconds)")]
    [SerializeField] private float spawnInterval = 2f;
    
    [Tooltip("Offset above the tile (Y axis)")]
    [SerializeField] private float heightOffset = 0.5f;
    
    [Header("Spawn Behavior")]
    [Tooltip("Start spawning automatically on Start")]
    [SerializeField] private bool spawnOnStart = true;
    
    [Tooltip("Only spawn on tiles that don't already have an object")]
    [SerializeField] private bool avoidOccupiedTiles = true;
    
    [Tooltip("Maximum number of objects to spawn (0 = unlimited)")]
    [SerializeField] private int maxSpawnedObjects = 0;
    
    [Header("Tile Selection")]
    [Tooltip("Only spawn on tiles within this distance from origin (0 = all tiles)")]
    [SerializeField] private int maxTileDistance = 0;
    
    [Tooltip("Randomize spawn interval (spawnInterval ± randomRange)")]
    [SerializeField] private bool useRandomInterval = false;
    [SerializeField] private float randomIntervalRange = 0.5f;
    
    private HexTilePlacer tilePlacer;
    private SpawnedObjectTracker objectTracker;
    private HexEnvController hexController;
    private Coroutine spawnCoroutine;
    private int currentSpawnedCount = 0;
    
    void Start()
    {
        // Get or find required components
        tilePlacer = FindFirstObjectByType<HexTilePlacer>();
        if (tilePlacer == null)
        {
            Debug.LogWarning("HexTileSpawner: HexTilePlacer not found. Spawning may not work correctly.");
        }
        
        // Get HexEnvController directly to access actual tiles
        hexController = HexEnvController.Instance;
        if (hexController == null)
        {
            Debug.LogError("HexTileSpawner: HexEnvController not found! Make sure it exists in the scene.");
        }
        
        objectTracker = FindFirstObjectByType<SpawnedObjectTracker>();
        if (objectTracker == null)
        {
            Debug.LogWarning("HexTileSpawner: SpawnedObjectTracker not found. Object locations won't be tracked.");
        }
        
        if (spawnOnStart)
        {
            // Wait a frame to ensure tiles are placed first
            StartCoroutine(DelayedStartSpawning());
        }
    }
    
    /// <summary>
    /// Wait a frame before starting to spawn to ensure tiles are placed
    /// </summary>
    private IEnumerator DelayedStartSpawning()
    {
        yield return null; // Wait one frame
        StartSpawning();
    }
    
    /// <summary>
    /// Start the spawning coroutine
    /// </summary>
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }
    
    /// <summary>
    /// Stop the spawning coroutine
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// Coroutine that spawns objects at intervals
    /// </summary>
    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            // Check if we've reached max spawn count
            if (maxSpawnedObjects > 0 && currentSpawnedCount >= maxSpawnedObjects)
            {
                yield break; // Stop spawning
            }
            
            // Spawn an object
            SpawnObjectOnRandomTile();
            
            // Wait for next spawn
            float waitTime = spawnInterval;
            if (useRandomInterval)
            {
                waitTime += Random.Range(-randomIntervalRange, randomIntervalRange);
                waitTime = Mathf.Max(0.1f, waitTime); // Ensure positive wait time
            }
            
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    /// <summary>
    /// Spawn an object on a random hex tile
    /// </summary>
    public void SpawnObjectOnRandomTile()
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning("HexTileSpawner: Spawn prefab not assigned!");
            return;
        }
        
        // Get HexEnvController directly to access actual tiles
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
            if (hexController == null)
            {
                Debug.LogError("HexTileSpawner: Cannot spawn - HexEnvController not found!");
                return;
            }
        }
        
        // Get all existing tiles from HexEnvController (the source of truth)
        var tiles = hexController.TilesDic;
        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogWarning("HexTileSpawner: No tiles available to spawn on!");
            return;
        }
        
        // Filter tiles based on criteria (only existing tiles)
        List<HexInt> availableTiles = GetAvailableTiles(tiles);
        
        if (availableTiles.Count == 0)
        {
            Debug.LogWarning("HexTileSpawner: No available tiles matching criteria!");
            return;
        }
        
        // Select random tile
        HexInt selectedTile = availableTiles[Random.Range(0, availableTiles.Count)];
        
        // Verify tile still exists before spawning
        if (!tiles.ContainsKey(selectedTile))
        {
            Debug.LogWarning($"HexTileSpawner: Selected tile ({selectedTile.x}, {selectedTile.y}) no longer exists!");
            return;
        }
        
        // Spawn object at tile center
        SpawnObjectAtTile(selectedTile);
    }
    
    /// <summary>
    /// Get list of available tiles based on spawn criteria
    /// </summary>
    private List<HexInt> GetAvailableTiles(Dictionary<HexInt, HexTiles> allTiles)
    {
        List<HexInt> available = new List<HexInt>();
        
        foreach (var kvp in allTiles)
        {
            HexInt hexCoord = kvp.Key;
            
            // Check distance filter
            if (maxTileDistance > 0)
            {
                int distance = hexCoord.r; // Ring distance from origin
                if (distance > maxTileDistance)
                {
                    continue;
                }
            }
            
            // Check if tile is occupied
            if (avoidOccupiedTiles && objectTracker != null)
            {
                if (objectTracker.HasObjectAtTile(hexCoord))
                {
                    continue;
                }
            }
            
            available.Add(hexCoord);
        }
        
        return available;
    }
    
    /// <summary>
    /// Spawn an object at a specific hex tile
    /// </summary>
    /// <param name="hexCoordinates">Hex coordinates of the tile</param>
    public void SpawnObjectAtTile(HexInt hexCoordinates)
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning("HexTileSpawner: Spawn prefab not assigned!");
            return;
        }
        
        // Get HexEnvController to verify tile exists
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
            if (hexController == null)
            {
                Debug.LogError("HexTileSpawner: Cannot spawn - HexEnvController not found!");
                return;
            }
        }
        
        // Verify tile actually exists in HexEnvController
        if (!hexController.TilesDic.ContainsKey(hexCoordinates))
        {
            Debug.LogWarning($"HexTileSpawner: Cannot spawn - Tile at ({hexCoordinates.x}, {hexCoordinates.y}) does not exist!");
            return;
        }
        
        // Get the actual tile GameObject to use its position
        HexTiles tile = hexController.TilesDic[hexCoordinates];
        if (tile == null)
        {
            Debug.LogWarning($"HexTileSpawner: Tile GameObject at ({hexCoordinates.x}, {hexCoordinates.y}) is null!");
            return;
        }
        
        // Get world position from the actual tile GameObject
        Vector3 tilePosition = tile.transform.position;
        
        // Add height offset
        Vector3 spawnPosition = tilePosition + new Vector3(0f, heightOffset, 0f);
        
        // Instantiate object
        GameObject spawnedObject = Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
        
        // Register with tracker if available
        if (objectTracker != null)
        {
            objectTracker.RegisterObject(spawnedObject, hexCoordinates);
        }
        
        currentSpawnedCount++;
    }
    
    /// <summary>
    /// Spawn an object at a specific world position (converts to hex coordinates)
    /// Only spawns if a tile exists at that position
    /// </summary>
    /// <param name="worldPosition">World position to spawn at</param>
    public void SpawnObjectAtWorldPosition(Vector2 worldPosition)
    {
        // Get HexEnvController to verify tile exists
        if (hexController == null)
        {
            hexController = HexEnvController.Instance;
            if (hexController == null)
            {
                Debug.LogError("HexTileSpawner: Cannot spawn - HexEnvController not found!");
                return;
            }
        }
        
        // Convert world position to hex coordinates
        Cart cart = new Cart(worldPosition.x, worldPosition.y);
        HexInt hexInt = cart.ToHex(hexController.HexSystem.Scale, hexController.HexSystem.Orientation);
        
        // Only spawn if tile exists
        if (hexController.TilesDic.ContainsKey(hexInt))
        {
            SpawnObjectAtTile(hexInt);
        }
        else
        {
            Debug.LogWarning($"HexTileSpawner: Cannot spawn at world position ({worldPosition.x}, {worldPosition.y}) - No tile exists at hex ({hexInt.x}, {hexInt.y})");
        }
    }
    
    /// <summary>
    /// Set the spawn prefab at runtime
    /// </summary>
    public void SetSpawnPrefab(GameObject prefab)
    {
        spawnPrefab = prefab;
    }
    
    /// <summary>
    /// Set the spawn interval at runtime
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
    }
    
    /// <summary>
    /// Get current number of spawned objects
    /// </summary>
    public int GetSpawnedCount()
    {
        return currentSpawnedCount;
    }
    
    void OnDestroy()
    {
        StopSpawning();
    }
}


