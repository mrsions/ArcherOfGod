#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 애니메이터 스테이트 동안 레이어 웨이트를 커브 따라 트윈함.
    /// 스테이트 끝나면 0으로 리셋됨. normalizedTime 씀.
    /// </summary>
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