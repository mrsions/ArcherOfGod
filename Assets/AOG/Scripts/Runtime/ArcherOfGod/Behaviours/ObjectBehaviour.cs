using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class ObjectBehaviour : MonoBehaviour
    {
        //-- Serializable
        [Header("Object")]
        [SerializeField] protected Animator m_Animator;
        [SerializeField] private Transform m_Center;
        [SerializeField] private int m_MaxHp;
        [SerializeField] private int m_MaxShield;

        //-- Events
        public event Action<ObjectBehaviour, float, float, float> OnChangedHp; // sender, before, after, max
        public event Action<ObjectBehaviour, float, float, float> OnChangedShield; // sender, before, after, max
        public event Action<ObjectBehaviour> OnDead; // sender, before, after, max

        //-- Private
        private float m_CurrentHp;
        private float m_CurrentShield;
        private int m_DieHash;

        //-- Properties
        public float CurrentHp { get => m_CurrentHp; set => SetCurrentHp(value); }
        public float CurrentShield { get => m_CurrentShield; set => SetCurrentShield(value); }
        public bool IsDead => m_CurrentHp <= 0;
        public bool IsLive => m_CurrentHp > 0;
        public Vector3 CenterPosition => m_Center.position;

        //------------------------------------------------------------------------------

        protected virtual void Awake()
        {
            m_DieHash = Animator.StringToHash("die");

            m_CurrentHp = m_MaxHp;
            m_CurrentShield = m_MaxShield;
        }

        protected virtual void Start()
        {
        }

        //------------------------------------------------------------------------------

        protected virtual void SetCurrentHp(float value)
        {
            if (m_CurrentHp == value || IsDead) return;

            float before = m_CurrentHp;
            m_CurrentHp = value;
            OnChangedHp?.Invoke(this, before, value, m_MaxHp);

            if (before > 0 && value <= 0)
            {
                m_Animator.SetBool(m_DieHash, true);
                OnDead?.Invoke(this);
            }
        }

        protected virtual void SetCurrentShield(float value)
        {
            if (m_CurrentShield == value || IsDead) return;

            float before = m_CurrentShield;
            m_CurrentShield = value;
            OnChangedShield?.Invoke(this, before, value, m_MaxShield);
        }

        internal void OnHit(ObjectBehaviour owner, ProjectileBehaviour projectileBehaviour)
        {
        }

        public virtual Vector3 FindEnemyNormal() => Vector2.zero;
    }
}