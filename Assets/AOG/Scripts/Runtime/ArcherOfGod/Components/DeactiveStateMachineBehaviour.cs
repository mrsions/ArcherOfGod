#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 스테이트 끝나면 애니메이터 꺼버림. 죽는 애니메이션 같은데 붙여서 쓰면 됨.
    /// </summary>
    public class DeactiveStateMachineBehaviour : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.enabled = false;
        }
    }
}