using UnityEngine;

/// <summary>
/// Simple bullet behavior script.
/// Handles bullet lifetime and collision/trigger detection.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float lifetime = 5f; // Time before bullet auto-destroys
    [SerializeField] private int damage = 1;
    [SerializeField] private bool destroyOnCollision = true;
    
    [Header("Visual Effects (Optional)")]
    [SerializeField] private GameObject hitEffectPrefab; // Particle effect on hit
    
    private float spawnTime;
    
    void Start()
    {
        spawnTime = Time.time;
    }
    
    void Update()
    {
        // Auto-destroy after lifetime expires
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Called when bullet collides with another collider (non-trigger)
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject, collision.contacts[0].point);
    }
    
    /// <summary>
    /// Called when bullet enters a trigger collider
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject, other.ClosestPoint(transform.position));
    }
    
    /// <summary>
    /// Handles what happens when bullet hits something
    /// </summary>
    /// <param name="hitObject">The object that was hit</param>
    /// <param name="hitPoint">The point where the hit occurred</param>
    private void HandleHit(GameObject hitObject, Vector2 hitPoint)
    {
        // Don't hit the player who shot it
        if (hitObject.CompareTag("Player"))
        {
            return;
        }
        
        // Try to damage the hit object if it has a health component
        // You can add your own damage system here
        // Example:
        // Health health = hitObject.GetComponent<Health>();
        // if (health != null)
        // {
        //     health.TakeDamage(damage);
        // }
        
        // Spawn hit effect if available
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
        }
        
        // Destroy bullet if configured to do so
        if (destroyOnCollision)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Sets the bullet damage (useful when instantiating)
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// Gets the bullet damage value
    /// </summary>
    public int GetDamage()
    {
        return damage;
    }
}

