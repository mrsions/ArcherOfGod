using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AOT
{
    [Serializable]
    public struct FPlayerStatus
    {
        public string name;
        public int hp;
        public int shield;
        public float damage;
        public float criticalDamageFactor;
        public float criticalPercent;
        public float attackSpeed;
        public float castingReduce;
        public float moveSpeed;

        public FPlayerStatus(string name, int hp = 1000, int shield = 100, float damage = 100, float criticalDamageFactor = 2,
                            float criticalPercent = 0.2f, float attackSpeed = 1, float castingReduce = 0, float moveSpeed = 1)
        {
            this.name = name;
            this.hp = hp;
            this.shield = shield;
            this.damage = damage;
            this.criticalDamageFactor = criticalDamageFactor;
            this.criticalPercent = criticalPercent;
            this.attackSpeed = attackSpeed;
            this.castingReduce = castingReduce;
            this.moveSpeed = moveSpeed;
        }
    }

    /// <summary>
    /// 플레이어나 AI가 조종하는 캐릭터. Rigidbody2D로 좌우 이동하고 스킬 5개까지 장착 가능.
    /// 데미지 주면 파워 게이지 참. walk/attack/ground/skill 애니메이션 상태 있음.
    /// FPlayerStatus로 데미지/크리티컬/속도 스탯 관리함. 스킬 쓰는 중엔 조작 안됨.
    /// 점프 없고 수평 이동만 됨. 입력은 PlayerController나 AiController가 해줌.
    /// </summary>
    public class CharacterBehaviour : ObjectBehaviour
    {
        public static readonly Vector3 RealScaleForward = new Vector3(1, 1, -1);
        public static readonly Vector3 RealScaleBackward = new Vector3(1, 1, 1);
        public static readonly Vector3 RealRotationForward = new Vector3(0, 180, 0);
        public static readonly Vector3 RealRotationBackward = new Vector3(0, 0, 0);

        private static class AnimParam
        {
            public static readonly int Walk = Animator.StringToHash("walk");
            public static readonly int Attack = Animator.StringToHash("attack");
            public static readonly int Ground = Animator.StringToHash("ground");
            public static readonly int SkillAnimId = Animator.StringToHash("animId");
            public static readonly int MoveSpeed = Animator.StringToHash("moveSpeed");
            public static readonly int AttackSpeed = Animator.StringToHash("attackSpeed");
        }


        //-- Serializable
        [Header("Player")]
        [SerializeField] private int m_Id;
        [SerializeField] private bool m_IsAi;
        [SerializeField] private bool m_IsPlayer;
        [SerializeField] private float m_MoveSpeed = 1f;
        [SerializeField] private float m_PowerGageMax = 1000;
        [SerializeField] private FPlayerStatus m_PlayerStatus = new FPlayerStatus("Player");

        [Header("Components")]
        [SerializeField] private Transform m_Arrow;
        [SerializeField] private Rigidbody2D m_Rigidbody;

        [Header("Skill")]
        [SerializeField] private BaseSkillBehaviour m_NormalAttack;
        [SerializeField] private List<BaseSkillBehaviour> m_Skills = new();

        //-- Private
        private Vector2 m_InputAxis;
        private Vector3 m_RotationBackward;
        private Vector3 m_RotationForward;
        private Vector3 m_ScaleBackward;
        private Vector3 m_ScaleForward;
        private BaseSkillBehaviour m_CurrentSkill;
        private float m_LastMoveTime;
        private EGameStatus m_GameStatus;
        private float m_Power;
        private GameManager m_GameManager;

        //-- Events
        public event Action<CharacterBehaviour, float, float, float> OnChangedPower; // bef, cur, max

        //-- Properties
        public float CurrentPower { get => m_Power; set => SetPower(value); }
        public float MaxPower => m_PowerGageMax;
        public bool IsAi { get => m_IsAi; set => m_IsAi = value; }
        public bool IsPlayer { get => m_IsPlayer; set => m_IsPlayer = value; }
        public List<BaseSkillBehaviour> Skills { get => m_Skills; set => m_Skills = value; }
        public FPlayerStatus PlayerStatus { get => m_PlayerStatus; set => SetPlayerStatus(value); }
        public float MoveSpeed => m_MoveSpeed * m_PlayerStatus.moveSpeed;
        public Vector2 InputAxis { get => m_InputAxis; set => m_InputAxis = value; }
        public Rigidbody2D Rigidbody => m_Rigidbody;
        public bool IsGround { get; private set; }
        public int Id { get => m_Id; internal set => m_Id = value; }

        public bool CanControl
            => m_CurrentSkill == null
            && IsGround
            && m_GameStatus switch { EGameStatus.Battle => true, EGameStatus.Battle_LimitOver => true, _ => false };

        private void SetPower(float value)
        {
            if (m_Power == value) return;

            var bef = m_Power;
            m_Power = Mathf.Clamp(value, 0, MaxPower);
            OnChangedPower?.Invoke(this, bef, m_Power, MaxPower);
        }


        //------------------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            GameManager.main.AttachCharacter(this);
        }

        protected override void Start()
        {
            base.Start();

            m_ScaleBackward = !IsLeft ? RealScaleForward : RealScaleBackward;
            m_ScaleForward = IsLeft ? RealScaleForward : RealScaleBackward;
            m_RotationBackward = !IsLeft ? RealRotationForward : RealRotationBackward;
            m_RotationForward = IsLeft ? RealRotationForward : RealRotationBackward;
            SetForward(true, true);

            const int SKILL_COUNT = 5;
            if (m_Skills.Count < SKILL_COUNT)
            {
                var newSkills = GetComponentsInChildren<BaseSkillBehaviour>();
                TRandom.Shuffle(newSkills);
                m_Skills = newSkills.Except(new[] { m_NormalAttack }).Take(SKILL_COUNT).ToList();
            }

            Debug.Log($"[Character] RegistOnChangedStatus");
            m_GameManager = GameManager.main;
            m_GameManager.OnChangedStatus += OnChangedGameStatus;

            UpdateAnimatorProperty();
        }

        private void OnChangedGameStatus(GameManager manager, EGameStatus status)
        {
            m_GameStatus = status;
            if (status == EGameStatus.Start)
            {
                m_Animator.SetBool(AnimParam.Attack, true);
            }
        }

#if UNITY_EDITOR|| NOPT
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateAnimatorProperty();
            }
        }
