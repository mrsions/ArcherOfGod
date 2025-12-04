#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public static class VectorExtension
    {
        public static Vector3 SetX(this Vector3 v, float x)
        {
            v.x = x;
            return v;
        }
        public static Vector3 SetY(this Vector3 v, float x)
        {
            v.y = x;
            return v;
        }
        public static Vector3 SetZ(this Vector3 v, float x)
        {
            v.z = x;
            return v;
        }

        public static Vector2 SetX(this Vector2 v, float x)
        {
            v.x = x;
            return v;
        }
        public static Vector2 SetY(this Vector2 v, float x)
        {
            v.y = x;
            return v;
        }
    }
}