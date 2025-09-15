using System.Collections;
using System.Collections.Generic;
using GameTool.Hex;
using JetBrains.Annotations;
using UnityEngine;

public class HexTiles : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

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

    public void SetColor(Color Color)
    {
        SpriteRenderer.color = Color;
    }
}


