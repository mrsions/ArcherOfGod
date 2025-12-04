using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class ArrowSkillBehaviour : BaseSkillBehaviour
    {
        //-- Serializable
        public ProjectileBehaviour m_Prefab;

        //-- Private

        //-- Properties


        //------------------------------------------------------------------------------

        internal override bool OnStartSkill(CharacterBehaviour playerBehaviour)
        {
            return base.OnStartSkill(playerBehaviour);
        }

        internal override void OnSkillActivate(ObjectBehaviour sender, Transform pose)
        {
            ProjectileBehaviour proj = GameObjectPool.main.Rent(m_Prefab, pose.position, AngleUtils.NormalizeAngle2D(pose.rotation));
            proj.Shoot(sender, sender.FindEnemyNormal()).Forget();
        }
    }
}