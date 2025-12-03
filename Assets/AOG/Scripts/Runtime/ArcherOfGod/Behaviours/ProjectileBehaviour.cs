using System;
using System.Threading.Tasks;
using Codice.CM.Common.Partial;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        //-- Serializable
        [SerializeField] private Rigidbody2D m_Rigidbody;
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private ParticleSystem m_Particle;
        [SerializeField] private Collider2D[] m_Colliders;
        [SerializeField] private float m_AutoDestroySec = 5f;
        //[SerializeField] private AnimationCurve m_DurationPerDistance = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField] private float m_ArrowSpeed = 0.3f;
        [SerializeField] private float m_BezierPower = 0.3f;
        [SerializeField] private float m_FadeDuration = 0.5f;
        [SerializeField] private Vector2Int m_HitSkipMs = new(50, 100); // x:고정대기 + y:랜덤대기(0~y)

        //-- Private
        private bool m_IsCollision;
        private bool m_IsHitObject;
        public float angleOffset;
        private Bullet m_Bullet;

        //-- Properties
        public ObjectBehaviour Owner { get; private set; }

        //------------------------------------------------------------------------------

        private void Awake()
        {
        }

        private void Update()
        {
        }

        private class Bullet : MonoBehaviour
        {
            public ProjectileBehaviour Parrent;

            private void OnCollisionEnter2D(Collision2D collision)
            {
                Parrent?.OnCollision2D(collision.collider);
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                Parrent?.OnCollision2D(collision);
            }
        }

        public async UniTaskVoid Shoot(ObjectBehaviour owner, Vector3 tPos)
        {
            this.Owner = owner;

            Vector3 mPos = m_Rigidbody.position;
            float mRot = m_Rigidbody.rotation;
            float tRot = AngleUtils.Reverse(mRot);
            FBezier bezier = new FBezier(mPos, tPos, mRot, tRot, m_BezierPower);

#if UNITY_EDITOR
            m_Editor_Bezier = bezier;
#endif

            float distance = bezier.Distance();
            float move = 0;

            Vector2 befPos = mPos;
            m_Bullet = m_Rigidbody.gameObject.AddComponent<Bullet>();

            // 도착할때까지 혹은 이동시간이 예상의 2배를 넘을때까지 이동
            while (!m_IsCollision && move < distance)
            {
                move += m_ArrowSpeed * Time.deltaTime;

                // 진행도
                float ratio = Mathf.Clamp01(move / distance);

                if (ratio > 0.5f)
                {
                    m_Bullet.Parrent = this;
                }

                // 새 위치 
                Vector2 newPos = bezier.Evaluate(ratio);

                float angle = Vector2.SignedAngle(Vector2.right, newPos - befPos);
                m_Rigidbody.MovePositionAndRotation(newPos, angle + angleOffset);

                befPos = newPos;

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
            }

            if (!m_IsCollision)
            {
                float destroyLimit = Time.time + m_AutoDestroySec;

                Vector2 dir = AngleUtils.ToDirection(m_Rigidbody.rotation);
                while (!m_IsCollision && Time.time < destroyLimit)
                {
                    m_Rigidbody.MovePosition(m_Rigidbody.position + (dir * (m_ArrowSpeed * Time.deltaTime)));

                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, destroyCancellationToken);
                }
            }

            if (m_IsHitObject) return;

            StopGeneration();

            // 랜더링 사라지기
            await m_Renderer.DOFade(0, m_FadeDuration).ToUniTask();

            // 삭제
            Destroy(gameObject);
        }

        private void StopGeneration()
        {
            // stop generate collision
            m_Rigidbody.Sleep();
            foreach (var col in m_Colliders)
            {
                col.enabled = false;
            }

            // stop generate particle
            m_Particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnCollision2D(Collider2D collision)
        {
            var obj = collision.GetComponentInParent<ObjectBehaviour>();
            if (obj != null)
            {
                m_IsHitObject = true;
                obj.OnHit(Owner, this);

                HitAsync(obj);

            }
            else
            {
                m_IsCollision = true;
            }

        }

        private async UniTask HitAsync(ObjectBehaviour obj)
        {
            StopGeneration();

            await UniTask.Delay(m_HitSkipMs.x + UnityEngine.Random.Range(0, m_HitSkipMs.y));

            this.ReturnPool();

            DestroyImmediate(gameObject);

            obj.AttachTo(attachment);

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
