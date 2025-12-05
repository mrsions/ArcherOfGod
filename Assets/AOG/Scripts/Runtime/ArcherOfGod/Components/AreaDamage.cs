#nullable enable

using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;

namespace AOT
{
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