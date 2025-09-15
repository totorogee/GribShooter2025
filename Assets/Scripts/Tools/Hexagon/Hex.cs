using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace GameTool.Hex
{
    public enum Orientations
    {
        Flat = 0,
        Pointy = 1
    }

    public static class Hex
    {
        public static readonly float SQRT_3 = Mathf.Sqrt(3f);
        public static readonly float REPC_3 = 1f / 3f;
        public static readonly float EPS = 0.0001f;

        // Flat   : start 12 clock-wise
        // Pointy : start 11 clock-wise
        public static List<HexInt> UnitRing = new List<HexInt>
        {
            new HexInt(0  ,1  ,-1), 
            new HexInt(1  ,0  ,-1), 
            new HexInt(1  ,-1 ,0 ), 
            new HexInt(0  ,-1 ,1 ), 
            new HexInt(-1 ,0  ,1 ), 
            new HexInt(-1 ,1  ,0 )  
        };

        public static float GetCartDistance(HexInt hexInt , HexSystem system)
        {
            HexEntity hexEntity = new HexEntity(hexInt, system);
            Cart cart = hexEntity.ToCart();
            return cart.GetDistance();
        }

        public static float GetHexDistance(HexInt hexInt, HexSystem system)
        {
            HexEntity hexEntity = new HexEntity(hexInt, system);
            Cart cart = hexEntity.ToCart();
            return cart.GetDistance() * SQRT_3 * REPC_3;
        }

        public static Dictionary<HexInt, HexData> HexDataDic = new Dictionary<HexInt, HexData>();

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
                Debug.LogError("Why?");
            }

            return result;
        }

        private static void AddHexData(HexInt hexInt)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            int r = hexInt.r;

            HexSystem system = new HexSystem
            {
                Orientation = Orientations.Flat,
                Scale = 1,
                Origin = new HexInt()
            };

            if ( r == 0 ) { HexDataDic.Add(hexInt, new HexData(hexInt, system, 0, 1)); }

            HexInt key = new HexInt();
            key += r * UnitRing[0];
            int q = 0;
            for (int j = 2; j < 8; j++)
            {
                for (int i = 0; i < r; i++)
                {
                    HexDataDic.Add(key, new HexData(key, system, r, q));
                    key += UnitRing[j % 6];
                    q++;
                }
            }

            stopWatch.Stop();
            Debug.LogWarning("Time Used : " + stopWatch.Elapsed);
        }
    }

    public struct HexData
    {
        public HexInt HexInt;
        public float CartDistance;
        public float HexDistance;
        public int Angle;
        public int R;
        public int Q;

        public HexData(HexInt hexInt , HexSystem system , int r , int q)
        {
            HexInt = hexInt;
            CartDistance = Hex.GetCartDistance(hexInt, system);
            HexDistance = Hex.GetHexDistance(hexInt, system);
            Angle = r == 0? 1 :  Mathf.RoundToInt( q * 48f / (r * 6f));
            R = r;
            Q = q;
        }

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

    [System.Serializable]
    public struct HexInt : IEquatable<HexInt>
    {
        public int x;
        public int y;
        public int z;
        public int r
        {
            get { return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(0 - x - y)); }
        }

        public HexInt(int x, int y, int z =0)
        {
            this.x = x;
            this.y = y;
            this.z = 0 - x - y;
        }

        public static bool operator ==(HexInt left, HexInt right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HexInt left, HexInt right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is HexInt a && x == a.x && y == a.y ;
        }

        public bool Equals(HexInt a)
        {
            return x == a.x && y == a.y;
        }

        public override int GetHashCode()
        {
            return (x << 16) ^ (y << 8);
        }

        public static HexInt operator + (HexInt a , HexInt b)
        {
            return new HexInt(a.x + b.x, a.y + b.y);
        }

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
