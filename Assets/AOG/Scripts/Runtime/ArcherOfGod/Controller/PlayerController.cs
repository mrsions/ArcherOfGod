using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class PlayerController : MonoBehaviour
    {
        //-- Serializable
        [SerializeField] private CharacterBehaviour m_Cha;

        //-- Private

        //-- Properties

        //------------------------------------------------------------------------------

        private void Start()
        {
            RunAsync().Forget();
        }

        private async UniTask RunAsync()
        {
            while(m_Cha.IsLive)
            {
                m_Cha.InputAxis = GameSettings.main.GetPlayerMoveAction().ReadValue<Vector2>();
                if(m_Cha.IsPlayer)
                {
                    await UniTask.Delay(2);
                }

                await UniTask.Yield(PlayerLoopTiming.PreUpdate, destroyCancellationToken);
            }
        }
    }
}