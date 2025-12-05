using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
            public Vector2 move;     // sec: x~y
            public float skillUse;
        }

        //-- Serializable
        [SerializeField] private CharacterBehaviour m_Cha;
        [SerializeField] private FActionPercent m_Actions;
        [Tooltip("AI가 활동할 수 있는 x좌표 범위다. (x:min, y:max)")]
        [SerializeField] private Vector2 m_Area;

        //-- Private

        //-- Properties

        //------------------------------------------------------------------------------

        private void Start()
        {
            RunAsync().Forget();
        }

        private async UniTask RunAsync()
        {
            print("[Ai] Wait for battle");
            while (GameManager.main.Status < EGameStatus.Battle)
            {
                await UniTask.Yield(cancellationToken: destroyCancellationToken);
            }

            print("[Ai] Start");

            RunSkillAsync().Forget();

            List<BaseSkillBehaviour> skills = new(m_Cha.Skills);
            EGameStatus status;
            while (m_Cha.IsLive
                && ((status = GameManager.main.Status) == EGameStatus.Battle || status == EGameStatus.Battle_LimitOver))
            {
                // 일정 시간동안 공격
                await AttackAsync();

                await MoveAsync();
            }
        }

        private async UniTask RunSkillAsync()
        {
            List<BaseSkillBehaviour> skills = new(m_Cha.Skills);

            EGameStatus status;
            while (m_Cha.IsLive
                && ((status = GameManager.main.Status) == EGameStatus.Battle || status == EGameStatus.Battle_LimitOver))
            {
                await UniTask.Delay(100, cancellationToken: destroyCancellationToken);

                if (TRandom.Value < m_Actions.skillUse)
                {
                    TRandom.Shuffle(skills);
                    for (int i = 0; i < skills.Count; i++)
                    {
                        BaseSkillBehaviour skill = skills[i];
                        if (m_Cha.StartSkill(skill))
                        {
                            print($"[Ai] Skill '{skill.name}'");
                            break;
                        }
                    }

                    print($"[Ai] Wait for control");
                    while (!m_Cha.CanControl)
                    {
                        await UniTask.Delay(100, cancellationToken: destroyCancellationToken);
                    }
                }
            }
        }

        private async UniTask AttackAsync()
        {
            m_Cha.InputAxis = default;

            float duration = TRandom.Range(m_Actions.attack.x, m_Actions.attack.y);
            print($"[Ai] Attack {duration:f1}s");
            await UniTask.WaitForSeconds(duration, cancellationToken: destroyCancellationToken);
        }

        private async UniTask MoveAsync()
        {
            float duration = TRandom.Range(m_Actions.move.x, m_Actions.move.y);

            // 위치 계산 (에디터 반영을 위해 캐싱X)
            var x = transform.position.x;
            var mid = Mathf.Lerp(m_Area.x, m_Area.y, 0.5f);
            var length = m_Area.y - m_Area.x;
            var halfLength = length * 0.5f;

            float ratio = x / length; // 좌측부터 우측까지의 범위 (0~1)

            // 우측으로 갈수록 높은 확률로 선택됨
            if (TRandom.Value < ratio)
            {
                m_Cha.InputAxis = Vector2.left;
            }
            // 좌측으로 갈수록 높은 확률로 선택됨
            else
            {
                m_Cha.InputAxis = Vector2.right;
            }

            print($"[Ai] Move {m_Cha.InputAxis} in {duration:f1}s");
            await UniTask.WaitForSeconds(duration, cancellationToken: destroyCancellationToken);
        }
    }
}