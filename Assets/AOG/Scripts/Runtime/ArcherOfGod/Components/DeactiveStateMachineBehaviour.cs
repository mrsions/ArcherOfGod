#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public class DeactiveStateMachineBehaviour : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.enabled = false;
        }
    }
}