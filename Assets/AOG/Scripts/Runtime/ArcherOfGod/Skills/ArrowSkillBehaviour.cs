using System.Threading.Tasks;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class ArrowSkillBehaviour : BaseSkillBehaviour
    {
        //-- Serializable
        [SerializeField]
        private ProjectileBehaviour m_Prefab;

        [SerializeField]
        private bool m_StraightShot;

        [SerializeField]
        private float m_Force = 0;

        [SerializeField]
        private ForceMode2D m_ForceMode = ForceMode2D.Impulse;

        [SerializeField]
        private bool m_ApplyInput = false;

        [SerializeField]
        private Vector2 m_DashDirection;

        //-- Private
        private ProjectileBehaviour m_ArrowInstance;

        //-- Properties


        //------------------------------------------------------------------------------

        internal override bool OnStartSkill(CharacterBehaviour cha)
        {
            if (!base.OnStartSkill(cha)) return false;

            if(!m_ApplyInput)
            {
                cha.SetForward(true, true);
            }

            if (m_Force > 0)
            {
                Vector2 dir = m_DashDirection;
                if (cha.IsLeft) dir.x = -dir.x;
                if (m_ApplyInput && cha.InputAxis.x < 0) dir.x = -dir.x;
                cha.Rigidbody.AddForce(dir * m_Force, m_ForceMode);
            }

            return true;
        }

        internal override void OnSkillPrepare(CharacterBehaviour sender, Transform pose)
        {
            base.OnSkillPrepare(sender, pose);

            if (!m_ArrowInstance)
            {
                m_ArrowInstance = GameObjectPool.main.Rent(m_Prefab, pose.position, pose.rotation, pose);
                if (m_ArrowInstance.UseStraight)
                {
                    m_ArrowInstance.LookAtEnemyAsync(pose).Forget();
                }
            }
        }

        internal override void OnSkillActivate(ObjectBehaviour sender, Transform pose)
        {
            var arrow = m_ArrowInstance;
            m_ArrowInstance = null;

            if (!arrow)
            {
                arrow = GameObjectPool.main.Rent(m_Prefab, pose.position, pose.rotation, pose);
            }

            Quaternion rot = AngleUtils.NormalizeAngle2D(pose.rotation);
            if (m_StraightShot)
            {
                if (sender.IsLeft)
                {
                    rot = Quaternion.identity;
                }
                else
                {
                    rot = Quaternion.Euler(0, 0, 180);
                }
            }

            arrow.transform.SetParent(GameManager.main.effectContainer, false);
            arrow.transform.SetLocalPositionAndRotation(pose.position, rot);

            var target = GameManager.main.GetTargetCharacter(((CharacterBehaviour)sender).Id);

            arrow.ShootAsync(sender, target.Center).Forget();

        }
    }
}