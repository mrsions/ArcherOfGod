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

        public override void Use(ObjectBehaviour sender, Transform pose)
        {
            ProjectileBehaviour proj = Instantiate(m_Prefab, pose.position, pose.rotation);
            proj.gameObject.SetActive(true);
            proj.Shoot(sender, sender.FindEnemyNormal()).Forget();
        }
    }
}