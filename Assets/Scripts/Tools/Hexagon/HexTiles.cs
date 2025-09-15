using System.Collections;
using System.Collections.Generic;
using GameTool.Hex;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Component for individual hexagon tiles in the hex grid system
/// Handles visual representation and stores hex coordinate data
/// </summary>
public class HexTiles : MonoBehaviour
{
    [Header("Visual Components")]
    public SpriteRenderer SpriteRenderer;
    
    [Header("Hex Data")]
    [Tooltip("Hex coordinates of this tile in the grid system")]
    public HexInt HexCoordinates;

    private void OnEnable()
    {
        if (!SpriteRenderer)
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (!SpriteRenderer)
        {
            Debug.LogWarning("No SpriteRenderer");
        }
    }

    /// <summary>
    /// Set the visual color of this hex tile
    /// </summary>
    /// <param name="Color">Color to apply to the tile</param>
    public void SetColor(Color Color)
    {
        SpriteRenderer.color = Color;
    }
    
    /// <summary>
    /// Set the hex coordinates for this tile
    /// </summary>
    /// <param name="hexInt">Hex coordinates to store</param>
    public void SetHexCoordinates(HexInt hexInt)
    {
        HexCoordinates = hexInt;
    }
    
    /// <summary>
    /// Get the hex coordinates of this tile
    /// </summary>
    /// <returns>Hex coordinates of this tile</returns>
    public HexInt GetHexCoordinates()
    {
        return HexCoordinates;
    }
}


