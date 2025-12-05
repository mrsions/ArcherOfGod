#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 스테이트 동안 Time.timeScale을 커브 따라 바꿈. 슬로모션 연출할 때 씀.
    /// realtimeSinceStartup 써서 timeScale 영향 안받음. 끝나도 자동으로 1로 안돌아감.
    /// </summary>
    public class SetTimeScaleStateMachineBehaviour : StateMachineBehaviour
    {
        public AnimationCurve timeCurve = AnimationCurve.Linear(0,0,1,1);
        private float enterTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            enterTime = Time.realtimeSinceStartup;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float t = Time.realtimeSinceStartup - enterTime;
            Time.timeScale = timeCurve.Evaluate(t);
        }
    }
}