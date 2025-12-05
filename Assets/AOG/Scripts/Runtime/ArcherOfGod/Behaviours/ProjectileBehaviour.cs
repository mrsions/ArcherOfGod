using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AOT
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        public enum EDeactiveType
        {
            Return,
            Deactive,
            Destroy
        }

        private class Bullet : MonoBehaviour
        {
            public ProjectileBehaviour Parent;

            private void OnCollisionEnter2D(Collision2D collision)
            {
                Parent?.OnCollision2D(collision.collider);
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                Parent?.OnCollision2D(collision);
            }
        }

        //-- Serializable
        [SerializeField] private GameObject m_ArrowPrefab;
        [SerializeField] private Transform m_Arrow;
        [SerializeField] private Rigidbody2D m_Rigidbody;
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private ParticleSystem m_Particle;
        [SerializeField] private Collider2D[] m_Colliders;
        [SerializeField] private float m_AutoDestroySec = 5f;
        //[SerializeField] private AnimationCurve m_DurationPerDistance = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField] private Vector2 m_Damage = new Vector2(1, 1.1f);
        [SerializeField] private float m_ArrowSpeed = 0.3f;
        [SerializeField] private float m_BezierPower = 0.3f;
        [SerializeField] private float m_FadeDuration = 0.5f;
        [SerializeField] private EDeactiveType m_DeactiveType = EDeactiveType.Return;

        [Tooltip("대상의 도착위치 수정. (좌측기준좌표)")]
        [SerializeField] private Vector2 m_TargetOffset;
        [Tooltip("true일 경우 지정된 rotation으로 도착한다. (좌측기준좌표)")]
        [SerializeField] private bool m_TargetRotationOverride;
        [Tooltip("m_TargetRotationOverride가 true일 경우 사용된다.")]
        [SerializeField] private float m_TargetRotationOverrideValue;

        [Tooltip("true일 경우 정해진 m_ArrowSpeed시간에 맞춰 속도가 조절됩니다.")]
        [SerializeField] private bool m_UseFixedDuration;

        [Tooltip("true일 대상을 향해 직선으로 날아가며 조준할때부터 대상을 바라보도록한다.")]
        [SerializeField] private bool m_UseStright;

        [SerializeField] private ProjectileBehaviour[] children;

        [Header("HitRandom")]
        [SerializeField] private Vector2Int m_HitSkipMs = new(50, 100); // x:고정대기 + y:랜덤대기(0~y)
        [SerializeField] private float m_HitRandomRotation = 30; // x:고정대기 + y:랜덤대기(0~y)


        //-- Private
        private Bullet m_Bullet;
        private bool m_IsCollision;
        private bool m_IsHitObject;
        private int m_AsyncId;
        private Vector3 m_RigidbodyInitPos;
        private Vector3 m_RigidbodyInitRot;

        //-- Properties
        public ObjectBehaviour Owner { get; private set; }
        public bool UseStraight { get => m_UseStright; set => m_UseStright = value; }

        //------------------------------------------------------------------------------

        private void Awake()
        {
            m_Bullet = m_Rigidbody.gameObject.AddComponent<Bullet>();
            m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            m_RigidbodyInitPos = m_Rigidbody.transform.localPosition;
            m_RigidbodyInitRot = m_Rigidbody.transform.localEulerAngles;

            for (int i = 0; i < children.Length; i++)
            {
                ProjectileBehaviour child = children[i];
                child.m_DeactiveType = EDeactiveType.Deactive;
            }
        }

        private void OnEnable()
        {
            Clear();
            m_Particle.Stop(true);
        }

        private void OnDisable()
        {
            Clear();
        }

        private void Clear()
        {
            m_AsyncId++;
            m_IsCollision = false;
            m_IsHitObject = false;

            Color color = m_Renderer.color;
            color.a = 1;
            m_Renderer.color = color;

            m_Rigidbody.transform.localPosition = m_RigidbodyInitPos;
            m_Rigidbody.transform.localEulerAngles = m_RigidbodyInitRot;

            m_Bullet.Parent = null;

            for (int i = 0; i < m_Colliders.Length; i++)
            {
                Collider2D col = m_Colliders[i];
                col.enabled = true;
            }

            for (int i = 0; i < children.Length; i++)
            {
                ProjectileBehaviour child = children[i];
                child.gameObject.SetActive(true);
            }
        }

        public async UniTask LookAtEnemyAsync(Transform target)
        {
            int id = ++m_AsyncId;
            while (id == m_AsyncId)
            {
                // 조준 상태에서 노리는것이기때문에 projectile 자체가 움직인다.
                Vector2 dir = target.position - transform.position;
                transform.rotation = Quaternion.LookRotation(dir);

                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            }
        }

        public async UniTask ShootAsync(ObjectBehaviour owner, Vector2 tPos)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.ShootAsync(owner, tPos).Forget();
                }
            }

            if (owner.IsLeft)
            {
                tPos += m_TargetOffset;
            }
            else
            {
                tPos += m_TargetOffset.SetX(-m_TargetOffset.x);
            }

            int id = ++m_AsyncId;
            this.Owner = owner;

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            Vector3 mPos = m_Rigidbody.position;
            float mRot = m_Rigidbody.rotation;
            float tRot = AngleUtils.Inverse(mRot);

            if (m_TargetRotationOverride)
            {
                if (owner.IsLeft)
                {
                    tRot = m_TargetRotationOverrideValue;
                }
                else
                {
                    tRot = AngleUtils.Inverse(m_TargetRotationOverrideValue);
                }
            }

            FBezier bezier = new FBezier(mPos, tPos, mRot, tRot, m_BezierPower);

