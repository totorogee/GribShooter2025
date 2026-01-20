using System.Collections.Generic;
using UnityEngine;
using GameTool.Hex;

/// <summary>
/// Tracks and remembers the locations of objects spawned on hex tiles.
/// Maintains a mapping between GameObjects and their hex tile coordinates.
/// </summary>
public class SpawnedObjectTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("Automatically track objects when they're registered")]
    [SerializeField] private bool autoTrack = true;
    
    [Tooltip("Remove tracking when objects are destroyed")]
    [SerializeField] private bool removeOnDestroy = true;
    
    [Header("Debug Info")]
    [Tooltip("Show tracked objects in Inspector (read-only)")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Dictionary mapping hex coordinates to spawned objects
    private Dictionary<HexInt, GameObject> tileToObject = new Dictionary<HexInt, GameObject>();
    
    // Dictionary mapping objects to their hex coordinates
    private Dictionary<GameObject, HexInt> objectToTile = new Dictionary<GameObject, HexInt>();
    
    // List of all tracked objects (for easy iteration)
    private List<GameObject> trackedObjects = new List<GameObject>();
    
    void Update()
    {
        // Clean up destroyed objects
        if (removeOnDestroy)
        {
            CleanupDestroyedObjects();
        }
    }
    
    /// <summary>
    /// Register an object at a specific hex tile location
    /// </summary>
    /// <param name="obj">GameObject to track</param>
    /// <param name="hexCoordinates">Hex coordinates of the tile</param>
    public void RegisterObject(GameObject obj, HexInt hexCoordinates)
    {
        if (obj == null)
        {
            Debug.LogWarning("SpawnedObjectTracker: Cannot register null object!");
            return;
        }
        
        // Remove old registration if object was already tracked
        if (objectToTile.ContainsKey(obj))
        {
            UnregisterObject(obj);
        }
        
        // Remove old object if tile was already occupied
        if (tileToObject.ContainsKey(hexCoordinates))
        {
            GameObject oldObj = tileToObject[hexCoordinates];
            if (oldObj != null)
            {
                UnregisterObject(oldObj);
            }
        }
        
        // Register new object
        tileToObject[hexCoordinates] = obj;
        objectToTile[obj] = hexCoordinates;
        trackedObjects.Add(obj);
        
        Debug.Log($"SpawnedObjectTracker: Registered {obj.name} at hex ({hexCoordinates.x}, {hexCoordinates.y})");
    }
    
    /// <summary>
    /// Unregister an object from tracking
    /// </summary>
    /// <param name="obj">GameObject to unregister</param>
    public void UnregisterObject(GameObject obj)
    {
        if (obj == null || !objectToTile.ContainsKey(obj))
        {
            return;
        }
        
        HexInt hexCoord = objectToTile[obj];
        tileToObject.Remove(hexCoord);
        objectToTile.Remove(obj);
        trackedObjects.Remove(obj);
        
        Debug.Log($"SpawnedObjectTracker: Unregistered {obj.name} from hex ({hexCoord.x}, {hexCoord.y})");
    }
    
    /// <summary>
    /// Get the hex coordinates of a tracked object
    /// </summary>
    /// <param name="obj">GameObject to look up</param>
    /// <returns>Hex coordinates, or (0,0) if not found</returns>
    public HexInt GetObjectTile(GameObject obj)
    {
        if (obj != null && objectToTile.TryGetValue(obj, out HexInt hexCoord))
        {
            return hexCoord;
        }
        return new HexInt(0, 0);
    }
    
    /// <summary>
    /// Get the object at a specific hex tile
    /// </summary>
    /// <param name="hexCoordinates">Hex coordinates to check</param>
    /// <returns>GameObject at that tile, or null if none</returns>
    public GameObject GetObjectAtTile(HexInt hexCoordinates)
    {
        if (tileToObject.TryGetValue(hexCoordinates, out GameObject obj))
        {
            // Check if object still exists
            if (obj != null)
            {
                return obj;
            }
            else
            {
                // Clean up destroyed object
                tileToObject.Remove(hexCoordinates);
            }
        }
        return null;
    }
    
    /// <summary>
    /// Check if there's an object at the given hex tile
    /// </summary>
    /// <param name="hexCoordinates">Hex coordinates to check</param>
    /// <returns>True if an object exists at that tile</returns>
    public bool HasObjectAtTile(HexInt hexCoordinates)
    {
        if (tileToObject.TryGetValue(hexCoordinates, out GameObject obj))
        {
            // Check if object still exists
            if (obj != null)
            {
                return true;
            }
            else
            {
                // Clean up destroyed object
                tileToObject.Remove(hexCoordinates);
            }
        }
        return false;
    }
    
    /// <summary>
    /// Check if an object is being tracked
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if object is tracked</returns>
    public bool IsTrackingObject(GameObject obj)
    {
        return obj != null && objectToTile.ContainsKey(obj);
    }
    
    /// <summary>
    /// Get all tracked objects
    /// </summary>
    /// <returns>List of all tracked GameObjects</returns>
    public List<GameObject> GetAllTrackedObjects()
    {
        CleanupDestroyedObjects();
        return new List<GameObject>(trackedObjects);
    }
    
    /// <summary>
    /// Get all hex coordinates that have objects
    /// </summary>
    /// <returns>List of hex coordinates with objects</returns>
    public List<HexInt> GetAllOccupiedTiles()
    {
        CleanupDestroyedObjects();
        return new List<HexInt>(tileToObject.Keys);
    }
    
    /// <summary>
    /// Get the count of tracked objects
    /// </summary>
    /// <returns>Number of tracked objects</returns>
    public int GetTrackedObjectCount()
    {
        CleanupDestroyedObjects();
        return trackedObjects.Count;
    }
    
    /// <summary>
    /// Clear all tracking data
    /// </summary>
    public void ClearAll()
    {
        tileToObject.Clear();
        objectToTile.Clear();
        trackedObjects.Clear();
        Debug.Log("SpawnedObjectTracker: Cleared all tracking data");
    }
    
    /// <summary>
    /// Remove tracking for all destroyed objects
    /// </summary>
    private void CleanupDestroyedObjects()
    {
        // Create list of objects to remove (can't modify dictionary while iterating)
        List<GameObject> toRemove = new List<GameObject>();
        
        foreach (var obj in trackedObjects)
        {
            if (obj == null)
            {
                toRemove.Add(obj);
            }
        }
        
        // Remove destroyed objects
        foreach (var obj in toRemove)
        {
            if (objectToTile.TryGetValue(obj, out HexInt hexCoord))
            {
                tileToObject.Remove(hexCoord);
                objectToTile.Remove(obj);
            }
            trackedObjects.Remove(obj);
        }
    }
    
    /// <summary>
    /// Get a dictionary of all tile-to-object mappings
    /// </summary>
    /// <returns>Dictionary mapping hex coordinates to GameObjects</returns>
    public Dictionary<HexInt, GameObject> GetAllTileMappings()
    {
        CleanupDestroyedObjects();
        return new Dictionary<HexInt, GameObject>(tileToObject);
    }
    
    /// <summary>
    /// Get a dictionary of all object-to-tile mappings
    /// </summary>
    /// <returns>Dictionary mapping GameObjects to hex coordinates</returns>
    public Dictionary<GameObject, HexInt> GetAllObjectMappings()
    {
        CleanupDestroyedObjects();
        return new Dictionary<GameObject, HexInt>(objectToTile);
    }
    
    // Debug display in Inspector
    #if UNITY_EDITOR
    [Header("Debug - Tracked Objects (Read-Only)")]
    [SerializeField] private List<TrackedObjectInfo> debugTrackedObjects = new List<TrackedObjectInfo>();
    
    [System.Serializable]
    private class TrackedObjectInfo
    {
        public string objectName;
        public int hexX;
        public int hexY;
    }
    
    void OnValidate()
    {
        if (showDebugInfo && Application.isPlaying)
        {
            UpdateDebugInfo();
        }
    }
    
    void UpdateDebugInfo()
    {
        debugTrackedObjects.Clear();
        CleanupDestroyedObjects();
        
        foreach (var kvp in tileToObject)
        {
            if (kvp.Value != null)
            {
                debugTrackedObjects.Add(new TrackedObjectInfo
                {
                    objectName = kvp.Value.name,
                    hexX = kvp.Key.x,
                    hexY = kvp.Key.y
                });
            }
        }
    }
    #endif
}

