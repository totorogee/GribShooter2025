using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns child objects in a ring pattern around this GameObject.
/// Tracks all spawned children and maintains their ring positions.
/// </summary>
public class RingSpawner : MonoBehaviour
{
    [System.Serializable]
    public enum SpawnDirection
    {
        Clockwise,
        Anticlockwise,
        Random
    }
    
    [System.Serializable]
    public enum RotationMode
    {
        KeepOriginal,      // Keep prefab's original rotation
        FaceSpawner,       // Face toward spawner center
        FaceOutward        // Face outward from spawner center
    }
    
    [System.Serializable]
    public enum SpawnTiming
    {
        AllAtStart,        // Spawn all children immediately
        OneByOne           // Spawn one child every few seconds
    }
    
    [Header("Spawn Settings")]
    [Tooltip("Prefab to spawn as child objects")]
    [SerializeField] private GameObject spawnPrefab;
    
    [Tooltip("Number of objects to spawn in the ring")]
    [SerializeField] private int spawnCount = 8;
    
    [Header("Ring Configuration")]
    [Tooltip("Distance from spawner center to child objects (radius)")]
    [SerializeField] private float spawnDistance = 5f;
    
    [Tooltip("Angular spacing between children in degrees")]
    [SerializeField] private float angularSpacing = 45f;
    
    [Tooltip("Starting angle in degrees (0 = right, 90 = up, 180 = left, 270 = down)")]
    [SerializeField] private float startingAngle = 0f;
    
    [Header("Spawn Direction")]
    [Tooltip("Direction to spawn children around the ring")]
    [SerializeField] private SpawnDirection spawnDirection = SpawnDirection.Clockwise;
    
    [Header("Spawn Behavior")]
    [Tooltip("Spawn children automatically on Start")]
    [SerializeField] private bool spawnOnStart = true;
    
    [Tooltip("Clear existing children before spawning")]
    [SerializeField] private bool clearExistingOnSpawn = false;
    
    [Tooltip("How to spawn children")]
    [SerializeField] private SpawnTiming spawnTiming = SpawnTiming.AllAtStart;
    
    [Tooltip("Time between each spawn when using OneByOne mode (in seconds)")]
    [SerializeField] private float spawnInterval = 1f;
    
    [Header("Child Management")]
    [Tooltip("Parent spawned objects to this GameObject")]
    [SerializeField] private bool parentToSpawner = true;
    
    [Tooltip("Track spawned children (enables query methods)")]
    [SerializeField] private bool trackChildren = true;
    
    [Tooltip("Maximum number of children allowed (0 = unlimited)")]
    [SerializeField] private int maxChildCount = 0;
    
    [Tooltip("Auto-respawn when children are deleted and count is below max")]
    [SerializeField] private bool autoRespawnOnDelete = true;
    
    [Header("Rotation Settings")]
    [Tooltip("How to handle child rotation")]
    [SerializeField] private RotationMode rotationMode = RotationMode.FaceOutward;
    
    // Tracked children and their positions
    private List<GameObject> spawnedChildren = new List<GameObject>();
    private Dictionary<GameObject, float> childAngles = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, int> childIndices = new Dictionary<GameObject, int>();
    private Coroutine spawnCoroutine;
    private List<float> pendingSpawnAngles = new List<float>();
    private int currentSpawnIndex = 0;
    private bool isRespawning = false; // Prevent multiple respawns in same frame
    
    void Start()
    {
        if (spawnOnStart)
        {
            if (spawnTiming == SpawnTiming.AllAtStart)
            {
                SpawnRing();
            }
            else
            {
                StartSpawnSequence();
            }
        }
    }
    
    void Update()
    {
        // Clean up null references
        CleanupNullChildren();
        
        // Check for auto-respawn if enabled (prevent multiple checks in same frame)
        if (autoRespawnOnDelete && !isRespawning)
        {
            isRespawning = true;
            CheckAndRespawn();
            isRespawning = false;
        }
    }
    
