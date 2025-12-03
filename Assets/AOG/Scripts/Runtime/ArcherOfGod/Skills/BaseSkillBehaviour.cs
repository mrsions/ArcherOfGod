using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AOT
{
    public abstract class BaseSkillBehaviour : MonoBehaviour
    {
        //-- Serializable
        [Header("Common")]
        [SerializeField] private string m_SkillName;
        [SerializeField] private float m_Duration;
        [SerializeField] private string m_AnimationName;
        [SerializeField] private GameObject m_FxCasting;

        [Header("Factors")]
        [SerializeField] private float m_FactorDamage = 1;
        [SerializeField] private float m_FactorArmor = 1;

        //-- Event
        public UnityEvent<BaseSkillBehaviour> OnUsed;
        public UnityEvent<BaseSkillBehaviour> OnReady;

        //-- Private
        private float m_NextUseTime;

        //-- Properties
        public virtual string SkillName => m_SkillName;
        public virtual float Duration => m_Duration;
        public virtual string AnimationName => m_AnimationName;
        public virtual GameObject FxCasting => m_FxCasting;

        public virtual float FactorDamage => m_FactorDamage;
        public virtual float FactorArmor => m_FactorArmor;

        public bool CanUse => m_NextUseTime <= Time.time;
        public bool IsWait => !(CanUse);

        public float RemainDelay => Mathf.Max(0, m_NextUseTime - Time.time);
        public float RemainPercent => Mathf.Clamp01(RemainDelay / Duration);


        //------------------------------------------------------------------------------

        public void SetDelay(float delay)
        {
            m_NextUseTime = Time.time + delay;
        }

        public abstract void Use(ObjectBehaviour sender, Transform pose);
    }
}