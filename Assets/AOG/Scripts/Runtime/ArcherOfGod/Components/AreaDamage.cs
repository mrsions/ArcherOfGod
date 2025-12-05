#nullable enable

using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 범위 데미지. Setup 호출되면 스피어 범위 안에 있는 캐릭터한테 다 데미지 줌.
    /// IProjectileSetup 구현해서 투사체가 터질 때 호출됨. 거리별 감소 없음.
    /// 3D Physics 씀 - 2D 게임인데 좀 이상함.
    /// </summary>
    public class AreaDamage : MonoBehaviour, IProjectileSetup
    {
        [SerializeField]
        private SphereCollider m_Collider;
        [SerializeField]
        private float m_Damage;

        public void Setup(ProjectileBehaviour sender)
        {
            Collider[] cols = Physics.OverlapSphere(m_Collider.center, m_Collider.radius, -1, QueryTriggerInteraction.Collide);
            for (int i = 0; i < cols.Length; i++)
            {
                Collider? col = cols[i];
                CharacterBehaviour cha = col.GetComponentInParent<CharacterBehaviour>();
                if(cha!= null)
                {
                    var hit = new FHitEvent(sender.Owner, sender, m_Damage, col.bounds.center, 0);
                    cha.OnHit(hit);
                }
            }
        }
    }
}