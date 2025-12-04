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
    public class AiController : MonoBehaviour
    {
        [Serializable]
        public struct FActionPercent
        {
            public Vector2 attack;   // sec: x~y
            public Vector3 move;     // percent: x   sec: y~z
            public Vector3 skillUse; // percent: x   sec: y~z
        }

        //-- Serializable
        [SerializeField] private CharacterBehaviour m_Cha;
        [SerializeField] private FActionPercent m_Actions;

        //-- Private

        //-- Properties

        //------------------------------------------------------------------------------

        private void Start()
        {
            RunAsync().Forget();
        }

        private async UniTask RunAsync()
        {
            List<BaseSkillBehaviour> skills = new(m_Cha.Skills);

            while (m_Cha.IsLive)
            {
                if (TRandom.Value < m_Actions.move.x)
                {
                    m_Cha.InputAxis = default;
                    await UniTask.WaitForSeconds(TRandom.Range(m_Actions.move.y, m_Actions.move.z));
                }
                else if (TRandom.Value < m_Actions.skillUse.x)
                {
#if UNITY_EDITOR 
                    // apply changed in editor
                    skills.Clear();
                    skills.AddRange(m_Cha.Skills);
#endif
                    TRandom.Shuffle(skills);

                    for (int i = 0; i < skills.Count; i++)
                    {
                        BaseSkillBehaviour skill = skills[i];
                        if (m_Cha.StartSkill(skill))
                        {
                            break;
                        }
                    }

                    await UniTask.WaitForSeconds(TRandom.Range(m_Actions.skillUse.y, m_Actions.skillUse.z));
                }
                else
                {
                    m_Cha.InputAxis = default;

                    await UniTask.WaitForSeconds(TRandom.Range(m_Actions.attack.x, m_Actions.attack.y));
                }
            }
        }
    }
}