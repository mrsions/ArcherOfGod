#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace AOT
{
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