    /// <summary>
    /// Spawn all children in a ring pattern
    /// </summary>
    public void SpawnRing()
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning("RingSpawner: Spawn prefab not assigned!");
            return;
        }
        
        // Clear existing if requested
        if (clearExistingOnSpawn)
        {
            ClearAllChildren();
        }
        
        // Check max child count
        int currentCount = GetChildCount();
        if (maxChildCount > 0 && currentCount >= maxChildCount)
        {
            Debug.LogWarning($"RingSpawner: Max child count ({maxChildCount}) reached!");
            return;
        }
        
        // Calculate angles for all children
        List<float> angles = CalculateSpawnAngles();
        
        // Limit spawn count based on max
        int remainingSlots = maxChildCount > 0 ? maxChildCount - currentCount : angles.Count;
        int spawnAmount = Mathf.Min(angles.Count, remainingSlots);
        
        // Spawn each child at its calculated angle
        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnChildAtAngle(angles[i], i);
        }
    }
    
    /// <summary>
    /// Start spawning children one by one
    /// </summary>
    public void StartSpawnSequence()
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning("RingSpawner: Spawn prefab not assigned!");
            return;
        }
        
        // Clear existing if requested
        if (clearExistingOnSpawn)
        {
            ClearAllChildren();
        }
        
        // Calculate angles for all children
        pendingSpawnAngles = CalculateSpawnAngles();
        currentSpawnIndex = 0;
        
        // Start coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnOneByOneCoroutine());
    }
    
    /// <summary>
    /// Coroutine to spawn children one by one
    /// </summary>
    private IEnumerator SpawnOneByOneCoroutine()
    {
        while (currentSpawnIndex < pendingSpawnAngles.Count)
        {
            // Check max child count
            int currentCount = GetChildCount();
            if (maxChildCount > 0 && currentCount >= maxChildCount)
            {
                yield break; // Stop spawning
            }
            
            // Spawn one child
            SpawnChildAtAngle(pendingSpawnAngles[currentSpawnIndex], currentSpawnIndex);
            currentSpawnIndex++;
            
            // Wait for next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// Calculate all spawn angles based on direction and spacing
    /// </summary>
    private List<float> CalculateSpawnAngles()
    {
        List<float> angles = new List<float>();
        
        // Calculate base angles
        for (int i = 0; i < spawnCount; i++)
        {
            float angle;
            
            if (spawnDirection == SpawnDirection.Clockwise)
            {
                // Clockwise: starting angle decreases
                angle = startingAngle - (i * angularSpacing);
            }
            else if (spawnDirection == SpawnDirection.Anticlockwise)
            {
                // Anticlockwise: starting angle increases
                angle = startingAngle + (i * angularSpacing);
            }
            else // Random
            {
                // Random: shuffle the order but keep spacing
                angle = startingAngle + (i * angularSpacing);
            }
            
            // Normalize angle to 0-360 range
            angle = NormalizeAngle(angle);
            angles.Add(angle);
        }
        
        // Shuffle if random direction
        if (spawnDirection == SpawnDirection.Random)
        {
            // Shuffle the list
            for (int i = 0; i < angles.Count; i++)
            {
                float temp = angles[i];
                int randomIndex = Random.Range(i, angles.Count);
                angles[i] = angles[randomIndex];
                angles[randomIndex] = temp;
            }
        }
        
        return angles;
    }
    
    /// <summary>
    /// Spawn a single child at a specific angle
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <param name="index">Index in the ring (0-based)</param>
    public void SpawnChildAtAngle(float angle, int index)
    {
        if (spawnPrefab == null)
        {
            Debug.LogWarning("RingSpawner: Spawn prefab not assigned!");
            return;
        }
        
        // Check max child count
        int currentCount = GetChildCount();
        if (maxChildCount > 0 && currentCount >= maxChildCount)
        {
            return; // Don't spawn if at max
        }
        
        // Calculate position based on angle and distance
        Vector3 position = CalculatePositionFromAngle(angle);
        
        // Instantiate child with prefab's rotation
        Quaternion initialRotation = spawnPrefab.transform.rotation;
        GameObject child = Instantiate(spawnPrefab, position, initialRotation);
        
        // Parent to spawner if requested
        if (parentToSpawner)
        {
            child.transform.SetParent(transform);
        }
        
        // Apply rotation based on mode
        ApplyRotation(child, angle);
        
        // Track child if enabled
        if (trackChildren)
        {
            spawnedChildren.Add(child);
            childAngles[child] = angle;
            childIndices[child] = index;
        }
    }
    
    /// <summary>
    /// Apply rotation to child based on rotation mode
    /// </summary>
    private void ApplyRotation(GameObject child, float angle)
    {
        if (rotationMode == RotationMode.KeepOriginal)
        {
            // Keep prefab's original rotation (already set during instantiate)
            return;
        }
        else if (rotationMode == RotationMode.FaceSpawner)
        {
            // Face toward spawner center
            Vector3 directionToSpawner = (transform.position - child.transform.position).normalized;
            if (directionToSpawner != Vector3.zero)
            {
                float rotationAngle = Mathf.Atan2(directionToSpawner.y, directionToSpawner.x) * Mathf.Rad2Deg;
                child.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
            }
        }
        else if (rotationMode == RotationMode.FaceOutward)
        {
            // Face outward from spawner center
            Vector3 directionFromSpawner = (child.transform.position - transform.position).normalized;
            if (directionFromSpawner != Vector3.zero)
            {
                float rotationAngle = Mathf.Atan2(directionFromSpawner.y, directionFromSpawner.x) * Mathf.Rad2Deg;
                child.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);
            }
        }
    }
    
    /// <summary>
    /// Calculate world position from angle and distance
    /// </summary>
    private Vector3 CalculatePositionFromAngle(float angle)
    {
        // Convert angle to radians
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate position using trigonometry
        float x = transform.position.x + spawnDistance * Mathf.Cos(angleRad);
        float y = transform.position.y + spawnDistance * Mathf.Sin(angleRad);
        
        return new Vector3(x, y, transform.position.z);
    }
    
    /// <summary>
    /// Normalize angle to 0-360 range
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0f)
        {
            angle += 360f;
        }
        return angle;
    }
    
    /// <summary>
    /// Clear all spawned children
    /// </summary>
    public void ClearAllChildren()
    {
        // Destroy all tracked children
        foreach (GameObject child in spawnedChildren)
        {
            if (child != null)
            {
                Destroy(child);
            }
        }
        
        // Clear tracking data
        spawnedChildren.Clear();
        childAngles.Clear();
        childIndices.Clear();
    }
    
    /// <summary>
    /// Remove a specific child from tracking and destroy it
    /// </summary>
    /// <param name="child">Child GameObject to remove</param>
    public void RemoveChild(GameObject child)
    {
        if (child == null) return;
        
        if (spawnedChildren.Contains(child))
        {
            spawnedChildren.Remove(child);
            childAngles.Remove(child);
            childIndices.Remove(child);
            Destroy(child);
            
            // Auto-respawn will be handled in Update if enabled
        }
    }
    
    /// <summary>
    /// Stop the one-by-one spawn sequence
    /// </summary>
    public void StopSpawnSequence()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// Update positions of all children (useful if spawnDistance or startingAngle changes)
    /// </summary>
    public void UpdateChildPositions()
    {
        foreach (var kvp in childAngles)
        {
            GameObject child = kvp.Key;
            float angle = kvp.Value;
            
            if (child != null)
            {
                Vector3 newPosition = CalculatePositionFromAngle(angle);
                child.transform.position = newPosition;
                
                // Update rotation based on current mode
                ApplyRotation(child, angle);
            }
        }
    }
    
    /// <summary>
    /// Clean up null children from tracking
    /// </summary>
    private void CleanupNullChildren()
    {
        // Remove null references from list
        for (int i = spawnedChildren.Count - 1; i >= 0; i--)
        {
            GameObject child = spawnedChildren[i];
            if (child == null)
            {
                spawnedChildren.RemoveAt(i);
            }
        }
        
        // Clean up dictionaries - remove entries where key is null or not in spawnedChildren list
        List<GameObject> keysToRemove = new List<GameObject>();
        foreach (var kvp in childAngles)
        {
            GameObject key = kvp.Key;
            if (key == null || !spawnedChildren.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }
        foreach (GameObject key in keysToRemove)
        {
            childAngles.Remove(key);
            childIndices.Remove(key);
        }
    }
    
    /// <summary>
    /// Check if we need to respawn children after deletion
    /// </summary>
    private void CheckAndRespawn()
    {
        int currentCount = GetChildCount();
        
        // Check if we need to respawn to maintain spawnCount
        if (currentCount < spawnCount)
        {
            // Calculate how many we need to spawn
            int needed = spawnCount - currentCount;
            
            // Limit by maxChildCount if set
            if (maxChildCount > 0)
            {
                needed = Mathf.Min(needed, maxChildCount - currentCount);
            }
            
            if (needed > 0)
            {
                // Get available angles
                List<float> allAngles = CalculateSpawnAngles();
                
                // Find angles that aren't currently used
                List<float> usedAngles = new List<float>();
                foreach (var kvp in childAngles)
                {
                    if (kvp.Key != null)
                    {
                        usedAngles.Add(kvp.Value);
                    }
                }
                
                // Find unused angles
                List<float> availableAngles = new List<float>();
                foreach (float angle in allAngles)
                {
                    bool isUsed = false;
                    foreach (float usedAngle in usedAngles)
                    {
                        if (Mathf.Abs(Mathf.DeltaAngle(angle, usedAngle)) < 1f)
                        {
                            isUsed = true;
                            break;
                        }
                    }
                    if (!isUsed)
                    {
                        availableAngles.Add(angle);
                    }
                }
                
                // Spawn needed children based on timing setting
                if (spawnTiming == SpawnTiming.AllAtStart)
                {
                    // Spawn all at once
                    for (int i = 0; i < needed && i < availableAngles.Count; i++)
                    {
                        int nextIndex = GetNextAvailableIndex();
                        SpawnChildAtAngle(availableAngles[i], nextIndex);
                    }
                }
                else
                {
                    // Spawn one at a time - add to pending list and start coroutine if not running
                    if (spawnCoroutine == null)
                    {
                        // Add available angles to pending list
                        foreach (float angle in availableAngles)
                        {
                            if (!pendingSpawnAngles.Contains(angle))
                            {
                                pendingSpawnAngles.Add(angle);
                            }
                        }
                        // Start coroutine if we have pending spawns
                        if (pendingSpawnAngles.Count > 0)
                        {
                            spawnCoroutine = StartCoroutine(SpawnOneByOneCoroutine());
                        }
                    }
                    else
                    {
                        // Coroutine is already running, just add to pending list
                        foreach (float angle in availableAngles)
                        {
                            if (!pendingSpawnAngles.Contains(angle))
                            {
                                pendingSpawnAngles.Add(angle);
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get next available index for spawning
    /// </summary>
    private int GetNextAvailableIndex()
    {
        int maxIndex = -1;
        foreach (var kvp in childIndices)
        {
            if (kvp.Key != null && kvp.Value > maxIndex)
            {
                maxIndex = kvp.Value;
            }
        }
        return maxIndex + 1;
    }
    
    /// <summary>
    /// Get all spawned children
    /// </summary>
    /// <returns>List of all child GameObjects</returns>
    public List<GameObject> GetAllChildren()
    {
        // Clean up null references
        spawnedChildren.RemoveAll(child => child == null);
        return new List<GameObject>(spawnedChildren);
    }
    
    /// <summary>
    /// Get the angle of a specific child
    /// </summary>
    /// <param name="child">Child GameObject</param>
    /// <returns>Angle in degrees, or 0 if not found</returns>
    public float GetChildAngle(GameObject child)
    {
        if (child != null && childAngles.TryGetValue(child, out float angle))
        {
            return angle;
        }
        return 0f;
    }
    
    /// <summary>
    /// Get the index of a specific child in the ring
    /// </summary>
    /// <param name="child">Child GameObject</param>
    /// <returns>Index (0-based), or -1 if not found</returns>
    public int GetChildIndex(GameObject child)
    {
        if (child != null && childIndices.TryGetValue(child, out int index))
        {
            return index;
        }
        return -1;
    }
    
    /// <summary>
    /// Get the number of spawned children
    /// </summary>
    /// <returns>Count of spawned children</returns>
    public int GetChildCount()
    {
        spawnedChildren.RemoveAll(child => child == null);
        return spawnedChildren.Count;
    }
    
    /// <summary>
    /// Set the spawn distance and update positions
    /// </summary>
    public void SetSpawnDistance(float distance)
    {
        spawnDistance = distance;
        UpdateChildPositions();
    }
    
    /// <summary>
    /// Set the starting angle and respawn
    /// </summary>
    public void SetStartingAngle(float angle)
    {
        startingAngle = angle;
        if (spawnedChildren.Count > 0)
        {
            ClearAllChildren();
            if (spawnTiming == SpawnTiming.AllAtStart)
            {
                SpawnRing();
            }
            else
            {
                StartSpawnSequence();
            }
        }
    }
    
    /// <summary>
    /// Set the angular spacing and respawn
    /// </summary>
    public void SetAngularSpacing(float spacing)
    {
        angularSpacing = spacing;
        if (spawnedChildren.Count > 0)
        {
            ClearAllChildren();
            if (spawnTiming == SpawnTiming.AllAtStart)
            {
                SpawnRing();
            }
            else
            {
                StartSpawnSequence();
            }
        }
    }
    
    /// <summary>
    /// Set the spawn count and respawn
    /// </summary>
    public void SetSpawnCount(int count)
    {
        spawnCount = Mathf.Max(0, count);
        if (spawnedChildren.Count > 0)
        {
            ClearAllChildren();
            if (spawnTiming == SpawnTiming.AllAtStart)
            {
                SpawnRing();
            }
            else
            {
                StartSpawnSequence();
            }
        }
    }
    
    /// <summary>
    /// Set the spawn direction and respawn
    /// </summary>
    public void SetSpawnDirection(SpawnDirection direction)
    {
        spawnDirection = direction;
        if (spawnedChildren.Count > 0)
        {
            ClearAllChildren();
            if (spawnTiming == SpawnTiming.AllAtStart)
            {
                SpawnRing();
            }
            else
            {
                StartSpawnSequence();
            }
        }
    }
    
    /// <summary>
    /// Set the rotation mode and update all children
    /// </summary>
    public void SetRotationMode(RotationMode mode)
    {
        rotationMode = mode;
        UpdateChildPositions();
    }
    
    /// <summary>
    /// Set the max child count
    /// </summary>
    public void SetMaxChildCount(int max)
    {
        maxChildCount = Mathf.Max(0, max);
        
        // Remove excess children if over limit
        if (maxChildCount > 0)
        {
            int currentCount = GetChildCount();
            if (currentCount > maxChildCount)
            {
                int toRemove = currentCount - maxChildCount;
                for (int i = 0; i < toRemove && spawnedChildren.Count > 0; i++)
                {
                    GameObject child = spawnedChildren[spawnedChildren.Count - 1];
                    RemoveChild(child);
                }
            }
        }
    }
    
    /// <summary>
    /// Set the spawn timing mode
    /// </summary>
    public void SetSpawnTiming(SpawnTiming timing)
    {
        spawnTiming = timing;
    }
    
    /// <summary>
    /// Set the spawn interval for one-by-one mode
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
    }
    
    /// <summary>
    /// Enable or disable auto-respawn on delete
    /// </summary>
    public void SetAutoRespawnOnDelete(bool enabled)
    {
        autoRespawnOnDelete = enabled;
    }
    
    void OnDestroy()
    {
        StopSpawnSequence();
    }
    
    // Visualize ring in editor
    void OnDrawGizmosSelected()
    {
        // Draw spawner center
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw ring circle
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        int segments = 64;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 pos1 = center + new Vector3(
                spawnDistance * Mathf.Cos(angle1),
                spawnDistance * Mathf.Sin(angle1),
                0f
            );
            Vector3 pos2 = center + new Vector3(
                spawnDistance * Mathf.Cos(angle2),
                spawnDistance * Mathf.Sin(angle2),
                0f
            );
            
            Gizmos.DrawLine(pos1, pos2);
        }
        
        // Draw spawn positions if in play mode
        if (Application.isPlaying && spawnedChildren.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (var kvp in childAngles)
            {
                if (kvp.Key != null)
                {
                    Vector3 pos = CalculatePositionFromAngle(kvp.Value);
                    Gizmos.DrawWireSphere(pos, 0.2f);
                }
            }
        }
        else if (!Application.isPlaying)
        {
            // Preview spawn positions in editor
            Gizmos.color = Color.green;
            List<float> previewAngles = CalculateSpawnAngles();
            foreach (float angle in previewAngles)
            {
                Vector3 pos = CalculatePositionFromAngle(angle);
                Gizmos.DrawWireSphere(pos, 0.2f);
            }
        }
    }
}


