#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public class LayerWeightTweenStateMachineBehaviour : StateMachineBehaviour
    {
        public int targetLayer = 0;
        public AnimationCurve weightCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetLayerWeight(targetLayer, weightCurve.Evaluate(0));
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetLayerWeight(targetLayer, weightCurve.Evaluate(stateInfo.normalizedTime));
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetLayerWeight(targetLayer, 0);
        }
    }
}