using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns items on destroy or when manually triggered.
/// Supports multiple spawn items with configurable spawn chances.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnItem
    {
        [Tooltip("The prefab to spawn")]
        public GameObject prefab;
        
        [Tooltip("Chance to spawn this item (0-100%)")]
        [Range(0f, 100f)]
        public float spawnChance = 100f;
        
        [Tooltip("Minimum number to spawn if chance succeeds")]
        [Min(0)]
        public int minCount = 1;
        
        [Tooltip("Maximum number to spawn if chance succeeds")]
        [Min(0)]
        public int maxCount = 1;
        
        [Tooltip("Spawn offset range (random position within this range)")]
        public Vector2 spawnOffsetRange = Vector2.zero;
        
        [Tooltip("Should this item inherit velocity from this object?")]
        public bool inheritVelocity = false;
        
        [Tooltip("Additional random velocity to apply")]
        public Vector2 randomVelocity = Vector2.zero;
    }
    
    [Header("Spawn Settings")]
    [SerializeField] private List<SpawnItem> spawnItems = new List<SpawnItem>();
    
    [Header("Spawn Triggers")]
    [SerializeField] private bool spawnOnDestroy = true;
    [SerializeField] private bool spawnOnDisable = false;
    
    [Header("Spawn Options")]
    [SerializeField] private bool useWorldPosition = true;
    [SerializeField] private Transform spawnParent = null; // Optional parent for spawned items
    
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void OnDestroy()
    {
        if (spawnOnDestroy && gameObject.scene.isLoaded) // Check scene is loaded to avoid errors when closing app
        {
            SpawnItems();
        }
    }
    
    void OnDisable()
    {
        if (spawnOnDisable)
        {
            SpawnItems();
        }
    }
    
    /// <summary>
    /// Manually trigger spawning of items
    /// </summary>
    public void Spawn()
    {
        SpawnItems();
    }
    
    /// <summary>
    /// Spawn a specific item from the list by index
    /// </summary>
    public void SpawnSpecificItem(int index)
    {
        if (index >= 0 && index < spawnItems.Count)
        {
            ProcessSpawnItem(spawnItems[index]);
        }
        else
        {
            Debug.LogWarning($"ItemSpawner: Invalid spawn item index {index}");
        }
    }
    
    /// <summary>
    /// Spawns all configured items based on their spawn chances
    /// </summary>
    private void SpawnItems()
    {
        foreach (SpawnItem item in spawnItems)
        {
            if (item.prefab == null)
            {
                Debug.LogWarning("ItemSpawner: Spawn item has no prefab assigned!");
                continue;
            }
            
            // Check spawn chance
            float roll = Random.Range(0f, 100f);
            if (roll <= item.spawnChance)
            {
                ProcessSpawnItem(item);
            }
        }
    }
    
    /// <summary>
    /// Processes and spawns a specific item configuration
    /// </summary>
    private void ProcessSpawnItem(SpawnItem item)
    {
        if (item.prefab == null)
            return;
        
        // Determine how many to spawn
        int spawnCount = Random.Range(item.minCount, item.maxCount + 1);
        
        for (int i = 0; i < spawnCount; i++)
        {
            // Calculate spawn position with random offset
            Vector3 spawnPosition = transform.position;
            if (item.spawnOffsetRange != Vector2.zero)
            {
                Vector2 randomOffset = new Vector2(
                    Random.Range(-item.spawnOffsetRange.x, item.spawnOffsetRange.x),
                    Random.Range(-item.spawnOffsetRange.y, item.spawnOffsetRange.y)
                );
                spawnPosition += (Vector3)randomOffset;
            }
            
            // Spawn the item
            GameObject spawnedObject = Instantiate(
                item.prefab,
                spawnPosition,
                transform.rotation,
                spawnParent
            );
            
            // Set position mode
            if (!useWorldPosition && spawnParent != null)
            {
                spawnedObject.transform.localPosition = spawnPosition;
            }
            
            // Apply velocity if needed
            if (item.inheritVelocity || item.randomVelocity != Vector2.zero)
            {
                Rigidbody2D spawnedRb = spawnedObject.GetComponent<Rigidbody2D>();
                if (spawnedRb != null)
                {
                    Vector2 velocity = Vector2.zero;
                    
                    // Inherit velocity from this object
                    if (item.inheritVelocity && rb != null)
                    {
                        velocity = rb.linearVelocity;
                    }
                    
                    // Add random velocity
                    if (item.randomVelocity != Vector2.zero)
                    {
                        velocity += new Vector2(
                            Random.Range(-item.randomVelocity.x, item.randomVelocity.x),
                            Random.Range(-item.randomVelocity.y, item.randomVelocity.y)
                        );
                    }
                    
                    spawnedRb.linearVelocity = velocity;
                }
            }
        }
    }
    
    /// <summary>
    /// Add a spawn item configuration at runtime
    /// </summary>
    public void AddSpawnItem(GameObject prefab, float spawnChance = 100f, int minCount = 1, int maxCount = 1)
    {
        SpawnItem newItem = new SpawnItem
        {
            prefab = prefab,
            spawnChance = spawnChance,
            minCount = minCount,
            maxCount = maxCount
        };
        spawnItems.Add(newItem);
    }
    
    /// <summary>
    /// Remove all spawn items
    /// </summary>
    public void ClearSpawnItems()
    {
        spawnItems.Clear();
    }
    
    /// <summary>
    /// Get the number of configured spawn items
    /// </summary>
    public int GetSpawnItemCount()
    {
        return spawnItems.Count;
    }
    
    /// <summary>
    /// Enable or disable spawning on destroy
    /// </summary>
    public void SetSpawnOnDestroy(bool enabled)
    {
        spawnOnDestroy = enabled;
    }
    
    /// <summary>
    /// Enable or disable spawning on disable
    /// </summary>
    public void SetSpawnOnDisable(bool enabled)
    {
        spawnOnDisable = enabled;
    }
}

