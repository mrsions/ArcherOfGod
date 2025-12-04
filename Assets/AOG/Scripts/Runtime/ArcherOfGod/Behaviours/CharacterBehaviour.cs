using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

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

    public class CharacterBehaviour : ObjectBehaviour
    {
        public static readonly Vector3 PlayerScaleForward = new Vector3(1, 1, -1);
        public static readonly Vector3 PlayerScaleBackward = new Vector3(1, 1, 1);
        public static readonly Vector3 PlayerRotationForward = new Vector3(0, 180, 0);
        public static readonly Vector3 PlayerRotationBackward = new Vector3(0, 0, 0);


        //-- Serializable
        [Header("Player")]
        [SerializeField] private int m_Id;
        [SerializeField] private bool m_IsAi;
        [SerializeField] private bool m_IsPlayer;
        [SerializeField] private Transform m_Arrow;
        [SerializeField] private Rigidbody2D m_Rigidbody;
        [SerializeField] private float m_MoveSpeed = 1f;
        [SerializeField] private FPlayerStatus m_PlayerStatus = new FPlayerStatus("Player");

        [Header("Skill")]
        [SerializeField] private BaseSkillBehaviour m_NormalAttack;

        //-- Private
        private Vector2 m_InputAxis;
        private Vector3 m_RotationBackward;
        private Vector3 m_RotationForward;
        private Vector3 m_ScaleBackward;
        private Vector3 m_ScaleForward;
        private int m_WalkHash;
        private int m_AttackHash;
        private int m_SkillAnimIdHash;
        private int m_MoveSpeedHash;
        private int m_AttackSpeedHash;
        private BaseSkillBehaviour m_CurrentSkill;
        [SerializeField]
        private List<BaseSkillBehaviour> m_Skills = new();
        private float m_LastMoveTime;
        private EGameStatus m_GameStatus;

        //-- Properties
        public bool IsAi { get => m_IsAi; set => m_IsAi = value; }
        public bool IsPlayer { get => m_IsPlayer; set => m_IsPlayer = value; }
        public List<BaseSkillBehaviour> Skills { get => m_Skills; set => m_Skills = value; }
        public FPlayerStatus PlayerStatus { get => m_PlayerStatus; set => SetPlayerStatus(value); }
        public float MoveSpeed => m_MoveSpeed * m_PlayerStatus.moveSpeed;

        public Vector2 InputAxis { get => m_InputAxis; set => m_InputAxis = value; }
        public Rigidbody2D Rigidbody => m_Rigidbody;
        public bool IsGround { get; private set; }

        //------------------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();

            m_ScaleBackward = !IsLeft ? PlayerScaleForward : PlayerScaleBackward;
            m_ScaleForward = IsLeft ? PlayerScaleForward : PlayerScaleBackward;
            m_RotationBackward = !IsLeft ? PlayerRotationForward : PlayerRotationBackward;
            m_RotationForward = IsLeft ? PlayerRotationForward : PlayerRotationBackward;
            SetForward(true);

            GetComponentsInChildren(true, m_Skills);
            for (int i = 0; i < m_Skills.Count; i++)
            {
                if (m_Skills[i] == m_NormalAttack)
                {
                    m_Skills.RemoveAt(i--);
                    break;
                }
            }

            m_WalkHash = Animator.StringToHash("walk");
            m_AttackHash = Animator.StringToHash("attack");
            m_SkillAnimIdHash = Animator.StringToHash("animId");
            m_MoveSpeedHash = Animator.StringToHash("moveSpeed");
            m_AttackSpeedHash = Animator.StringToHash("attackSpeed");

            GameManager.main.OnChangedStatus += OnChangedGameStatus;

            UpdateAnimatorProperty();
        }

        private void OnChangedGameStatus(GameManager manager, EGameStatus status)
        {
            m_GameStatus = status;
            if (status == EGameStatus.Start)
            {
                m_Animator.SetBool(m_AttackHash, true);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateAnimatorProperty();
            }
        }

        private void UpdateAnimatorProperty()
        {
            m_Animator.SetFloat(m_MoveSpeedHash, m_PlayerStatus.moveSpeed);
            m_Animator.SetFloat(m_AttackSpeedHash, m_PlayerStatus.attackSpeed);
        }

        protected virtual void Update()
        {
            if (IsDead || m_CurrentSkill != null) return;

            IsGround = m_Rigidbody.linearVelocity.sqrMagnitude < 0.1f;

            switch (m_GameStatus)
            {
                case EGameStatus.Battle:
                case EGameStatus.Battle_LimitOver:
                    {
                        m_Animator.speed = 1f;
                        if (m_InputAxis.x == 0)
                        {
                            // 일장 시간만큼 이동명령을 유지한다.
                            if (m_LastMoveTime != 0)
                            {
                                if (m_LastMoveTime + GameSettings.main.move_input_delay < Time.time)
                                {
                                    break;
                                }
                                m_LastMoveTime = 0;
                            }

                            SetForward(true);
                            m_Animator.SetBool(m_WalkHash, false);
                        }
                        else
                        {
                            m_Animator.SetBool(m_WalkHash, true);
                            SetForward(m_InputAxis.x > 0);
                            m_LastMoveTime = Time.time;
                        }

                    }
                    break;
                default:
                    return;
            }
        }

        private void SetForward(bool forward)
        {
            if (forward)
            {
                transform.localScale = m_ScaleForward;
                transform.localEulerAngles = m_RotationForward;
            }
            else
            {
                transform.localScale = m_ScaleBackward;
                transform.localEulerAngles = m_RotationBackward;
            }
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

            m_CurrentSkill = skill;
            if (skill.AnimationId > 0)
            {
                m_Animator.SetInteger(m_SkillAnimIdHash, skill.AnimationId);
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
            m_Rigidbody.Sleep();
            m_CurrentSkill = null;
        }

        public override Vector3 FindEnemyNormal()
        {
            return GameManager.main.GetTargetCharacter(m_Id).CenterPosition;
        }

        public override void GetDamageProperty(out float damage, out bool isCritical)
        {
            isCritical = m_PlayerStatus.criticalPercent < TRandom.Value;
            damage = m_PlayerStatus.damage;

            if (isCritical)
            {
                damage *= m_PlayerStatus.criticalDamageFactor;
            }
        }

        private void SetPlayerStatus(FPlayerStatus value)
        {
            MaxHp = value.hp;
            MaxShield = value.shield;
            UpdateAnimatorProperty();
        }

    }
}