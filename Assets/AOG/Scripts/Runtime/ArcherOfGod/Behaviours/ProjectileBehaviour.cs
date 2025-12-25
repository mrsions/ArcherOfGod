using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace AOT
{
    public interface IProjectileSetup
    {
        void Setup(ProjectileBehaviour sender);
    }

    /// <summary>
    /// 화살 투사체. 베지어 곡선으로 날아가거나 직선으로 쏠 수도 있음. 유도 기능도 있음.
    /// 맞으면 FHitEvent로 데미지 넣고 화살이 타겟에 박힘. 피어싱이면 관통함.
    /// 자식 투사체 있으면 같이 쏨. 풀링 쓰고 충돌 안하면 일정시간 후 사라짐.
    /// 2D 물리만 됨. 내가 쏜건 안맞는데 아군 오사격은 가능함.
    /// </summary>
    public class ProjectileBehaviour : MonoBehaviour
    {
        public enum EDeactiveType
        {
            Return,
            Deactive,
            Destroy
        }

        /// <summary>
        /// 타격 판정 전달자
        /// </summary>
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
        [SerializeField] private GameObject m_HitFx;
        [SerializeField] private AudioSource m_Shot;

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

        [Tooltip("true일 경우 대상을 관통해도 계속 나아간다")]
        [SerializeField] private bool m_IsPiercing;

        [Tooltip("대상 추적이 활성화되는 시점 (거리)")]
        [SerializeField] private float m_GuideArrowStart = 1e9f;
        [Tooltip("추적이 시작된 뒤로 거리마다 붙는 추가 속도")]
        [SerializeField] private float m_GuideArrowSpeedPerDistance = 0.2f;
        [Tooltip("화살이 캐릭터와 닿았을때 박히는 정도")]
        [SerializeField] private float m_EmbeddingDepth = 0.4f;
        [Tooltip("화살 효과음의 시작 시간을 정할 수 있다. 0=애니메이션 발사시에 소리가 시작됨.  그외=생성된 이후 해당 초에 소리가 시작됨.")]
        [SerializeField] private float m_ShotSfxOnEnable = 0;

        [SerializeField] private ProjectileBehaviour[] children;

        [Header("HitRandom")]
        [SerializeField] private Vector2Int m_HitSkipMs = new(50, 100); // x:고정대기 + y:랜덤대기(0~y)
        [SerializeField] private float m_HitRandomRotation = 30; // x:고정대기 + y:랜덤대기(0~y)

        //-- Private
        private Bullet m_Bullet;
        private bool m_IsCollision;
        private bool m_IsHitObject;
        private bool m_IsDestroying;
        private int m_AsyncId;
        private Vector3 m_RigidbodyInitPos;
        private Vector3 m_RigidbodyInitRot;
        private float m_LastArrowSpeed;

        //-- Properties
        public ObjectBehaviour Owner { get; private set; }
        public bool UseStraight { get => m_UseStright; set => m_UseStright = value; }

        //------------------------------------------------------------------------------

        private void Awake()
        {
            m_Bullet = m_Rigidbody.gameObject.AddComponent<Bullet>();
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

            if(m_ShotSfxOnEnable > 0)
            {
                PlayShotAsync().Forget();
            }
        }

        private async UniTask PlayShotAsync()
        {
            await UniTask.WaitForSeconds(m_ShotSfxOnEnable);

            m_Shot.Play();
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
            m_IsDestroying = false;

            Color color = m_Renderer.color;
            color.a = 1;
            m_Renderer.color = color;

            m_Rigidbody.transform.localPosition = m_RigidbodyInitPos;
            m_Rigidbody.transform.localEulerAngles = m_RigidbodyInitRot;

            m_Bullet.Parent = null;

            SetKinematic();

            for (int i = 0; i < children.Length; i++)
            {
                ProjectileBehaviour child = children[i];
                child.gameObject.SetActive(true);
            }
        }

        private void SetPhysicsMode(bool isDynamic)
        {
            m_Rigidbody.bodyType = isDynamic ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                m_Colliders[i].enabled = isDynamic;
            }
        }

        private void SetKinematic() => SetPhysicsMode(false);
        private void SetDynamic() => SetPhysicsMode(true);

        public async UniTask LookAtEnemyAsync(Transform target)
        {
            int id = ++m_AsyncId;
            while (id == m_AsyncId)
            {
                // 조준 상태에서 노리는것이기때문에 projectile 자체가 움직인다.
                Vector2 dir = target.position - transform.position;
                transform.rotation = Quaternion.LookRotation(dir);

                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken: destroyCancellationToken);
            }
        }

        public async UniTask ShootAsync(ObjectBehaviour owner, Transform target)
        {
            Vector2 tPos = target.position;

            if (children != null)
            {
                foreach (var child in children)
                {
                    child.ShootAsync(owner, target).Forget();
                }
            }

            Vector2 targetOffset = m_TargetOffset;
            if (owner.IsRight)
            {
                targetOffset *= new Vector2(-1, 1);
            }
            tPos += targetOffset;


            int id = ++m_AsyncId;
            this.Owner = owner;

            Vector3 mPos = m_Rigidbody.position;
            float mRot = AngleUtils.GetAngleByDir(m_Rigidbody.transform.right);
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

#if UNITY_EDITOR|| NOPT
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

            if (m_ShotSfxOnEnable <=0 && m_Shot) m_Shot.Play();

            // 도착할때까지 혹은 이동시간이 예상의 2배를 넘을때까지 이동
            while (!m_IsCollision && move < distance)
            {
                move += arrowSpeed * Time.deltaTime;

                if (move > m_GuideArrowStart)
                {
                    // 거리별 속도 계산
                    float speed = arrowSpeed + (m_GuideArrowStart * (move - m_GuideArrowStart));
                    m_LastArrowSpeed = speed;

                    // Calculate direction to target
                    Vector2 direction = ((Vector2)target.position + targetOffset) - befPos;

                    // Calculate pos
                    Vector2 nPos = befPos + direction * (speed * Time.deltaTime);

                    // 회전 반영
                    Vector2 rDir = Vector2.Lerp(transform.right, direction.normalized, speed * Time.deltaTime);
                    m_Rigidbody.MovePositionAndRotation(nPos, AngleUtils.GetAngleByDir(rDir));

                    befPos = nPos;
                }
                else
                {
                    // 진행도
                    float ratio = Mathf.Clamp01(move / distance);

                    // 새 위치 
                    Vector2 nPos = bezier.Evaluate(ratio);

                    float angle = Vector2.SignedAngle(Vector2.right, nPos - befPos);
                    m_Rigidbody.MovePositionAndRotation(nPos, angle);

                    befPos = nPos;

                    m_LastArrowSpeed = arrowSpeed;
                }

                if (m_Bullet.Parent == null)
                {
                    if (isStartInLeft)
                    {
                        if (befPos.x > 0)
                        {
                            m_Bullet.Parent = this;
                            SetDynamic();
                        }
                    }
                    else
                    {
                        if (befPos.x < 0)
                        {
                            m_Bullet.Parent = this;
                            SetDynamic();
                        }
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
                if (id != m_AsyncId) return;
            }

            if (!m_IsCollision)
            {
                float destroyLimit = Time.time + m_AutoDestroySec;

                arrowSpeed = m_LastArrowSpeed;
                Vector2 dir = AngleUtils.ToDirection(m_Rigidbody.rotation);
                while (!m_IsCollision && Time.time < destroyLimit)
                {
                    float delta = (arrowSpeed * Time.deltaTime);
                    move += delta;

                    m_Rigidbody.MovePosition(m_Rigidbody.position + (dir * delta));

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
                    if (id != m_AsyncId) return;
                }
            }

            if (!m_IsDestroying)
            {
                m_IsDestroying = true;
                StopGeneration();

                // 랜더링 사라지기
                await m_Renderer.DOFade(0, m_FadeDuration).ToUniTask();

                // 삭제
                ReturnOrDeactive();
            }
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
            SetKinematic();

            // stop generate collision
            m_Rigidbody.Sleep();

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

                float dmg = Random.Range(m_Damage.x, m_Damage.y);
                FHitEvent hitEvent = new FHitEvent(Owner, this, dmg, m_Rigidbody.position, m_Rigidbody.rotation);
                obj.OnHit(hitEvent);

                if (!m_IsPiercing)
                {
                    m_IsHitObject = true;
                    SpawnHitEffect();
                    HitAsync(obj);
                }
            }
            else
            {
                m_IsCollision = true;
                SpawnHitEffect();
            }
        }

        private void SpawnHitEffect()
        {
            if (!m_HitFx) return;

            GameObject fx = GameObjectPool.main.Rent(m_HitFx, m_Rigidbody.position, m_Rigidbody.transform.rotation);
            IProjectileSetup[] array = fx.GetComponentsInChildren<IProjectileSetup>();
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Setup(this);
            }
        }

        private void HitAsync(ObjectBehaviour obj)
        {
            if (m_IsDestroying) return;
            m_IsDestroying = true;

            StopGeneration();

            Quaternion rot = m_Arrow.rotation * Quaternion.Euler(0, 0, Random.Range(-m_HitRandomRotation, m_HitRandomRotation));
            GameObject arrow = GameObjectPool.main.Rent(m_ArrowPrefab, m_Arrow.position, rot, obj.AttachTarget);

            float spd = m_EmbeddingDepth / m_LastArrowSpeed;
            Vector3 end = arrow.transform.localPosition + (arrow.transform.localRotation * Vector3.up) * m_EmbeddingDepth;
            arrow.transform.DOLocalMove(end, spd).SetEase(Ease.Linear);

            ReturnOrDeactive();
        }

#if UNITY_EDITOR|| NOPT
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