#if UNITY_EDITOR
            m_Editor_Bezier = bezier;
#endif

            float distance = bezier.Distance();
            float move = 0;

            var arrowSpeed = m_ArrowSpeed;
            if (m_UseFixedDuration)
            {
                arrowSpeed = distance / arrowSpeed;
            }

            Vector2 befPos = mPos;
            bool isStartInLeft = mPos.x < 0;

            m_Particle.Play(true);

            // 도착할때까지 혹은 이동시간이 예상의 2배를 넘을때까지 이동
            while (!m_IsCollision && move < distance)
            {
                move += arrowSpeed * Time.deltaTime;

                // 진행도
                float ratio = Mathf.Clamp01(move / distance);

                // 새 위치 
                Vector2 nPos = bezier.Evaluate(ratio);

                float angle = Vector2.SignedAngle(Vector2.right, nPos - befPos);
                m_Rigidbody.MovePositionAndRotation(nPos, angle);

                befPos = nPos;

                if (m_Bullet.Parent == null)
                {
                    if (isStartInLeft)
                    {
                        if (nPos.x > 0)
                        {
                            m_Bullet.Parent = this;
                            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
                        }
                    }
                    else
                    {
                        if (nPos.x < 0)
                        {
                            m_Bullet.Parent = this;
                            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
                        }
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
                if (id != m_AsyncId) return;
            }

            if (!m_IsCollision)
            {
                float destroyLimit = Time.time + m_AutoDestroySec;

                Vector2 dir = AngleUtils.ToDirection(m_Rigidbody.rotation);
                while (!m_IsCollision && Time.time < destroyLimit)
                {
                    m_Rigidbody.MovePosition(m_Rigidbody.position + (dir * (arrowSpeed * Time.deltaTime)));

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
                    if (id != m_AsyncId) return;
                }
            }

            if (m_IsHitObject) return;

            StopGeneration();

            // 랜더링 사라지기
            await m_Renderer.DOFade(0, m_FadeDuration).ToUniTask();

            // 삭제
            ReturnOrDeactive();
        }

        private void ReturnOrDeactive()
        {
            switch (m_DeactiveType)
            {
                case EDeactiveType.Return:
                    this.ReturnPool();
                    break;
                case EDeactiveType.Deactive:
                    gameObject.SetActive(false);
                    break;
                case EDeactiveType.Destroy:
                    Destroy(gameObject);
                    break;
            }
        }

        private void StopGeneration()
        {
            // stop generate collision
            m_Rigidbody.Sleep();
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                Collider2D col = m_Colliders[i];
                col.enabled = false;
            }

            // stop generate particle
            m_Particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnCollision2D(Collider2D collision)
        {
            var obj = collision.GetComponentInParent<ObjectBehaviour>();
            if (obj != null && !m_IsCollision)
            {
                // 주인과 접촉한경우 리턴
                if (obj == Owner)
                {
                    return;
                }

                m_IsHitObject = true;

                Owner.GetDamageProperty(out float damage, out bool isCritical);

                float dmg = damage * Random.Range(m_Damage.x, m_Damage.y);
                FHitEvent hitEvent = new FHitEvent(Owner, this, dmg, m_Rigidbody.position, m_Rigidbody.rotation, isCritical);
                obj.OnHit(hitEvent);

                HitAsync(obj).Forget();
            }
            else
            {
                m_IsCollision = true;
            }

        }

        private async UniTask HitAsync(ObjectBehaviour obj)
        {
            StopGeneration();

            await UniTask.Delay(Random.Range(m_HitSkipMs.x, m_HitSkipMs.y));

            Quaternion rot = m_Arrow.rotation * Quaternion.Euler(0, 0, Random.Range(-m_HitRandomRotation, m_HitRandomRotation));
            GameObjectPool.main.Rent(m_ArrowPrefab, m_Arrow.position, rot, obj.attachTarget);

            ReturnOrDeactive();
        }

#if UNITY_EDITOR
        private FBezier m_Editor_Bezier;

        private void OnDrawGizmos()
        {
            Vector2 a = m_Editor_Bezier.Evaluate(0);
            for (int i = 1; i <= 32; i++)
            {
                Vector2 b = m_Editor_Bezier.Evaluate(i / 32f);
                Gizmos.DrawLine(a, b);
                a = b;
            }

            Gizmos.color = Color.yellow;
            Vector2 z = a + (m_Editor_Bezier.Evaluate(1) - m_Editor_Bezier.Evaluate(0.99f)).normalized * 10;
            Gizmos.DrawLine(a, z);
        }
#endif
    }
}
