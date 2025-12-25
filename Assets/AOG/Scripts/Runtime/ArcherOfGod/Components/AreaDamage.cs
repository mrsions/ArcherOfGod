#nullable enable

using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 범위 데미지. Setup 호출되면 원형 범위 안에 있는 캐릭터한테 다 데미지 줌.
    /// IProjectileSetup 구현해서 투사체가 터질 때 호출됨. 거리별 감소 없음.
    /// </summary>
    public class AreaDamage : MonoBehaviour, IProjectileSetup
    {
        [SerializeField]
        private CircleCollider2D m_Collider = null!;
        [SerializeField]
        private float m_Damage;

        public void Setup(ProjectileBehaviour sender)
        {
            Vector2 worldCenter = transform.TransformPoint(m_Collider.offset);
            Collider2D[] cols = Physics2D.OverlapCircleAll(worldCenter, m_Collider.radius);
            for (int i = 0; i < cols.Length; i++)
            {
                Collider2D col = cols[i];
                CharacterBehaviour? cha = col.GetComponentInParent<CharacterBehaviour>();
                if (cha != null)
                {
                    var hit = new FHitEvent(sender.Owner, sender, m_Damage, col.bounds.center, 0);
                    cha.OnHit(hit);
                }
            }
        }
    }
}
