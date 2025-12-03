using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class MultipleShotSkillBehaviour : BaseSkillBehaviour
    {
        //-- Serializable
        private int m_ShotCount;
        private int m_PerAngle;

        //-- Private

        //-- Properties


        //------------------------------------------------------------------------------

        private void Start()
        {
        }

        private void Update()
        {
        }

        public override void Use(ObjectBehaviour sender, Transform pose)
        {
            throw new System.NotImplementedException();
        }

    }
}