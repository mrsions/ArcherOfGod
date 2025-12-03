#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public static class AngleUtils
    {
        public static Vector2 ToDirection(float angleDeg)
        {
            return new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
        }

        public static float Reverse(float angleDeg)
        {
            return -angleDeg;
        }
    }
}