using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace GameTool.Hex
{
    /// <summary>
    /// Event names for hex tile system communication
    /// Used by EventManager for decoupled hex tile operations
    /// </summary>
    public static partial class EventName 
    {
        // Single tile operations
        public static string InitHexAtMouse         = "InitHexAtMouse";
        public static string ClearTileAtMouse       = "ClearTileAtMouse";
        public static string InitHexByHexInt        = "InitHexByHexInt";
        
        // Multiple tile operations
        public static string InitTilesHexRing       = "InitTilesHexRing";
        public static string InitTilesHexPlane      = "InitTilesHexPlane";
        public static string InitTilesCirPlane      = "InitTilesCirPlane";
        public static string ClearAllTiles          = "ClearAllTiles";
        
        // Debug operations
        public static string DisplayDataByHexInt    = "DisplayDataByHexInt";
    }


    /// <summary>
    /// Hexagon orientation types for different visual layouts
    /// </summary>
    public enum Orientations
    {
        /// <summary>Flat-topped hexagons (pointy sides up/down)</summary>
        Flat = 0,
        /// <summary>Pointy-topped hexagons (flat sides up/down)</summary>
        Pointy = 1
    }

    /// <summary>
    /// Static utility class containing core hexagonal grid mathematics and operations
    /// Provides coordinate conversion, distance calculations, and hex data management
    /// </summary>
    public static class Hex
    {
        /// <summary>Square root of 3 constant for hexagonal calculations</summary>
        public static readonly float SQRT_3 = Mathf.Sqrt(3f);
        /// <summary>Reciprocal of 3 (1/3) for hexagonal calculations</summary>
        public static readonly float REPC_3 = 1f / 3f;
        /// <summary>Epsilon value for floating point comparisons</summary>
        public static readonly float EPS = 0.0001f;

        /// <summary>
        /// Unit ring pattern for hexagonal movement and ring generation
        /// Represents the 6 adjacent hex positions around any hex
        /// Flat orientation: starts at 12 o'clock, moves clockwise
        /// Pointy orientation: starts at 11 o'clock, moves clockwise
        /// </summary>
        public static List<HexInt> UnitRing = new List<HexInt>
        {
            new HexInt(0  ,1  ,-1), // North
            new HexInt(1  ,0  ,-1), // Northeast
            new HexInt(1  ,-1 ,0 ), // Southeast
            new HexInt(0  ,-1 ,1 ), // South
            new HexInt(-1 ,0  ,1 ), // Southwest
            new HexInt(-1 ,1  ,0 )  // Northwest
        };

        /// <summary>
        /// Calculate the Cartesian distance from origin to a hex coordinate
        /// </summary>
        /// <param name="hexInt">Hex coordinates to measure from origin</param>
        /// <param name="system">Hex system configuration</param>
        /// <returns>Distance in world units</returns>
        public static float GetCartDistance(HexInt hexInt , HexSystem system)
        {
            HexEntity hexEntity = new HexEntity(hexInt, system);
            Cart cart = hexEntity.ToCart();
            return cart.GetDistance();
        }

        /// <summary>
        /// Calculate the hexagonal distance from origin to a hex coordinate
        /// This is the number of hex steps needed to reach the coordinate
        /// </summary>
        /// <param name="hexInt">Hex coordinates to measure from origin</param>
        /// <param name="system">Hex system configuration</param>
        /// <returns>Distance in hex units</returns>
        public static float GetHexDistance(HexInt hexInt, HexSystem system)
        {
            HexEntity hexEntity = new HexEntity(hexInt, system);
            Cart cart = hexEntity.ToCart();
            return cart.GetDistance() * SQRT_3 * REPC_3;
        }

        /// <summary>
        /// Cache for pre-calculated hex data to improve performance
        /// Stores HexData objects for frequently accessed hex coordinates
        /// </summary>
        public static Dictionary<HexInt, HexData> HexDataDic = new Dictionary<HexInt, HexData>();

        /// <summary>
        /// Get or create hex data for a specific coordinate
        /// Uses caching to avoid recalculating data for the same coordinates
        /// </summary>
        /// <param name="hexInt">Hex coordinates to get data for</param>
        /// <returns>HexData containing distance, angle, and other calculated values</returns>
        public static HexData GetData (HexInt hexInt)
        {
            HexData result;
            if (HexDataDic.TryGetValue(hexInt , out result))
            {
                return result;
            }

            AddHexData(hexInt);

            if (!HexDataDic.TryGetValue(hexInt, out result))
            {
                Debug.LogError("Failed to add hex data for coordinates: " + hexInt.x + ", " + hexInt.y);
            }

            return result;
        }

        /// <summary>
        /// Add hex data for a ring of coordinates around the given hex
        /// Pre-calculates distance, angle, and other data for performance
        /// </summary>
        /// <param name="hexInt">Center hex coordinate to generate ring data for</param>
        private static void AddHexData(HexInt hexInt)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            int r = hexInt.r; // Get the ring radius

            // Create a default hex system for calculations
            HexSystem system = new HexSystem
            {
                Orientation = Orientations.Flat,
                Scale = 1,
                Origin = new HexInt()
            };

            // Center hex (radius 0)
            if ( r == 0 ) { HexDataDic.Add(hexInt, new HexData(hexInt, system, 0, 1)); return; }

            // Generate data for all hexes in the ring
            HexInt key = new HexInt();
            key += r * UnitRing[0]; // Start at first position of the ring
            int q = 0; // Position counter within the ring
            
            // Walk around the ring using UnitRing pattern
            for (int j = 2; j < 8; j++)
            {
                for (int i = 0; i < r; i++)
                {
                    HexDataDic.Add(key, new HexData(key, system, r, q));
                    key += UnitRing[ j % 6]; // Move to next position in ring
                    q++;
                }
            }

            stopWatch.Stop();
            Debug.LogWarning("Time Used for hex data generation: " + stopWatch.Elapsed);
        }
    }

    /// <summary>
    /// Pre-calculated data structure for hex coordinates
    /// Contains distance, angle, and ring information for performance optimization
    /// </summary>
    public struct HexData
    {
        /// <summary>Original hex coordinates</summary>
        public HexInt HexInt;
        /// <summary>Cartesian distance from origin in world units</summary>
        public float CartDistance;
        /// <summary>Hexagonal distance from origin in hex units</summary>
        public float HexDistance;
        /// <summary>Angle position around the ring (0-47 for 48 divisions)</summary>
        public int Angle;
        /// <summary>Ring radius from center (0 = center, 1 = first ring, etc.)</summary>
        public int R;
        /// <summary>Position within the ring (0 to 6*R-1)</summary>
        public int Q;

        /// <summary>
        /// Create hex data with pre-calculated values
        /// </summary>
        /// <param name="hexInt">Hex coordinates</param>
        /// <param name="system">Hex system for calculations</param>
        /// <param name="r">Ring radius</param>
        /// <param name="q">Position within ring</param>
        public HexData(HexInt hexInt , HexSystem system , int r , int q)
        {
            HexInt = hexInt;
            CartDistance = Hex.GetCartDistance(hexInt, system);
            HexDistance = Hex.GetHexDistance(hexInt, system);
            // Calculate angle: 48 divisions around the ring
            Angle = r == 0? 1 :  Mathf.RoundToInt( q * 48f / (r * 6f));
            R = r;
            Q = q;
        }

        /// <summary>
        /// Get formatted string representation of hex data
        /// </summary>
        /// <returns>Multi-line string with all hex data values</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HexInt :" + HexInt.x + " " + HexInt.y);
            sb.AppendLine("CartD :" + CartDistance);
            sb.AppendLine("HexDistance :" + HexDistance);
            sb.AppendLine("Angle :" + Angle);
            sb.AppendLine("Cir.r :" + R + " q :" + Q);

            return sb.ToString();
        }
    }

    /// <summary>
    /// Integer-based hexagonal coordinate system
    /// Uses cube coordinates (x, y, z) where z = -x - y
    /// Implements IEquatable for efficient dictionary lookups
    /// </summary>
    [System.Serializable]
    public struct HexInt : IEquatable<HexInt>
    {
        /// <summary>X coordinate in cube space</summary>
        public int x;
        /// <summary>Y coordinate in cube space</summary>
        public int y;
        /// <summary>Z coordinate in cube space (calculated as -x - y)</summary>
        public int z;
        
        /// <summary>
        /// Ring radius from origin (0, 0, 0)
        /// Calculated as the maximum absolute value of the three coordinates
        /// </summary>
        public int r
        {
            get { return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(0 - x - y)); }
        }

        /// <summary>
        /// Create hex coordinates from x, y values
        /// Z is automatically calculated to maintain cube coordinate constraint
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate (ignored, calculated as -x - y)</param>
        public HexInt(int x, int y, int z =0)
        {
            this.x = x;
            this.y = y;
            this.z = 0 - x - y; // Maintain cube coordinate constraint
        }

        /// <summary>Equality operator for hex coordinates</summary>
        public static bool operator ==(HexInt left, HexInt right)
        {
            return left.Equals(right);
        }

        /// <summary>Inequality operator for hex coordinates</summary>
        public static bool operator !=(HexInt left, HexInt right)
        {
            return !(left == right);
        }

        /// <summary>Check equality with any object</summary>
        public override bool Equals(object obj)
        {
            return obj is HexInt a && x == a.x && y == a.y ;
        }

        /// <summary>Check equality with another HexInt (only x, y matter due to constraint)</summary>
        public bool Equals(HexInt a)
        {
            return x == a.x && y == a.y;
        }

        /// <summary>Generate hash code for dictionary lookups</summary>
        public override int GetHashCode()
        {
            return (x << 16) ^ (y << 8);
        }

        /// <summary>Add two hex coordinates together</summary>
        public static HexInt operator + (HexInt a , HexInt b)
        {
            return new HexInt(a.x + b.x, a.y + b.y);
        }

        /// <summary>Multiply hex coordinates by a scalar</summary>
        public static HexInt operator * (int s, HexInt a)
        {
            return new HexInt(s * a.x, s * a.y);
        }
    }

    [System.Serializable]
    public struct HexFloat
    {
        public float x;
        public float y;
        public float z;

        public HexFloat(float x, float y, float z=0)
        {
            this.x = x;
            this.y = y;
            this.z = 0 - x - y;
        }

        public static explicit operator HexInt(HexFloat hexF)
        {
            return new HexInt
            {
                x = Mathf.RoundToInt(hexF.x),
                y = Mathf.RoundToInt(hexF.y),
            };
        }

        public static explicit operator HexFloat(HexInt hex)
        {
            return new HexFloat
            {
                x = hex.x,
                y = hex.y,
            };
        }
    }

    [System.Serializable]
    public struct Cart
    {
        public float x;
        public float y;

        public Cart(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float GetDistance()
        {
            return Mathf.Sqrt(x * x + y * y);
        }

        public HexEntity ToHexEntity(int scale, Orientations orientation)
        {
            HexInt hex = ToHex(scale, orientation);
            return new HexEntity(hex, scale, orientation);
        }

        public HexEntity ToHexEntity(HexSystem system)
        {
            HexInt hex = ToHex(system.Scale, system.Orientation);
            return new HexEntity(hex, system);
        }

        public HexInt ToHex(float scale, Orientations orientation)
        {
            return (orientation == Orientations.Flat) ? ToFlatHex(scale) : ToPointyHex(scale);
        }

        public HexInt ToFlatHex(float size)
        {
            return (HexInt) new HexFloat
            {
                x = (2f * Hex.REPC_3 * x) / size,
                y = (-Hex.REPC_3 * x + Hex.SQRT_3 * Hex.REPC_3 * y) / size,
            };
        }

        public HexInt ToPointyHex(float size)
        {
            return (HexInt) new HexFloat
            {
                x = (Hex.SQRT_3 * Hex.REPC_3 * x - Hex.REPC_3 * y) / size,
                y = (2f * Hex.REPC_3 * y) / size,
            };
        }

        public static explicit operator Vector3(Cart cart)
        {
            return new Vector3(cart.x, cart.y, 0f);
        }
    }
}
