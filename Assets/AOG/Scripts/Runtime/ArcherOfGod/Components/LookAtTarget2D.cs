using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AOT
{
    /// <summary>
    /// 타겟 바라보게 회전시킴. LateUpdate에서 갱신해서 애니메이션 후에 적용됨.
    /// offset으로 회전 보정 가능. 에디터에서도 됨. 스무딩 없이 바로 돌아감.
    /// </summary>
    [ExecuteInEditMode]
    public class LookAtTarget2D : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;
        
        private Quaternion m_CalcOffset;

        private void Start()
        {
            m_CalcOffset = Quaternion.Euler(offset);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector2 mPos = (Vector2)transform.position;
            Vector2 tPos = (Vector2)target.position;

#if UNITY_EDITOR || NOPT
            transform.rotation = Quaternion.LookRotation(tPos - mPos) * Quaternion.Euler(offset);
#else
            transform.rotation = Quaternion.LookRotation(tPos - mPos) * m_CalcOffset;
#endif
        }
    }
}