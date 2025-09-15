using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Text;
using Unity.Collections;
using System.Linq;

public static class Coordinate
{
    private static List<int[]> StoredTileList = new List<int[]>();


    public static Vector3 ScreenToWorld(Vector3 screenPosition, Camera worldCamera)
    {
        if (screenPosition.sqrMagnitude > 100000000)
        {
            Debug.Log("Error : Screen Position Out of range");
            return Vector3.zero;
        }

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

}
