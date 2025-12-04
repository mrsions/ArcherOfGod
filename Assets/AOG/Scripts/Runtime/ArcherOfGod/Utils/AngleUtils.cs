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

        public static float GetAngleByDir(Vector3 dir)
        {
            return Vector2.SignedAngle(Vector2.right, dir);
        }

        public static float Reverse(float angleDeg)
        {
            return -angleDeg;
        }

        public static Quaternion GetQuaternion(float r)
        {
            return Quaternion.Euler(0, 0, r);
        }

        public static Vector3 GetEulerAngles(float r)
        {
            return new Vector3(0, 0, r);
        }

        public static Quaternion NormalizeAngle2D(Quaternion rotation)
        {
            Vector2 dir = rotation * Vector3.right;
            return Quaternion.Euler(0, 0, GetAngleByDir(dir));
        }
    }
}