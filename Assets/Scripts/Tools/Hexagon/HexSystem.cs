using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GameTool.Hex
{
    [System.Serializable]
    public struct HexSystem
    {
        public Orientations Orientation;
        public HexInt Origin;
        public float Scale;

        public static HexSystem GetDefault()
        {
            return new HexSystem
            {
                Scale = 1f,
                Orientation = Orientations.Flat,
                Origin = new HexInt(0, 0, 0),
            };
        }

        public Cart GetOriginCart()
        {
            return (new HexEntity(Origin, this)).ToCart();
        }
    }

    [System.Serializable]
    public struct HexEntity
    {
        public HexSystem System;
        public HexInt HexInt;

        public HexEntity(HexInt hexInt, HexSystem hexSystem)
        {
            System = hexSystem;
            HexInt = hexInt;
        }

        public HexEntity(HexInt hexInt, HexInt origin, int scale = 1, Orientations orientation = Orientations.Flat)
        {
            System = new HexSystem
            {
                Orientation = orientation,
                Origin = origin,
                Scale = scale
            };
            HexInt = hexInt;
        }

        public HexEntity(HexInt hexInt, int scale = 1, Orientations orientation = Orientations.Flat)
        {
            System = new HexSystem
            {
                Orientation = orientation,
                Origin = new HexInt(0, 0, 0),
                Scale = scale
            };
            HexInt = hexInt;
        }

        public Cart ToCart()
        {
            return new Cart
            {
                x = System.Orientation == Orientations.Flat ?
                    System.Scale * (3f * 0.5f * HexInt.x) :
                    System.Scale * (Hex.SQRT_3 * HexInt.x + Hex.SQRT_3 * 0.5f * HexInt.y),

                y = System.Orientation == Orientations.Flat ?
                    System.Scale * (Hex.SQRT_3 * 0.5f * HexInt.x + Hex.SQRT_3 * HexInt.y) :
                    System.Scale * (3f * 0.5f * HexInt.y)
            };
        }
    }
}