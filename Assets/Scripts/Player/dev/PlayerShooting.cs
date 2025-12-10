using UnityEngine;

/// <summary>
/// Controls player shooting behavior with burst fire mechanics.
/// Press/Hold Space to shoot bullets in bursts of 5 with a cooldown between bursts.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint; // Where bullets spawn (optional)
    [SerializeField] private float bulletSpeed = 10f;
    
    [Header("Shooting Settings")]
    [SerializeField] private int burstSize = 5; // Number of bullets per burst
    [SerializeField] private float bulletInterval = 0.1f; // Time between bullets in a burst
    [SerializeField] private float burstCooldown = 0.5f; // Cooldown after a burst completes
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip shootSound;
    
    private int bulletsShot = 0; // Current bullets shot in this burst
    private float nextShotTime = 0f; // Time when next bullet can be shot
    private bool isBurstComplete = true; // Whether the current burst is complete
    private bool isInCooldown = false; // Whether we're in cooldown between bursts
    
    void Update()
    {
        HandleShooting();
    }
    
    /// <summary>
    /// Handles shooting input and burst fire logic
    /// </summary>
    private void HandleShooting()
    {
        bool spacePressed = Input.GetKey(KeyCode.Space);
        
        // Check if player is holding space and can shoot
        if (spacePressed && Time.time >= nextShotTime)
        {
            // If burst is complete and we're not in cooldown, start a new burst
            if (isBurstComplete && !isInCooldown)
            {
                StartNewBurst();
            }
            // If currently in a burst, continue shooting
            else if (!isBurstComplete && !isInCooldown)
            {
                ShootBullet();
                bulletsShot++;
                
                // Check if burst is complete
                if (bulletsShot >= burstSize)
                {
                    CompleteBurst();
                }
                else
                {
                    // Set next shot time for next bullet in burst
                    nextShotTime = Time.time + bulletInterval;
                }
            }
        }
        
        // Handle cooldown expiration
        if (isInCooldown && Time.time >= nextShotTime)
        {
            isInCooldown = false;
            isBurstComplete = true;
        }
    }
    
    /// <summary>
    /// Starts a new burst of bullets
    /// </summary>
    private void StartNewBurst()
    {
        bulletsShot = 0;
        isBurstComplete = false;
        ShootBullet();
        bulletsShot++;
        
        if (bulletsShot >= burstSize)
        {
            CompleteBurst();
        }
        else
        {
            nextShotTime = Time.time + bulletInterval;
        }
    }
    
    /// <summary>
    /// Completes the current burst and starts cooldown
    /// </summary>
    private void CompleteBurst()
    {
        isBurstComplete = true;
        isInCooldown = true;
        nextShotTime = Time.time + burstCooldown;
    }
    
    /// <summary>
    /// Shoots a single bullet
    /// </summary>
    private void ShootBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not assigned!");
            return;
        }
        
        // Determine spawn position (use firePoint if available, otherwise use player position)
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        // Get shooting direction based on player's rotation
        Vector2 shootDirection = transform.up; // Assuming player's "forward" is up in 2D
        
        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        // Set bullet velocity (check for Rigidbody2D first)
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }
        
        // Rotate bullet to face the direction it's moving
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90 if bullet sprite points up
        
        // Play sound effect if available
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position);
        }
    }
    
    /// <summary>
    /// Gets the current burst progress (for UI or debugging)
    /// </summary>
    public int GetBulletsRemainingInBurst()
    {
        return isBurstComplete ? 0 : burstSize - bulletsShot;
    }
    
    /// <summary>
    /// Checks if currently in cooldown between bursts
    /// </summary>
    public bool IsInCooldown()
    {
        return isInCooldown;
    }
    
    /// <summary>
    /// Gets the time remaining until cooldown ends
    /// </summary>
    public float GetCooldownTimeRemaining()
    {
        if (!isInCooldown) return 0f;
        return Mathf.Max(0f, nextShotTime - Time.time);
    }
}

