using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public readonly struct FHitEvent
    {
        public readonly ObjectBehaviour Owner;
        public readonly ProjectileBehaviour Sender;
        public readonly float Damage;
        public readonly Vector2 ContactPosition;
        public readonly float ContactRotation;
        public readonly bool IsCritical;

        public FHitEvent(ObjectBehaviour owner, ProjectileBehaviour sender, float damage, Vector2 contactPosition, float contactRotation, bool isCritical)
        {
            Owner = owner;
            Sender = sender;
            Damage = damage;
            ContactPosition = contactPosition;
            ContactRotation = contactRotation;
            IsCritical = isCritical;
        }
    }

    public class ObjectBehaviour : MonoBehaviour
    {
        //-- Serializable
        [Header("Object")]
        [SerializeField] protected Animator m_Animator;
        [SerializeField] private Transform m_Center;
        [SerializeField] private int m_MaxHp;
        [SerializeField] private int m_MaxShield;
        public Transform attachTarget;
        public GameObject fxOnHit;

        //-- Events
        public event Action<ObjectBehaviour, float, float, float> OnChangedHp; // sender, before, after, max
        public event Action<ObjectBehaviour, float, float, float> OnChangedShield; // sender, before, after, max
        public event Action<ObjectBehaviour> OnDead; // sender, before, after, max

        //-- Private
        private float m_CurrentHp;
        private float m_CurrentShield;
        private int m_DieHash;
        private int m_HitHash;

        //-- Properties
        public float CurrentHp { get => m_CurrentHp; set => SetCurrentHp(value); }
        public int MaxHp { get => m_MaxHp; set => SetMaxHp(value); }
        public float CurrentShield { get => m_CurrentShield; set => SetCurrentShield(value); }
        public int MaxShield { get => m_MaxShield; set => SetMaxShield(value); }
        public bool IsDead => m_CurrentHp <= 0;
        public bool IsLive => m_CurrentHp > 0;
        public Vector3 CenterPosition => m_Center.position;

        //------------------------------------------------------------------------------

        protected virtual void Awake()
        {
            m_DieHash = Animator.StringToHash("die");
            m_HitHash = Animator.StringToHash("hit");

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
            m_CurrentHp = Mathf.Clamp(value, 0, m_MaxHp);
            OnChangedHp?.Invoke(this, before, value, m_MaxHp);

            if (before > 0 && value <= 0)
            {
                m_Animator.SetBool(m_DieHash, true);
                OnDead?.Invoke(this);
            }
        }

        private void SetMaxHp(int value)
        {
            if (m_MaxHp == value || IsDead) return;

            value = Mathf.Max(value, 1); ;

            int delta = Mathf.Max(0, value - m_MaxHp);
            float befHp = m_CurrentHp;

            m_MaxHp = value;
            m_CurrentHp = Mathf.Min(m_CurrentHp + delta, value);

            OnChangedHp?.Invoke(this, befHp, m_CurrentHp, value);
        }

        protected virtual void SetCurrentShield(float value)
        {
            if (m_CurrentShield == value || IsDead) return;

            float before = m_CurrentShield;
            m_CurrentShield = value;
            OnChangedShield?.Invoke(this, before, value, m_MaxShield);
        }

        private void SetMaxShield(int value)
        {
            if (m_MaxShield == value || IsDead) return;

            value = Mathf.Max(value, 1); ;

            int delta = Mathf.Max(0, value - m_MaxShield);
            float befShield = m_CurrentShield;

            m_MaxShield = value;
            m_CurrentShield = Mathf.Min(m_CurrentShield + delta, value);

            OnChangedShield?.Invoke(this, befShield, m_CurrentShield, value);
        }

        public bool OnHit(FHitEvent eventData)
        {
            if (IsDead) return false;

            if(eventData.IsCritical)
            {
                GameObjectPool.main.Rent(GameSettings.main.criticalPrefab, eventData.ContactPosition)?.SetText(eventData.Damage.ToString("N0"));
            }
            else
            {
                GameObjectPool.main.Rent(GameSettings.main.damagePrefab, eventData.ContactPosition)?.SetText(eventData.Damage.ToString("N0"));
            }

            float edmg = eventData.Damage;
            if (m_CurrentShield > 0)
            {
                if (edmg <= m_CurrentShield)
                {
                    CurrentShield -= edmg;
                    edmg = 0;
                }
                else
                {
                    edmg -= m_CurrentShield;
                    CurrentShield = 0;
                }
            }

            if (edmg > 0)
            {
                CurrentHp -= edmg;
                m_Animator.SetTrigger(m_HitHash);

                if (fxOnHit != null && eventData.ContactPosition != default)
                {
                    GameObjectPool.main.Rent(fxOnHit, eventData.ContactPosition, AngleUtils.GetQuaternion(eventData.ContactRotation), attachTarget);
                }
            }

            return true;
        }

        public virtual Vector3 FindEnemyNormal() => Vector2.zero;

        public virtual void GetDamageProperty(out float damage, out bool isCritical)
        {
            damage = 0;
            isCritical = false;
        }
    }
}