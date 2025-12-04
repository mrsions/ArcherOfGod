using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class DashSkillBehaviour : BaseSkillBehaviour
    {
        //-- Serializable
        [SerializeField] private ProjectileBehaviour m_Prefab;
        [SerializeField] private float m_Force = 10;
        private ForceMode2D m_ForceMode = ForceMode2D.Impulse;

        //-- Private

        //-- Properties


        //------------------------------------------------------------------------------

        internal override bool OnStartSkill(CharacterBehaviour playerBehaviour)
        {
            if (!base.OnStartSkill(playerBehaviour)) return false;

            playerBehaviour.Rigidbody.AddForce(playerBehaviour.transform.right * m_Force, m_ForceMode);
            return true;
        }

        internal override void OnSkillActivate(ObjectBehaviour sender, Transform pose)
        {
            ProjectileBehaviour proj = GameObjectPool.main.Rent(m_Prefab, pose.position, AngleUtils.NormalizeAngle2D(pose.rotation));
            proj.Shoot(sender, sender.FindEnemyNormal()).Forget();
        }
    }
}