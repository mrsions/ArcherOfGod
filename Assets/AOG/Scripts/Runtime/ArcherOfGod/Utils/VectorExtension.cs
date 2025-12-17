#nullable enable

using UnityEngine;

namespace AOT
{
    /// <summary>
    /// Vector 확장 메서드. SetX/SetY/SetZ로 한 컴포넌트만 바꾼 복사본 리턴.
    /// Vector2, Vector3 둘 다 됨.
    /// </summary>
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