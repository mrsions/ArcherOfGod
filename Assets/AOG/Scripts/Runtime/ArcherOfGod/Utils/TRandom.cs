#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 랜덤 유틸. Value로 0~1, Range로 범위 랜덤, FromMinus로 -b~b 랜덤.
    /// Shuffle로 스킬 리스트 섞음. Unity Random 래핑한거라 스레드세이프 아님.
    /// </summary>
    public static class TRandom
    {
        public static float Value => Random.value;

        public static float Range(float a, float b) => Random.Range(a, b);
        public static float RangeInt(int a, int b) => Random.Range(a, b);

        public static float From0(float b) => Random.Range(0, b);
        public static float FromMinus(float b) => Random.Range(-b, b);

        public static void Shuffle(IList<BaseSkillBehaviour> skills)
        {
            int len = skills.Count - 1;
            for (int i = 0; i < len; i++)
            {
                int j = Random.Range(i, skills.Count);

                (skills[i], skills[j]) = (skills[j], skills[i]);
            }
        }
    }
}