#endif

        private void UpdateAnimatorProperty()
        {

            m_Animator.SetFloat(AnimParam.MoveSpeed, m_PlayerStatus.moveSpeed);
            m_Animator.SetFloat(AnimParam.AttackSpeed, m_PlayerStatus.attackSpeed);
        }

        protected virtual void Update()
        {
            if (IsDead) return;

            if (m_CurrentSkill != null)
            {
                m_Animator.SetBool(AnimParam.Ground, IsGround = false);
                return;
            }

            IsGround = m_Rigidbody.linearVelocity.sqrMagnitude < 0.1f && m_Rigidbody.position.y < 0.1f;
            m_Animator.SetBool(AnimParam.Ground, IsGround);

            switch (m_GameStatus)
            {
                case EGameStatus.Battle:
                case EGameStatus.Battle_LimitOver:
                    {
                        m_Animator.speed = 1f;
                        if (m_InputAxis.x == 0)
                        {
                            // 일정 시간만큼 이동명령을 유지한다.
                            if (m_LastMoveTime != 0)
                            {
                                if (m_LastMoveTime + GameSettings.main.move_input_delay > Time.time)
                                {
                                    break;
                                }
                                m_LastMoveTime = 0;
                            }

                            SetForward(true, true);
                            m_Animator.SetBool(AnimParam.Walk, false);
                        }
                        else
                        {
                            m_Animator.SetBool(AnimParam.Walk, true);
                            SetForward(m_InputAxis.x > 0, false);
                            m_LastMoveTime = Time.time;
                        }

                    }
                    break;
                default:
                    return;
            }
        }

        public void SetForward(bool forward, bool relative)
        {
            Vector3 scale, rotation;
            if (relative)
            {
                scale = forward ? m_ScaleForward : m_ScaleBackward;
                rotation = forward ? m_RotationForward : m_RotationBackward;
            }
            else
            {
                scale = forward ? RealScaleForward : RealScaleBackward;
                rotation = forward ? RealRotationForward : RealRotationBackward;
            }
            transform.localScale = scale;
            transform.localEulerAngles = rotation;
        }

        private void FixedUpdate()
        {
            switch (m_GameStatus)
            {
                case EGameStatus.Battle:
                case EGameStatus.Battle_LimitOver:
                    {
                        if (m_CurrentSkill != null) return;

                        if (!IsGround) return;

                        if (m_InputAxis != Vector2.zero)
                        {
                            Vector2 moveDelta = Vector2.right * (MoveSpeed * m_InputAxis.x * Time.fixedDeltaTime);
                            m_Rigidbody.MovePosition(m_Rigidbody.position + moveDelta);
                        }
                    }
                    break;
            }
        }

        public bool StartSkill(BaseSkillBehaviour skill)
        {
            if (m_CurrentSkill != null) return false;

            if (!skill.OnStartSkill(this)) return false;

            skill.Consume(this);

            m_CurrentSkill = skill;
            if (skill.AnimationId > 0)
            {
                m_Animator.SetInteger(AnimParam.SkillAnimId, skill.AnimationId);
                m_Animator.Play("SkillAnim");
            }
            else
            {
                OnAnimEndSkill();
            }
            return true;
        }

        public void OnAnimPrepareArrow()
        {
            if (m_CurrentSkill != null)
            {
                m_CurrentSkill.OnSkillPrepare(this, m_Arrow);
            }
            else
            {
                m_NormalAttack.OnSkillPrepare(this, m_Arrow);
            }
        }

        public void OnAnimShot()
        {
            if (m_CurrentSkill != null)
            {
                m_CurrentSkill.OnSkillActivate(this, m_Arrow);
            }
            else
            {
                m_NormalAttack.OnSkillActivate(this, m_Arrow);
            }
        }

        public void OnAnimEndSkill()
        {
            if (m_CurrentSkill == null) return;

            if (m_CurrentSkill.IsResetVelocityOnEnd)
            {
                m_Rigidbody.linearVelocity = default;
                m_Rigidbody.angularVelocity = default;
            }
            m_CurrentSkill = null;
        }

        protected override FHitEvent OnAttack(ObjectBehaviour objectBehaviour, FHitEvent eventData)
        {
            eventData.Damage *= m_PlayerStatus.damage;

            if (TRandom.Value < m_PlayerStatus.criticalPercent)
            {
                eventData.Damage *= m_PlayerStatus.criticalDamageFactor;
                eventData.IsCritical = true;
            }

            CurrentPower += eventData.Damage;

            return eventData;
        }

        private void SetPlayerStatus(FPlayerStatus value)
        {
            MaxHp = value.hp;
            MaxShield = value.shield;
            UpdateAnimatorProperty();
        }

        protected virtual void OnDestroy()
        {
            if (m_GameManager != null)
                m_GameManager.OnChangedStatus -= OnChangedGameStatus;
        }
    }
}