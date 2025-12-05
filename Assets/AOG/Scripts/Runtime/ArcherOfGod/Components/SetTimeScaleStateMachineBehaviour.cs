#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
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