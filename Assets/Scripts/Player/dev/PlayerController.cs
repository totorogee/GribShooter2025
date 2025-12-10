using UnityEngine;

/// <summary>
/// Controls player movement and rotation in 8 directions on the XY plane.
/// Movement: QWEADZXC (8 directions) or WASD (4 directions)
/// Rotation: O (counter-clockwise), P (clockwise)
/// Auto-snaps to nearest 45-degree angle when rotation input stops
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Control Settings")]
    [SerializeField] private bool useWASD = false; // If true, uses WASD instead of QWEADZXC
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second
    [SerializeField] private float snapSpeed = 360f; // Speed for snapping to 45-degree angles
    
    [Header("Grid Snapping")]
    [SerializeField] private float gridSize = 0.01f; // Size of grid cells
    
    private bool isRotating = false;
    private float targetRotation = 0f;
    private bool isSnapping = false;

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    /// <summary>
    /// Handles player rotation with O and P keys
    /// O = counter-clockwise, P = clockwise
    /// Auto-snaps to nearest 45-degree angle when input stops
    /// </summary>
    private void HandleRotation()
    {
        float rotationInput = 0f;

        // Check for rotation input
        if (Input.GetKey(KeyCode.O))
        {
            rotationInput = 1f; // Counter-clockwise
            isRotating = true;
            isSnapping = false;
        }
        else if (Input.GetKey(KeyCode.P))
        {
            rotationInput = -1f; // Clockwise
            isRotating = true;
            isSnapping = false;
        }
        else if (isRotating)
        {
            // Input just stopped, start snapping to nearest 45-degree angle
            isRotating = false;
            isSnapping = true;
            targetRotation = GetNearest45DegreeAngle(transform.eulerAngles.z);
        }

        if (isRotating)
        {
            // Manual rotation
            float rotation = rotationInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, rotation);
        }
        else if (isSnapping)
        {
            // Snap to nearest 45-degree angle
            float currentZ = transform.eulerAngles.z;
            float newZ = Mathf.MoveTowardsAngle(currentZ, targetRotation, snapSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0f, 0f, newZ);

            // Check if snapping is complete
            if (Mathf.Abs(Mathf.DeltaAngle(currentZ, targetRotation)) < 0.1f)
            {
                transform.eulerAngles = new Vector3(0f, 0f, targetRotation);
                isSnapping = false;
            }
        }
    }

    /// <summary>
    /// Handles player movement in 8 directions using QWEADZXC keys or WASD
    /// QWEADZXC mode:
    /// Q=↖, W=↑, E=↗
    /// A=←,     D=→
    /// Z=↙, X=↓, C=↘
    /// 
    /// WASD mode (4 directions):
    /// W=↑
    /// A=←, S=↓, D=→
    /// </summary>
    private void HandleMovement()
    {
        Vector2 moveDirection = Vector2.zero;

        if (useWASD)
        {
            // WASD mode - 4 directions only
            if (Input.GetKey(KeyCode.W)) moveDirection.y += 1f;
            if (Input.GetKey(KeyCode.S)) moveDirection.y -= 1f;
            if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1f;
            if (Input.GetKey(KeyCode.D)) moveDirection.x += 1f;
        }
        else
        {
            // QWEADZXC mode - 8 directions
            // Vertical movement
            if (Input.GetKey(KeyCode.W)) moveDirection.y += 1f;
            if (Input.GetKey(KeyCode.X)) moveDirection.y -= 1f;

            // Horizontal movement
            if (Input.GetKey(KeyCode.D)) moveDirection.x += 1f;
            if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1f;

            // Diagonal movement
            if (Input.GetKey(KeyCode.Q))
            {
                moveDirection.x -= 1f;
                moveDirection.y += 1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                moveDirection.x += 1f;
                moveDirection.y += 1f;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                moveDirection.x -= 1f;
                moveDirection.y -= 1f;
            }
            if (Input.GetKey(KeyCode.C))
            {
                moveDirection.x += 1f;
                moveDirection.y -= 1f;
            }
        }

        // Normalize to prevent faster diagonal movement
        if (moveDirection.magnitude > 0)
        {
            moveDirection.Normalize();
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    /// <summary>
    /// Returns the nearest 45-degree angle to the given angle
    /// </summary>
    /// <param name="angle">Current angle in degrees</param>
    /// <returns>Nearest 45-degree increment (0, 45, 90, 135, 180, 225, 270, 315)</returns>
    private float GetNearest45DegreeAngle(float angle)
    {
        // Normalize angle to 0-360 range
        angle = angle % 360f;
        if (angle < 0f) angle += 360f;

        // Round to nearest 45-degree increment
        float nearest = Mathf.Round(angle / 45f) * 45f;
        
        return nearest;
    }

    /// <summary>
    /// Snaps the player's position to the nearest grid point
    /// </summary>
    public void SnapToGrid()
    {
        Vector3 currentPos = transform.position;
        
        float snappedX = Mathf.Round(currentPos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(currentPos.y / gridSize) * gridSize;
        
        transform.position = new Vector3(snappedX, snappedY, currentPos.z);
    }

    /// <summary>
    /// Snaps position to a specific grid point on the XY plane
    /// </summary>
    /// <param name="position">The position to snap</param>
    /// <returns>The snapped position</returns>
    public Vector2 SnapPositionToGrid(Vector2 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
        
        return new Vector2(snappedX, snappedY);
    }

    /// <summary>
    /// Gets the nearest grid point to the player's current position without moving the player
    /// </summary>
    /// <returns>The nearest grid point</returns>
    public Vector2 GetNearestGridPoint()
    {
        return SnapPositionToGrid(transform.position);
    }
}

