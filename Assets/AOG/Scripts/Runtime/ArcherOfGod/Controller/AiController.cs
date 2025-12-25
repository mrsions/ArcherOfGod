using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// AI 컨트롤러. 랜덤 시간동안 공격하다가 랜덤 시간동안 이동하는 패턴.
    /// skillUse 확률로 스킬 랜덤하게 씀. 위치 가중치로 중앙 쪽으로 가려고 함.
    /// Battle 상태 되면 활성화됨. 예측이나 회피 로직 없음.
    /// </summary>
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
            Debug.Log("[Ai] Wait for battle");
            while (GameManager.main.Status < EGameStatus.Battle)
            {
                await UniTask.Yield(cancellationToken: destroyCancellationToken);
            }

            Debug.Log("[Ai] Start");

            RunSkillAsync().Forget();

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

            foreach(var skill in m_Cha.Skills)
            {
                skill.Status = ESkillStatus.Ready;
            }

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
                            Debug.Log($"[Ai] Skill '{skill.name}'");
                            break;
                        }
                    }

                    Debug.Log($"[Ai] Wait for control");
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
            Debug.Log($"[Ai] Attack {duration:f1}s");
            await UniTask.WaitForSeconds(duration, cancellationToken: destroyCancellationToken);
        }

        private async UniTask MoveAsync()
        {
            float duration = TRandom.Range(m_Actions.move.x, m_Actions.move.y);

            // 위치 기반 이동 방향 결정 (중앙으로 돌아가려는 경향)
            float x = transform.position.x;
            float length = m_Area.y - m_Area.x;
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

            Debug.Log($"[Ai] Move {m_Cha.InputAxis} in {duration:f1}s");
            await UniTask.WaitForSeconds(duration, cancellationToken: destroyCancellationToken);
        }
    }
}