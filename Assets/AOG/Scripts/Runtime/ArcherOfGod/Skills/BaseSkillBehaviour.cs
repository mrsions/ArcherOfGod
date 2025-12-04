using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AOT
{
    public enum ESkillStatus
    {
        None,
        Ready,
        Cooldown,
        Lock
    }

    public abstract class BaseSkillBehaviour : MonoBehaviour
    {
        //-- Serializable
        [Header("Common")]
        [SerializeField] private string m_SkillName;
        [SerializeField] private float m_Duration;
        [SerializeField] private int m_AnimationId;
        [SerializeField] private GameObject m_FxCasting;

        [Header("Factors")]
        [SerializeField] private float m_FactorDamage = 1;
        [SerializeField] private float m_FactorArmor = 1;

        //-- Event
        public UnityEvent<BaseSkillBehaviour, ESkillStatus, ESkillStatus> OnChangedStatus;

        //-- Private
        private float m_NextUseTime;
        private float m_CurrentDuration;
        private ESkillStatus m_Status;
        private CharacterBehaviour m_Owner;

        public ESkillStatus Status { get => m_Status; private set => SetStatus(value, Duration); }

        //-- Properties
        public virtual string SkillName => m_SkillName;
        public virtual float Duration => m_Duration * (1 - (m_Owner?.PlayerStatus.castingReduce ?? 0));
        public virtual int AnimationId => m_AnimationId;
        public virtual GameObject FxCasting => m_FxCasting;

        public virtual float FactorDamage => m_FactorDamage;
        public virtual float FactorArmor => m_FactorArmor;

        public float RemainDelay => Mathf.Max(0, m_NextUseTime - Time.time);
        public float RemainPercent => Mathf.Clamp01(RemainDelay / Duration);

        public bool IsReady => m_Status == ESkillStatus.Ready;
        public bool IsCooldown => m_Status == ESkillStatus.Cooldown;

        //------------------------------------------------------------------------------

        private void Awake()
        {
            m_Owner = GetComponentInParent<CharacterBehaviour>();
        }

        public void SetForceDelay(float delay)
        {
            SetStatus(ESkillStatus.Cooldown, delay);
        }

        protected virtual void SetStatus(ESkillStatus value, float duration)
        {
            if (m_Status == value) return;

            ESkillStatus bef = m_Status;
            m_Status = value;

            UpdateStatus(value, duration);

            OnChangedStatus?.Invoke(this, bef, value);
        }

        protected virtual void UpdateStatus(ESkillStatus value, float duration)
        {
            switch (value)
            {
                case ESkillStatus.None:
                    break;
                case ESkillStatus.Ready:
                    m_NextUseTime = 0;
                    break;
                case ESkillStatus.Cooldown:
                    m_NextUseTime = Time.time + duration;
                    m_CurrentDuration = duration;
                    WaitForCooldowngAsync().Forget();
                    break;
                case ESkillStatus.Lock:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async UniTask WaitForCooldowngAsync()
        {
            float endTime = m_NextUseTime;
            float duration = m_CurrentDuration;

            await UniTask.Delay((int)(duration * 1000), cancellationToken: destroyCancellationToken);

            if (m_Status != ESkillStatus.Cooldown) return;
            if (endTime != m_NextUseTime || m_CurrentDuration != duration) return;

            Status = ESkillStatus.Ready;
        }

        //------------------------------------------------------------------------------

        internal virtual bool OnStartSkill(CharacterBehaviour playerBehaviour)
        {
            return true;
        }

        internal virtual void OnSkillActivate(ObjectBehaviour sender, Transform pose)
        {
        }
    }
}