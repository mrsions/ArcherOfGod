#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public struct FBezier
    {
        // A, B: 시작/끝 점
        private readonly Vector2 A;
        private readonly Vector2 B;
        // startAngleDeg, endAngleDeg: 각도(도 단위, 0도 = 오른쪽, 반시계 방향 기준)
        private readonly float startAngleDeg;
        private readonly float endAngleDeg;
        // strength: 곡선 휘어짐 정도 (0.1 ~ 0.5 정도로 조절)
        private readonly float strength;

        public FBezier(Vector2 a, Vector2 b, float startAngleDeg, float endAngleDeg, float strength)
        {
            A = a;
            B = b;
            this.startAngleDeg = startAngleDeg;
            this.endAngleDeg = endAngleDeg;
            this.strength = strength;
        }

        public Vector2 Evaluate(float t)
        {
            if (strength != 0)
            {
                // 두 점 사이 거리
                float dist = Vector2.Distance(A, B);

                // 컨트롤 포인트까지의 거리 (원하면 L1, L2 따로 둘 수도 있음)
                float L1 = dist * strength;
                float L2 = dist * strength;

                // 각도(도) → 라디안
                float startRad = startAngleDeg * Mathf.Deg2Rad;
                float endRad = endAngleDeg * Mathf.Deg2Rad;

                // 각도 → 방향 벡터
                Vector2 dirStart = new Vector2(Mathf.Cos(startRad), Mathf.Sin(startRad));
                Vector2 dirEnd = new Vector2(Mathf.Cos(endRad), Mathf.Sin(endRad));

                // 베지어 포인트 4개
                Vector2 P0 = A;
                Vector2 P1 = A + dirStart * L1;
                Vector2 P2 = B - dirEnd * L2;
                Vector2 P3 = B;

                // 3차 베지어 공식
                float u = 1f - t;
                float uu = u * u;
                float uuu = uu * u;
                float tt = t * t;
                float ttt = tt * t;

                Vector2 point =
                    uuu * P0 +
                    3f * uu * t * P1 +
                    3f * u * tt * P2 +
                    ttt * P3;

                return point;
            }
            else
            {
                return Vector2.LerpUnclamped(A, B, t);
            }
        }

        public float Distance(int angle = 32)
        {
            if (strength != 0)
            {
                float length = 0;
                Vector2 a = A;
                for (int i = 1; i <= angle; i++)
                {
                    Vector2 b = Evaluate((float)i / angle);
                    length += Vector2.Distance(a, b);
                    a = b;
                }
                return length;
            }
            else
            {
                return Vector2.Distance(A, B);
            }
        }
    }
}