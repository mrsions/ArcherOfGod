using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace AOT
{
    public enum ESkillStatus
    {
        None,
        Ready,
        Cooldown,
        Lock
    }

    /// <summary>
    /// 스킬 기반 클래스. Ready/Cooldown/Lock 상태 있고 쿨타임 관리함.
    /// 캐스팅감소 스탯 적용되고 데미지/방어 계수 있음. AnimationId로 애니메이션 연동.
    /// 상태 변경 이벤트로 UI 바인딩 가능. 마나 없고 차지 시스템 없음. 시작하면 취소 안됨.
    /// </summary>
    public abstract class BaseSkillBehaviour : MonoBehaviour
    {
        //-- Serializable
        [Header("Common")]
        [SerializeField] private Sprite m_Icon;
        [SerializeField] private string m_SkillName;
        [SerializeField] private float m_Duration;
        [SerializeField] private int m_AnimationId;
        [SerializeField] private GameObject m_FxCasting;
        [SerializeField] private bool m_ResetVelocityOnEnd;

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

        //-- Properties
        public ESkillStatus Status { get => m_Status; set => SetStatus(value, Duration); }
        public Sprite Icon { get => m_Icon; set => m_Icon = value; }
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
        public bool IsResetVelocityOnEnd => m_ResetVelocityOnEnd;

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

        internal virtual bool OnStartSkill(CharacterBehaviour sender)
        {
            return IsReady;
        }

        internal virtual void OnSkillActivate(ObjectBehaviour sender, Transform pose)
        {
        }

        internal virtual void OnSkillPrepare(CharacterBehaviour sender, Transform pose)
        {
        }

        internal virtual void Consume(CharacterBehaviour characterBehaviour)
        {
            Status = ESkillStatus.Cooldown;
        }
    }
}