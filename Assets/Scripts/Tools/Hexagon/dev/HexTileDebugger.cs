using UnityEngine;
using GameTool.Hex;

/// <summary>
/// Debug script to analyze hex tile prefab sizes
/// </summary>
public class HexTileDebugger : MonoBehaviour
{
    [Header("Hex Tile Prefabs")]
    public HexTiles flatHexPrefab;
    public HexTiles pointyHexPrefab;
    
    [Header("Debug Settings")]
    public bool debugOnStart = true;

    private void Start()
    {
        if (debugOnStart)
        {
            DebugHexTileSizes();
        }
    }

    [ContextMenu("Debug Hex Tile Sizes")]
    public void DebugHexTileSizes()
    {
        Debug.Log("=== HEX TILE SIZE DEBUG ===");
        
        // Try to get prefabs from HexEnvController first
        HexEnvController controller = FindFirstObjectByType<HexEnvController>();
        if (controller != null)
        {
            Debug.Log("Found HexEnvController, using its prefabs:");
            if (controller.PrefabsHexagonTilesFlat != null)
            {
                AnalyzeHexTile(controller.PrefabsHexagonTilesFlat, "FLAT-TOPPED (from controller)");
            }
            if (controller.PrefabsHexagonTilesPointy != null)
            {
                AnalyzeHexTile(controller.PrefabsHexagonTilesPointy, "POINTY-TOPPED (from controller)");
            }
        }
        else
        {
            Debug.Log("No HexEnvController found, using assigned prefabs:");
        }
        
        // Fallback to assigned prefabs
        if (flatHexPrefab != null)
        {
            AnalyzeHexTile(flatHexPrefab, "FLAT-TOPPED (assigned)");
        }
        
        if (pointyHexPrefab != null)
        {
            AnalyzeHexTile(pointyHexPrefab, "POINTY-TOPPED (assigned)");
        }
        
        Debug.Log("=== END HEX TILE DEBUG ===");
    }

    private void AnalyzeHexTile(HexTiles hexTile, string tileType)
    {
        if (hexTile == null) return;

        Debug.Log($"--- {tileType} HEX TILE ---");
        
        SpriteRenderer spriteRenderer = hexTile.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogError($"{tileType} has no sprite!");
            return;
        }

        // Basic info
        Debug.Log($"GameObject: {hexTile.gameObject.name}");
        Debug.Log($"Transform Scale: {hexTile.transform.localScale}");
        
        // Sprite info
        Sprite sprite = spriteRenderer.sprite;
        Rect spriteRect = sprite.rect;
        Vector2 pixelSize = new Vector2(spriteRect.width, spriteRect.height);
        Vector2 worldSize = pixelSize / sprite.pixelsPerUnit;
        
        Debug.Log($"Sprite Pixels: {pixelSize.x} x {pixelSize.y}");
        Debug.Log($"Sprite World Size: {worldSize.x} x {worldSize.y}");
        Debug.Log($"Pixels Per Unit: {sprite.pixelsPerUnit}");
        
        // Current bounds
        Bounds bounds = spriteRenderer.bounds;
        Vector3 currentSize = bounds.size;
        Debug.Log($"Current Size: {currentSize.x} x {currentSize.y}");
        
        // Scale factor
        Vector2 scaleFactor = new Vector2(
            currentSize.x / worldSize.x,
            currentSize.y / worldSize.y
        );
        Debug.Log($"Scale Factor: {scaleFactor.x} x {scaleFactor.y}");
    }
}
