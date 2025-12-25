using System;
using UnityEngine;

namespace AOT
{
    public struct FHitEvent
    {
        public ObjectBehaviour Owner;
        public ProjectileBehaviour Sender;
        public float Damage;
        public Vector2 ContactPosition;
        public float ContactRotation;
        public bool IsCritical;

        public FHitEvent(ObjectBehaviour owner, ProjectileBehaviour sender, float damage, Vector2 contactPosition, float contactRotation, bool isCritical = false)
        {
            Owner = owner;
            Sender = sender;
            Damage = damage;
            ContactPosition = contactPosition;
            ContactRotation = contactRotation;
            IsCritical = isCritical;
        }
    }

    /// <summary>
    /// HP랑 실드 있는 오브젝트 기반 클래스. 맞으면 실드 먼저 깎이고 그다음 HP 깎임.
    /// HP 변경/실드 변경/사망 이벤트 있음. 초기 위치로 왼쪽인지 오른쪽인지 판단함.
    /// 이동이나 스킬은 여기서 안하고 자식 클래스에서 함. 실드 자동회복 없음.
    /// </summary>
    public class ObjectBehaviour : MonoBehaviour
    {
        //-- Serializable
        [Header("Object")]
        [SerializeField] protected Animator m_Animator;
        [SerializeField] private Transform m_Center;
        [SerializeField] private int m_MaxHp;
        [SerializeField] private int m_MaxShield;
        [SerializeField]
        [FormerlySerializedAs("attachTarget")]
        private Transform m_AttachTarget;
        [SerializeField]
        [FormerlySerializedAs("fxOnHit")]
        private GameObject m_FxOnHit;

        //-- Private
        private float m_CurrentHp = 1;
        private float m_CurrentShield;
        private int m_DieHash;
        private int m_HitHash;

        //-- Properties
        public Transform AttachTarget => m_AttachTarget;
        public GameObject FxOnHit => m_FxOnHit;
        public float CurrentHp { get => m_CurrentHp; set => SetCurrentHp(value); }
        public int MaxHp { get => m_MaxHp; set => SetMaxHp(value); }
        public float CurrentShield { get => m_CurrentShield; set => SetCurrentShield(value); }
        public int MaxShield { get => m_MaxShield; set => SetMaxShield(value); }
        public bool IsDead => m_CurrentHp <= 0;
        public bool IsLive => m_CurrentHp > 0;
        public Transform Center => m_Center;
        public bool IsLeft { get; private set; }
        public bool IsRight => !IsLeft;

        //-- Events
        public event Action<ObjectBehaviour, float, float, float> OnChangedHp; // sender, before, after, max
        public event Action<ObjectBehaviour, float, float, float> OnChangedShield; // sender, before, after, max
        public event Action<ObjectBehaviour> OnDead; // sender

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
            IsLeft = transform.position.x < 0;
        }

        //------------------------------------------------------------------------------

        protected virtual void SetCurrentHp(float value)
        {
            if (m_CurrentHp == value || IsDead) return;

            float before = m_CurrentHp;
            m_CurrentHp = Mathf.Clamp(value, 0, m_MaxHp);
            OnChangedHp?.Invoke(this, before, m_CurrentHp, m_MaxHp);

            if (before > 0 && value <= 0)
            {
                m_Animator.SetBool(m_DieHash, true);
                OnDead?.Invoke(this);
            }
        }

        private void SetMaxHp(int value)
        {
            if (m_MaxHp == value || IsDead) return;

            value = Mathf.Max(value, 1);

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
            OnChangedShield?.Invoke(this, before, m_CurrentShield, m_MaxShield);
        }

        private void SetMaxShield(int value)
        {
            if (m_MaxShield == value || IsDead) return;

            value = Mathf.Max(value, 1);

            int delta = Mathf.Max(0, value - m_MaxShield);
            float befShield = m_CurrentShield;

            m_MaxShield = value;
            m_CurrentShield = Mathf.Min(m_CurrentShield + delta, value);

            OnChangedShield?.Invoke(this, befShield, m_CurrentShield, value);
        }

        public bool OnHit(FHitEvent eventData)
        {
            if (IsDead) return false;

            eventData = eventData.Owner.OnAttack(this, eventData);

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

                if (m_FxOnHit != null && eventData.ContactPosition != default)
                {
                    GameObjectPool.main.Rent(m_FxOnHit, eventData.ContactPosition, AngleUtils.GetQuaternion(eventData.ContactRotation), m_AttachTarget);
                }
            }

            return true;
        }

        protected virtual FHitEvent OnAttack(ObjectBehaviour objectBehaviour, FHitEvent eventData)
        {
            return eventData;
        }

        public virtual Vector3 FindEnemy() => Vector2.zero;

        public virtual void GetDamageProperty(out float damage, out bool isCritical)
        {
            damage = 0;
            isCritical = false;
        }
    }
}