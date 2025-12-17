#nullable enable

using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 2D 각도 유틸. 각도→방향벡터, 방향벡터→각도 변환해줌.
    /// Inverse로 좌우반전, GetQuaternion으로 Z축 회전 쿼터니언 만듦.
    /// 0도가 오른쪽이고 반시계가 양수.
    /// </summary>
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

        public static float Inverse(float angleDeg)
        {
            if(angleDeg > 0)
            {
                return 180 - angleDeg;
            }
            else
            {
                return -(180 + angleDeg);
            }
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