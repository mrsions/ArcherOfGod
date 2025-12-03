using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class PlayerBehaviour : ObjectBehaviour
    {
        //-- Serializable
        [Header("Player")]
        [SerializeField] private Transform m_Arrow;
        [SerializeField] private Rigidbody2D m_Rigidbody;
        [SerializeField] private float m_MoveSpeed = 1f;

        [Header("Skill")]
        [SerializeField] private BaseSkillBehaviour m_NormalAttack;
        [SerializeField] private BaseSkillBehaviour[] m_Skills;


        [Header("Control")]
        [SerializeField] private InputActionReference m_InputMoveAction;

        //-- Private
        private bool m_IsPlayer;
        private Vector2 inputAxis;
        private Vector3 m_ScaleBackward;
        private Vector3 m_ScaleForward;
        private int m_WalkHash;
        private int m_AttackHash;

        //-- Properties
        public bool IsPlayer { get => m_IsPlayer; set => m_IsPlayer = value; }
        public BaseSkillBehaviour[] Skills { get => m_Skills; set => m_Skills = value; }


        //------------------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();

            m_IsPlayer = transform.position.x < 0;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            Vector3 mscale = scale;
            mscale.x = -scale.x;
            m_ScaleBackward = m_IsPlayer ? scale : mscale;
            m_ScaleForward = !m_IsPlayer ? scale : mscale;

            m_WalkHash = Animator.StringToHash("walk");
            m_AttackHash = Animator.StringToHash("attack");

            // 임시 게임매니저
            UniTask.Void(async () =>
            {
                await UniTask.Delay(1000);

                m_Animator.SetBool(m_AttackHash, true);
            });
        }

        private void Update()
        {
            if (IsDead) return;
            if (!IsPlayer) return;

            m_Animator.speed = 1f;
            inputAxis = m_InputMoveAction.action.ReadValue<Vector2>();
            if (inputAxis.x == 0)
            {
                transform.localScale = m_ScaleForward;
                m_Animator.SetBool(m_WalkHash, false);
            }
            else
            {
                m_Animator.SetBool(m_WalkHash, true);
                if (inputAxis.x < 0)
                {
                    transform.localScale = m_ScaleBackward;
                }
                else
                {
                    transform.localScale = m_ScaleForward;
                }
            }
        }

        private void FixedUpdate()
        {
            if (inputAxis != Vector2.zero)
            {
                Vector3 pos = transform.position;
                pos += (Vector3)(Vector2.right * (m_MoveSpeed * inputAxis.x * Time.fixedDeltaTime));
                pos += Physics.gravity * Time.fixedDeltaTime;
                m_Rigidbody.MovePosition(pos);
            }
        }

        public void OnAnimShootNormalArrow()
        {
            m_NormalAttack.Use(this, m_Arrow);
        }

        public override Vector3 FindEnemyNormal()
        {
            if(IsPlayer)
            {
                return GameManager.main.enemy.CenterPosition;
            }
            else
            {
                return GameManager.main.player.CenterPosition;
            }
        }
    }
}